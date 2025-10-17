namespace CRM;

public partial interface IDataAccess
{
    Task<DataObjects.BooleanResponse> DeleteTenant(Guid TenantId);
    Task<DataObjects.BooleanResponse> DeleteTenantLogo(Guid TenantId);
    DataObjects.Tenant? GetTenant(Guid TenantId, DataObjects.User? CurrentUser = null);
    Task<DataObjects.Tenant> GetTenantFull(Guid TenantId, DataObjects.User? CurrentUser = null);
    Task<DataObjects.Tenant> GetTenantFromCode(string tenantCode, DataObjects.User? CurrentUser = null);
    Guid GetTenantIdFromCode(string tenantCode);
    DataObjects.Language GetTenantLanguage(Guid TenantId, string Culture = "en-US");
    Task<List<DataObjects.TenantList>> GetTenantList();
    Task<DataObjects.FileStorage> GetTenantLogo(Guid TenantId);
    Task<DataObjects.SimpleResponse> GetTenantLogoId(Guid TenantId);
    Task<List<DataObjects.Tenant>> GetTenants();
    Task<DataObjects.LoginTenantListing> GetTenantsForLogin();
    DataObjects.TenantSettings GetTenantSettings(Guid TenantId);
    List<DataObjects.UserListing> GetTenantUsers(Guid TenantId, int MaxRecords = 500, DataObjects.User? CurrentUser = null);
    Task<DataObjects.Tenant> SaveTenant(DataObjects.Tenant tenant, DataObjects.User? CurrentUser = null);
    void SaveTenantSettings(Guid TenantId, DataObjects.TenantSettings settings, DataObjects.User? CurrentUser = null);
}

public partial class DataAccess
{
    public async Task<DataObjects.BooleanResponse> DeleteTenant(Guid TenantId)
    {
        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();

        if (TenantId == _guid1) {
            output.Messages.Add("Cannot delete the built-in Admin account");
            return output;
        } else if (TenantId == _guid2) {
            output.Messages.Add("Cannot delete the built-in initial customer account.");
            return output;
        }

        try {
            var rec = await data.Tenants.FirstOrDefaultAsync(x => x.TenantId == TenantId);
            if (rec != null) {
                var deleteAppRecords = await DeleteRecordsApp(rec);
                if (!deleteAppRecords.Result) {
                    output.Messages.AddRange(deleteAppRecords.Messages);
                    return output;
                }

                var users = data.Users.Where(x => x.TenantId == TenantId).Select(o => o.UserId).ToList();

                // {{ModuleItemStart:Tags}}
                data.TagItems.RemoveRange(data.TagItems.Where(x => x.TenantId == TenantId));
                await data.SaveChangesAsync();

                data.Tags.RemoveRange(data.Tags.Where(x => x.TenantId == TenantId));
                await data.SaveChangesAsync();
                // {{ModuleItemEnd:Tags}}

                // {{ModuleItemStart:Appointments}}
                data.AppointmentNotes.RemoveRange(data.AppointmentNotes.Where(x => x.TenantId == TenantId));
                await data.SaveChangesAsync();

                data.AppointmentServices.RemoveRange(data.AppointmentServices.Where(x => x.TenantId == TenantId));
                await data.SaveChangesAsync();

                data.AppointmentUsers.RemoveRange(data.AppointmentUsers.Where(x => x.TenantId == TenantId));
                await data.SaveChangesAsync();

                data.Appointments.RemoveRange(data.Appointments.Where(x => x.TenantId == TenantId));
                await data.SaveChangesAsync();
                // {{ModuleItemEnd:Appointments}}

                // {{ModuleItemStart:EmailTemplates}}
                data.EmailTemplates.RemoveRange(data.EmailTemplates.Where(x => x.TenantId != TenantId));
                await data.SaveChangesAsync();
                // {{ModuleItemEnd:EmailTemplates}}

                // {{ModuleItemStart:Locations}}
                data.Locations.RemoveRange(data.Locations.Where(x => x.TenantId == TenantId));
                await data.SaveChangesAsync();
                // {{ModuleItemEnd:Locations}}

                // {{ModuleItemStart:Services}}
                data.Services.RemoveRange(data.Services.Where(x => x.TenantId == TenantId));
                await data.SaveChangesAsync();
                // {{ModuleItemEnd:Services}}

                data.FileStorages.RemoveRange(data.FileStorages.Where(x => x.TenantId == TenantId || users.Contains((Guid)x.UserId!)));
                await data.SaveChangesAsync();

                data.Settings.RemoveRange(data.Settings.Where(x => x.TenantId == TenantId));
                await data.SaveChangesAsync();

                data.DepartmentGroups.RemoveRange(data.DepartmentGroups.Where(x => x.TenantId == TenantId));
                await data.SaveChangesAsync();

                data.Departments.RemoveRange(data.Departments.Where(x => x.TenantId == TenantId));
                await data.SaveChangesAsync();

                data.Users.RemoveRange(data.Users.Where(x => x.TenantId == TenantId));
                await data.SaveChangesAsync();

                data.UDFLabels.RemoveRange(data.UDFLabels.Where(x => x.TenantId == TenantId));
                await data.SaveChangesAsync();

                data.Tenants.Remove(rec);
                await data.SaveChangesAsync();
            }
        } catch (Exception ex) {
            output.Messages.Add("An error occurred attempting to delete the tenant '" + TenantId.ToString() + "'");
            output.Messages.AddRange(RecurseException(ex));
        }

        output.Result = output.Messages.Count() == 0;

        if (output.Result) {
            await SignalRUpdate(new DataObjects.SignalRUpdate { 
                TenantId = TenantId,
                ItemId = TenantId,
                UpdateType = DataObjects.SignalRUpdateType.Tenant,
                Message = "Deleted",
            });

            // Also, notify all other tenants that this has been deleted since some users have
            // access to multiple tenants and the SignalR updates only go out per-tenant.
            var tenants = await GetTenants();
            if (tenants != null && tenants.Any()) {
                foreach(var item in tenants) {
                    if(item.TenantId != TenantId) {
                        await SignalRUpdate(new DataObjects.SignalRUpdate {
                            TenantId = item.TenantId,
                            ItemId = TenantId,
                            UpdateType = DataObjects.SignalRUpdateType.Tenant,
                            Message = "Deleted",
                        });
                    }
                }
            }
        }

        return output;
    }

