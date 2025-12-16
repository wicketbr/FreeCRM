using Azure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph.Models;
using Plugins;
using System.Net.Http;
using System.Reflection;
using Utilities;

namespace CRM;

public partial interface IDataAccess
{
    bool AdminUser(DataObjects.User? user);
    string AppendWithComma(string Original, string New);
    string ApplicationURL { get; }
    string ApplicationUrl(Guid? TenantId);
    string ApplicationUrl(Microsoft.AspNetCore.Http.HttpContext? context);
    string AppName { get; }
    bool BooleanValue(bool? value);
    string BytesToFileSizeLabel(long? bytes, List<string>? labels = null);
    string CleanHtml(string? html);
    IConfigurationHelper? ConfigurationHelper { get; }
    string ConnectionString(bool full = false);
    string ConnectionStringReport(string input);
    string CookiePrefix { get; }
    string CookieRead(string cookieName);
    void CookieWrite(string cookieName, string value, string cookieDomain = "");
    string Copyright { get; }
    string CultureCodeDisplay(string cc);
    Guid? CurrentUserId(DataObjects.User? user);
    string? CurrentUserIdString(DataObjects.User? user);
    EFDataModel Data { get; }
    string DatabaseType { get; }
    DateTime? DateOnlyToDateTime(DateOnly? dateOnly);
    public DateOnly? DateTimeToDateOnly(DateTime? dateTime);
    decimal DecimalValue(decimal? value);
    public string DefaultTenantCode { get; }
    Task<DataObjects.BooleanResponse> DeleteAllPendingDeletedRecords(Guid TenantId);
    Task<DataObjects.BooleanResponse> DeleteAllPendingDeletedRecords(Guid TenantId, DateTime OlderThan);
    Task<DataObjects.BooleanResponse> DeletePendingDeletedRecords();
    Task<DataObjects.BooleanResponse> DeleteRecordImmediately(string? Type, Guid RecordId, DataObjects.User CurrentUser);
    T? DeserializeObject<T>(string? SerializedObject);
    T? DeserializeObjectFromXmlOrJson<T>(string? SerializedObject);
    T? DuplicateObject<T>(object? o);
    string FormatStringAsGuid(string input);
    string GenerateRandomCode(int Length);
    DataObjects.AuthenticationProviders GetAuthenticationProviders();
    DataObjects.BlazorDataModelLoader GetBlazorDataModel();
    Task<DataObjects.BlazorDataModelLoader> GetBlazorDataModel(DataObjects.User CurrentUser, string fingerprint = "");
    Task<DataObjects.BlazorDataModelLoader> GetBlazorDataModelByTenantCode(string TenantCode);
    Task<DataObjects.DeletedRecordCounts> GetDeletedRecordCounts(Guid TenantId);
    Task<DataObjects.DeletedRecords> GetDeletedRecords(Guid TenantId);
    string GetFullUrl();
    string GetFullUrlWithoutQuerystring();
    DataObjects.BooleanResponse GetNewActionResponse(bool result = false, string? message = null);
    Guid GuidFromNumber(int number);
    Guid GuidFromNumber(long number);
    Guid GuidFromNumber(double number);
    Guid GuidValue(Guid? guid);
    string HashPassword(string? password);
    public bool HashPasswordValidate(string? password, string? hashedPassword);
    string HtmlToPlainText(string html);
    int IntValue(int? value);
    double NowFromUnixEpoch();
    List<string> MessageToListOfString(string message);
    string QueryStringValue(string valueName);
    List<string> RecurseException(Exception ex, bool ShowExceptionType = true);
    string RecurseExceptionAsString(Exception ex, bool ShowExceptionType = true);
    void Redirect(string url);
    DateOnly Released { get; }
    object RemoveSensitiveData(object o);
    string Replace(string input, string replaceText, string withText);
    string Request(string parameter);
    double RunningSince { get; }
    DataObjects.BooleanResponse SendEmail(DataObjects.EmailMessage message, DataObjects.MailServerConfig? config = null);
    string Serialize_ObjectToXml(object o, bool OmitXmlDeclaration = true);
    T? Serialize_XmlToObject<T>(string? xml);
    string SerializeObject(object? Object);
    void SetAuthenticationProviders(DataObjects.AuthenticationProviders? authenticationProviders);
    void SetHttpContext(Microsoft.AspNetCore.Http.HttpContext? context);
    void SetHttpRequest(Microsoft.AspNetCore.Http.HttpRequest? request);
    void SetHttpResponse(Microsoft.AspNetCore.Http.HttpResponse? response);
    Guid StringToGuid(string? input);
    string StringValue(string? input);
    Task<DataObjects.BooleanResponse> UndeleteRecord(string? Type, Guid RecordId, DataObjects.User CurrentUser);
    /// <summary>
    /// A unique string generated when the application starts.
    /// </summary>
    string UniqueId { get; }
    void UpdateApplicationURL(string? url);
    string UrlDecode(string? input);
    string UrlEncode(string? input);
    bool UseBackgroundService { get; }
    bool UseTenantCodeInUrl { get; }
    string Version { get; }
    DataObjects.VersionInfo VersionInfo { get; }
}

public partial class DataAccess
{
    public bool AdminUser(DataObjects.User? user)
    {
        return user != null && user.Admin;
    }

    public string AppendWithComma(string Original, string New)
    {
        string output = Original;
        if (!String.IsNullOrWhiteSpace(Original)) {
            output += ", ";
        }
        output += New;
        return output;
    }

    public string ApplicationURL { 
        get {
            if (!String.IsNullOrWhiteSpace(_localModeUrl)) {
                return _localModeUrl;
            }

            string output = StringValue(CacheStore.GetCachedItem<string>(Guid.Empty, "ApplicationURL"));
            if (String.IsNullOrWhiteSpace(output)) {
                output += GetSetting<string>("ApplicationURL", DataObjects.SettingType.Text);
                CacheStore.SetCacheItem(Guid.Empty, "ApplicationURL", output);
            }
            return output;
        }
    }

    public string ApplicationUrl(Guid? TenantId)
    {
        if (!TenantId.HasValue) {
            return ApplicationURL;
        }

        string output = StringValue(CacheStore.GetCachedItem<string>(TenantId.Value, "ApplicationURL"));
        if (String.IsNullOrWhiteSpace(output)) {
            var tenantSettings = GetTenantSettings(TenantId.Value);
            output = StringValue(tenantSettings.ApplicationUrl);

            if (!String.IsNullOrWhiteSpace(output)) {
                CacheStore.SetCacheItem(TenantId.Value, "ApplicationURL", output);
            }
        }

        if (String.IsNullOrEmpty(output)) {
            // This tenant does not use a custom Application URL, so return the main application URL.
            output = ApplicationURL;
        }

        return output;
    }

    public string ApplicationUrl(Microsoft.AspNetCore.Http.HttpContext? context)
    {
        string output = ApplicationURL;

        if (context != null) {
            var host = context.Request.Host.ToString();

            if (!String.IsNullOrWhiteSpace(host)) {
                string hostName = host;
                if ((!context.Request.IsHttps && hostName.EndsWith(":80")) || (context.Request.IsHttps && hostName.EndsWith(":443"))) {
                    hostName = hostName.Substring(0, hostName.IndexOf(":"));
                }

                var path = context.Request.Path.ToString();

                string hostUrl =
                    (context.Request.IsHttps ? "https://" : "http://") +
                    hostName + path;

                // Find any tenants with a matching Application URL. The longest match wins.
                var tenants = data.Tenants.Where(x => x.Enabled == true).Select(x => x.TenantId).ToList();
                if (tenants != null && tenants.Any()) {
                    List<string> tenantUrls = new List<string>();

                    foreach(var tenantId in tenants) {
                        var settings = GetSetting<DataObjects.TenantSettings>("Settings", DataObjects.SettingType.Object, tenantId);
                        if (settings != null && !String.IsNullOrWhiteSpace(settings.ApplicationUrl) && hostUrl.ToLower().StartsWith(settings.ApplicationUrl.ToLower())) {
                            if (!tenantUrls.Contains(settings.ApplicationUrl)) {
                                tenantUrls.Add(settings.ApplicationUrl);
                            }
                        }
                    }

                    if (tenantUrls.Any()) {
                        output = tenantUrls.OrderByDescending(x => x.Length).First();
                    }
                }
            }
        }

        return output;
    }

    public string AppName {
        get {
            return _appName;
        }
    }

    private DataObjects.ApplicationSettingsUpdate AppSettings
    {
        get {
            var output = new DataObjects.ApplicationSettingsUpdate {
                ApplicationURL = ApplicationURL,
                DefaultTenantCode = DefaultTenantCode,
                MaintenanceMode = MaintenanceMode,
                ShowTenantListingWhenMissingTenantCode = ShowTenantListingWhenMissingTenantCode,
                UseTenantCodeInUrl = UseTenantCodeInUrl,
            };

            return GetApplicationSettingsUpdateApp(output);
        }
    }

    public bool BooleanValue(bool? value)
    {
        bool output = value.HasValue ? (bool)value : false;
        return output;
    }

    public string BytesToFileSizeLabel(long? bytes, List<string>? labels = null)
    {
        string output = "";

        if (labels == null || labels.Count() < 4) {
            labels = new List<string> { "b", "kb", "m", "gb" };
        }

        if (bytes > 0) {
            if (bytes < 1024) {
                output = ((int)bytes).ToString() + labels[0];
            } else if (bytes < (1024 * 1024)) {
                output = ((int)(bytes / 1024)).ToString() + labels[1];
            } else if (bytes < (1024 * 1024 * 1024)) {
                output = ((int)(bytes / 1024 / 1024)).ToString() + labels[2];
            } else if (bytes < (1099511627776)) {
                output = ((int)(bytes / 1024 / 1024 / 1024)).ToString() + labels[3];
            }
        }

        return output;
    }

    public string CleanHtml(string? html)
    {
        string output = StringValue(html);
        if (!String.IsNullOrWhiteSpace(output)) {
            // First, if there are body tags only get the text in between the start and end tag.
            int BodyStart = output.ToLower().IndexOf("<body");
            int BodyEnd = output.ToLower().IndexOf("</body");
            if (BodyStart > -1 && BodyEnd > -1) {
                BodyStart = output.IndexOf(">", BodyStart + 1);
                if (BodyStart > -1) {
                    output = output.Substring(BodyStart + 1, (BodyEnd - BodyStart - 1));
                }
            }

            // Next, remove any style tags
            int Safety = 0;
            int StyleStart = output.ToLower().IndexOf("<style");
            while (StyleStart > -1) {
                Safety++;
                if (Safety > 100) {
                    return output;
                }
                int StyleEnd = output.ToLower().IndexOf("</style");
                if (StyleStart > -1 && StyleEnd > -1) {
                    StyleEnd = output.IndexOf(">", StyleEnd + 1);
                    if (StyleEnd > -1) {
                        string style = output.Substring(StyleStart, (StyleEnd - StyleStart + 1));
                        output = output.Replace(style, "");
                    }
                }
                StyleStart = output.ToLower().IndexOf("<style>");
            }
        }
        return output;
    }

    public IConfigurationHelper? ConfigurationHelper {
        get {
            IConfigurationHelper? output = null;

            if (_serviceProvider != null) {
                output = _serviceProvider.GetRequiredService<IConfigurationHelper>();
            }

            return output;
        }
    }

