namespace CRM;

public partial interface IDataAccess
{
    DataObjects.ApplicationSettings GetApplicationSettings();
    Task<DataObjects.ApplicationSettings> SaveApplicationSettings(DataObjects.ApplicationSettings settings, DataObjects.User CurrentUser);
}

public partial class DataAccess
{
    public DataObjects.ApplicationSettings GetApplicationSettings()
    {
        DataObjects.ApplicationSettings output = new DataObjects.ApplicationSettings {
            ActionResponse = GetNewActionResponse(true),
            ApplicationURL = ApplicationURL,
            DefaultTenantCode = DefaultTenantCode,
            EncryptionKey = ConvertByteArrayToString(GetEncryptionKey),
            MailServerConfig = MailServerConfig,
            MaintenanceMode = MaintenanceMode,
            DefaultReplyToAddress = DefaultReplyToAddress,
            UseTenantCodeInUrl = UseTenantCodeInUrl,
            ShowTenantListingWhenMissingTenantCode = ShowTenantListingWhenMissingTenantCode,
        };

        return GetApplicationSettingsApp(output);
    }

    public async Task<DataObjects.ApplicationSettings> SaveApplicationSettings(DataObjects.ApplicationSettings settings, DataObjects.User CurrentUser)
    {
        DataObjects.ApplicationSettings output = settings;
        output.ActionResponse = GetNewActionResponse();

        var originalJson = SerializeObject(AppSettings);
        DataObjects.ApplicationSettingsUpdate originalValues = new DataObjects.ApplicationSettingsUpdate {
            ApplicationURL = ApplicationURL,
            DefaultTenantCode = DefaultTenantCode,
            MaintenanceMode = MaintenanceMode,
            UseTenantCodeInUrl = UseTenantCodeInUrl,
            ShowTenantListingWhenMissingTenantCode = ShowTenantListingWhenMissingTenantCode,
        };

        if (!CurrentUser.AppAdmin) {
            output.ActionResponse.Messages.Add("Access Denied");
            return output;
        }

        // Save each individual settings item.
        List<DataObjects.BooleanResponse> savedItems = new List<DataObjects.BooleanResponse>();

        savedItems.Add(SaveSetting("ApplicationURL", DataObjects.SettingType.Text, output.ApplicationURL, null, null, "", CurrentUser));
        savedItems.Add(SaveSetting("DefaultReplyToAddress", DataObjects.SettingType.Text, output.DefaultReplyToAddress, null, null, "", CurrentUser));
        savedItems.Add(SaveSetting("DefaultTenantCode", DataObjects.SettingType.Text, output.DefaultTenantCode, null, null, "", CurrentUser));
        savedItems.Add(SaveSetting("MailServerConfig", DataObjects.SettingType.EncryptedObject, output.MailServerConfig, null, null, "", CurrentUser));
        savedItems.Add(SaveSetting("MaintenanceMode", DataObjects.SettingType.Boolean, output.MaintenanceMode, null, null, "", CurrentUser));
        savedItems.Add(SaveSetting("ShowTenantListingWhenMissingTenantCode", DataObjects.SettingType.Boolean, output.ShowTenantListingWhenMissingTenantCode, null, null, "", CurrentUser));
        savedItems.Add(SaveSetting("UseTenantCodeInUrl", DataObjects.SettingType.Boolean, output.UseTenantCodeInUrl, null, null, "", CurrentUser));

        foreach (var item in savedItems) {
            if (!item.Result && item.Messages.Any()) {
                foreach (var msg in item.Messages) {
                    output.ActionResponse.Messages.Add(msg);
                }
            }
        }

        if (output.ActionResponse.Messages.Any()) {
            return output;
        } else {
            output.ActionResponse.Result = true;
        }

        CacheStore.SetCacheItem(Guid.Empty, "ApplicationURL", output.ApplicationURL);
        CacheStore.SetCacheItem(Guid.Empty, "DefaultReplyToAddress", output.DefaultReplyToAddress);
        CacheStore.SetCacheItem(Guid.Empty, "DefaultTenantCode", output.DefaultTenantCode);
        CacheStore.SetCacheItem(Guid.Empty, "MailServerConfig", output.MailServerConfig);
        CacheStore.SetCacheItem(Guid.Empty, "MaintenanceMode", output.MaintenanceMode);
        CacheStore.SetCacheItem(Guid.Empty, "ShowTenantListingWhenMissingTenantCode", output.ShowTenantListingWhenMissingTenantCode);
        CacheStore.SetCacheItem(Guid.Empty, "UseTenantCodeInUrl", output.UseTenantCodeInUrl);

        // See if the EncryptionKey value has changed.
        var encryptionKey = GetSetting<string>("EncryptionKey", DataObjects.SettingType.Text);
        if (output.EncryptionKey != encryptionKey) {
            var keyChanged = UpdateApplicationEncryptionKey(encryptionKey, output.EncryptionKey);
            if (!keyChanged.Result) {
                output.ActionResponse.Result = false;
                Utilities.ConcatenateListsOfStrings(output.ActionResponse.Messages, keyChanged.Messages);
            }
        }

        output = await SaveApplicationSettingsApp(output, CurrentUser);
        var updatedJson = SerializeObject(AppSettings);

        if (output.ActionResponse.Result) {
            // Get a fresh copy of the settings to return
            output = GetApplicationSettings();
        }

        DataObjects.ApplicationSettingsUpdate updatedValues = GetApplicationSettingsUpdateApp(new DataObjects.ApplicationSettingsUpdate {
            ApplicationURL = ApplicationURL,
            DefaultTenantCode = DefaultTenantCode,
            MaintenanceMode = MaintenanceMode,
            ShowTenantListingWhenMissingTenantCode = ShowTenantListingWhenMissingTenantCode,
            UseTenantCodeInUrl = UseTenantCodeInUrl,
        });

        if (
            originalValues.ApplicationURL != updatedValues.ApplicationURL ||
            originalValues.DefaultTenantCode != updatedValues.DefaultTenantCode ||
            originalValues.MaintenanceMode != updatedValues.MaintenanceMode ||
            originalValues.ShowTenantListingWhenMissingTenantCode != updatedValues.ShowTenantListingWhenMissingTenantCode ||
            originalValues.UseTenantCodeInUrl != updatedValues.UseTenantCodeInUrl ||
            originalJson != updatedJson
            ) {

            // A value has changed that clients need to be aware of. Send out a signalr message to every tenant.
            await SignalRUpdate(new DataObjects.SignalRUpdate {
                TenantId = null,
                UpdateType = DataObjects.SignalRUpdateType.Setting,
                Message = "applicationsettingsupdate",
                Object = updatedValues,
                UserId = CurrentUserId(CurrentUser),
            });
        }

        return output;
    }
}