    public async Task<DataObjects.BooleanResponse> DeleteTenantLogo(Guid TenantId)
    {
        var output = new DataObjects.BooleanResponse();

        var rec = await data.FileStorages.FirstOrDefaultAsync(x => x.TenantId == TenantId && x.SourceFileId == "logo");
        if(rec != null) {
            try {
                data.FileStorages.Remove(rec);

                await data.SaveChangesAsync();
            }catch(Exception ex) {
                output.Messages.Add("Error Deleting Logo");
                output.Messages.AddRange(RecurseException(ex));
            }
            
        }

        output.Result = output.Messages.Count() == 0;

        return output;
    }

    public DataObjects.Tenant? GetTenant(Guid TenantId, DataObjects.User? CurrentUser = null)
    {
        DataObjects.Tenant? output = null;

        var rec = data.Tenants.FirstOrDefault(x => x.TenantId == TenantId);
        if (rec != null) {
            output = new DataObjects.Tenant {
                ActionResponse = GetNewActionResponse(true),
                Added = rec.Added,
                AddedBy = LastModifiedDisplayName(rec.AddedBy),
                Enabled = rec.Enabled,
                LastModified = rec.LastModified,
                LastModifiedBy = LastModifiedDisplayName(rec.LastModifiedBy),
                Name = rec.Name,
                TenantId = rec.TenantId,
                TenantCode = rec.TenantCode,
                Users = GetTenantUsers(TenantId, 0, CurrentUser),
            };

            var settings = GetTenantSettings(TenantId);
            if (settings != null) {
                output.TenantSettings = settings;
            }
        }

        return output;
    }

    public async Task<DataObjects.Tenant> GetTenantFull(Guid TenantId, DataObjects.User? CurrentUser = null)
    {
        DataObjects.Tenant output = new DataObjects.Tenant();

        var cached = CacheStore.GetCachedItem<DataObjects.Tenant>(TenantId, "FullTenant");
        if (cached != null) {
            output = cached;
        } else {
            var tenant = GetTenant(TenantId, CurrentUser);
            if (tenant != null) {
                output = tenant;
                output.Departments = await GetDepartments(TenantId, CurrentUser);
                output.DepartmentGroups = await GetDepartmentGroups(TenantId, CurrentUser);
                output.udfLabels = await GetUDFLabels(TenantId);
            }
            CacheStore.SetCacheItem(TenantId, "FullTenant", output);
        }

        return output;
    }