    private List<string> ConcatenateErrorMessages(DataObjects.User ReportedBy,
        DataObjects.User AffectedUser, List<DataObjects.User> AdditionalAffectedUsers)
    {
        List<string> output = new List<string>();
        if (!ReportedBy.ActionResponse.Result) {
            if (ReportedBy.ActionResponse.Messages != null && ReportedBy.ActionResponse.Messages.Count() > 0) {
                foreach (var msg in ReportedBy.ActionResponse.Messages) {
                    output.Add(msg);
                }
            }
        }

        if (!AffectedUser.ActionResponse.Result) {
            if (AffectedUser.ActionResponse.Messages != null && AffectedUser.ActionResponse.Messages.Count() > 0) {
                foreach (var msg in AffectedUser.ActionResponse.Messages) {
                    output.Add(msg);
                }
            }
        }

        if (AdditionalAffectedUsers != null && AdditionalAffectedUsers.Count() > 0) {
            foreach (var user in AdditionalAffectedUsers) {
                if (!user.ActionResponse.Result) {
                    if (user.ActionResponse.Messages != null && user.ActionResponse.Messages.Count() > 0) {
                        foreach (var msg in user.ActionResponse.Messages) {
                            output.Add(msg);
                        }
                    }
                }
            }
        }
        return output;
    }

    public string ConnectionString(bool full = false)
    {
        string output = ConnectionStringReport(_connectionString);
        if (full) {
            output = _connectionString;
        }
        return output;
    }

    public string ConnectionStringReport(string input)
    {
        string output = input;

        if (!String.IsNullOrWhiteSpace(output)) {
            List<string> elements = output.Split(';').ToList();
            if (elements != null && elements.Count() > 0) {
                string report = "";
                foreach (var element in elements) {
                    List<string> items = element.Split('=').ToList();
                    if (items != null && items.Count() > 0) {
                        switch (items[0].ToUpper()) {
                            case "DATABASE":
                            case "DATA SOURCE":
                            case "INITIAL CATALOG":
                            case "MULTIPLEACTIVERESULTSETS":
                            case "PROVIDER":
                            case "SERVER":
                            case "USER ID":
                                report += element + ";";
                                break;

                            case "PASSWORD":
                                report += "Password=***;";
                                break;
                        }
                    }
                    output = report;
                }
            } else {
                int loc = output.ToLower().IndexOf("initial catalog=");
                if (loc > -1) {
                    int locEnd = output.IndexOf(";", loc + 1);
                    if (locEnd > -1) {
                        output = output.Substring(0, locEnd);
                    }
                }
            }
        }

        return output;
    }

    public string CookiePrefix {
        get {
            return _cookiePrefix;
        }
    }

    public string CookieRead(string cookieName)
    {
        string output = String.Empty;

        if(!String.IsNullOrWhiteSpace(cookieName)) {
            if (_httpContext != null) {
                try {
                    if(_httpContext.Request != null) {
                        var ck = _httpContext.Request.Cookies[_cookiePrefix + cookieName];
                        if (!String.IsNullOrWhiteSpace(ck)) {
                            output = ck;
                        }
                    }
                } catch (Exception ex) {
                    if (ex != null) { }
                }
                if (output.ToLower() == "cleared") { output = String.Empty; }
            } else if (_httpRequest != null) {
                var cookieValue = _httpRequest.Cookies[_cookiePrefix + cookieName];
                if(!String.IsNullOrWhiteSpace(cookieValue)) {
                    output = cookieValue;
                }
            }
        }

        return System.Web.HttpUtility.HtmlDecode(output);
    }

    public void CookieWrite(string cookieName, string value, string cookieDomain = "")
    {
        if(!String.IsNullOrWhiteSpace(cookieName)) {
            DateTime now = DateTime.UtcNow;
            Microsoft.AspNetCore.Http.CookieOptions option = new Microsoft.AspNetCore.Http.CookieOptions();
            option.Expires = now.AddYears(1);
            string fullUrl = GetFullUrl();
            if (!String.IsNullOrWhiteSpace(cookieDomain) && !String.IsNullOrWhiteSpace(fullUrl) && !fullUrl.ToLower().Contains("localhost")) {
                option.Domain = cookieDomain;
            }

            if (_httpContext != null) {
                _httpContext.Response.Cookies.Append(_cookiePrefix + cookieName, value, option);
            } else if (_httpResponse != null) {
                _httpResponse.Cookies.Append(_cookiePrefix + cookieName, value, option);
            }
        }
    }

    public string Copyright {
        get {
            return _copyright;
        }
    }

    public string CultureCodeDisplay(string cc)
    {
        string output = String.Empty;

        try {
            var culture = System.Globalization.CultureInfo.GetCultureInfo(cc);
            if (culture != null) {
                output = culture.DisplayName;
            }
        } catch { }

        return output;
    }

    public Guid? CurrentUserId(DataObjects.User? user)
    {
        return user != null ? user.UserId : null;
    }

    public string? CurrentUserIdString(DataObjects.User? user)
    {
        return user != null ? user.UserId.ToString() : null;
    }

    public EFDataModel Data {
        get {
            return data;
        }
    }

    public string DatabaseType {
        get {
            return _databaseType;
        }
    }

    public DateTime? DateOnlyToDateTime(DateOnly? dateOnly)
    {
        DateTime? output = null;

        if (dateOnly.HasValue) {
            output = dateOnly.Value.ToDateTime(TimeOnly.Parse("12:00:00 PM"));
        }

        return output;
    }

    public DateOnly? DateTimeToDateOnly(DateTime? dateTime)
    {
        DateOnly? output = null;

        if (dateTime.HasValue) {
            output = DateOnly.FromDateTime(dateTime.Value);
        }

        return output;
    }

    public decimal DecimalValue(decimal? value)
    {
        decimal output = value.HasValue ? (decimal)value : 0;
        return output;
    }

    private string DefaultReplyToAddress {
        get {
            string output = String.Empty;
            if(CacheStore.ContainsKey(Guid.Empty, "DefaultReplyToAddress")) {
                output += CacheStore.GetCachedItem<string>(Guid.Empty, "DefaultReplyToAddress");
            } else {
                output += GetSetting<string>("DefaultReplyToAddress", DataObjects.SettingType.Text);
                CacheStore.SetCacheItem(Guid.Empty, "DefaultReplyToAddress", output);
            }
            return output;
        }
    }

    private string DefaultReplyToAddressForTenant(Guid TenantId)
    {
        string output = String.Empty;

        var settings = GetTenantSettings(TenantId);
        if (!String.IsNullOrWhiteSpace(settings.DefaultReplyToAddress)) {
            output = settings.DefaultReplyToAddress;
        }

        if (String.IsNullOrWhiteSpace(output)) {
            output = DefaultReplyToAddress;
        }

        return output;
    }

    public string DefaultTenantCode
    {
        get {
            string output = StringValue(CacheStore.GetCachedItem<string>(Guid.Empty, "DefaultTenantCode"));
            if (String.IsNullOrWhiteSpace(output)) {
                output += GetSetting<string>("DefaultTenantCode", DataObjects.SettingType.Text);
                CacheStore.SetCacheItem(Guid.Empty, "DefaultTenantCode", output);
            }
            return output;
        }
    }

    public async Task<DataObjects.BooleanResponse> DeleteAllPendingDeletedRecords(Guid TenantId)
    {
        var output = await DeleteAllPendingDeletedRecords(TenantId, Convert.ToDateTime("1/1/2000"));
        return output;
    }

    public async Task<DataObjects.BooleanResponse> DeleteAllPendingDeletedRecords(Guid TenantId, DateTime OlderThan)
    {
        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();

        // First, try any app-specific deletions.
        var deleteAppRecords = await DeleteAllPendingDeletedRecordsApp(TenantId, OlderThan);
        if (!deleteAppRecords.Result) {
            output.Messages.AddRange(deleteAppRecords.Messages);
            return output;
        }

        try {
            // {{ModuleItemStart:Appointments}}
            var appointments = await data.Appointments.Where(x => x.TenantId == TenantId && x.Deleted == true && (x.DeletedAt == null || x.DeletedAt < OlderThan)).ToListAsync();
            if(appointments != null && appointments.Any()) {
                foreach(var rec in appointments) {
                    var result = await DeleteAppointment(rec.AppointmentId, null, true);
                    if (!result.Result) {
                        output.Messages = result.Messages;
                        return output;
                    }
                }
            }

            var appointmentNotes = await data.AppointmentNotes.Where(x => x.TenantId == TenantId && x.Deleted == true && (x.DeletedAt == null || x.DeletedAt < OlderThan)).ToListAsync();
            if (appointmentNotes != null && appointmentNotes.Any()) {
                foreach(var rec in appointmentNotes) {
                    var result = await DeleteAppointmentNote(rec.AppointmentNoteId, null, true);
                    if (!result.Result) {
                        output.Messages = result.Messages;
                        return output;
                    }
                }
            }

            var appointmentServices = await data.AppointmentServices.Where(x => x.TenantId == TenantId && x.Deleted == true && (x.DeletedAt == null || x.DeletedAt < OlderThan)).ToListAsync();
            if (appointmentServices != null && appointmentServices.Any()) {
                foreach(var rec in appointmentServices) {
                    var result = await DeleteAppointmentService(rec.AppointmentServiceId, null, true);
                    if (!result.Result) {
                        output.Messages = result.Messages;
                        return output;
                    }
                }
            }
            // {{ModuleItemEnd:Appointments}}

            // Other items need to call their delete method to get all related data.
            var departmentGroups = await data.DepartmentGroups.Where(x => x.TenantId == TenantId && x.Deleted == true && (x.DeletedAt == null || x.DeletedAt < OlderThan)).ToListAsync();
            if(departmentGroups != null && departmentGroups.Any()) {
                foreach(var rec in departmentGroups) {
                    var result = await DeleteDepartmentGroup(rec.DepartmentGroupId, null, true);
                    if (!result.Result) {
                        output.Messages = result.Messages;
                        return output;
                    }
                }
            }

            var departments = await data.Departments.Where(x => x.TenantId == TenantId && x.Deleted == true && (x.DeletedAt == null || x.DeletedAt < OlderThan)).ToListAsync();
            if(departments != null && departments.Any()) {
                foreach(var rec in departments) {
                    var result = await DeleteDepartment(rec.DepartmentId, null, true);
                    if (!result.Result) {
                        output.Messages = result.Messages;
                        return output;
                    }
                }
            }

            // {{ModuleItemStart:EmailTemplates}}
            var emailTemplates = await data.EmailTemplates.Where(x => x.TenantId == TenantId && x.Deleted == true && (x.DeletedAt == null || x.DeletedAt < OlderThan)).ToListAsync();
            if (emailTemplates != null && emailTemplates.Any()) {
                foreach(var rec in emailTemplates) {
                    var result = await DeleteEmailTemplate(rec.EmailTemplateId, null, true);
                    if (!result.Result) {
                        output.Messages = result.Messages;
                        return output;
                    }
                }
            }
            // {{ModuleItemEnd:EmailTemplates}}

            var fileStorage = await data.FileStorages.Where(x => x.TenantId == TenantId && x.Deleted == true && (x.DeletedAt == null || x.DeletedAt < OlderThan)).ToListAsync();
            if (fileStorage != null && fileStorage.Any()) {
                foreach (var rec in fileStorage) {
                    var result = await DeleteFileStorage(rec.FileId, null, true);
                    if (!result.Result) {
                        output.Messages = result.Messages;
                        return output;
                    }
                }
            }

            // {{ModuleItemStart:Locations}}
            var locations = await data.Locations.Where(x => x.TenantId == TenantId && x.Deleted == true && (x.DeletedAt == null || x.DeletedAt < OlderThan)).ToListAsync();
            if(locations != null && locations.Any()) {
                foreach (var rec in locations) {
                    // Clear out this location in any appointments
                    await data.Database.ExecuteSqlRawAsync("UPDATE Appointments SET LocationId = NULL WHERE LocationId={0}", rec.LocationId);
                    await data.SaveChangesAsync();

                    var result = await DeleteLocation(rec.LocationId, null, true);
                    if (!result.Result) {
                        output.Messages = result.Messages;
                        return output;
                    }
                }
            }
            // {{ModuleItemEnd:Locations}}

            // {{ModuleItemStart:Services}}
            var services = await data.Services.Where(x => x.TenantId == TenantId && x.Deleted == true && (x.DeletedAt == null || x.DeletedAt < OlderThan)).ToListAsync();
            if(services != null && services.Any()) {
                foreach(var rec in services) {
                    await data.Database.ExecuteSqlRawAsync("DELETE FROM AppointmentServices WHERE ServiceId={0}", rec.ServiceId);
                    await data.SaveChangesAsync();

                    var result = await DeleteService(rec.ServiceId, null, true);
                    if (!result.Result) {
                        output.Messages = result.Messages;
                        return output;
                    }
                }
            }
            // {{ModuleItemEnd:Services}}

            // {{ModuleItemStart:Tags}}
            // For tags, remove any related items first, then delete the tags.
            var tags = await data.Tags.Where(x => x.TenantId == TenantId && x.Deleted == true && (x.DeletedAt == null || x.DeletedAt < OlderThan)).ToListAsync();
            if(tags != null && tags.Any()) {
                foreach(var rec in tags) {
                    await data.Database.ExecuteSqlRawAsync("DELETE FROM TagItems WHERE TagId={0}", rec.TagId);
                    await data.SaveChangesAsync();

                    var result = await DeleteTag(rec.TagId, null, true);
                    if (!result.Result) {
                        output.Messages = result.Messages;
                        return output;
                    }
                }
            }
            // {{ModuleItemEnd:Tags}}

            var userGroups = await data.UserGroups.Where(x => x.TenantId == TenantId && x.Deleted == true && (x.DeletedAt == null || x.DeletedAt < OlderThan)).ToListAsync();
            if(userGroups != null &&  userGroups.Any()) {
                foreach (var rec in userGroups) {
                    var result = await DeleteUserGroup(rec.GroupId, null, true);
                    if (!result.Result) {
                        output.Messages = result.Messages;
                        return output;
                    }
                }
            }

            var users = await data.Users.Where(x => x.TenantId == TenantId && x.Deleted == true && (x.DeletedAt == null || x.DeletedAt < OlderThan)).ToListAsync();
            if(users != null && users.Any()) {
                foreach(var rec in users) { 
                    var result = await DeleteUser(rec.UserId, null, true);
                    if (!result.Result) {
                        output.Messages = result.Messages;
                        return output;
                    }
                }
            }
        } catch (Exception ex) {
            output.Messages.Add("Error Deleting Records:");
            output.Messages.AddRange(RecurseException(ex));
        }

        output.Result = output.Messages.Count() == 0;

        return output;
    }

