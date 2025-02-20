namespace CRM;

public partial interface IDataAccess
{
    Task<T?> AppSetting<T>(string SettingName);
    Task<DataObjects.Setting> GetSetting(string SettingName);
    T? GetSetting<T>(string SettingName, DataObjects.SettingType SettingType, Guid? TenantId = null, Guid? UserId = null);
    DataObjects.TenantSettings GetSettings(Guid TenantId, bool FullSettings = false);
    Task<DataObjects.Setting> SaveSetting(DataObjects.Setting setting, Guid? TenantId = null, Guid? UserId = null, DataObjects.User? CurrentUser = null);
    DataObjects.BooleanResponse SaveSetting(string SettingName, DataObjects.SettingType SettingType, dynamic? Value, Guid? TenantId = null, Guid? UserId = null, string? Description = "", DataObjects.User? CurrentUser = null);
}

public partial class DataAccess
{
    /// <summary>
    /// Gets one of the application settings stored in the Settings table
    /// </summary>
    /// <param name="SettingName">The name of the setting to retrieve</param>
    /// <returns>the value stored in the settings table</returns>
    public async Task<T?> AppSetting<T>(string SettingName)
    {
        var output = default(T);

        bool newSetting = false;
        var rec = await data.Settings.FirstOrDefaultAsync(x => x.SettingName != null && x.SettingName.ToLower() == SettingName.ToLower());
        if (rec == null) {
            // Setting does not yet exist, so add it
            rec = new Setting();
            newSetting = true;

            rec.LastModified = DateTime.UtcNow;

            rec.SettingName = SettingName;
            switch (SettingName.ToLower()) {
                case "applicationurl":
                    rec.SettingName = "ApplicationURL";
                    rec.SettingType = "text";
                    rec.SettingNotes = "The Base URL to the Application";
                    rec.SettingText = "https://your.app.url/";
                    break;
                case "activedirectoryroot":
                    rec.SettingName = "ActiveDirectoryRoot";
                    rec.SettingType = "text";
                    rec.SettingNotes = "the root of your Active Directory for LDAP lookup purposes";
                    rec.SettingText = "your.ad.root";
                    break;
                case "mailserver":
                    rec.SettingName = "MailServer";
                    rec.SettingType = "text";
                    rec.SettingNotes = "The name of your SMTP Server.";
                    break;
                case "mailserverpassword":
                    rec.SettingName = "MailServerPassword";
                    rec.SettingType = "encrypted";
                    rec.SettingNotes = "The optional password for your SMTP Server";
                    break;
                case "mailserverport":
                    rec.SettingName = "MailServerPort";
                    rec.SettingType = "number";
                    rec.SettingNotes = "The port of your SMTP Server (default is 25)";
                    break;
                case "mailserverusername":
                    rec.SettingName = "MailServerUsername";
                    rec.SettingType = "text";
                    rec.SettingNotes = "The optional username for your SMTP Server";
                    break;
                case "ldapusername":
                    rec.SettingName = "ldapUsername";
                    rec.SettingType = "encrypted";
                    rec.SettingNotes = "Optional username to use when accessing your LDAP server";
                    break;
                case "ldappassword":
                    rec.SettingName = "ldapPassword";
                    rec.SettingType = "encrypted";
                    rec.SettingNotes = "Optional password to use when accessing your LDAP server";
                    break;
                case "ldaplocationattribute":
                    rec.SettingName = "ldapLocationAttribute";
                    rec.SettingType = "text";
                    rec.SettingNotes = "Optional LDAP attribute to use for setting the Location property for users.";
                    rec.SettingText = "physicalDeliveryOfficeName";
                    break;
            }
            if (newSetting) {
                data.Add(rec);
            }
            await data.SaveChangesAsync();
        }
        // Now, return the value based on the type of setting

        string value = StringValue(rec.SettingText);

        switch (StringValue(rec.SettingType).ToLower()) {
            case "boolean":
            case "bool":
                bool boolOut = value.ToLower() == "true";
                output = (T)(object)boolOut;
                break;

            case "guid":
                Guid guidOut = Guid.Empty;
                try {
                    guidOut = new Guid(value);
                } catch { }
                output = (T)(object)guidOut;
                break;

            case "number":
                double dOut = 0;
                try {
                    dOut = Convert.ToDouble(value);
                } catch { }
                output = (T)(object)dOut;
                break;

            case "text":
                output = (T)(object)value;
                break;
        }

        return output;
    }

