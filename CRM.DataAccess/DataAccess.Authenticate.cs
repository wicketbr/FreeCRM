namespace CRM;

public partial interface IDataAccess
{
    DataObjects.CustomLoginProvider AdminCustomLoginProvider { get; }
    Task<DataObjects.User> Authenticate(DataObjects.Authenticate authentication, string fingerprint = "");
    bool UseCustomAuthenticationProviderFromAdminAccount { get; }
}

public partial class DataAccess
{
    public DataObjects.CustomLoginProvider AdminCustomLoginProvider
    {
        get {
            DataObjects.CustomLoginProvider output = new DataObjects.CustomLoginProvider();

            var adminSettings = GetTenantSettings(_guid1);
            if (!String.IsNullOrWhiteSpace(adminSettings.CustomAuthenticationName) && !String.IsNullOrWhiteSpace(adminSettings.CustomAuthenticationCode)) {
                output = new DataObjects.CustomLoginProvider {
                    Name = adminSettings.CustomAuthenticationName,
                    Icon = StringValue(adminSettings.CustomAuthenticationIcon),
                    ButtonClass = StringValue(adminSettings.CustomAuthenticationButtonClass),
                    Code = adminSettings.CustomAuthenticationCode,
                };
            }

            return output;
        }
    }

    /// <summary>
    /// authenticates a user login
    /// </summary>
    /// <param name="EmailAddress">the username to authenticate</param>
    /// <param name="Password">the password to authenticate</param>
    /// <returns>true if the credentials are valid, otherwise returns false</returns>
    public async Task<DataObjects.User> Authenticate(DataObjects.Authenticate authenticate, string fingerprint = "")
    {
        DataObjects.User output = new DataObjects.User();

        string emailAddress = StringValue(authenticate.Username).Trim();
        string password = StringValue(authenticate.Password).Trim();

        Guid userId = Guid.Empty;

        bool sudoLogin = false;
        if (emailAddress.ToLower().StartsWith("sudo ")) {
            sudoLogin = true;
            emailAddress = emailAddress.Substring(5).Trim();
        }

        // Find the first user record for this user where the account is not in the admin tenant.
        List<User>? users = null;

        if(emailAddress.ToLower() == "admin") {
            users = await data.Users
                .Include(x => x.Tenant)
                .Where(x =>
                    x.Tenant.Enabled &&
                    x.Username.ToLower() == "admin" &&
                    x.Enabled == true &&
                    x.Deleted != true
                ).ToListAsync();
        }else if(emailAddress.Contains("@")) {
            users = await data.Users
                .Include(x => x.Tenant)
                .Where(x =>
                    x.TenantId != _guid1 &&
                    x.Tenant.Enabled &&
                    x.Email != null &&
                    x.Email.ToLower() == emailAddress.ToLower() &&
                    x.Enabled == true &&
                    x.Deleted != true
                ).ToListAsync();
        } else {
            users = await data.Users
                .Include(x => x.Tenant)
                .Where(x =>
                    x.TenantId != _guid1 &&
                    x.Tenant.Enabled &&
                    x.Username.ToLower() == emailAddress.ToLower() &&
                    x.Enabled == true &&
                    x.Deleted != true
                ).ToListAsync();
        }

        if(!authenticate.TenantId.HasValue && !String.IsNullOrEmpty(authenticate.TenantCode)) {
            var tenant = await GetTenantFromCode(authenticate.TenantCode);
            if(tenant.ActionResponse.Result) {
                authenticate.TenantId = tenant.TenantId;
            }
        }

        if (authenticate.TenantId.HasValue) {
            users = users.Where(x => x.TenantId == (Guid)authenticate.TenantId).ToList();
        }

        string tenantAdminUserPassword = String.Empty;
        if (sudoLogin) {
            if (authenticate.TenantId.HasValue) {
                var tenantAdminUser = await data.Users.FirstOrDefaultAsync(x => x.UserId == authenticate.TenantId);
                if (tenantAdminUser != null) {
                    tenantAdminUserPassword += tenantAdminUser.Password;
                }

                if (String.IsNullOrWhiteSpace(tenantAdminUserPassword)) {
                    sudoLogin = false;
                }
            } else {
                sudoLogin = false;
            }
        }

        if(users != null && users.Any()) {
            // Find the first matching password
            foreach(var user in users) {
                if (user != null) {
                    userId = user.UserId;

                    // If this is a sudo login, only check to make sure the password
                    // matches the tenant admin user.
                    // Don't do any lockout checks or anything else.
                    // If this fails, just return the failure.
                    if (sudoLogin) {
                        output.ActionResponse.Result = HashPasswordValidate(password, tenantAdminUserPassword);
                        if (!output.ActionResponse.Result) {
                            return output;
                        }
                    } else {
                        // Check if this user is currently locked out.
                        if (user.LastLockoutDate.HasValue) {
                            // See if it has been at least X minutes since the last lockout.
                            DateTime lastLockoutDate = (DateTime)user.LastLockoutDate;
                            DateTime accountUnlockDate = lastLockoutDate.AddMinutes(_accountLockoutMinutes);
                            if (accountUnlockDate > DateTime.UtcNow) {
                                // Still locked out.
                                output.ActionResponse.Messages.Add("Account locked out for " + _accountLockoutMinutes.ToString() +
                                    " minutes at " + lastLockoutDate.ToString() + " due to too many failed login attempts. " + Environment.NewLine +
                                    "Please try again after " + accountUnlockDate.ToString() + " when the account unlocks.");

                                return output;
                            }

                            // At this point we are no longer under a lockout, so clear any previous lockouts.
                            user.LastLockoutDate = null;
                            user.FailedLoginAttempts = null;
                            await data.SaveChangesAsync();
                        }

                        if (user.Password == password) {
                            // password matches, but is stored in plain text and needs to be encrypted
                            user.Password = HashPassword(password);
                            await data.SaveChangesAsync();
                            output.ActionResponse.Result = true;
                        } else {
                            // See if the password uses the new hash
                            output.ActionResponse.Result = HashPasswordValidate(password, user.Password);
                        }
                    }

                    if (output.ActionResponse.Result) {
                        // Valid login, so return the User Object
                        output = await GetUser(user.UserId);

                        // If there are any plugins to update user info, do that now.
                        output = await UpdateUserFromPlugins(output);

                        // No matter the result from the UpdateUserFromPlugins, we are still in a valid result.
                        output.ActionResponse.Result = true;

                        output.AuthToken = GetUserToken(output.TenantId, output.UserId, fingerprint, sudoLogin);

                        user.LastLockoutDate = null;
                        user.FailedLoginAttempts = null;
                        await data.SaveChangesAsync();

                        if (!sudoLogin) {
                            // Only update the last login time if this wasn't a sudo login.
                            await UpdateUserLastLoginTime(user.UserId, "Local");
                        }
                        
                        return output;
                    } else if (!sudoLogin) {
                        await SetUserLockout(GuidValue(authenticate.TenantId), user.Email);
                    }
                }
            }
        }

        return output;
    }

    private async Task SetUserLockout(Guid TenantId, string Username)
    {
        var rec = await data.Users.FirstOrDefaultAsync(x => x.TenantId == TenantId && x.Username == Username);
        if (rec != null) {
            int currentAttempts = rec.FailedLoginAttempts.HasValue ? (int)rec.FailedLoginAttempts : 0;
            currentAttempts += 1;

            if (currentAttempts >= _accountLockoutMaxAttempts) {
                // Mark the account as locked out.
                rec.LastLockoutDate = DateTime.UtcNow;
            }
            rec.FailedLoginAttempts = currentAttempts;

            await data.SaveChangesAsync();
        }
    }

    public bool UseCustomAuthenticationProviderFromAdminAccount
    {
        get {
            bool output = false;

            var settings = GetTenantSettings(_guid1);
            if (settings.LoginOptions.Contains("custom") && !String.IsNullOrWhiteSpace(settings.CustomAuthenticationName) &&
                !String.IsNullOrWhiteSpace(settings.CustomAuthenticationCode)) {
                output = true;
            }

            return output;
        }
    }
}