    public async Task<DataObjects.Tenant> GetTenantFromCode(string tenantCode, DataObjects.User? CurrentUser = null)
    {
        DataObjects.Tenant output = new DataObjects.Tenant();

        Guid tenantId = Guid.Empty;
        var rec = await data.Tenants.FirstOrDefaultAsync(x => x.TenantCode != null && x.TenantCode.ToLower() == tenantCode.ToLower());
        if (rec != null) {
            tenantId = rec.TenantId;
        }

        if(tenantId != Guid.Empty) {
            output = await GetTenantFull(tenantId, CurrentUser);
        } else {
            output.ActionResponse.Messages.Add("Invalid Tenant Code '" + tenantCode + "'");
        }

        return output;
    }

    public Guid GetTenantIdFromCode(string tenantCode)
    {
        Guid output = Guid.Empty;

        if (!String.IsNullOrEmpty(tenantCode)) {
            var rec = data.Tenants.FirstOrDefault(x => x.TenantCode != null && x.TenantCode.ToLower() == tenantCode.ToLower());
            if (rec != null) {
                output = rec.TenantId;
            }
        }

        return output;
    }

    public DataObjects.Language GetTenantLanguage(Guid TenantId, string Culture = "en-US")
    {
        var output = GetDefaultLanguage();
        output.Culture = Culture;
        output.Description = CultureCodeDisplay(output.Culture);
        output.TenantId = TenantId;

        bool updated = false;

        // See if there is a saved language object for this tenant.
        var language = GetSetting<List<DataObjects.OptionPair>>("Language_" + Culture, DataObjects.SettingType.Object, TenantId);
        if (language != null) {

            // Go through each item in the defaults.
            // If any items in the Tenant's language are different than the defaults then update the output
            // and flag that it's been updated so we can save.
            List<DataObjects.OptionPair> missing = new List<DataObjects.OptionPair>();

            foreach (var item in output.Phrases) {
                var tenantItem = language.FirstOrDefault(x => StringValue(x.Id).ToLower() == StringValue(item.Id).ToLower());
                if (tenantItem != null) {
                    string value = StringValue(tenantItem.Value);
                    if (item.Value != value) {
                        item.Value = value;
                        updated = true;
                    }
                } else {
                    // Item does not exist in the user's saved language, so add it
                    missing.Add(item);
                }
            }

            if (missing.Count() > 0) {
                foreach (var item in missing) {
                    output.Phrases.Add(item);
                }
                output.Phrases = output.Phrases.OrderBy(x => x.Id).ToList();
            }
        } else {
            // Need to save for this Tenant with the defaults since they didn't have a value.
            updated = true;
        }

        if (updated && !String.IsNullOrWhiteSpace(Culture)) {
            SaveSetting("Language_" + Culture, DataObjects.SettingType.Object, output.Phrases, TenantId);
        }

        return output;
    }

    public async Task<List<DataObjects.TenantList>> GetTenantList()
    {
        var output = new List<DataObjects.TenantList>();

        var recs = await data.Tenants.Where(x => x.Enabled == true).ToListAsync();
        if(recs != null && recs.Any()) {
            foreach(var rec in recs) {
                output.Add(new DataObjects.TenantList { 
                    Name = rec.Name,
                    TenantCode = rec.TenantCode,
                    TenantId = rec.TenantId,
                });
            }
        }

        return output;
    }

    public async Task<DataObjects.FileStorage> GetTenantLogo(Guid TenantId)
    {
        DataObjects.FileStorage output = new DataObjects.FileStorage();

        var rec = await data.FileStorages.FirstOrDefaultAsync(x => x.ItemId == null && x.TenantId == TenantId && x.SourceFileId == "logo" && x.Deleted != true);
        if (rec != null) {
            output = new DataObjects.FileStorage {
                ActionResponse = GetNewActionResponse(true),
                TenantId = GuidValue(rec.TenantId),
                Extension = rec.Extension,
                Bytes = rec.Bytes.HasValue ? (long)rec.Bytes : (long?)null,
                Deleted = rec.Deleted,
                DeletedAt = rec.DeletedAt,
                FileId = rec.FileId,
                FileName = rec.FileName,
                ItemId = rec.ItemId,
                LastModified = rec.LastModified,
                LastModifiedBy = LastModifiedDisplayName(rec.LastModifiedBy),
                SourceFileId = rec.SourceFileId,
                UploadDate = rec.UploadDate,
                UploadedBy = LastModifiedDisplayName(rec.UploadedBy),
                UserId = rec.UserId,
                Value = rec.Value != null ? rec.Value.ToArray() : null,
            };
        }

        return output;
    }