    protected void ClearTenantCache(Guid TenantId)
    {
        CacheStore.SetCacheItem(TenantId, "FullTenant", null);
        CacheStore.SetCacheItem(TenantId, "JasonWebTokenKey", null);
        CacheStore.SetCacheItem(TenantId, "Settings", null);
    }

    public async Task<DataObjects.Setting> GetSetting(string SettingName)
    {
        DataObjects.Setting output = new DataObjects.Setting();

        if (_open) {
            var rec = await data.Settings.FirstOrDefaultAsync(x => x.SettingName.ToLower() == SettingName.ToLower());
            if (rec == null) {
                // Call the common function to see if this needs to be created
                var result = await AppSetting<string>(SettingName);
                // Now try to get the item
                rec = await data.Settings.FirstOrDefaultAsync(x => x.SettingName.ToLower() == SettingName.ToLower());
                if (rec == null) {
                    output.ActionResponse.Messages.Add("Setting '" + SettingName + "' Does Not Exist");
                    return output;
                }
            }

            output = new DataObjects.Setting { 
                ActionResponse = GetNewActionResponse(true),
                LastModified = rec.LastModified,
                LastModifiedBy = LastModifiedDisplayName(rec.LastModifiedBy),
                SettingId = rec.SettingId,
                SettingName = rec.SettingName,
                SettingNotes = rec.SettingNotes,
                SettingText = rec.SettingText,
                SettingType = rec.SettingType,
                TenantId = rec.TenantId,
                UserId = rec.UserId,
            };
        }

        return output;
    }