    public async Task<DataObjects.BooleanResponse> DeletePendingDeletedRecords() {
        var output = new DataObjects.BooleanResponse();
        var errors = new List<string>();
        var messages = new List<string>();

        var tenants = await GetTenants();

        foreach (var tenant in tenants) {
            if (tenant.TenantSettings.DeletePreference == DataObjects.DeletePreference.MarkAsDeleted) {
                var days = tenant.TenantSettings.DeleteMarkedRecordsAfterDays;
                if (days > 0) {
                    var deleteDate = DateTime.UtcNow.AddDays(-days);
                    var deleted = await DeleteAllPendingDeletedRecords(tenant.TenantId, deleteDate);

                    if (deleted.Result) {
                        messages.Add("Deleted Records Older Than '" + deleteDate.ToString() + "' for Tenant '" + tenant.Name + "'");
                        messages.AddRange(deleted.Messages);
                    } else {
                        messages.Add("Error Deleting Records Older Than '" + deleteDate.ToString() + "' for Tenant '" + tenant.Name + "'");
                        errors.AddRange(deleted.Messages);
                    }
                }
            }
        }

        output.Result = errors.Count() == 0;

        output.Messages.AddRange(messages);
        output.Messages.AddRange(errors);

        return output;
    }

    public async Task<DataObjects.BooleanResponse> DeleteRecordImmediately(string? Type, Guid RecordId, DataObjects.User CurrentUser)
    {
        var output = new DataObjects.BooleanResponse();

        if (!String.IsNullOrWhiteSpace(Type)) {
            switch (Type.ToLower()) {
                // {{ModuleItemStart:Appointments}}
                case "appointment":
                    output = await DeleteAppointment(RecordId, CurrentUser, true);
                    break;

                case "appointmentnote":
                    output = await DeleteAppointmentNote(RecordId, CurrentUser, true);
                    break;

                case "appointmentservice":
                    output = await DeleteAppointmentService(RecordId, CurrentUser, true);
                    break;
                // {{ModuleItemEnd:Appointments}}

                case "departmentgroup":
                    output = await DeleteDepartmentGroup(RecordId, CurrentUser, true);
                    break;

                case "department":
                    output = await DeleteDepartment(RecordId, CurrentUser, true);
                    break;

                // {{ModuleItemStart:EmailTemplates}}
                case "emailtemplate":
                    output = await DeleteEmailTemplate(RecordId, CurrentUser, true);
                    break;
                // {{ModuleItemEnd:EmailTemplates}}

                case "filestorage":
                    output = await DeleteFileStorage(RecordId, CurrentUser, true);
                    break;

                // {{ModuleItemStart:Locations}}
                case "location":
                    output = await DeleteLocation(RecordId, CurrentUser, true);
                    break;
                // {{ModuleItemEnd:Locations}}

                // {{ModuleItemStart:Services}}
                case "service":
                    output = await DeleteService(RecordId, CurrentUser, true);
                    break;
                // {{ModuleItemEnd:Services}}

                // {{ModuleItemStart:Tags}}
                case "tag":
                    output = await DeleteTag(RecordId, CurrentUser, true);
                    break;
                // {{ModuleItemEnd:Tags}}

                case "usergroup":
                    output = await DeleteUserGroup(RecordId, CurrentUser, true);
                    break;

                case "user":
                    output = await DeleteUser(RecordId, CurrentUser, true);
                    break;

                default:
                    output = await DeleteRecordImmediatelyApp(Type, RecordId, CurrentUser);
                    break;
            }
        } else {
            output.Messages.Add("Missing Required Data Type");
        }

        return output;
    }