    public async Task<DataObjects.SimpleResponse> GetTenantLogoId(Guid TenantId)
    {
        DataObjects.SimpleResponse output = new DataObjects.SimpleResponse();

        var rec = await data.FileStorages.FirstOrDefaultAsync(x => x.ItemId == null && x.TenantId == TenantId && x.SourceFileId == "logo" && x.Deleted != true);
        if (rec != null) {
            output = new DataObjects.SimpleResponse {
                Result = true,
                Message = rec.FileId.ToString(),
            };
        }

        return output;
    }

    public async Task<List<DataObjects.Tenant>> GetTenants()
    {
        List<DataObjects.Tenant> output = new List<DataObjects.Tenant>();

        var recs = await data.Tenants.ToListAsync();
        if (recs != null && recs.Any()) {
            var nonBuiltIn = new List<DataObjects.Tenant>();

            foreach (var rec in recs) {
                if (rec != null) {
                    var tenant = new DataObjects.Tenant {
                        ActionResponse = GetNewActionResponse(true),
                        Added = rec.Added,
                        AddedBy = LastModifiedDisplayName(rec.AddedBy),
                        Enabled = rec.Enabled,
                        LastModified = rec.LastModified,
                        LastModifiedBy = LastModifiedDisplayName(rec.LastModifiedBy),
                        Name = rec.Name,
                        TenantId = rec.TenantId,
                        TenantCode = rec.TenantCode,
                    };

                    var settings = GetTenantSettings(tenant.TenantId);
                    if (settings != null) {
                        tenant.TenantSettings = settings;
                    }

                    if (tenant.TenantId == _guid1 || tenant.TenantId == _guid2) {
                        output.Add(tenant);
                    } else {
                        nonBuiltIn.Add(tenant);
                    }
                }
            }

            output = output.OrderBy(x => x.TenantId.ToString()).ToList();

            if (nonBuiltIn.Any()) {
                output.AddRange(nonBuiltIn.OrderBy(x => x.Name).ToList());
            }
        }

        return output;
    }

    private List<DataObjects.Tenant> GetTenantsList()
    {
        var output = new List<DataObjects.Tenant>();

        var recs = data.Tenants.ToList();
        if (recs != null && recs.Any()) {
            foreach (var rec in recs) {
                if (rec != null) {
                    var tenant = new DataObjects.Tenant {
                        ActionResponse = GetNewActionResponse(true),
                        Added = rec.Added,
                        AddedBy = LastModifiedDisplayName(rec.AddedBy),
                        Enabled = rec.Enabled,
                        LastModified = rec.LastModified,
                        LastModifiedBy = LastModifiedDisplayName(rec.LastModifiedBy),
                        Name = rec.Name,
                        TenantId = rec.TenantId,
                        TenantCode = rec.TenantCode,
                    };

                    var settings = GetTenantSettings(tenant.TenantId);
                    if (settings != null) {
                        tenant.TenantSettings = settings;
                    }
                    output.Add(tenant);
                }
            }
        }

        return output;
    }

    public async Task<DataObjects.LoginTenantListing> GetTenantsForLogin()
    {
        DataObjects.LoginTenantListing output = new DataObjects.LoginTenantListing();

        var recs = await data.Tenants.Where(x => x.Enabled == true && x.TenantId != _guid1).ToListAsync();
        if (recs != null && recs.Any()) {
            foreach (var rec in recs) {
                if (rec != null) {
                    var tenant = new DataObjects.Tenant {
                        ActionResponse = GetNewActionResponse(true),
                        Added = rec.Added,
                        AddedBy = LastModifiedDisplayName(rec.AddedBy),
                        Enabled = true,
                        LastModified = rec.LastModified,
                        LastModifiedBy = LastModifiedDisplayName(rec.LastModifiedBy),
                        Name = rec.Name,
                        TenantId = rec.TenantId,
                        TenantCode = rec.TenantCode,
                    };

                    var settings = GetTenantSettings(tenant.TenantId);
                    if (settings != null) {
                        tenant.TenantSettings = settings;
                    }

                    output.Tenants.Add(tenant);
                    var languages = await GetTenantLanguages(tenant.TenantId);
                    if(languages != null && languages.Any()) {
                        foreach(var language in languages) {
                            output.Languages.Add(language);
                        }
                    }
                }
            }
        }

        return output;
    }