    public T? GetSetting<T>(string SettingName, DataObjects.SettingType SettingType, Guid? TenantId = null, Guid? UserId = null)
    {
        var output = default(T);

        if (_open && !String.IsNullOrEmpty(SettingName)) {
            Setting? rec = null;

            if (UserId != null && TenantId != null) {
                rec = data.Settings.FirstOrDefault(x => x.SettingName != null && x.SettingName.ToLower() == SettingName.ToLower() && x.TenantId == TenantId && x.UserId == UserId);
            } else if (TenantId != null) {
                rec = data.Settings.FirstOrDefault(x => x.SettingName != null && x.SettingName.ToLower() == SettingName.ToLower() && x.TenantId == TenantId && x.UserId == null);
            } else if (UserId != null) {
                rec = data.Settings.FirstOrDefault(x => x.SettingName != null && x.SettingName.ToLower() == SettingName.ToLower() && x.TenantId == null && x.UserId == UserId);
            } else {
                rec = data.Settings.FirstOrDefault(x => x.SettingName != null && x.SettingName.ToLower() == SettingName.ToLower() && x.TenantId == null && x.UserId == null);
            }

            if (rec != null) {
                string value = String.Empty;
                if (!String.IsNullOrEmpty(rec.SettingText)) {
                    value = rec.SettingText;
                }

                switch (SettingType) {
                    case DataObjects.SettingType.Boolean:
                        bool bValue = value.ToLower() == "true";
                        output = (T)(object)bValue;
                        break;

                    case DataObjects.SettingType.DateTime:
                        DateTime dOut = DateTime.MinValue;
                        try {
                            dOut = Convert.ToDateTime(value);
                        } catch { }
                        output = (T)(object)dOut;
                        break;

                    case DataObjects.SettingType.EncryptedObject:
                        if (!String.IsNullOrEmpty(value)) {
                            string decrypted = Decrypt(value);
                            if (!String.IsNullOrWhiteSpace(decrypted)) {
                                value = decrypted;
                            }

                            var d = DeserializeObject<T>(value);
                            if (d != null) {
                                output = (T)(object)d;
                            }
                        }
                        break;

                    case DataObjects.SettingType.EncryptedText:
                        string decrytped = Decrypt(value);
                        if (!String.IsNullOrWhiteSpace(value) && String.IsNullOrWhiteSpace(decrytped)) {
                            // A value was stored in the table, but could not be decrypted. Assume this
                            // values was an unencrypted value that needs to be encrypted. So, encrypt
                            // the value here and update the record.
                            rec.SettingText = Encrypt(value);
                            data.SaveChanges();

                            decrytped = value;
                        }

                        output = (T)(object)(decrytped);
                        break;

                    case DataObjects.SettingType.Guid:
                        Guid gOut = Guid.Empty;
                        try {
                            gOut = new Guid(value);
                        } catch { }
                        output = (T)(object)gOut;
                        break;

                    case DataObjects.SettingType.NumberDecimal:
                        decimal decOut = 0;
                        try {
                            decOut = Convert.ToDecimal(value);
                        } catch { }
                        output = (T)(object)decOut;
                        break;

                    case DataObjects.SettingType.NumberDouble:
                        double dblOut = 0;
                        try {
                            dblOut = Convert.ToDouble(value);
                        } catch { }
                        output = (T)(object)dblOut;
                        break;

                    case DataObjects.SettingType.NumberInt:
                        int iOut = 0;
                        try {
                            iOut = Convert.ToInt32(value);
                        } catch { }
                        output = (T)(object)iOut;
                        break;

                    case DataObjects.SettingType.Object:
                        if (!String.IsNullOrEmpty(value)) {
                            var d = DeserializeObject<T>(value);
                            if (d != null) {
                                output = (T)(object)d;
                            }
                        }
                        break;

                    case DataObjects.SettingType.Text:
                        output = (T)(object)value;
                        break;

                }
            } else {
                // Record did not exist, so create it.
                SaveSetting(SettingName, SettingType, output, TenantId, UserId);
            }

            // Certain settings get cached, so if this is one of those settings set the cache here.
            switch (SettingName.ToUpper()) {
                case "MAILSERVER":
                    var mailServer = output != null ? (string)(object)output : String.Empty;
                    CacheStore.SetCacheItem(Guid.Empty, "MailServer", mailServer);
                    break;

                case "MAILSERVERPORT":
                    var mailServerPort = output != null ? (int)(object)output : 25;
                    CacheStore.SetCacheItem(Guid.Empty, "MailServerPort", mailServerPort);
                    break;

                case "MAILSERVERUSERNAME":
                    var mailServerUsername = output != null ? (string)(object)output : String.Empty;
                    CacheStore.SetCacheItem(Guid.Empty, "MailServerUsername", mailServerUsername);
                    break;

                case "MAILSERVERPASSWORD":
                    var mailServerPassword = output != null ? (string)(object)output : String.Empty;
                    CacheStore.SetCacheItem(Guid.Empty, "MailServerPassword", mailServerPassword);
                    break;

                case "MAILSERVERUSESSSL":
                    var mailServerUseSSL = output != null ? (bool)(object)output : false;
                    CacheStore.SetCacheItem(Guid.Empty, "MailServerUsesSSL", mailServerUseSSL);
                    break;
            }
        }

        return output;
    }

    public DataObjects.TenantSettings GetSettings(Guid TenantId, bool FullSettings = false)
    {
        DataObjects.TenantSettings output = new DataObjects.TenantSettings();

        // Check the cache first.
        var cached = CacheStore.GetCachedItem<DataObjects.TenantSettings>(TenantId, "Settings");

        if (cached != null) {
            output = cached;
        } else {
            var saved = GetTenantSettings(TenantId);
            if (saved != null) {
                output = saved;
                // Add to the cache store
                // CacheStore.GetCachedItem<DataObjects.Settings>(TenantId, "Settings");
                CacheStore.SetCacheItem(TenantId, "Settings", output);
            }
        }

        return output;
    }