    public T? DeserializeObject<T>(string? SerializedObject)
    {
        var output = default(T);

        if (!String.IsNullOrWhiteSpace(SerializedObject)) {
            try {
                var d = System.Text.Json.JsonSerializer.Deserialize<T>(SerializedObject, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (d != null) {
                    output = d;
                }
            } catch { }
        }

        return output;
    }

    public T? DeserializeObjectFromXmlOrJson<T>(string? SerializedObject)
    {
        var output = default(T);

        if (!String.IsNullOrWhiteSpace(SerializedObject)) {
            if (SerializedObject.StartsWith("<") || SerializedObject.ToLower().Contains("xmlns:")) {
                var deserializedXML = Serialize_XmlToObject<T>(SerializedObject);
                if (deserializedXML != null) {
                    output = deserializedXML;
                }
            } else {
                var deserializedJson = DeserializeObject<T>(SerializedObject);
                if (deserializedJson != null) {
                    output = deserializedJson;
                }
            }
        }

        return output;
    }

    private List<DataObjects.Dictionary>? DictionaryToListOfDictionary(Dictionary<string, string>? dict)
    {
        List<DataObjects.Dictionary>? output = null;

        if (dict != null && dict.Any()) {
            output = new List<DataObjects.Dictionary>();
            foreach (var item in dict) {
                output.Add(new DataObjects.Dictionary {
                    Key = item.Key,
                    Value = item.Value
                });
            }
        }

        return output;
    }

    public T? DuplicateObject<T>(object? o)
    {
        T? output = default(T);

        if (o != null) {
            try {
                // To make a new copy serialize the object and then deserialize it back to a new object.
                var serialized = System.Text.Json.JsonSerializer.Serialize(o);
                if (!String.IsNullOrEmpty(serialized)) {
                    var duplicate = System.Text.Json.JsonSerializer.Deserialize<T>(serialized, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (duplicate != null) {
                        output = duplicate;
                    }
                }
            } catch { }
        }

        return output;
    }

    private string FormatAppointmentTitle(string title, DateTime start, DateTime end, bool allDay)
    {
        string output = String.Empty;

        string startDate = start.ToString("d");
        string startTime = start.ToString("t");
        string endDate = end.ToString("d");
        string endTime = end.ToString("t");

        if (allDay) {
            if(startDate == endDate) {
                output = startDate + " All Day";
            } else {
                output = startDate + " - " + endDate + " All Day";
            }
        } else {
            if(startDate == endDate) {
                output = startDate + " " + startTime;

                if(endTime != startTime) {
                    output += " - " + endTime;
                }
            } else {
                output = startDate + " " + startTime + " - " + endDate + " " + endTime;
            }
        }

        if (!String.IsNullOrWhiteSpace(title)) {
            output += " - " + title;
        }

        return output;
    }

    public string FormatStringAsGuid(string input)
    {
        string output = String.Empty;

        if (!String.IsNullOrEmpty(input)) {
            string guid = input;
            if (guid.Length > 32) {
                guid = input.Substring(0, 32);
            }

            output = guid.Substring(0, 8) + "-" + guid.Substring(8, 4) + "-" + guid.Substring(12, 4) + "-" + guid.Substring(16, 4) + "-" + guid.Substring(20);
        }

        return output;
    }

    public string GenerateRandomCode(int Length)
    {
        string output = String.Empty;
        char[] Possibilities = "1234567890".ToCharArray();
        Random Randomizer = new Random();

        while (Length > 0) {
            output += Possibilities[Randomizer.Next(0, Possibilities.Length)];
            Length--;
        }

        return output;
    }

    public DataObjects.AuthenticationProviders GetAuthenticationProviders()
    {
        DataObjects.AuthenticationProviders output = new DataObjects.AuthenticationProviders();

        if(_authenticationProviders != null) {
            output = _authenticationProviders;
        }

        return output;
    }

    public DataObjects.BlazorDataModelLoader GetBlazorDataModel()
    {
        // Gets a limited version of the model.
        DataObjects.BlazorDataModelLoader output = new DataObjects.BlazorDataModelLoader();

        var tenantsList = GetTenantsList();
        if (tenantsList.Any()) {
            foreach(var item in tenantsList) {
                RemoveSensitiveData(item);
            }
        }

        output.ActiveUsers = new List<DataObjects.ActiveUser>();
        output.AdminCustomLoginProvider = AdminCustomLoginProvider;
        output.AllTenants = tenantsList;
        output.ApplicationUrl = ApplicationURL;
        output.AppSettings = AppSettings;
        output.AuthenticationProviders = _authenticationProviders;
        output.DefaultLanguage = GetDefaultLanguage();
        output.CultureCode = "en-US";
        output.CultureCodes = GetLanguageCultureCodes();
        output.Languages = new List<DataObjects.Language>();
        output.LoggedIn = false;
        output.Plugins = GetPluginsWithoutCode();
        output.Released = Released;
        output.TenantId = Guid.Empty;
        output.Tenants = new List<DataObjects.Tenant>();
        output.UseCustomAuthenticationProviderFromAdminAccount = UseCustomAuthenticationProviderFromAdminAccount;
        output.UseBackgroundService = _useBackgroundService;
        output.User = new DataObjects.User();
        output.Users = new List<DataObjects.User>();
        output.UseTenantCodeInUrl = UseTenantCodeInUrl;
        output.Version = Version;

        output = GetBlazorDataModelApp(output).Result;

        return output;
    }

    public async Task<DataObjects.BlazorDataModelLoader> GetBlazorDataModel(DataObjects.User CurrentUser, string fingerprint = "")
    {
        DataObjects.BlazorDataModelLoader output = new DataObjects.BlazorDataModelLoader();

        // First, get this user.
        if (CurrentUser != null && CurrentUser.ActionResponse.Result && CurrentUser.Enabled) {
            if (CurrentUser.AppAdmin) {
                await ValidateMainAdminUser(CurrentUser.UserId);
            }

            CurrentUser.AuthToken = GetUserToken(CurrentUser.TenantId, CurrentUser.UserId, fingerprint, CurrentUser.Sudo);

            List<DataObjects.Language> languages = new List<DataObjects.Language>();
            List<DataObjects.Tenant> tenants = new List<DataObjects.Tenant>();
            List<DataObjects.User> users = new List<DataObjects.User>();

            var allUsers = await GetUsersForEmailAddress(CurrentUser.Email, fingerprint, CurrentUser.Sudo);
            if (allUsers != null && allUsers.Any()) {
                users = allUsers;
            }

            if (users.Any()) {
                var allTenantIds = users.Select(x => x.TenantId).Distinct().ToList();
                foreach (var tenantId in allTenantIds) {
                    var tenant = GetTenant(tenantId, CurrentUser);
                    if (tenant != null && tenant.ActionResponse.Result && tenant.Enabled) {
                        tenants.Add(tenant);
                        var tenantLanguages = await GetTenantLanguages(tenant.TenantId);
                        if (tenantLanguages != null && tenantLanguages.Any()) {
                            foreach (var language in tenantLanguages) {
                                languages.Add(language);
                            }
                        }
                    }
                }
            }

            var tenantsList = GetTenantsList();
            if (tenantsList.Any()) {
                foreach (var item in tenantsList) {
                    RemoveSensitiveData(item);
                }
            }

            output.ActiveUsers = await GetActiveUsers(CurrentUser);
            output.AdminCustomLoginProvider = AdminCustomLoginProvider;
            output.AllTenants = tenantsList;
            output.ApplicationUrl = ApplicationURL;
            output.AppSettings = AppSettings;
            output.AuthenticationProviders = _authenticationProviders;
            output.DefaultLanguage = GetDefaultLanguage();
            output.CultureCode = "en-US";
            output.CultureCodes = GetLanguageCultureCodes();
            output.Languages = languages;
            output.LoggedIn = true;
            output.Plugins = GetPluginsWithoutCode();
            output.Released = Released;
            output.TenantId = CurrentUser.TenantId;
            output.Tenants = tenants;
            output.UseCustomAuthenticationProviderFromAdminAccount = UseCustomAuthenticationProviderFromAdminAccount;
            output.UseBackgroundService = _useBackgroundService;
            output.User = CurrentUser;
            output.Users = users;
            output.UseTenantCodeInUrl = UseTenantCodeInUrl;
            output.Version = Version;

            output = await GetBlazorDataModelApp(output, CurrentUser);
        }

        return output;
    }

    public async Task<DataObjects.BlazorDataModelLoader> GetBlazorDataModelByTenantCode(string TenantCode)
    {
        // Gets a limited version of the model for a single tenant using the TenantCode.
        DataObjects.BlazorDataModelLoader output = new DataObjects.BlazorDataModelLoader();

        var tenant = await GetTenantFromCode(TenantCode);
        if (tenant.ActionResponse.Result) {
            var tenantLanguages = await GetTenantLanguages(tenant.TenantId);

            var tenantsList = GetTenantsList();
            if (tenantsList.Any()) {
                foreach (var item in tenantsList) {
                    RemoveSensitiveData(item);
                }
            }

            output.AdminCustomLoginProvider = AdminCustomLoginProvider;
            output.AllTenants = tenantsList;
            output.ApplicationUrl = ApplicationURL;
            output.AppSettings = AppSettings;
            output.AuthenticationProviders = _authenticationProviders;
            output.DefaultLanguage = GetDefaultLanguage();
            output.CultureCode = "en-US";
            output.CultureCodes = GetLanguageCultureCodes();
            output.Languages = tenantLanguages;
            output.LoggedIn = false;
            output.Plugins = GetPluginsWithoutCode();
            output.Released = Released;
            output.TenantId = tenant.TenantId;
            output.Tenants = new List<DataObjects.Tenant>{ tenant };
            output.UseCustomAuthenticationProviderFromAdminAccount = UseCustomAuthenticationProviderFromAdminAccount;
            output.UseBackgroundService = _useBackgroundService;
            output.User = new DataObjects.User();
            output.Users = new List<DataObjects.User>();
            output.UseTenantCodeInUrl = UseTenantCodeInUrl;
            output.Version = Version;

            output = await GetBlazorDataModelApp(output);
        } else {
            output = GetBlazorDataModel();
        }

        return output;
    }

    public async Task<DataObjects.DeletedRecordCounts> GetDeletedRecordCounts(Guid TenantId)
    {
        // {{ModuleItemStart:Appointments}}
        var appointmentNotes = await data.AppointmentNotes.CountAsync(x => x.TenantId == TenantId && x.Deleted == true);
        var appointments = await data.Appointments.CountAsync(x => x.TenantId == TenantId && x.Deleted == true);
        var appointmentServices = await data.AppointmentServices.CountAsync(x => x.TenantId == TenantId && x.Deleted == true);
        // {{ModuleItemEnd:Appointments}}
        var departmentGroups = await data.DepartmentGroups.CountAsync(x => x.TenantId == TenantId && x.Deleted == true);
        var departments = await data.Departments.CountAsync(x => x.TenantId == TenantId && x.Deleted == true);
        // {{ModuleItemStart:EmailTemplates}}
        var emailTemplates = await data.EmailTemplates.CountAsync(x => x.TenantId == TenantId && x.Deleted == true);
        // {{ModuleItemEnd:EmailTemplates}}
        var fileStorage = await data.FileStorages.CountAsync(x => x.TenantId == TenantId && x.Deleted == true);
        // {{ModuleItemStart:Locations}}
        var locations = await data.Locations.CountAsync(x => x.TenantId == TenantId && x.Deleted == true);
        // {{ModuleItemEnd:Locations}}
        // {{ModuleItemStart:Services}}
        var services = await data.Services.CountAsync(x => x.TenantId == TenantId && x.Deleted == true);
        // {{ModuleItemEnd:Services}}
        // {{ModuleItemStart:Tags}}
        var tags = await data.Tags.CountAsync(x => x.TenantId == TenantId && x.Deleted == true);
        // {{ModuleItemEnd:Tags}}
        var userGroups = await data.UserGroups.CountAsync(x => x.TenantId == TenantId && x.Deleted == true);
        var users = await data.Users.CountAsync(x => x.TenantId == TenantId && x.Deleted == true);

        DataObjects.DeletedRecordCounts output = new DataObjects.DeletedRecordCounts {
            // {{ModuleItemStart:Appointments}}
            AppointmentNotes = appointmentNotes,
            Appointments = appointments,
            AppointmentServices = appointmentServices,
            // {{ModuleItemEnd:Appointments}}
            DepartmentGroups = departmentGroups,
            Departments = departments,
            // {{ModuleItemStart:EmailTemplates}}
            EmailTemplates = emailTemplates,
            // {{ModuleItemEnd:EmailTemplates}}
            FileStorage = fileStorage,
            // {{ModuleItemStart:Locations}}
            Locations = locations,
            // {{ModuleItemEnd:Locations}}
            // {{ModuleItemStart:Services}}
            Services = services,
            // {{ModuleItemEnd:Services}}
            // {{ModuleItemStart:Tags}}
            Tags = tags,
            // {{ModuleItemEnd:Tags}}
            UserGroups = userGroups,
            Users = users,
        };

        output = await GetDeletedRecordCountsApp(TenantId, output);

        return output;
    }

    public async Task<DataObjects.DeletedRecords> GetDeletedRecords(Guid TenantId)
    {
        // {{ModuleItemStart:Appointments}}
        List<DataObjects.DeletedRecordItem> appointmentNotes = new List<DataObjects.DeletedRecordItem>();
        var appointmentNoteRecords = await data.AppointmentNotes
            .Where(x => x.TenantId == TenantId && x.Deleted == true)
            .Select(x => new { x.DeletedAt, x.LastModified, x.LastModifiedBy, x.Note, x.AppointmentNoteId })
            .ToListAsync();
        if (appointmentNoteRecords != null && appointmentNoteRecords.Any()) {
            foreach(var item in appointmentNoteRecords) {
                appointmentNotes.Add(new DataObjects.DeletedRecordItem { 
                    DeletedAt = item.DeletedAt.HasValue ? (DateTime)item.DeletedAt : DateTime.Now,
                    DeletedBy = LastModifiedDisplayName(item.LastModifiedBy),
                    Display = StringValue(item.Note),
                    ItemId = item.AppointmentNoteId,
                });
            }
        }

        List<DataObjects.DeletedRecordItem> appointments = new List<DataObjects.DeletedRecordItem>();
        var appointmentRecords = await data.Appointments
            .Where(x => x.TenantId == TenantId && x.Deleted == true)
            .Select(x => new { x.Deleted, x.LastModified, x.LastModifiedBy, x.DeletedAt, x.Title, x.AppointmentId, x.Start, x.End, x.AllDay })
            .ToListAsync();
        if (appointmentRecords != null && appointmentRecords.Any()) {
            foreach(var item in appointmentRecords) {
                appointments.Add(new DataObjects.DeletedRecordItem { 
                    DeletedAt = item.DeletedAt.HasValue ? (DateTime)item.DeletedAt : DateTime.Now,
                    DeletedBy = LastModifiedDisplayName(item.LastModifiedBy),
                    Display = FormatAppointmentTitle(item.Title, item.Start, item.End, item.AllDay),
                    ItemId = item.AppointmentId,
                });
            }
        }

        List<DataObjects.DeletedRecordItem> appointmentServices = new List<DataObjects.DeletedRecordItem>();
        var appointmentServiceRecords = await data.AppointmentServices
            // {{ModuleItemStart:Services}}
            .Include(x => x.Service)
            // {{ModuleItemEnd:Services}}
            .Where(x => x.TenantId == TenantId && x.Deleted == true).ToListAsync();
        if (appointmentServiceRecords != null && appointmentServiceRecords.Any()) {
            foreach (var item in appointmentServiceRecords) {
                appointmentServices.Add(new DataObjects.DeletedRecordItem {
                    DeletedAt = item.DeletedAt.HasValue ? (DateTime)item.DeletedAt : DateTime.Now,
                    DeletedBy = LastModifiedDisplayName(item.LastModifiedBy),
                    // {{ModuleItemStart:Services}}
                    Display = StringValue(item.Service.Description),
                    // {{ModuleItemEnd:Services}}
                    ItemId = item.AppointmentServiceId,
                });
            }
        }
        // {{ModuleItemEnd:Appointments}}

        List<DataObjects.DeletedRecordItem> departmentGroups = new List<DataObjects.DeletedRecordItem>();
        var departmentGroupRecords = await data.DepartmentGroups
            .Where(x => x.TenantId == TenantId && x.Deleted == true)
            .Select(x => new { x.DeletedAt, x.LastModified, x.LastModifiedBy, x.DepartmentGroupName, x.DepartmentGroupId })
            .ToListAsync();
        if (departmentGroupRecords != null && departmentGroupRecords.Any()) {
            foreach(var item in departmentGroupRecords) {
                departmentGroups.Add(new DataObjects.DeletedRecordItem {
                    DeletedAt = item.DeletedAt.HasValue ? (DateTime)item.DeletedAt : DateTime.Now,
                    DeletedBy = LastModifiedDisplayName(item.LastModifiedBy),
                    Display = StringValue(item.DepartmentGroupName),
                    ItemId = item.DepartmentGroupId,
                });
            }
        }

        List<DataObjects.DeletedRecordItem> departments = new List<DataObjects.DeletedRecordItem>();
        var departmentRecords = await data.Departments
            .Where(x => x.TenantId == TenantId && x.Deleted == true)
            .Select(x => new { x.DeletedAt, x.LastModified, x.LastModifiedBy, x.DepartmentName, x.DepartmentId })
            .ToListAsync();
        if (departmentRecords != null && departmentRecords.Any()) {
            foreach(var item in departmentRecords) {
                departments.Add(new DataObjects.DeletedRecordItem {
                    DeletedAt = item.DeletedAt.HasValue ? (DateTime)item.DeletedAt : DateTime.Now,
                    DeletedBy = LastModifiedDisplayName(item.LastModifiedBy),
                    Display = StringValue(item.DepartmentName),
                    ItemId = item.DepartmentId,
                });
            }
        }

        // {{ModuleItemStart:EmailTemplates}}
        List<DataObjects.DeletedRecordItem> emailTemplates = new List<DataObjects.DeletedRecordItem>();
        var emailTemplateRecords = await data.EmailTemplates
            .Where(x => x.TenantId == TenantId && x.Deleted == true)
            .Select(x => new { x.DeletedAt, x.LastModified, x.LastModifiedBy, x.Name, x.EmailTemplateId })
            .ToListAsync();
        if(emailTemplateRecords != null && emailTemplateRecords.Any()) {
            foreach(var item in emailTemplateRecords) {
                emailTemplates.Add(new DataObjects.DeletedRecordItem {
                    DeletedAt = item.DeletedAt.HasValue ? (DateTime)item.DeletedAt : DateTime.Now,
                    DeletedBy = LastModifiedDisplayName(item.LastModifiedBy),
                    Display = item.Name,
                    ItemId = item.EmailTemplateId,
                });
            }
        }
        // {{ModuleItemEnd:EmailTemplates}}

        List<DataObjects.DeletedRecordItem> fileStorage = new List<DataObjects.DeletedRecordItem>();
        var fileStorageRecord = await data.FileStorages
            .Where(x => x.TenantId == TenantId && x.Deleted == true)
            .Select(x => new { x.DeletedAt, x.LastModified, x.LastModifiedBy, x.FileName, x.FileId })
            .ToListAsync();
        if (fileStorageRecord != null && fileStorageRecord.Any()) {
            foreach(var item in fileStorageRecord) {
                fileStorage.Add(new DataObjects.DeletedRecordItem {
                    DeletedAt = item.DeletedAt.HasValue ? (DateTime)item.DeletedAt : DateTime.Now,
                    DeletedBy = LastModifiedDisplayName(item.LastModifiedBy),
                    Display = StringValue(item.FileName),
                    ItemId = item.FileId,
                });
            }
        }

        // {{ModuleItemStart:Locations}}
        List<DataObjects.DeletedRecordItem> locations = new List<DataObjects.DeletedRecordItem>();
        var locationRecords = await data.Locations
            .Where(x => x.TenantId == TenantId && x.Deleted == true)
            .Select(x => new { x.DeletedAt, x.LastModified, x.LastModifiedBy, x.Name, x.LocationId })
            .ToListAsync();
        if (locationRecords != null && locationRecords.Any()) {
            foreach(var item in locationRecords) {
                locations.Add(new DataObjects.DeletedRecordItem {
                    DeletedAt = item.DeletedAt.HasValue ? (DateTime)item.DeletedAt : DateTime.Now,
                    DeletedBy = LastModifiedDisplayName(item.LastModifiedBy),
                    Display = StringValue(item.Name),
                    ItemId = item.LocationId,
                });
            }
        }
        // {{ModuleItemEnd:Locations}}

        // {{ModuleItemStart:Services}}
        List<DataObjects.DeletedRecordItem> services = new List<DataObjects.DeletedRecordItem>();
        var serviceRecords = await data.Services
            .Where(x => x.TenantId == TenantId && x.Deleted == true)
            .Select(x => new { x.DeletedAt, x.LastModified, x.LastModifiedBy, x.Description, x.ServiceId })
            .ToListAsync();
        if (serviceRecords != null && serviceRecords.Any()) {
            foreach(var item in serviceRecords) {
                services.Add(new DataObjects.DeletedRecordItem {
                    DeletedAt = item.DeletedAt.HasValue ? (DateTime)item.DeletedAt : DateTime.Now,
                    DeletedBy = LastModifiedDisplayName(item.LastModifiedBy),
                    Display = StringValue(item.Description),
                    ItemId = item.ServiceId,
                });
            }
        }
        // {{ModuleItemEnd:Services}}

        // {{ModuleItemStart:Tags}}
        List<DataObjects.DeletedRecordItem> tags = new List<DataObjects.DeletedRecordItem>();
        var tagRecords = await data.Tags
            .Where(x => x.TenantId == TenantId && x.Deleted == true)
            .Select(x => new { x.DeletedAt, x.LastModified, x.LastModifiedBy, x.Name, x.TagId})
            .ToListAsync();
        if(tagRecords != null && tagRecords.Any()) {
            foreach(var item in tagRecords) {
                tags.Add(new DataObjects.DeletedRecordItem {
                    DeletedAt = item.DeletedAt.HasValue ? (DateTime)item.DeletedAt : DateTime.Now,
                    DeletedBy = LastModifiedDisplayName(item.LastModifiedBy),
                    Display = item.Name,
                    ItemId = item.TagId,
                });
            }
        }
        // {{ModuleItemEnd:Tags}}

        List<DataObjects.DeletedRecordItem> userGroups = new List<DataObjects.DeletedRecordItem>();
        var userGroupRecords = await data.UserGroups
            .Where(x => x.TenantId == TenantId && x.Deleted == true)
            .Select(x => new { x.DeletedAt, x.LastModified, x.LastModifiedBy, x.Name, x.GroupId })
            .ToListAsync();
        if (userGroupRecords != null && userGroupRecords.Any()) {
            foreach(var item in userGroupRecords) {
                userGroups.Add(new DataObjects.DeletedRecordItem {
                    DeletedAt = item.DeletedAt.HasValue ? (DateTime)item.DeletedAt : DateTime.Now,
                    DeletedBy = LastModifiedDisplayName(item.LastModifiedBy),
                    Display = StringValue(item.Name),
                    ItemId = item.GroupId,
                });
            }
        }

        List<DataObjects.DeletedRecordItem> users = new List<DataObjects.DeletedRecordItem>();
        var userRecords = await data.Users
            .Where(x => x.TenantId == TenantId && x.Deleted == true)
            .Select(x => new { x.DeletedAt, x.LastModified, x.LastModifiedBy, x.FirstName, x.LastName, x.Email, x.UserId })
            .ToListAsync();
        if (userRecords != null && userRecords.Any()) {
            foreach(var item in userRecords) {
                users.Add(new DataObjects.DeletedRecordItem {
                    DeletedAt = item.DeletedAt.HasValue ? (DateTime)item.DeletedAt : DateTime.Now,
                    DeletedBy = LastModifiedDisplayName(item.LastModifiedBy),
                    Display = StringValue(item.FirstName + " " + item.LastName + (!String.IsNullOrWhiteSpace(item.Email) ? " (" + item.Email + ")": "")),
                    ItemId = item.UserId,
                });
            }
        }

        DataObjects.DeletedRecords output = new DataObjects.DeletedRecords {
            // {{ModuleItemStart:Appointments}}
            AppointmentNotes = appointmentNotes,
            Appointments = appointments,
            AppointmentServices = appointmentServices,
            // {{ModuleItemEnd:Appointments}}
            DepartmentGroups = departmentGroups,
            Departments = departments,
            // {{ModuleItemStart:EmailTemplates}}
            EmailTemplates = emailTemplates,
            // {{ModuleItemEnd:EmailTemplates}}
            FileStorage = fileStorage,
            // {{ModuleItemStart:Locations}}
            Locations = locations,
            // {{ModuleItemEnd:Locations}}
            // {{ModuleItemStart:Services}}
            Services = services,
            // {{ModuleItemEnd:Services}}
            // {{ModuleItemStart:Tags}}
            Tags = tags,
            // {{ModuleItemEnd:Tags}}
            UserGroups = userGroups,
            Users = users,
        };

        output = await GetDeletedRecordsApp(TenantId, output);

        return output;
    }

    public string GetFullUrl()
    {
        string output = "";

        if (_httpContext != null) {
            try {
                output = string.Concat(
                    _httpContext.Request.Scheme,
                    "://",
                    _httpContext.Request.Host.ToUriComponent(),
                    _httpContext.Request.PathBase.ToUriComponent(),
                    _httpContext.Request.Path.ToUriComponent(),
                    _httpContext.Request.QueryString.ToUriComponent()
                );
            } catch { }
        } else if (_httpRequest != null) {
            try {
                output = string.Concat(
                    _httpRequest.Scheme,
                    "://",
                    _httpRequest.Host.ToUriComponent(),
                    _httpRequest.PathBase.ToUriComponent(),
                    _httpRequest.Path.ToUriComponent(),
                    _httpRequest.QueryString.ToUriComponent()
                );
            } catch { }
        }

        return output;
    }

    public string GetFullUrlWithoutQuerystring()
    {
        string output = "";

        if (_httpContext != null) {
            try {
                output = string.Concat(
                    _httpContext.Request.Scheme,
                    "://",
                    _httpContext.Request.Host.ToUriComponent(),
                    _httpContext.Request.PathBase.ToUriComponent(),
                    _httpContext.Request.Path.ToUriComponent()
                );
            } catch { }
        } else if(_httpRequest != null) {
            output = string.Concat(
                _httpRequest.Scheme,
                "://",
                _httpRequest.Host.ToUriComponent(),
                _httpRequest.PathBase.ToUriComponent(),
                _httpRequest.Path.ToUriComponent()
            );
        }

        return output;
    }

    public DataObjects.BooleanResponse GetNewActionResponse(bool result = false, string? message = null)
    {
        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse {
            Result = result,
            Messages = new List<string>()
        };
        if (!String.IsNullOrEmpty(message)) {
            output.Messages.Add(message);
        }
        return output;
    }

    public Guid GuidFromNumber(int number)
    {
        return GuidFromNumber((double)number);
    }

    public Guid GuidFromNumber(long number)
    {
        return GuidFromNumber((double)number);
    }

    public Guid GuidFromNumber(double number)
    {
        Guid output = Guid.Empty;

        string guid = number.ToString().Replace(".", "").Replace("-", "").Replace("+", "").PadLeft(32, '0');
        try {
            output = new Guid(FormatStringAsGuid(guid));
        } catch { }

        return output;
    }

    public Guid GuidValue(Guid? guid)
    {
        return guid.HasValue ? (Guid)guid : Guid.Empty;
    }

    public string HashPassword(string? password)
    {
        string output = String.Empty;

        if (!String.IsNullOrWhiteSpace(password)) {
            output = new Microsoft.AspNetCore.Identity.PasswordHasher<string>().HashPassword("", password);
        }
        
        return output;
    }

    public bool HashPasswordValidate(string? password, string? hashedPassword)
    {
        bool output = false;

        if (!String.IsNullOrWhiteSpace(password) && !String.IsNullOrWhiteSpace(hashedPassword)) {
            try {
                var validated = new Microsoft.AspNetCore.Identity.PasswordHasher<string>().VerifyHashedPassword("", hashedPassword, password);
                switch (validated) {
                    case Microsoft.AspNetCore.Identity.PasswordVerificationResult.Success:
                    case Microsoft.AspNetCore.Identity.PasswordVerificationResult.SuccessRehashNeeded:
                        output = true;
                        break;
                }
            } catch { }
        }

        return output;
    }

    public string HtmlToPlainText(string html)
    {
        string output = Regex.Replace(html, @"<(.|\n)*?>", "");
        output = System.Web.HttpUtility.HtmlDecode(output);
        return output;
    }

    public int IntValue(int? value)
    {
        int output = value.HasValue ? (int)value : 0;
        return output;
    }

    private DataObjects.MailServerConfig MailServerConfig {
        get {
            DataObjects.MailServerConfig output = new DataObjects.MailServerConfig();
            if (CacheStore.ContainsKey(Guid.Empty, "MailServerConfig")) {
                var cachedItem = CacheStore.GetCachedItem<DataObjects.MailServerConfig>(Guid.Empty, "MailServerConfig");
                if(cachedItem != null) {
                    output = cachedItem;
                }
            } else {
                var savedItem = GetSetting<DataObjects.MailServerConfig>("MailServerConfig", DataObjects.SettingType.EncryptedObject);
                if(savedItem != null) {
                    output = savedItem;
                    CacheStore.SetCacheItem(Guid.Empty, "MailServerConfig", output);
                }
            }
            return output;
        }
    }

    private bool MaintenanceMode
    {
        get {
            bool output = false;

            if (CacheStore.ContainsKey(Guid.Empty, "MaintenanceMode")) {
                output = CacheStore.GetCachedItem<bool>(Guid.Empty, "MaintenanceMode");
            } else {
                output = GetSetting<bool>("MaintenanceMode", DataObjects.SettingType.Boolean);
                CacheStore.SetCacheItem(Guid.Empty, "MaintenanceMode", output);
            }

            return output;
        }
    }

    private string MaxStringLength(string? value, int maxLength)
    {
        string output = String.Empty;

        if (!String.IsNullOrWhiteSpace(value)) {
            output = value;
            if (output.Length > maxLength) {
                output = output.Substring(0, maxLength);
            }
        }

        return output;
    }

    public double NowFromUnixEpoch()
    {
        return (double)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string NumbersOnly(string? input)
    {
        string output = String.Empty;

        if (!String.IsNullOrWhiteSpace(input)) {
            foreach (char c in input) {
                switch (c) {
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        output += c.ToString();
                        break;
                }
            }
        }

        return output;
    }

    public List<string> MessageToListOfString(string message)
    {
        return new List<string> { message };
    }

    private string ObjectToCSV(object[] o)
    {
        string output = String.Empty;

        if (o != null && o.Any()) {
            using (var writer = new StringWriter()) {
                using (var csv = new CsvHelper.CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture)) {
                    csv.WriteRecords(o);
                    output = writer.ToString();
                }
            }
        }

        return output;
    }

    private string OptionPairValue(List<DataObjects.OptionPair>? Options, string Id)
    {
        string output = String.Empty;
        if (Options != null && Options.Count() > 0) {
            var opt = Options.FirstOrDefault(x => x.Id != null && x.Id.ToLower() == Id.ToLower());
            if (opt != null) {
                output += opt.Value;
            }
        }
        return output;
    }

    public string QueryStringValue(string valueName)
    {
        string output = String.Empty;

        if (_httpContext != null) {
            try {
                output += _httpContext.Request.Query[valueName].ToString();
            } catch { }
        } else if (_httpRequest != null) {
            try {
                output += _httpRequest.Query[valueName].ToString();
            } catch { }
        }

        return output;
    }

    public List<string> RecurseException(Exception ex, bool ShowExceptionType = true)
    {
        List<string> output = new List<string>();

        if (ex != null) {
            if (!String.IsNullOrWhiteSpace(ex.Message)) {
                if (ShowExceptionType) {
                    output.Add(ex.GetType().ToString() + ": " + ex.Message);
                } else {
                    output.Add(ex.Message);
                }
            }

            if (ex.InnerException != null) {
                var inner = RecurseException(ex.InnerException, ShowExceptionType);
                if (inner.Any()) {
                    foreach (var message in inner) {
                        output.Add(message);
                    }
                }
            }
        }

        return output;
    }

    public string RecurseExceptionAsString(Exception ex, bool ShowExceptionType = true)
    {
        string output = String.Empty;

        var exceptions = RecurseException(ex, ShowExceptionType);

        if (exceptions.Any()) {
            output = String.Join(" | ", exceptions);
        }

        return output;
    }

    public void Redirect(string url)
    {
        if (_httpContext != null) {
            _httpContext.Response.Redirect(url);
        } else if (_httpResponse != null) {
            _httpResponse.Redirect(url);
        }
    }

    public DateOnly Released {
        get {
            return _released;
        }
    }

    public object RemoveSensitiveData(object o)
    {
        var type = o.GetType();
        var properties = type.GetProperties();

        foreach (var property in properties) {
            var propertyType = property.PropertyType;
            var thisObject = property.GetValue(o);

            if (property.IsDefined(typeof(SensitiveAttribute))) {
                object? defaultValue = null;
                try {
                    if (propertyType == typeof(System.String)) {
                        defaultValue = String.Empty;
                    } else {
                        defaultValue = Activator.CreateInstance(propertyType);
                    }
                } catch { }

                // For specific item types return a value instead of null.
                // In my testing so far it seems like only string is a problem.
                if (thisObject != null) {
                    if (thisObject.GetType() == typeof(System.String)) {
                        defaultValue = "";
                    }
                }

                property.SetValue(o, defaultValue);
            }

            if (!propertyType.ToString().ToLower().StartsWith("system.")) {
                // This might be an object.
                if (thisObject != null) {
                    thisObject = RemoveSensitiveData(thisObject);
                }
            }
        }

        return o;
    }

    public string Replace(string input, string replaceText, string withText)
    {
        string output = input;

        if (!String.IsNullOrWhiteSpace(output) && !String.IsNullOrWhiteSpace(replaceText)) {
            if (String.IsNullOrWhiteSpace(withText)) {
                withText = "";
            }

            output = Regex.Replace(
                input,
                Regex.Escape(replaceText),
                withText.Replace("$", "$$"),
                RegexOptions.IgnoreCase
            );
        }

        return output;
    }

    private DataObjects.EmailMessage ReplaceTagsInEmail(DataObjects.EmailMessage message, DataObjects.User? user = null, object? obj = null)
    {
        var output = message;

        output.Subject = ReplaceTagsInText(output.Subject, user, obj);
        output.Body = ReplaceTagsInText(output.Body, user, obj);

        return output;
    }

    private string ReplaceTagsInText(string? input, DataObjects.User? user = null, object? obj = null)
    {
        string output = String.Empty;

        if (!String.IsNullOrWhiteSpace(input)) {
            output = input;

            if (user != null) {
                output = output.Replace("{{FirstName}}", user.FirstName, StringComparison.InvariantCultureIgnoreCase);
                output = output.Replace("{{LastName}}", user.LastName, StringComparison.InvariantCultureIgnoreCase);
                output = output.Replace("{{Email}}", user.Email, StringComparison.InvariantCultureIgnoreCase);
            }

            if (obj != null) {
                // {{ModuleItemStart:Appointments}}
                if (obj.GetType() == typeof(DataObjects.Appointment)) {
                    var appt = (DataObjects.Appointment)obj;

                    output = output.Replace("{{Appointment:Title}}", appt.Title, StringComparison.InvariantCultureIgnoreCase);
                    output = output.Replace("{{Appointment:Note}}", appt.Note, StringComparison.InvariantCultureIgnoreCase);

                    string startDate = appt.Start.ToLocalTime().ToString();
                    string endDate = appt.End.ToLocalTime().ToString();

                    string datesAndTimes = "Format Dates and Times Here";

                    output = output.Replace("{{Appointment:Start}}", startDate, StringComparison.InvariantCultureIgnoreCase);
                    output = output.Replace("{{Appointment:End}}", endDate, StringComparison.InvariantCultureIgnoreCase);
                    output = output.Replace("{{Appointment:DatesAndTimes}}", datesAndTimes, StringComparison.InvariantCultureIgnoreCase);
                }
                // {{ModuleItemEnd:Appointments}}

                // {{ModuleItemStart:Services}}
                if (obj.GetType() == typeof(DataObjects.Service)) {
                    var service = (DataObjects.Service)obj;

                    output = output.Replace("{{Service:Code}}", service.Code, StringComparison.InvariantCultureIgnoreCase);
                    output = output.Replace("{{Service:Description}}", service.Description, StringComparison.InvariantCultureIgnoreCase);
                    output = output.Replace("{{Service:Rate}}", service.Rate.ToString("C"), StringComparison.InvariantCultureIgnoreCase);
                }
                // {{ModuleItemEnd:Services}}
            }

            // Now, replace any potential empty tags that weren't caught above.
            List<string> tags = new List<string> { "{{FirstName}}", "{{LastName}}", "{{Email}}",
            "{{Appointment:Title}}", "{{Appointment:Note}}", "{{Appointment:Start}}", "{{Appointment:End}}", "{{Appointment:DatesAndTimes}}",
            "{{Service:Code}}", "{{Service:Description}}", "{{Service:Rate}}"};

            foreach (var tag in tags) {
                output = output.Replace(tag, "", StringComparison.InvariantCultureIgnoreCase);
            }
        }

        return output;
    }

    public string Request(string parameter)
    {
        string output = "";

        if (_httpContext != null) {
            // First, try the querystring.
            output = QueryStringValue(parameter);

            if (String.IsNullOrWhiteSpace(output)) {
                // Check form fields
                try {
                    output += _httpContext.Request.Form[parameter].ToString();
                } catch { }
            }
        }else if (_httpRequest != null) {
            output = QueryStringValue(parameter);

            if (String.IsNullOrWhiteSpace(output)) {
                try {
                    output += _httpRequest.Form[parameter].ToString();
                } catch { }
            }
        }

        return output;
    }

    public double RunningSince {
        get {
            return GlobalSettings.RunningSince;
        }
    }

    private DataObjects.BooleanResponse SaveFile(string FullPath, string Contents)
    {
        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();

        bool exists = System.IO.File.Exists(FullPath);
        try {
            System.IO.File.WriteAllText(FullPath, Contents);
            output.Result = true;

            if (exists) {
                output.Messages.Add("File Updated");
            } else {
                output.Messages.Add("File Created");
            }
        } catch (Exception ex) {
            output.Messages.Add("Error Saving File " + FullPath + ":");
            output.Messages.AddRange(RecurseException(ex));
        }

        return output;
    }

    public DataObjects.BooleanResponse SendEmail(DataObjects.EmailMessage message, DataObjects.MailServerConfig? config = null)
    {
        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();

        if(config == null) {
            config = MailServerConfig;
        }

        if (String.IsNullOrWhiteSpace(message.From)) {
            message.From = DefaultReplyToAddress;
        }

        switch (StringValue(config.Type).ToUpper()) {
            case "GRAPH":
                var graphConfig = DeserializeObject<DataObjects.MailServerConfigMicrosoftGraph>(config.Config);
                if(graphConfig != null) {
                    if(!String.IsNullOrWhiteSpace(graphConfig.ClientId) && !String.IsNullOrWhiteSpace(graphConfig.TenantId) && !String.IsNullOrWhiteSpace(graphConfig.ClientSecret)) {
                        var graph = new GraphClient(graphConfig.ClientId, graphConfig.TenantId, graphConfig.ClientSecret);

                        output = graph.SendEmail(message).Result;
                    } else {
                        output.Messages.Add("Incomplete Graph API Configuration");
                        if (String.IsNullOrWhiteSpace(graphConfig.ClientId)) {
                            output.Messages.Add("Missing Graph ClientId");
                        }
                        if (String.IsNullOrWhiteSpace(graphConfig.TenantId)) {
                            output.Messages.Add("Missing Graph TenantId");
                        }
                        if (String.IsNullOrWhiteSpace(graphConfig.ClientSecret)) {
                            output.Messages.Add("Missing Graph Client Secret");
                        }
                    }
                } else {
                    output.Messages.Add("Graph API Mail Configuration Not Set");
                }
                break;

            case "SMTP":
                var smtpConfig = DeserializeObject<DataObjects.MailServerConfigSMTP>(config.Config);
                if(smtpConfig != null) {
                    output = SendEmailViaSMTP(message, smtpConfig);
                } else {
                    output.Messages.Add("SMTP Mail Configuration Not Set");
                }
                break;

            default:
                output.Messages.Add("No Mail Configuration Set");
                break;
        }

        return output;
    }

    public DataObjects.BooleanResponse SendEmailViaSMTP(DataObjects.EmailMessage message, DataObjects.MailServerConfigSMTP config)
    {
        //if (!String.IsNullOrWhiteSpace(config.Username)) {
        //    config.Username = Decrypt(config.Username);
        //}

        //if (!String.IsNullOrWhiteSpace(config.Password)) {
        //    config.Password = Decrypt(config.Password);
        //}

        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();

        if (String.IsNullOrWhiteSpace(message.From)) {
            message.From = DefaultReplyToAddress;
        }

        if (String.IsNullOrWhiteSpace(message.From)) {
            output.Messages.Add("Sending an email requires a valid From address.");
        }
        if (String.IsNullOrWhiteSpace(message.Subject)) {
            output.Messages.Add("Sending an email requires a Subject.");
        }
        if (String.IsNullOrWhiteSpace(message.Body)) {
            output.Messages.Add("Sending an email requires a Body.");
        }
        int Recipients = 0;
        if (message.To != null) {
            Recipients += message.To.Count();
        }
        if (message.Cc != null) {
            Recipients += message.Cc.Count();
        }
        if (message.Bcc != null) {
            Recipients += message.Bcc.Count();
        }
        if (Recipients == 0) {
            output.Messages.Add("Sending an email requires at least one recipient.");
        }
        if (output.Messages.Count() > 0) {
            return output;
        }

        MailMessage m = new MailMessage();

        if (!String.IsNullOrEmpty(message.From)) {
            m.From = String.IsNullOrWhiteSpace(message.FromDisplayName)
                ? new MailAddress(message.From)
                : new MailAddress(message.From, message.FromDisplayName);
        }

        if (!String.IsNullOrWhiteSpace(message.ReplyTo)) {
            m.ReplyToList.Add(message.ReplyTo);
        }

        m.SubjectEncoding = System.Text.Encoding.UTF8;
        m.Subject = message.Subject;
        m.IsBodyHtml = true;
        m.Body = message.Body;

        if (message.To != null && message.To.Count() > 0) {
            foreach (var To in message.To) {
                if (!String.IsNullOrWhiteSpace(To)) {
                    if (To.Contains("@")) {
                        m.To.Add(To);
                    }
                }
            }
        }

        if (message.Cc != null && message.Cc.Count() > 0) {
            foreach (var Cc in message.Cc) {
                if (!String.IsNullOrWhiteSpace(Cc)) {
                    if (Cc.Contains("@")) {
                        m.CC.Add(Cc);
                    }
                }
            }
        }

        if (message.Bcc != null && message.Bcc.Count() > 0) {
            foreach (var Bcc in message.Bcc) {
                if (!String.IsNullOrWhiteSpace(Bcc)) {
                    if (Bcc.Contains("@")) {
                        m.Bcc.Add(Bcc);
                    }
                }
            }
        }

        if (message.Files != null && message.Files.Count() > 0) {
            foreach (var file in message.Files) {
                if (file.Value != null) {
                    System.Net.Mail.Attachment att = new System.Net.Mail.Attachment(new MemoryStream(file.Value), file.FileName);
                    m.Attachments.Add(att);
                }
            }
        }

        SmtpClient? s = null;

        try {
            if (String.IsNullOrWhiteSpace(config.Server)) {
                s = new SmtpClient();
            } else {
                s = new SmtpClient(config.Server, config.Port);
                if (config.UseSSL) {
                    s.EnableSsl = true;
                }
            }

            if (!String.IsNullOrWhiteSpace(config.Username) && !String.IsNullOrWhiteSpace(config.Password)) {
                s.UseDefaultCredentials = false;
                s.Credentials = new System.Net.NetworkCredential(config.Username, config.Password);
            }
        } catch (Exception ex) {
            output.Messages.Add("Error Creating SmtpClient:");
            output.Messages.AddRange(RecurseException(ex));
            return output;
        }

        if (s != null) {
            int sendAttempts = 25;
#if DEBUG
            //sendAttempts = 0;
            //output.Result = true;
#endif
            if (sendAttempts > 0) {
                // Sometimes a sendmail fails, so give it multiple attempts to send
                string SendError = String.Empty;
                for (var x = 0; x < sendAttempts; x++) {
                    if (output.Result == false) {
                        try {
                            s.Send(m);
                            output.Result = true;
                        } catch (Exception ex) {
                            SendError = "Error Sending Email - " + RecurseExceptionAsString(ex);
                        }
                    }
                }

                if (output.Result == false) {
                    output.Messages.Add(SendError);
                }
            }
        } else {
            output.Messages.Add("Unable to Connect to Mail Server");
        }
        return output;
    }

    public string Serialize_ObjectToXml(object o, bool OmitXmlDeclaration = true)
    {
        XmlSerializer serializer = new XmlSerializer(o.GetType());

        XmlWriterSettings settings = new XmlWriterSettings();
        //settings.Encoding = new UnicodeEncoding(false, false); // no BOM in a .NET string
        settings.Indent = true;
        settings.OmitXmlDeclaration = OmitXmlDeclaration;

        string strSerializedXML = String.Empty;

        using (StringWriter textWriter = new StringWriter()) {
            using (XmlWriter xmlWriter = XmlWriter.Create(textWriter, settings)) {
                serializer.Serialize(xmlWriter, o);
            }
            return textWriter.ToString();
        }
    }

    public T? Serialize_XmlToObject<T>(string? xml)
    {
        var output = default(T);

        if (!string.IsNullOrEmpty(xml)) {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            XmlReaderSettings settings = new XmlReaderSettings();

            using (StringReader textReader = new StringReader(xml)) {
                using (XmlReader xmlReader = XmlReader.Create(textReader, settings)) {
                    var deserialized = serializer.Deserialize(xmlReader);
                    if (deserialized != null) {
                        output = (T)deserialized;
                    }
                }
            }
        }

        return output;
    }

    public string SerializeObject(object? Object)
    {
        string output = String.Empty;

        if (Object != null) {
            output += System.Text.Json.JsonSerializer.Serialize(Object);
        }

        return output;
    }

    public void SetAuthenticationProviders(DataObjects.AuthenticationProviders? authenticationProviders)
    {
        if(authenticationProviders != null) {
            _authenticationProviders = authenticationProviders;
        }
    }

    public void SetHttpContext(Microsoft.AspNetCore.Http.HttpContext? context)
    {
        if(context != null) {
            _httpContext = context;
        }
    }

    public void SetHttpRequest(HttpRequest? request)
    {
        if(request != null) {
            _httpRequest = request;
        }
    }

    public void SetHttpResponse(HttpResponse? response)
    {
        if(response != null) {
            _httpResponse = response;
        }
    }

    public bool ShowTenantListingWhenMissingTenantCode
    {
        get {
            var output = false;

            if (CacheStore.ContainsKey(Guid.Empty, "ShowTenantListingWhenMissingTenantCode")) {
                output = CacheStore.GetCachedItem<bool>(Guid.Empty, "ShowTenantListingWhenMissingTenantCode");
            } else {
                output = GetSetting<bool>("ShowTenantListingWhenMissingTenantCode", DataObjects.SettingType.Boolean);
                CacheStore.SetCacheItem(Guid.Empty, "ShowTenantListingWhenMissingTenantCode", output);
            }

            return output;
        }
    }

    public Guid StringToGuid(string? input)
    {
        Guid output = Guid.Empty;

        if(!String.IsNullOrWhiteSpace(input)) {
            try {
                Guid g = new Guid(input);
                output = g;
            } catch { }
        }

        return output;
    }

    public string StringValue(string? input)
    {
        return !String.IsNullOrEmpty(input) ? input : String.Empty;
    }

    public async Task<DataObjects.BooleanResponse> UndeleteRecord(string? Type, Guid RecordId, DataObjects.User CurrentUser)
    {
        var output = new DataObjects.BooleanResponse();

        try {
            if (!String.IsNullOrWhiteSpace(Type)) {
                object? obj = null;
                bool sendSignalRUpdate = false;

                switch (Type.ToLower()) {
                    // {{ModuleItemStart:Appointments}}
                    case "appointment":
                        var recAppt = await data.Appointments.FirstOrDefaultAsync(x => x.AppointmentId == RecordId);
                        if (recAppt != null) {
                            recAppt.Deleted = false;
                            recAppt.DeletedAt = null;
                            await data.SaveChangesAsync();
                            output.Result = true;
                            sendSignalRUpdate = true;

                            obj = await GetAppointment(RecordId, CurrentUser);
                        } else {
                            output.Messages.Add(Type + " Record '" + RecordId.ToString() + "' Not Found");
                        }
                        break;

                    case "appointmentnote":
                        var recApptNote = await data.AppointmentNotes.FirstOrDefaultAsync(x => x.AppointmentNoteId == RecordId);
                        if (recApptNote != null) {
                            recApptNote.Deleted = false;
                            recApptNote.DeletedAt = null;
                            await data.SaveChangesAsync();
                            output.Result = true;
                            sendSignalRUpdate = true;

                            obj = await GetAppointmentNote(RecordId);
                        } else {
                            output.Messages.Add(Type + " Record '" + RecordId.ToString() + "' Not Found");
                        }
                        break;

                    case "appointmentservice":
                        var recApptService = await data.AppointmentServices.FirstOrDefaultAsync(x => x.AppointmentServiceId == RecordId);
                        if (recApptService != null) {
                            recApptService.Deleted = false;
                            recApptService.DeletedAt = null;
                            await data.SaveChangesAsync();
                            output.Result = true;
                            sendSignalRUpdate = true;
                        } else {
                            output.Messages.Add(Type + " Record '" + RecordId.ToString() + "' Not Found");
                        }
                        break;
                    // {{ModuleItemEnd:Appointments}}

                    case "departmentgroup":
                        var recDeptGroup = await data.DepartmentGroups.FirstOrDefaultAsync(x => x.DepartmentGroupId == RecordId);
                        if (recDeptGroup != null) {
                            recDeptGroup.Deleted = false;
                            recDeptGroup.DeletedAt = null;
                            await data.SaveChangesAsync();
                            output.Result = true;
                            sendSignalRUpdate = true;

                            obj = await GetDepartmentGroup(RecordId, CurrentUser);
                        } else {
                            output.Messages.Add(Type + " Record '" + RecordId.ToString() + "' Not Found");
                        }
                        break;

                    case "department":
                        var recDept = await data.Departments.FirstOrDefaultAsync(x => x.DepartmentId == RecordId);
                        if (recDept != null) {
                            recDept.Deleted = false;
                            recDept.DeletedAt = null;
                            await data.SaveChangesAsync();
                            output.Result = true;
                            sendSignalRUpdate = true;

                            obj = await GetDepartment(RecordId, CurrentUser);
                        } else {
                            output.Messages.Add(Type + " Record '" + RecordId.ToString() + "' Not Found");
                        }
                        break;

                    // {{ModuleItemStart:EmailTemplates}}
                    case "emailtemplate":
                        var recEmailTemplate = await data.EmailTemplates.FirstOrDefaultAsync(x => x.EmailTemplateId == RecordId);
                        if(recEmailTemplate != null) {
                            recEmailTemplate.Deleted = false;
                            recEmailTemplate.DeletedAt = null;
                            await data.SaveChangesAsync();
                            output.Result = true;
                            sendSignalRUpdate = true;

                            obj = await GetEmailTemplate(RecordId, CurrentUser);
                        } else {
                            output.Messages.Add(Type + " Record '" + RecordId.ToString() + "' Not Found");
                        }
                        break;
                    // {{ModuleItemEnd:EmailTemplates}}

                    case "filestorage":
                        var recFile = await data.FileStorages.FirstOrDefaultAsync(x => x.FileId == RecordId);
                        if (recFile != null) {
                            recFile.Deleted = false;
                            recFile.DeletedAt = null;
                            await data.SaveChangesAsync();
                            output.Result = true;
                            sendSignalRUpdate = true;

                            obj = await GetFileStorage(RecordId, CurrentUser);
                        } else {
                            output.Messages.Add(Type + " Record '" + RecordId.ToString() + "' Not Found");
                        }
                        break;

                    // {{ModuleItemStart:Locations}}
                    case "location":
                        var recLocation = await data.Locations.FirstOrDefaultAsync(x => x.LocationId == RecordId);
                        if (recLocation != null) {
                            recLocation.Deleted = false;
                            recLocation.DeletedAt = null;
                            await data.SaveChangesAsync();
                            output.Result = true;
                            sendSignalRUpdate = true;

                            obj = await GetLocation(RecordId, CurrentUser);
                        } else {
                            output.Messages.Add(Type + " Record '" + RecordId.ToString() + "' Not Found");
                        }
                        break;
                    // {{ModuleItemEnd:Locations}}

                    // {{ModuleItemStart:Services}}
                    case "service":
                        var recService = await data.Services.FirstOrDefaultAsync(x => x.ServiceId == RecordId);
                        if (recService != null) {
                            recService.Deleted = false;
                            recService.DeletedAt = null;
                            await data.SaveChangesAsync();
                            output.Result = true;
                            sendSignalRUpdate = true;

                            obj = await GetService(RecordId, CurrentUser);
                        } else {
                            output.Messages.Add(Type + " Record '" + RecordId.ToString() + "' Not Found");
                        }
                        break;
                    // {{ModuleItemEnd:Services}}

                    // {{ModuleItemStart:Tags}}
                    case "tag":
                        var recTag = await data.Tags.FirstOrDefaultAsync(x => x.TagId == RecordId);
                        if (recTag != null) {
                            recTag.Deleted = false;
                            recTag.DeletedAt = null;
                            await data.SaveChangesAsync();
                            output.Result = true;
                            sendSignalRUpdate = true;

                            obj = await GetTag(RecordId, CurrentUser);
                        } else {
                            output.Messages.Add(Type + " Record '" + RecordId.ToString() + "' Not Found");
                        }
                        break;
                    // {{ModuleItemEnd:Tags}}

                    case "usergroup":
                        var recUserGroup = await data.UserGroups.FirstOrDefaultAsync(x => x.GroupId == RecordId);
                        if (recUserGroup != null) {
                            recUserGroup.Deleted = false;
                            recUserGroup.DeletedAt = null;
                            await data.SaveChangesAsync();
                            output.Result = true;
                            sendSignalRUpdate = true;

                            obj = await GetUserGroup(RecordId, true, CurrentUser);
                        } else {
                            output.Messages.Add(Type + " Record '" + RecordId.ToString() + "' Not Found");
                        }
                        break;

                    case "user":
                        var recUser = await data.Users.FirstOrDefaultAsync(x => x.UserId == RecordId);
                        if (recUser != null) {
                            recUser.Deleted = false;
                            recUser.DeletedAt = null;
                            await data.SaveChangesAsync();
                            output.Result = true;
                            sendSignalRUpdate = true;

                            obj = await GetUser(RecordId, false, CurrentUser);
                        } else {
                            output.Messages.Add(Type + " Record '" + RecordId.ToString() + "' Not Found");
                        }
                        break;

                    default:
                        output = await UndeleteRecordApp(Type, RecordId, CurrentUser);
                        break;
                }

                if (sendSignalRUpdate) {
                    await SignalRUpdate(new DataObjects.SignalRUpdate {
                        TenantId = CurrentUser.TenantId,
                        ItemId = RecordId,
                        UpdateType = DataObjects.SignalRUpdateType.Undelete,
                        Message = Type,
                        Object = obj,
                        UserId = CurrentUserId(CurrentUser),
                    });
                }
            } else {
                output.Messages.Add("Missing Required Data Type");
            }
        } catch (Exception ex) {
            output.Messages.Add("Error Undeleting '" + Type + "' " + RecordId.ToString() + " - " + RecurseExceptionAsString(ex));
        }

        return output;
    }

    public string UniqueId
    {
        get {
            return _uniqueId;
        }
    }

    public void UpdateApplicationURL(string? url)
    {
        // Only set the application URL if it is not already set.
        if (!String.IsNullOrWhiteSpace(url) && String.IsNullOrWhiteSpace(ApplicationURL)) {
            SaveSetting("ApplicationURL", DataObjects.SettingType.Text, url);
            CacheStore.SetCacheItem(Guid.Empty, "ApplicationURL", url);
        }

        //if (!String.IsNullOrWhiteSpace(url) && url.ToLower() != ApplicationURL.ToLower() && url.ToLower().StartsWith("https://")) {
        //    SaveSetting("ApplicationURL", DataObjects.SettingType.Text, url);
        //    CacheStore.SetCacheItem(Guid.Empty, "ApplicationURL", url);
        //}
    }

    private DataObjects.EmailMessage UpdateEmailReplyAddress(Guid TenantId, DataObjects.EmailMessage message)
    {
        var output = message;

        var config = MailServerConfig;

        string defaultReplyToAddress = DefaultReplyToAddress;

        if (String.IsNullOrWhiteSpace(message.From)) {
            message.From = DefaultReplyToAddressForTenant(TenantId);
        }

        if(message.From.ToLower() != defaultReplyToAddress.ToLower()) {
            // If the mail server does not support allowing other reply address, then just use the default from the config.
            if (config.AllowSendingFromIndividualEmailAddresses) {
                // The server allows sending from other addresses, so this is OK.
            } else {
                // Set the reply to address instead and change the from address to the default. If the message had already
                // specified both a from and reply-to address then use the reply-to address.
                message.ReplyTo = !String.IsNullOrWhiteSpace(message.ReplyTo) ? message.ReplyTo : message.From;
                message.From = defaultReplyToAddress;
            }
        } else {
            // This is being sent from the default reply to address, so no checks are needed.
        }

        return output;
    }

    public string UrlDecode(string? input)
    {
        string output = String.Empty;

        if (!String.IsNullOrEmpty(input)) {
            output = System.Net.WebUtility.UrlDecode(input);
        }

        return output;
    }

    public string UrlEncode(string? input)
    {
        string output = String.Empty;

        if (!String.IsNullOrEmpty(input)) {
            output = System.Net.WebUtility.UrlEncode(input);
        }

        return output;
    }

    public bool UseBackgroundService {
        get {
            return _useBackgroundService;
        }
    }

    public bool UseTenantCodeInUrl
    {
        get {
            var output = false;

            if (CacheStore.ContainsKey(Guid.Empty, "UseTenantCodeInUrl")) {
                output = CacheStore.GetCachedItem<bool>(Guid.Empty, "UseTenantCodeInUrl");
            } else {
                output = GetSetting<bool>("UseTenantCodeInUrl", DataObjects.SettingType.Boolean);
                CacheStore.SetCacheItem(Guid.Empty, "UseTenantCodeInUrl", output);
            }

            return output;
        }
    }

    public string Version {
        get {
            return _version;
        }
    }

    public DataObjects.VersionInfo VersionInfo {
        get {
            return new DataObjects.VersionInfo {
                Released = _released,
                RunningSince = RunningSince,
                Version = _version
            };
        }
    }

    private string WebsiteName(string? url)
    {
        string output = WebsiteRoot(url);

        if (output.ToLower().StartsWith("https://")) {
            output = output.Substring(8);
        } else if (output.ToLower().StartsWith("http://")) {
            output = output.Substring(7);
        }

        if (output.EndsWith("/")) {
            output = output.Substring(0, output.Length - 1);
        }


        return output;
    }

    private string WebsiteRoot(string? url)
    {
        string output = String.Empty;

        if (!String.IsNullOrWhiteSpace(url)) {
            int start = url.IndexOf("://");
            if (start > -1) {
                int end = url.IndexOf("/", start + 3);
                if (end > -1) {
                    output = url.Substring(0, end + 1);
                }
            }
        }

        return output;
    }
}