    public DataObjects.TenantSettings GetTenantSettings(Guid TenantId)
    {
        var defaultWorkSchedule = new DataObjects.WorkSchedule {
            Sunday = false,
            SundayAllDay = false,
            SundayStart = "",
            SundayEnd = "",

            Monday = true,
            MondayAllDay = false,
            MondayStart = "8:00 am",
            MondayEnd = "5:00 pm",

            Tuesday = true,
            TuesdayAllDay = false,
            TuesdayStart = "8:00 am",
            TuesdayEnd = "5:00 pm",

            Wednesday = true,
            WednesdayAllDay = false,
            WednesdayStart = "8:00 am",
            WednesdayEnd = "5:00 pm",

            Thursday = true,
            ThursdayAllDay = false,
            ThursdayStart = "8:00 am",
            ThursdayEnd = "5:00 pm",

            Friday = true,
            FridayAllDay = false,
            FridayStart = "8:00 am",
            FridayEnd = "5:00 pm",

            Saturday = false,
            SaturdayAllDay = false,
            SaturdayStart = "",
            SaturdayEnd = ""
        };

        DataObjects.TenantSettings output = new DataObjects.TenantSettings();

        bool saveSettings = false;

        var settings = GetSetting<DataObjects.TenantSettings>("Settings", DataObjects.SettingType.Object, TenantId);
        if (settings != null) {
            output = settings;
            if (settings.WorkSchedule == null) {
                settings.WorkSchedule = defaultWorkSchedule;
                saveSettings = true;
            } else if (!settings.WorkSchedule.Sunday && !settings.WorkSchedule.Monday && !settings.WorkSchedule.Tuesday
                 && !settings.WorkSchedule.Wednesday && !settings.WorkSchedule.Thursday && !settings.WorkSchedule.Friday && !settings.WorkSchedule.Saturday) {
                settings.WorkSchedule = defaultWorkSchedule;
                saveSettings = true;
            }
        } else {
            // Create default settings for this tenant.
            output = new DataObjects.TenantSettings {
                LoginOptions = new List<string>() { "local", "eitsso" },
                WorkSchedule = defaultWorkSchedule
            };

            saveSettings = true;
        }

        if (output.MaxToastMessages < 0) {
            output.MaxToastMessages = 8;
        }

        if (String.IsNullOrWhiteSpace(output.JwtRsaPublicKey) || String.IsNullOrWhiteSpace(output.JwtRsaPrivateKey)) {
            // Create a default RSA public and private key.
            var rsaKeys = RSAHelper.GenerateNewRsaKey();

            output.JwtRsaPrivateKey = rsaKeys.PrivateKey;
            output.JwtRsaPublicKey = rsaKeys.PublicKey;

            saveSettings = true;
        }

        // See if a tenant logo file has been uploaded.
        var file = data.FileStorages.FirstOrDefault(x => x.TenantId == TenantId && x.SourceFileId == "logo");
        if(file != null) {
            output.Logo = file.FileId;
        }

        if (saveSettings) {
            SaveTenantSettings(TenantId, output);
        }

        return output;
    }

    public List<DataObjects.UserListing> GetTenantUsers(Guid TenantId, int MaxRecords = 500, DataObjects.User? CurrentUser = null)
    {
        List<DataObjects.UserListing> output = new List<DataObjects.UserListing>();

        int count = 0;

        if (MaxRecords > 0) {
            if (AdminUser(CurrentUser)) {
                count = data.Users
                    .Include(x => x.Department)
                    .Where(x => x.TenantId == TenantId).Count();
            } else {
                count = data.Users
                    .Include(x => x.Department)
                    .Where(x => x.TenantId == TenantId && x.Deleted != true).Count();
            }
        }

        if(MaxRecords < 1 || count <= MaxRecords) {
            IQueryable<User>? recs = null;

            if (AdminUser(CurrentUser)) {
                recs = data.Users
                    .Include(x => x.Department)
                    .Where(x => x.TenantId == TenantId);

            } else {
                recs = data.Users
                    .Include(x => x.Department)
                    .Where(x => x.TenantId == TenantId && x.Deleted != true);

            }

            if (recs != null && recs.Any()) {
                foreach(var rec in recs) {
                    output.Add(new DataObjects.UserListing { 
                        UserId = rec.UserId,
                        FirstName = rec.FirstName,
                        LastName = rec.LastName,
                        Email = rec.Email,
                        Username = rec.Username,
                        Location = rec.Location,
                        Department = rec.DepartmentId != null && rec.Department != null && rec.Department.DepartmentName != null
                            ? rec.Department.DepartmentName
                            : String.Empty,
                        Enabled = rec.Enabled,
                        Deleted = rec.Deleted,
                        Admin = BooleanValue(rec.Admin),
                    });
                }
            }
        }

        return output;
    }