    public async Task<DataObjects.Setting> SaveSetting(DataObjects.Setting setting, Guid? TenantId = null, Guid? UserId = null, DataObjects.User? CurrentUser = null)
    {
        DataObjects.Setting output = setting;
        output.ActionResponse = GetNewActionResponse();

        if (setting != null && !String.IsNullOrEmpty(setting.SettingType)) {
            Setting? rec = await data.Settings.FirstOrDefaultAsync(x => x.SettingId == setting.SettingId);

            if (rec != null) {
                bool save = true;

                switch (setting.SettingType.ToLower()) {
                    case "boolean":
                    case "datetime":
                    case "guid":
                    case "number":
                    case "text":
                        rec.SettingText = setting.SettingText;
                        break;

                    case "encryptedtext":
                        // Only encrypt the value if it's not already encrypted. This can be tested
                        // by attempting to decrypt the current value. If that returns a valid string
                        // then this value is already encrypted, so just store it.
                        string decrypted = Decrypt(StringValue(setting.SettingText));
                        if (String.IsNullOrEmpty(decrypted)) {
                            string encrypted = Encrypt(StringValue(setting.SettingText));
                            if (!String.IsNullOrWhiteSpace(encrypted)) {
                                rec.SettingText = encrypted;
                            } else {
                                rec.SettingText = "";
                            }
                        } else {
                            rec.SettingText = StringValue(setting.SettingText);
                        }
                        break;

                    case "object":
                        // Cannot use this method to save objects
                        save = false;
                        output.ActionResponse.Messages.Add("Cannot save Object settings with this method.");
                        break;
                }

                if (save) {
                    try {
                        rec.LastModified = DateTime.UtcNow;
                        if(CurrentUser != null) {
                            rec.LastModifiedBy = CurrentUserIdString(CurrentUser);
                        }

                        await data.SaveChangesAsync();
                        output.ActionResponse.Result = true;

                        // Clear tenant cache
                        if (TenantId != null) {
                            ClearTenantCache(GuidValue(TenantId));
                        }

                        await SignalRUpdate(new DataObjects.SignalRUpdate {
                            TenantId = output.TenantId,
                            UpdateType = DataObjects.SignalRUpdateType.Setting,
                            Message = "Saved",
                            Object = output,
                            UserId = CurrentUserId(CurrentUser),
                        });
                    } catch (Exception ex) {
                        output.ActionResponse.Messages.Add("Error Saving Setting " + setting.SettingName);
                        output.ActionResponse.Messages.AddRange(RecurseException(ex));
                    }
                }
            } else {
                output.ActionResponse.Messages.Add("Setting '" + setting.SettingName + "' Not Found");
            }
        }

        return output;
    }

    public DataObjects.BooleanResponse SaveSetting(string SettingName, DataObjects.SettingType SettingType, dynamic? Value, Guid? TenantId = null, Guid? UserId = null, string? Description = "", DataObjects.User? CurrentUser = null)
    {
        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();
        output.Messages = new List<string>();

        Setting? rec = null;

        if (UserId != null && TenantId != null) {
            rec = data.Settings.FirstOrDefault(x => x.SettingName == SettingName && x.TenantId == TenantId && x.UserId == UserId);
        } else if (TenantId != null) {
            rec = data.Settings.FirstOrDefault(x => x.SettingName == SettingName && x.TenantId == TenantId && x.UserId == null);
        } else if (UserId != null) {
            rec = data.Settings.FirstOrDefault(x => x.SettingName == SettingName && x.TenantId == null && x.UserId == UserId);
        } else {
            rec = data.Settings.FirstOrDefault(x => x.SettingName == SettingName && x.TenantId == null && x.UserId == null);
        }

        bool newRecord = false;
        if (rec == null) {
            newRecord = true;

            rec = new Setting {
                SettingNotes = Description,
                SettingName = SettingName,
                TenantId = TenantId,
                UserId = UserId,
            };
        }

        switch (SettingType) {
            case DataObjects.SettingType.Boolean:
                rec.SettingType = "Boolean";
                rec.SettingText = ((bool)Value).ToString();
                if (Value != null) {
                    rec.SettingText = ((bool)Value).ToString();
                } else {
                    rec.SettingText = "";
                }
                break;

            case DataObjects.SettingType.DateTime:
                rec.SettingType = "DateTime";
                if (Value != null) {
                    rec.SettingText = ((DateTime)Value).ToString();
                } else {
                    rec.SettingText = "";
                }
                break;

            case DataObjects.SettingType.EncryptedObject:
                rec.SettingType = "EncryptedObject";
                if (Value != null) {
                    rec.SettingText = Encrypt(SerializeObject(Value));
                } else {
                    rec.SettingText = "";
                }
                break;

            case DataObjects.SettingType.EncryptedText:
                rec.SettingType = "EncryptedText";
                if (Value != null) {
                    // Only encrypt the value if it's not already encrypted. This can be tested
                    // by attempting to decrypt the current value. If that returns a valid string
                    // then this value is already encrypted, so just store it.
                    string decrypted = Decrypt((string)Value);
                    if (String.IsNullOrEmpty(decrypted)) {
                        string encrypted = Encrypt((string)Value);
                        if (!String.IsNullOrWhiteSpace(encrypted)) {
                            rec.SettingText = encrypted;
                        } else {
                            rec.SettingText = "";
                        }
                    } else {
                        rec.SettingText = (string)Value;
                    }

                } else {
                    rec.SettingText = "";
                }
                break;

            case DataObjects.SettingType.Guid:
                rec.SettingType = "Guid";
                if (Value != null) {
                    rec.SettingText = ((Guid)Value).ToString();
                } else {
                    rec.SettingText = "";
                }
                break;

            case DataObjects.SettingType.NumberDecimal:
                rec.SettingType = "Number";
                if (Value != null) {
                    rec.SettingText = ((decimal)Value).ToString();
                } else {
                    rec.SettingText = "";
                }
                break;

            case DataObjects.SettingType.NumberDouble:
                rec.SettingType = "Number";
                if (Value != null) {
                    rec.SettingText = ((double)Value).ToString();
                } else {
                    rec.SettingText = "";
                }
                break;

            case DataObjects.SettingType.NumberInt:
                rec.SettingType = "Number";
                if (Value != null) {
                    rec.SettingText = ((int)Value).ToString();
                } else {
                    rec.SettingText = "";
                }
                break;

            case DataObjects.SettingType.Object:
                rec.SettingType = "Object";
                if (Value != null) {
                    rec.SettingText = SerializeObject(Value);
                } else {
                    rec.SettingText = "";
                }
                break;

            case DataObjects.SettingType.Text:
                rec.SettingType = "Text";
                if (Value != null) {
                    rec.SettingText = (string)Value;
                } else {
                    rec.SettingText = "";
                }
                break;

        }

        try {
            rec.LastModified = DateTime.UtcNow;
            if(CurrentUser != null) {
                rec.LastModifiedBy = CurrentUserIdString(CurrentUser);
            }

            if (newRecord) {
                data.Settings.Add(rec);
            }

            data.SaveChanges();
            output.Result = true;
        } catch (Exception ex) {
            output.Messages.Add("An error occurred while " + (newRecord ? "adding" : "updating") + " the setting " + SettingName);
            output.Messages.AddRange(RecurseException(ex));
        }

        if (output.Result) {
            // Certain settings get cached, so if this is one of those settings clear the cache here.
            switch (SettingName.ToUpper()) {
                case "APPLICATIONURL":
                    CacheStore.SetCacheItem(Guid.Empty, "ApplicationURL", null);
                    break;

                case "DEFAULTREPLYTOADDRESS":
                    CacheStore.SetCacheItem(Guid.Empty, "DefaultReplyToAddress", null);
                    break;

                case "MAILSERVER":
                    CacheStore.SetCacheItem(Guid.Empty, "MailServer", null);
                    break;

                case "MAILSERVERPORT":
                    CacheStore.SetCacheItem(Guid.Empty, "MailServerPort", null);
                    break;

                case "MAILSERVERUSERNAME":
                    CacheStore.SetCacheItem(Guid.Empty, "MailServerUsername", null);
                    break;

                case "MAILSERVERPASSWORD":
                    CacheStore.SetCacheItem(Guid.Empty, "MailServerPassword", null);
                    break;

                case "MAILSERVERUSESSL":
                    CacheStore.SetCacheItem(Guid.Empty, "MailServerUsesSSL", null);
                    break;
            }
        }

        return output;
    }
}