    private bool ModuleEnabled(string module, DataObjects.TenantSettings tenantSettings)
    {
        bool output = false;

        if (!String.IsNullOrWhiteSpace(module)) {
            bool blocked = tenantSettings.ModuleHideElements.Where(x => x.ToLower() == module.ToLower()).Count() > 0;
            if (!blocked) {
                output = tenantSettings.ModuleOptInElements.Where(x => x.ToLower() == module.ToLower()).Count() > 0;
            }
        }

        return output;
    }

    public async Task<DataObjects.Tenant> SaveTenant(DataObjects.Tenant tenant, DataObjects.User? CurrentUser = null)
    {
        DataObjects.Tenant output = tenant;
        output.ActionResponse = GetNewActionResponse();
        DateTime now = DateTime.UtcNow;

        if (tenant.TenantId != _guid1 && !String.IsNullOrEmpty(tenant.TenantCode) && tenant.TenantCode.ToLower() == "admin") {
            output.ActionResponse.Messages.Add("Invalid TenantCode. 'Admin' is a Reserved Code.");
            return output;
        }

        bool newRecord = false;
        var rec = await data.Tenants.FirstOrDefaultAsync(x => x.TenantId == tenant.TenantId);
        if (rec == null) {
            if (output.TenantId == Guid.Empty) {
                newRecord = true;
                output.TenantId = Guid.NewGuid();
                rec = new Tenant();
                rec.Added = now;
                rec.AddedBy = CurrentUserIdString(CurrentUser);
                rec.TenantId = output.TenantId;
            } else {
                output.ActionResponse.Messages.Add("Tenant '" + tenant.TenantId.ToString() + "' Not Found");
                return output;
            }
        }

        output.Name = MaxStringLength(output.Name, 200);
        output.TenantCode = MaxStringLength(output.TenantCode, 50);

        rec.Name = output.Name;
        rec.TenantCode = output.TenantCode;
        rec.Enabled = output.Enabled;
        rec.LastModified = now;

        if(CurrentUser != null) {
            rec.LastModifiedBy = CurrentUserIdString(CurrentUser);
        }

        try {
            if (newRecord) {
                await data.Tenants.AddAsync(rec);
            }
            await data.SaveChangesAsync();
            output.ActionResponse.Result = true;
            output.ActionResponse.Messages.Add(newRecord ? "New Tenant Added" : "Tenant Updated");

            if (newRecord) {
                SeedTestData();
                SeedTestData_CreateDefaultTenantData(output.TenantId);
            } else {
                //if (output.TenantSettings.ExternalUserDataSources != null && output.TenantSettings.ExternalUserDataSources.Any()) {
                //    output.TenantSettings.ExternalUserDataSources =
                //        output.TenantSettings.ExternalUserDataSources.OrderBy(x => x.SortOrder).ThenBy(x => x.Name).ToList();
                //}

                SaveTenantSettings(output.TenantId, output.TenantSettings, CurrentUser);
            }

            ClearTenantCache(output.TenantId);

            // Notify all tenants that this has been saved since some users have
            // access to multiple tenants and the SignalR updates only go out per-tenant.
            var tenants = await GetTenants();
            if (tenants != null && tenants.Any()) {
                foreach (var item in tenants) {
                    await SignalRUpdate(new DataObjects.SignalRUpdate {
                        TenantId = item.TenantId,
                        ItemId = output.TenantId,
                        UpdateType = DataObjects.SignalRUpdateType.Tenant,
                        Message = "Saved",
                        Object = output,
                        UserId = CurrentUserId(CurrentUser),
                    });
                }
            }
        } catch (Exception ex) {
            output.ActionResponse.Messages.Add("Error Saving Tenant '" + output.TenantId.ToString() + "'");
            output.ActionResponse.Messages.AddRange(RecurseException(ex));
        }

        return output;
    }

    public void SaveTenantSettings(Guid TenantId, DataObjects.TenantSettings settings, DataObjects.User? CurrentUser = null)
    {
        SaveSetting("Settings", DataObjects.SettingType.Object, settings, TenantId, null, "", CurrentUser);

        // Clear the cache
        CacheStore.SetCacheItem(TenantId, "FullTenant", null);
        CacheStore.SetCacheItem(TenantId, "ApplicationUrl", settings.ApplicationUrl);
    }
}