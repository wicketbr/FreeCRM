using BlazorBootstrap;
using MudBlazor;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Markup;

namespace CRM.Client;

#region Enumerations and Data Objects used by the CRM
public enum MessageType
{
    Primary,
    Secondary,
    Success,
    Danger,
    Warning,
    Info,
    Light,
    Dark,
}

public class Message
{
    public string Id { get; set; } = "";
    public bool AutoHide { get; set; }
    public DateTime Shown { get; set; } = DateTime.UtcNow;
    public string Text { get; set; } = "";
    public string TimeLabel { get; set; } = "";
    public MessageType MessageType { get; set; } = MessageType.Dark;
    public bool ReplaceLineBreaks { get; set; }
}

public class NewMessage
{
    public string Text { get; set; } = "";
    public MessageType MessageType { get; set; } = MessageType.Dark;
}

public enum TextCase
{
    Normal,
    Lowercase,
    Uppercase,
    Sentence,
    Title,
}
#endregion

/// <summary>
/// The Model used on every page in the Blazor application to share database in the interface.
/// </summary>
public partial class BlazorDataModel
{
    private List<DataObjects.ActiveUser> _ActiveUsers = new List<DataObjects.ActiveUser>();
    private DataObjects.CustomLoginProvider _AdminCustomLoginProvider = new DataObjects.CustomLoginProvider();
    private List<DataObjects.Tenant> _AllTenants = new List<DataObjects.Tenant>();
    private bool _AppOnline = true;
    private DataObjects.ApplicationSettingsUpdate _AppSettings = new DataObjects.ApplicationSettingsUpdate();
    private string _ApplicationUrl = "";
    private DataObjects.AuthenticationProviders _AuthenticationProviders = new DataObjects.AuthenticationProviders();
    private string _CultureCode = "en-US";
    private List<DataObjects.OptionPair> _CultureCodes = new List<DataObjects.OptionPair>();
    private DataObjects.Language _DefaultLanguage = new DataObjects.Language();
    private DataObjects.DeletedRecordCounts _DeletedRecordCounts = new DataObjects.DeletedRecordCounts();
    private List<DataObjects.DepartmentGroup> _DepartmentGroups = new List<DataObjects.DepartmentGroup>();
    private List<DataObjects.Department> _Departments = new List<DataObjects.Department>();
    private List<string> _DotNetHelperMessages = new List<string>();
    // {{ModuleItemStart:EmailTemplates}}
    private List<DataObjects.EmailTemplate> _EmailTemplates = new List<DataObjects.EmailTemplate>();
    // {{ModuleItemEnd:EmailTemplates}}
    private string _Fingerprint = "";
    private List<string>? _GloballyDisabledModules = null;
    private List<string>? _GloballyEnabledModules = null;
    private List<DataObjects.FileStorage> _ImageFiles = new List<DataObjects.FileStorage>();
    private DataObjects.Language _Language = new DataObjects.Language();
    private List<DataObjects.Language> _Languages = new List<DataObjects.Language>();
    private bool _Loaded = false;
    // {{ModuleItemStart:Locations}}
    private List<DataObjects.Location> _Locations = new List<DataObjects.Location>();
    // {{ModuleItemEnd:Locations}}
    private bool _LoggedIn = false;
    private List<Message> _Messages = new List<Message>();
    private DateTime _ModelUpdated = DateTime.UtcNow;
    private string? _NavigationId = "";
    private List<Plugins.Plugin> _Plugins = new List<Plugins.Plugin>();
    private string _QuickAction = "";
    private Delegate? _QuickActionOnComplete;
    // {{ModuleItemStart:Appointments}}
    private DataObjects.AppointmentNote _QuickAddAppointmentNote = new DataObjects.AppointmentNote();
    // {{ModuleItemEnd:Appointments}}
    private DataObjects.User _QuickAddUser = new DataObjects.User();
    private DateOnly _Released = DateOnly.FromDateTime(DateTime.MinValue);
    // {{ModuleItemStart:Services}}
    private List<DataObjects.Service> _Services = new List<DataObjects.Service>();
    // {{ModuleItemEnd:Services}}
    private bool _ShowTenantListingWhenMissingTenantCode = false;
    private List<string> _StartupErrors = new List<string>();
    private bool _StartupValidated = false;
    private List<string> _Subscribers_OnChange = new List<string>();
    private List<string> _Subscribers_OnDotNetHelperHandler = new List<string>();
    private List<string> _Subscribers_OnSignalRUpdate = new List<string>();
    // {{ModuleItemStart:Tags}}
    private List<DataObjects.Tag> _Tags = new List<DataObjects.Tag>();
    // {{ModuleItemEnd:Tags}}
    private DataObjects.Tenant _Tenant = new DataObjects.Tenant();
    private string? _TenantCodeFromUrl;
    private Guid _TenantId = Guid.Empty;
    private List<DataObjects.TenantList> _TenantList = new List<DataObjects.TenantList>();
    private List<DataObjects.Tenant> _Tenants = new List<DataObjects.Tenant>();
    private string _Theme = "";
    private List<DataObjects.udfLabel> _udfLabels = new List<DataObjects.udfLabel>();
    private bool _UseCustomAuthenticationProviderFromAdminAccount = false;
    private DataObjects.User _User = new DataObjects.User();
    private List<DataObjects.UserGroup> _UserGroups = new List<DataObjects.UserGroup>();
    private List<DataObjects.User> _Users = new List<DataObjects.User>();
    private bool _UseTenantCodeInUrl = false;
    private string _Version = "";
    private string _View = "";
    private bool _ViewIsEditPage = false;

    /// <summary>
    /// Gets or sets the list of active users.
    /// </summary>
    public List<DataObjects.ActiveUser> ActiveUsers {
        get { return _ActiveUsers; }
        set {
            if (!ObjectsAreEqual(_ActiveUsers, value)) {
                _ActiveUsers = value;
                _ModelUpdated = DateTime.UtcNow;
                NotifyDataChanged();
            }
        }
    }

    /// <summary>
    /// Adds a Toast message to the user interface.
    /// </summary>
    /// <param name="message">The message to add. And text inside double curly brackets (eg: {{Tag}}) will be replaced with the language tag for that item.</param>
    /// <param name="messageType">The message type, related to a Bootstrap type.</param>
    /// <param name="AutoHide">If true the message will automatically be hidden after 5 seconds.</param>
    /// <param name="RemovePreviousMessages">If true any previous messages will be removed and only this new message will be shown.</param>
    /// <param name="ReplaceLineBreaks">Option to replace any line breaks in the text with HTML an &lt;br /&gt; element.</param>
    public void AddMessage(string message, MessageType messageType = MessageType.Primary, bool AutoHide = true, bool RemovePreviousMessages = false, bool ReplaceLineBreaks = false)
    {
        AddMessage(new NewMessage {
            Text = message,
            MessageType = messageType,
        }, AutoHide, RemovePreviousMessages, ReplaceLineBreaks);
    }

    /// <summary>
    /// Adds a Toast message to the user interface.
    /// </summary>
    /// <param name="message">A message object to be added.</param>
    /// <param name="AutoHide">If true the message will automatically be hidden after 5 seconds.</param>
    /// <param name="RemovePreviousMessages">If true any previous messages will be removed and only this new message will be shown.</param>
    /// <param name="ReplaceLineBreaks">Option to replace any line breaks in the text with HTML an &lt;br /&gt; element.</param>
    public void AddMessage(NewMessage message, bool AutoHide = true, bool RemovePreviousMessages = false, bool ReplaceLineBreaks = false)
    {
        if (message.Text.Contains("{{") && message.Text.Contains("}}")) {
            message.Text = ReplaceLanguageTagsInString(message.Text);
        }

        if (RemovePreviousMessages) {
            _Messages = new List<Message> {
                new Message {
                    Id = Guid.NewGuid().ToString(),
                    AutoHide = AutoHide,
                    MessageType = message.MessageType,
                    Shown = DateTime.UtcNow,
                    Text = message.Text,
                    TimeLabel = "",
                    ReplaceLineBreaks = ReplaceLineBreaks,
                }
            };
        } else {
            _Messages.Add(new Message {
                Id = Guid.NewGuid().ToString(),
                AutoHide = AutoHide,
                MessageType = message.MessageType,
                Shown = DateTime.UtcNow,
                Text = message.Text,
                TimeLabel = "",
                ReplaceLineBreaks = ReplaceLineBreaks,
            });
        }
        _ModelUpdated = DateTime.UtcNow;
        NotifyDataChanged();
    }

    /// <summary>
    /// Gets or sets the Custom Login Provider from the Admin account.
    /// </summary>
    public DataObjects.CustomLoginProvider AdminCustomLoginProvider {
        get { return _AdminCustomLoginProvider; }
        set {
            if (!ObjectsAreEqual(_AdminCustomLoginProvider, value)) {
                _AdminCustomLoginProvider = value;
                _ModelUpdated = DateTime.UtcNow;
                NotifyDataChanged();
            }
        }
    }

    /// <summary>
    /// Indicates if users for this tenant are allowed to use a feature.
    /// </summary>
    /// <param name="feature">The name of the feature.</param>
    /// <returns>True/False</returns>
    protected bool AllowUsersToManageAFeature(string feature)
    {
        bool output = false;

        if (_Tenant.TenantSettings.AllowUsersToManageBasicProfileInfoElements != null) {
            var allowed = _Tenant.TenantSettings.AllowUsersToManageBasicProfileInfoElements.FirstOrDefault(x => x.ToLower() == feature.ToLower());
            output = allowed != null;
        }

        return output;
    }

    /// <summary>
    /// Indicates if users can edit their Department.
    /// </summary>
    public bool AllowUsersToManageDepartment {
        get { return FeatureEnabledDepartments && AllowUsersToManageAFeature("department"); }
    }

    /// <summary>
    /// Indicates if users can edit their email address.
    /// </summary>
    public bool AllowUsersToManageEmail {
        get { return AllowUsersToManageAFeature("email"); }
    }

    /// <summary>
    /// Indicates if users can edit their EmployeeId.
    /// </summary>
    public bool AllowUsersToManageEmployeeId {
        get { return FeatureEnabledEmployeeId && AllowUsersToManageAFeature("employeeid"); }
    }

    // {{ModuleItemStart:Locations}}
    /// <summary>
    /// Indicates if users can edit their location.
    /// </summary>
    public bool AllowUsersToManageLocation {
        get { return FeatureEnabledLocation && AllowUsersToManageAFeature("location"); }
    }
    // {{ModuleItemEnd:Locations}}

    /// <summary>
    /// Indicates if users can edit their name.
    /// </summary>
    public bool AllowUsersToManageName {
        get { return AllowUsersToManageAFeature("name"); }
    }

    /// <summary>
    /// Indicates if users can edit their phone number.
    /// </summary>
    public bool AllowUsersToManagePhone {
        get { return AllowUsersToManageAFeature("phone"); }
    }

    /// <summary>
    /// Indicates if users can edit their job title.
    /// </summary>
    public bool AllowUsersToManageTitle {
        get { return AllowUsersToManageAFeature("title"); }
    }

    /// <summary>
    /// A list of all tenants. This is only populated for AppAdmin users.
    /// </summary>
    public List<DataObjects.Tenant> AllTenants {
        get { return _AllTenants; }
        set {
            if (!ObjectsAreEqual(_AllTenants, value)) {
                _AllTenants = value;
                _ModelUpdated = DateTime.UtcNow;
                NotifyDataChanged();
            }
        }
    }

    /// <summary>
    /// Indicates if the application is online.
    /// </summary>
    public bool AppOnline {
        get { return _AppOnline; }
        set {
            if (_AppOnline != value) {
                _AppOnline = value;
                _ModelUpdated = DateTime.UtcNow;
                NotifyDataChanged();
            }
        }
    }

    /// <summary>
    /// The App Settings that get pushed out via SignalR
    /// </summary>
    public DataObjects.ApplicationSettingsUpdate AppSettings {
        get { return _AppSettings; }
        set {
            if (!ObjectsAreEqual(_AppSettings, value)) {
                _AppSettings = value;
                _ModelUpdated = DateTime.UtcNow;
                NotifyDataChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the base URL for the application.
    /// </summary>
    public string ApplicationUrl {
        get {
            return !String.IsNullOrWhiteSpace(_Tenant.TenantSettings.ApplicationUrl) ? _Tenant.TenantSettings.ApplicationUrl : _ApplicationUrl;
        }
        set {
            if (_ApplicationUrl != value) {
                _ApplicationUrl = value;
                _ModelUpdated = DateTime.UtcNow;
                NotifyDataChanged();
            }
        }
    }

    /// <summary>
    /// Gets the base default URL for the application, regardless of any tenant-specific ApplicationUrl setting.
    /// </summary>
    public string ApplicationUrlDefault {
        get {
            return _ApplicationUrl;
        }
    }

    /// <summary>
    /// Gets the full URL to the application, which is the root plus a Tenant Code if they are used in the URL.
    /// </summary>
    public string ApplicationUrlFull {
        get {
            string output = ApplicationUrl;

            if (_UseTenantCodeInUrl) {
                if (!output.EndsWith("/")) {
                    output += "/";
                }

                if (!String.IsNullOrWhiteSpace(_Tenant.TenantCode)) {
                    output += _Tenant.TenantCode + "/";
                } else if (!String.IsNullOrWhiteSpace(_TenantCodeFromUrl)) {
                    output += _TenantCodeFromUrl + "/";
                }
            }

            return output;
        }
    }

    /// <summary>
    /// Gets the full URL to the application, which is the root plus a Tenant Code if they are used in the URL, regardless of any tenant-specific ApplicationUrl setting.
    /// </summary>
    public string ApplicationUrlFullDefault {
        get {
            string output = _ApplicationUrl;

            if (_UseTenantCodeInUrl) {
                if (!output.EndsWith("/")) {
                    output += "/";
                }

                if (!String.IsNullOrWhiteSpace(_Tenant.TenantCode)) {
                    output += _Tenant.TenantCode + "/";
                } else if (!String.IsNullOrWhiteSpace(_TenantCodeFromUrl)) {
                    output += _TenantCodeFromUrl + "/";
                }
            }

            return output;
        }
    }

    /// <summary>
    /// Gets or sets the AuthenticationProviders.
    /// </summary>
    public DataObjects.AuthenticationProviders AuthenticationProviders {
        get { return _AuthenticationProviders; }
        set {
            if (!ObjectsAreEqual(_AuthenticationProviders, value)) {
                _AuthenticationProviders = value;
                _ModelUpdated = DateTime.UtcNow;
                NotifyDataChanged();
            }
        }
    }

    /// <summary>
    /// Clears any Toast messages in the user interface.
    /// </summary>
    public void ClearMessages()
    {
        _Messages = new List<Message>();
        _ModelUpdated = DateTime.UtcNow;
        NotifyDataChanged();
    }

    /// <summary>
    /// The current culture code (defaults to 'en-US').
    /// </summary>
    public string CultureCode {
        get { return _CultureCode; }
        set {
            if (_CultureCode != value) {
                _CultureCode = value;
                _ModelUpdated = DateTime.UtcNow;
                NotifyDataChanged();
            }
        }
    }

    /// <summary>
    /// A collection of all available culture codes.
    /// </summary>
    public List<DataObjects.OptionPair> CultureCodes {
        get { return _CultureCodes; }
        set {
            if (!ObjectsAreEqual(_CultureCodes, value)) {
                _CultureCodes = value;
                _ModelUpdated = DateTime.UtcNow;
                NotifyDataChanged();
            }
        }
    }

    /// <summary>
    /// Holds the values for admin users to see information about deleted records pending deletion.
    /// </summary>
    public DataObjects.DeletedRecordCounts DeletedRecordCounts {
        get { return _DeletedRecordCounts; }
        set {
            if (!ObjectsAreEqual(_DeletedRecordCounts, value)) {
                _DeletedRecordCounts = value;
                _ModelUpdated = DateTime.UtcNow;
                NotifyDataChanged();
            }
        }
    }

    /// <summary>
    /// Indicates if the app is in development mode (running on localhost)
    /// </summary>
    public bool DevelopmentMode {
        get {
            bool output = false;

            if (!String.IsNullOrWhiteSpace(_ApplicationUrl) && _ApplicationUrl.ToLower().Contains("localhost")) {
                output = true;
            }

            return output;
        }
    }

    /// <summary>
    /// A helper invoked from javascript to pass messages from the .NET Helper back into the data model.
    /// </summary>
    /// <param name="messages"></param>
    public void DotNetHelperHandler(List<string> messages)
    {
        _DotNetHelperMessages = messages;
        NotifyDotNetHelperHandler();
    }

    /// <summary>
    /// Indicates if the given feature is enabled.
    /// </summary>
    /// <param name="feature">The name of the feature.</param>
    /// <returns>True/False</returns>
    protected bool FeatureEnabled(string feature)
    {
        bool output = false;

        // See if this is globally unavailable.
        if (_GloballyDisabledModules != null && _GloballyDisabledModules.Any()) {
            var globallyDisabled = _GloballyDisabledModules.Contains(feature.ToLower());
            if (globallyDisabled) {
                return false;
            }
        }

        // See if this is globally enabled.
        if (_GloballyEnabledModules != null && _GloballyEnabledModules.Any()) {
            var globallyEnabled = _GloballyEnabledModules.Contains(feature.ToLower());
            if (globallyEnabled) {
                return true;
            }
        }

        // See if this is blocked for this tenant.
        bool blocked = false;
        if (_Tenant.TenantSettings.ModuleHideElements != null && _Tenant.TenantSettings.ModuleHideElements.Any()) {
            var element = _Tenant.TenantSettings.ModuleHideElements.FirstOrDefault(x => x.ToLower() == feature.ToLower());
            if (element != null) {
                blocked = true;
            }
        }

        // See if they have opted in if this is not blocked.
        if (!blocked) {
            if (_Tenant.TenantSettings.ModuleOptInElements != null && _Tenant.TenantSettings.ModuleOptInElements.Any()) {
                var optedIn = _Tenant.TenantSettings.ModuleOptInElements.FirstOrDefault(x => x.ToLower() == feature.ToLower());
                if (optedIn != null) {
                    output = true;
                }
            }
        }

        return output;
    }

    /// <summary>
    /// Indicates if the Departments feature is enabled.
    /// </summary>
    public bool FeatureEnabledDepartments {
        get { return FeatureEnabled("departments"); }
    }

    // {{ModuleItemStart:Invoices}}
    /// <summary>
    /// Indicates if the Invoices feature is enabled.
    /// </summary>
    public bool FeatureEnabledInvoices {
        get { return FeatureEnabled("invoices"); }
    }
    // {{ModuleItemEnd:Invoices}}

    // {{ModuleItemStart:EmailTemplates}}
    /// <summary>
    /// Indicates if the EmailTemplates feature is enabled.
    /// </summary>
    public bool FeatureEnabledEmailTemplates {
        get { return FeatureEnabled("emailtemplates"); }
    }
    // {{ModuleItemEnd:EmailTemplates}}

    /// <summary>
    /// Indicates if the EmployeeId feature is enabled.
    /// </summary>
    public bool FeatureEnabledEmployeeId {
        get { return FeatureEnabled("employeeid"); }
    }

    /// <summary>
    /// Indicates if the Files feature is enabled.
    /// </summary>
    public bool FeatureEnabledFiles {
        get { return FeatureEnabled("files"); }
    }

    // {{ModuleItemStart:Locations}}
    /// <summary>
    /// Indicates if the Location feature is enabled.
    /// </summary>
    public bool FeatureEnabledLocation {
        get { return FeatureEnabled("location"); }
    }
    // {{ModuleItemEnd:Locations}}

    // {{ModuleItemStart:Appointments}}
    /// <summary>
    /// Indicates if the Scheduling feature is enabled.
    /// </summary>
    public bool FeatureEnabledScheduling {
        get { return FeatureEnabled("scheduling"); }
    }
    // {{ModuleItemEnd:Appointments}}

    // {{ModuleItemStart:Services}}
    /// <summary>
    /// Indicates if the Services feature is enabled.
    /// </summary>
    public bool FeatureEnabledServices {
        get { return FeatureEnabled("services"); }
    }
    // {{ModuleItemEnd:Services}}

    // {{ModuleItemStart:Tags}}
    /// <summary>
    /// Indicates if the Tags feature is enabled.
    /// </summary>
    public bool FeatureEnabledTags {
        get { return FeatureEnabled("tags"); }
    }
    // {{ModuleItemEnd:Tags}}

    /// <summary>
    /// Indicates if the Themes feature is enabled.
    /// </summary>
    public bool FeatureEnabledThemes {
        get { return FeatureEnabled("themes"); }
    }

    /// <summary>
    /// Indicates if the User-Defined Fields feature is enabled.
    /// </summary>
    public bool FeatureEnabledUDF {
        get { return FeatureEnabled("udf"); }
    }

    /// <summary>
    /// Indicates if the User Groups feature is enabled.
    /// </summary>
    public bool FeatureEnabledUserGroups {
        get { return FeatureEnabled("usergroups"); }
    }

    /// <summary>
    /// Indicates if the Work Schedule feature is enabled.
    /// </summary>
    public bool FeatureEnabledWorkSchedule {
        get { return FeatureEnabled("workschedule"); }
    }

    /// <summary>
    /// The default language collection for the application.
    /// </summary>
    public DataObjects.Language DefaultLanguage {
        get { return _DefaultLanguage; }
        set {
            if (!ObjectsAreEqual(_DefaultLanguage, value)) {
                _DefaultLanguage = value;
                _ModelUpdated = DateTime.UtcNow;
                NotifyDataChanged();
            }
        }
    }

    /// <summary>
    /// Gets the name of a Department Group from the unique id.
    /// </summary>
    /// <param name="DepartmentGroupId">The unique id of the group.</param>
    /// <returns>The name of the group.</returns>
    public string DepartmentGroupName(Guid? DepartmentGroupId)
    {
        string output = "";

        if (_DepartmentGroups.Any() && DepartmentGroupId.HasValue) {
            var departmentGroup = _DepartmentGroups.FirstOrDefault(x => x.DepartmentGroupId == DepartmentGroupId);
            if (departmentGroup != null) {
                output += departmentGroup.DepartmentGroupName;
            }
        }

        return output;
    }

    /// <summary>
    /// Gets the name of the Department from the unique id.
    /// </summary>
    /// <param name="DepartmentId">The unique id of the department.</param>
    /// <returns>The name of the department.</returns>
    public string DepartmentName(Guid? DepartmentId)
    {
        string output = "";

        if (_Departments.Any() && DepartmentId.HasValue) {
            var department = _Departments.FirstOrDefault(x => x.DepartmentId == DepartmentId);
            if (department != null) {
                output += department.DepartmentName;
            }
        }

        return output;
    }

    /// <summary>
    /// The list of all department groups.
    /// </summary>
    public List<DataObjects.DepartmentGroup> DepartmentGroups {
        get { return _DepartmentGroups; }
        set {
            if (!ObjectsAreEqual(_DepartmentGroups, value)) {
                _DepartmentGroups = value;
                _ModelUpdated = DateTime.UtcNow;
                NotifyDataChanged();
            }
        }
    }

    /// <summary>
    /// The list of all deparments.
    /// </summary>
    public List<DataObjects.Department> Departments {
        get { return _Departments; }
        set {
            if (!ObjectsAreEqual(_Departments, value)) {
                _Departments = value;
                _ModelUpdated = DateTime.UtcNow;
                NotifyDataChanged();
            }
        }
    }

    // {{ModuleItemStart:EmailTemplates}}
    /// <summary>
    /// The list of email templates
    /// </summary>
    public List<DataObjects.EmailTemplate> EmailTemplates {
        get { return _EmailTemplates; }
        set {
            if (!ObjectsAreEqual(_EmailTemplates, value)) {
                _EmailTemplates = value;
                _ModelUpdated = DateTime.UtcNow;
                NotifyDataChanged();
            }
        }
    }
    // {{ModuleItemEnd:EmailTemplates}}

    /// <summary>
    /// Shows a Toast with an error message.
    /// </summary>
    /// <param name="message">A message object to be added.</param>
    /// <param name="AutoHide">If true the message will automatically be hidden after 5 seconds.</param>
    /// <param name="RemovePreviousMessages">If true any previous messages will be removed and only this new message will be shown.</param>
    /// <param name="ReplaceLineBreaks">Option to replace any line breaks in the text with HTML an &lt;br /&gt; element.</param>
    /// <param name="introMessage">An optional intro message to show before the error. The default will show a standard message. Set the string to an empty string for no message.</param>
    public void ErrorMessage(string message, bool AutoHide = false, bool RemovePreviousMessages = true, bool ReplaceLineBreaks = false, string introMessage = "{{default}}")
    {
        if (!String.IsNullOrWhiteSpace(message)) {
            if (message.Contains("{{") && message.Contains("}}")) {
                message = ReplaceLanguageTagsInString(message);
            }

            if (!String.IsNullOrWhiteSpace(introMessage)) {
                if (introMessage == "{{default}}") {
                    message = "<div class='mb-2'>" + Helpers.Text("ErrorOccurred") + "</div>" + message;
                } else {
                    message = "<div class='mb-2'>" + ReplaceLanguageTagsInString(introMessage) + "</div>" + message;
                }
            }

            AddMessage(new NewMessage {
                Text = message,
                MessageType = MessageType.Danger,
            }, AutoHide, RemovePreviousMessages, ReplaceLineBreaks);
        }
    }

    /// <summary>
    /// Shows a Toast with one or more error messages.
    /// </summary>
    /// <param name="messages">A Collection of Messages</param>
    /// <param name="AutoHide">If true the message will automatically be hidden after 5 seconds.</param>
    /// <param name="RemovePreviousMessages">If true any previous messages will be removed and only this new message will be shown.</param>
    /// <param name="ReplaceLineBreaks">Option to replace any line breaks in the text with HTML an &lt;br /&gt; element.</param>
    /// <param name="introMessage">An optional intro message to show before the error. The default will show a standard message. Set the string to an empty string for no message.</param>
    public void ErrorMessages(List<string> messages, bool AutoHide = false, bool RemovePreviousMessages = true, bool ReplaceLineBreaks = false, string introMessage = "{{default}}")
    {
        if (messages.Any()) {
            string message = "";

            if (!String.IsNullOrWhiteSpace(introMessage)) {
                if (introMessage == "{{default}}") {
                    if (messages.Count() == 1) {
                        message = Helpers.Text("ErrorOccurred");
                    } else {
                        message = Helpers.Text("ErrorsOccurred");
                    }
                } else {
                    message = introMessage;
                }
            }

            if (messages.Count() == 1) {
                message += "<div class=\"mt-2\">" + ReplaceLanguageTagsInString(messages[0]) + "</div>";
            } else {
                message += "<ul class=\"mt-2\">";
                foreach (var msg in messages) {
                    message += "<li>" + ReplaceLanguageTagsInString(msg) + "</li>";
                }
                message += "</ul>";
            }

            AddMessage(new NewMessage {
                Text = message,
                MessageType = MessageType.Danger,
            }, AutoHide, RemovePreviousMessages, ReplaceLineBreaks);
        } else {
            UnknownError();
        }
    }

    /// <summary>
    /// Gets or sets the browser fingerprint.
    /// </summary>
    public string Fingerprint {
        get { return _Fingerprint; }
        set {
            if (_Fingerprint != value) {
                _Fingerprint = value;
                _ModelUpdated = DateTime.UtcNow;
                NotifyDataChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the items that have been disabled globally in the appsettings.json file.
    /// </summary>
    public List<string>? GloballyDisabledModules {
        get { return _GloballyDisabledModules; }
        set {
            if (!ObjectsAreEqual(_GloballyDisabledModules, value)) {
                _GloballyDisabledModules = value;
                _ModelUpdated = DateTime.UtcNow;
                NotifyDataChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the items that have been globally enabled in the appsettings.json file.
    /// </summary>
    public List<string>? GloballyEnabledModules {
        get { return _GloballyEnabledModules; }
        set {
            if (!ObjectsAreEqual(_GloballyEnabledModules, value)) {
                _GloballyEnabledModules = value;
                _ModelUpdated = DateTime.UtcNow;
                NotifyDataChanged();
            }
        }
    }

    /// <summary>
    /// Indicates if there are any pending deleted items.
    /// </summary>
    public bool HaveDeletedRecords {
        get {
            bool output = false;

            if (
                // {{ModuleItemStart:Appointments}}
                DeletedRecordCounts.AppointmentNotes > 0 ||
                DeletedRecordCounts.Appointments > 0 ||
                DeletedRecordCounts.AppointmentServices > 0 ||
                // {{ModuleItemEnd:Appointments}}
                DeletedRecordCounts.DepartmentGroups > 0 ||
                DeletedRecordCounts.Departments > 0 ||
                // {{ModuleItemStart:EmailTemplates}}
                DeletedRecordCounts.EmailTemplates > 0 ||
                // {{ModuleItemEnd:EmailTemplates}}
                DeletedRecordCounts.FileStorage > 0 ||
                // {{ModuleItemStart:Locations}}
                DeletedRecordCounts.Locations > 0 ||
                // {{ModuleItemEnd:Locations}}
                // {{ModuleItemStart:Services}}
                DeletedRecordCounts.Services > 0 ||
                // {{ModuleItemEnd:Services}}
                // {{ModuleItemStart:Tags}}
                DeletedRecordCounts.Tags > 0 ||
                // {{ModuleItemEnd:Tags}}
                DeletedRecordCounts.UserGroups > 0 ||
                DeletedRecordCounts.Users > 0
            ) {
                output = true;
            }

            if (!output) {
                output = HaveDeletedRecordsApp;
            }

            return output;
        }
    }

    /// <summary>
    /// Determines the hour format for the current user's browser settings (12 or 24 hour).
    /// </summary>
    public string HourFormat {
        get {
            string output = Convert.ToDateTime("1/1/2000 13:00:00").ToString("t").ToUpper().Contains("PM") ? "12" : "24";
            return output;
        }
    }

    /// <summary>
    /// Gets or sets the collection of ImageFiles.
    /// </summary>
    public List<DataObjects.FileStorage> ImageFiles {
        get { return _ImageFiles; }
        set {
            if (!ObjectsAreEqual(_ImageFiles, value)) {
                _ImageFiles = value;
                _ModelUpdated = DateTime.UtcNow;
                NotifyDataChanged();
            }
        }
    }

    /// <summary>
    /// Contains the current Toast messages for the user interface.
    /// </summary>
    public List<Message> Messages {
        get { return _Messages; }
        set {
            if (!ObjectsAreEqual(_Messages, value)) {
                _Messages = value;
                _ModelUpdated = DateTime.UtcNow;
                NotifyDataChanged();
            }
        }
    }

    /// <summary>
    /// The DateTime value of when the model was last updated.
    /// </summary>
    public DateTime ModelUpdated {
        get { return _ModelUpdated; }
    }

    /// <summary>
    /// The list of available module in the applicaiton.
    /// </summary>
    public List<string> Modules {
        get {
            var output = new List<string> {
                "departments",
                "employeeid",
                // {{ModuleItemStart:EmailTemplates}}
                "emailtemplates",
                // {{ModuleItemEnd:EmailTemplates}}
                "files",
                // {{ModuleItemStart:Invoices}}
                "invoices",
                // {{ModuleItemEnd:Invoices}}
                // {{ModuleItemStart:Locations}}
                "location",
                // {{ModuleItemEnd:Locations}}
                // {{ModuleItemStart:Appointments}}
                "scheduling",
                // {{ModuleItemEnd:Appointments}}
                // {{ModuleItemStart:Services}}
                "services",
                // {{ModuleItemEnd:Services}}
                // {{ModuleItemStart:Tags}}
                "tags",
                // {{ModuleItemEnd:Tags}}
                "themes",
                "udf",
                "usergroups",
                "workschedule",
            };

            if (_GloballyDisabledModules != null && _GloballyDisabledModules.Any()) {
                foreach (var item in _GloballyDisabledModules) {
                    output = output.Where(x => x != item).ToList();
                }
            }

            return output;
        }
    }

    /// <summary>
    /// The current language for the user interface.
    /// </summary>
    public DataObjects.Language Language {
        get { return _Language; }
        set {
            if (!ObjectsAreEqual(_Language, value)) {
                _Language = value;
                _ModelUpdated = DateTime.UtcNow;
                NotifyDataChanged();
            }
        }
    }

    /// <summary>
    /// A collection of all the available languages for the current user.
    /// </summary>
    public List<DataObjects.Language> Languages {
        get { return _Languages; }
        set {
            if (!ObjectsAreEqual(_Languages, value)) {
                _Languages = value;
                _ModelUpdated = DateTime.UtcNow;
                NotifyDataChanged();
            }
        }
    }

    /// <summary>
    /// Indicates if the model has completed loading.
    /// This happens after the Blazor Data Model is loaded.
    /// </summary>
    public bool Loaded {
        get { return _Loaded; }
        set {
            if (_Loaded != value) {
                _Loaded = value;
                _ModelUpdated = DateTime.UtcNow;
                NotifyDataChanged();
            }
        }
    }

    /// <summary>
    /// The loading message shown on various pages when data is loading.
    /// </summary>
    public string LoadingMessage {
        get {
            return "<div><div class=\"me-2 spinner-border loading-spinner text-primary\" role=\"status\"></div>" + Helpers.Text("LoadingWait") + "</div>";
        }
    }

    // {{ModuleItemStart:Locations}}
    /// <summary>
    /// The list of locations for the current tenant.
    /// </summary>
    public List<DataObjects.Location> Locations {
        get { return _Locations; }
        set {
            if (!ObjectsAreEqual(_Locations, value)) {
                _Locations = value;
                _ModelUpdated = DateTime.UtcNow;
                NotifyDataChanged();
            }
        }
    }
    // {{ModuleItemEnd:Locations}}

    /// <summary>
    /// Indicates if a user is logged in.
    /// </summary>
    public bool LoggedIn {
        get { return _LoggedIn; }
        set {
            if (_LoggedIn != value) {
                _LoggedIn = value;
                _ModelUpdated = DateTime.UtcNow;
                NotifyDataChanged();
            }
        }
    }

    /// <summary>
    /// Shows a standard "Deleting" message.
    /// </summary>
    /// <param name="message">Optional message to override the default message.</param>
    public void Message_Deleting(string message = "")
    {
        if (String.IsNullOrWhiteSpace(message)) {
            message =
                "<div class='d-flex align-items-center'>\n" +
                "  <div class='me-2 toast-large-icon'><i class='fas fa-trash'></i></div>\n" +
                "  <div class='me-auto toast-large'>" + Helpers.Text("DeletingWait") + "</div>\n" +
                "</div>\n";
        } else {
            message = ReplaceLanguageTagsInString(message);
        }

        AddMessage(message, MessageType.Danger, false);
    }

    /// <summary>
    /// Shows a standard "Loading" message.
    /// </summary>
    /// <param name="message">Optional message to override the default message.</param>
    public void Message_Loading(string message = "")
    {
        if (String.IsNullOrWhiteSpace(message)) {
            message =
                "<div class='d-flex align-items-center'>\n" +
                "  <div class='spinner-grow me-2' role='status'></div>\n" +
                "  <div class='me-auto toast-large'>" + Helpers.Text("LoadingWait") + "</div>\n" +
                "</div>\n";
        } else {
            message = ReplaceLanguageTagsInString(message);
        }

        AddMessage(message, MessageType.Success, false);
    }

    /// <summary>
    /// Shows a standard "Processing" message.
    /// </summary>
    /// <param name="message">Optional message to override the default message.</param>
    public void Message_Processing(string message = "")
    {
        if (String.IsNullOrWhiteSpace(message)) {
            message =
                "<div class='d-flex align-items-center'>\n" +
                "  <div class='spinner-grow me-2' role='status'></div>\n" +
                "  <div class='me-auto toast-large'>" + Helpers.Text("ProcessingWait") + "</div>\n" +
                "</div>\n";
        } else {
            message = ReplaceLanguageTagsInString(message);
        }

        AddMessage(message, MessageType.Success, false);
    }

    /// <summary>
    /// Shows a standard "Record Deleted" message.
    /// </summary>
    /// <param name="message">Optional message to override the default message.</param>
    public void Message_RecordDeleted(string message = "", string? deletedBy = "")
    {
        if (String.IsNullOrWhiteSpace(message)) {
            message = !String.IsNullOrWhiteSpace(deletedBy)
                ? Helpers.Text("RecordDeletedBy") + " " + deletedBy
                : Helpers.Text("RecordDeleted");
        } else {
            message = ReplaceLanguageTagsInString(message);
        }

        AddMessage(message, MessageType.Danger, false, true);
    }

    /// <summary>
    /// Shows a standard "Record Updated" message.
    /// </summary>
    /// <param name="message">Optional message to override the default message.</param>
    public void Message_RecordUpdated(string message = "", string? updatedBy = "")
    {
        if (String.IsNullOrWhiteSpace(message)) {
            message = !String.IsNullOrWhiteSpace(updatedBy)
                ? Helpers.Text("RecordUpdatedBy") + " " + updatedBy
                : Helpers.Text("RecordUpdated");
        } else {
            message = ReplaceLanguageTagsInString(message);
        }

        AddMessage(message, MessageType.Warning, false, true);
    }

    /// <summary>
    /// Shows a standard "Saved" message.
    /// </summary>
    /// <param name="message">Optional message to override the default message.</param>
    public void Message_Saved(string message = "")
    {
        if (String.IsNullOrWhiteSpace(message)) {
            message =
                "<div class='d-flex align-items-center'>\n" +
                "  <div class='me-2 toast-large-icon'><i class='fa fa-save'></i></div>\n" +
                "  <div class='me-auto toast-large'>" + Helpers.Text("SavedAt") + " " + DateTime.Now.ToShortTimeString() + "</div>\n" +
                "</div>\n";
        } else {
            message = ReplaceLanguageTagsInString(message);
        }

        AddMessage(message, MessageType.Success, true);
    }

    /// <summary>
    /// Shows a standard "Saving" message.
    /// </summary>
    /// <param name="message">Optional message to override the default message.</param>
    public void Message_Saving(string message = "")
    {
        if (String.IsNullOrWhiteSpace(message)) {
            message =
                "<div class='d-flex align-items-center'>\n" +
                "  <div class='me-2 toast-large-icon'><i class='fa fa-save'></i></div>\n" +
                "  <div class='me-auto toast-large'>" + Helpers.Text("SavingWait") + "</div>\n" +
                "</div>\n";
        } else {
            message = ReplaceLanguageTagsInString(message);
        }

        AddMessage(message, MessageType.Success, false);
    }

    /// <summary>
    /// Shows a standard "Sending" message.
    /// </summary>
    /// <param name="message">Optional message to override the default message.</param>
    public void Message_Sending(string message = "")
    {
        if (String.IsNullOrWhiteSpace(message)) {
            message =
                "<div class='d-flex align-items-center'>\n" +
                "  <div class='me-2 toast-large-icon'><i class='fa fa-save'></i></div>\n" +
                "  <div class='me-auto toast-large'>" + Helpers.Text("SendingWait") + "</div>\n" +
                "</div>\n";
        } else {
            message = ReplaceLanguageTagsInString(message);
        }

        AddMessage(message, MessageType.Success, false);
    }

    /// <summary>
    /// Shows a standard "Sending" message.
    /// </summary>
    /// <param name="message">Optional message to override the default message.</param>
    public void Message_Sent(string message = "")
    {
        if (String.IsNullOrWhiteSpace(message)) {
            message =
                "<div class='d-flex align-items-center'>\n" +
                "  <div class='me-2 toast-large-icon'><i class='fa fa-save'></i></div>\n" +
                "  <div class='me-auto toast-large'>" + Helpers.Text("Sent") + "</div>\n" +
                "</div>\n";
        } else {
            message = ReplaceLanguageTagsInString(message);
        }

        AddMessage(message, MessageType.Success, true);
    }

    /// <summary>
    /// Shows a standard "Success" message.
    /// </summary>
    /// <param name="message">Optional message to override the default message.</param>
    public void Message_Success(string message = "")
    {
        if (String.IsNullOrWhiteSpace(message)) {
            message =
                "<div class='d-flex align-items-center'>\n" +
                "  <div class='me-2 toast-large-icon'><i class='fa fa-save'></i></div>\n" +
                "  <div class='me-auto toast-large'>" + Helpers.Text("Success") + "</div>\n" +
                "</div>\n";
        } else {
            message = ReplaceLanguageTagsInString(message);
        }

        AddMessage(message, MessageType.Success, true);
    }

    /// <summary>
    /// Shows a standard "Validating" message.
    /// </summary>
    /// <param name="message">Optional message to override the default message.</param>
    public void Message_Validating(string message = "")
    {
        if (String.IsNullOrWhiteSpace(message)) {
            message =
                "<div class='d-flex align-items-center'>\n" +
                "  <div class='spinner-grow me-2' role='status'></div>\n" +
                "  <div class='me-auto toast-large'>" + Helpers.Text("ValidatingWait") + "</div>\n" +
                "</div>\n";
        } else {
            message = ReplaceLanguageTagsInString(message);
        }

        AddMessage(message, MessageType.Success, false);
    }

    /// <summary>
    /// Gets or sets the optional ID in the navigation.
    /// </summary>
    public string? NavigationId {
        get { return _NavigationId; }
        set {
            if (_NavigationId != value) {
                _NavigationId = value;
                _ModelUpdated = DateTime.UtcNow;
                NotifyDataChanged();

                if (_LoggedIn) {
                    UpdateUserViewPreferences();
                }
            }
        }
    }

    /// <summary>
    /// Used to compare if two objects are equal.
    /// </summary>
    /// <param name="obj1">The first object.</param>
    /// <param name="obj2">The second object.</param>
    /// <returns>True if the objects serialize the same.</returns>
    public bool ObjectsAreEqual(object? obj1, object? obj2)
    {
        if (obj1 == null && obj2 != null) {
            return false;
        } else if (obj2 == null && obj1 != null) {
            return false;
        } else {
            var stringObject1 = String.Empty;
            var stringObject2 = String.Empty;

            try {
                stringObject1 = System.Text.Json.JsonSerializer.Serialize(obj1);
            } catch {
                return false;
            }

            try {
                stringObject2 = System.Text.Json.JsonSerializer.Serialize(obj2);
            } catch {
                return false;
            }

            return stringObject1 == stringObject2;
        }
    }

    /// <summary>
    /// Gets or sets the collection of available plugins.
    /// </summary>
    public List<Plugins.Plugin> Plugins {
        get { return _Plugins.Where(x => x.LimitToTenants.Count() == 0 || x.LimitToTenants.Contains(_TenantId)).ToList(); }
        set {
            if (!ObjectsAreEqual(_Plugins, value)) {
                _Plugins = value;
                _ModelUpdated = DateTime.UtcNow;
                NotifyDataChanged();
            }
        }
    }

    /// <summary>
    /// Used to indicate which quick action is being used in the offcanvas area.
    /// </summary>
    public string QuickAction {
        get { return _QuickAction; }
        set {
            if (_QuickAction != value) {
                _QuickAction = value;
                _ModelUpdated = DateTime.UtcNow;
                NotifyDataChanged();
            }
        }
    }

    /// <summary>
    /// An optional delegate that will be called with a quick action is complete.
    /// </summary>
    public Delegate? QuickActionOnComplete {
        get { return _QuickActionOnComplete; }
        set {
            if (!ObjectsAreEqual(_QuickActionOnComplete, value)) {
                _QuickActionOnComplete = value;
                _ModelUpdated = DateTime.UtcNow;
                NotifyDataChanged();
            }
        }
    }

    // {{ModuleItemStart:Appointments}}
    /// <summary>
    /// The AppointmentNote object used when adding a note via the quick action menu.
    /// </summary>
    public DataObjects.AppointmentNote QuickAddAppointmentNote {
        get { return _QuickAddAppointmentNote; }
        set {
            if (!ObjectsAreEqual(_QuickAddAppointmentNote, value)) {
                _QuickAddAppointmentNote = value;
                _ModelUpdated = DateTime.UtcNow;
                NotifyDataChanged();
            }
        }
    }
    // {{ModuleItemEnd:Appointments}}

    /// <summary>
    /// The user object used when adding a user via the quick action menu.
    /// </summary>
    public DataObjects.User QuickAddUser {
        get { return _QuickAddUser; }
        set {
            if (!ObjectsAreEqual(_QuickAddUser, value)) {
                _QuickAddUser = value;
                _ModelUpdated = DateTime.UtcNow;
                NotifyDataChanged();
            }
        }
    }

    /// <summary>
    /// Holds the DateTime value for when the application was released.
    /// </summary>
    public DateOnly Released {
        get { return _Released; }
        set {
            if (_Released != value) {
                _Released = value;
                _ModelUpdated = DateTime.UtcNow;
                NotifyDataChanged();
            }
        }
    }

    /// <summary>
    /// Replaces text in a string (non-case-senstive).
    /// </summary>
    /// <param name="input">The string containing the text you want to update.</param>
    /// <param name="replaceText">The text to find inside the string (eg: {{Tag}}).</param>
    /// <param name="withText">The text to use to replace any of the found text with.</param>
    /// <returns></returns>
    public static string Replace(string? input, string? replaceText, string? withText)
    {
        string output = String.Empty;

        if (!String.IsNullOrWhiteSpace(input) && !String.IsNullOrWhiteSpace(replaceText)) {
            output = input;

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

    /// <summary>
    /// Replaces language tags in a string.
    /// </summary>
    /// <param name="text">The string that might contain language tags (eg: {{Tag}}) that need to be replaced.</param>
    /// <returns>The updated string with any language tags replaced.</returns>
    public string ReplaceLanguageTagsInString(string text)
    {
        string output = text;

        if (output.Contains("{{")) {
            DataObjects.Language lang = new DataObjects.Language();

            if (this.LoggedIn) {
                lang = this.Language;
            }

            if (!lang.Phrases.Any()) {
                lang = this.DefaultLanguage;
            }

            if (lang.Phrases.Any()) {
                string lower = output.ToLower();

                foreach (var phrase in lang.Phrases) {
                    if (!String.IsNullOrWhiteSpace(phrase.Id)) {
                        string thisPhrase = "{{" + phrase.Id.ToLower() + "}}";

                        if (lower.Contains(thisPhrase)) {
                            output = Replace(output, thisPhrase, phrase.Value);
                        }
                    }

                    if (!output.Contains("{{")) {
                        break;
                    }
                }
            }
        }

        return output;
    }

    // {{ModuleItemStart:Services}}
    /// <summary>
    /// The list of available services.
    /// </summary>
    public List<DataObjects.Service> Services {
        get { return _Services; }
        set {
            if (!ObjectsAreEqual(_Services, value)) {
                _Services = value;
                _ModelUpdated = DateTime.UtcNow;
                NotifyDataChanged();
            }
        }
    }
    // {{ModuleItemEnd:Services}}

    /// <summary>
    /// Gets or sets the option to show the tenant listing when missing the tenant code.
    /// </summary>
    public bool ShowTenantListingWhenMissingTenantCode {
        get { return _ShowTenantListingWhenMissingTenantCode; }
        set {
            if (_ShowTenantListingWhenMissingTenantCode != value) {
                _ShowTenantListingWhenMissingTenantCode = value;
                _ModelUpdated = DateTime.UtcNow;
                NotifyDataChanged();
            }
        }
    }

    /// <summary>
    /// The method that notifies other pages of SignalR updates.
    /// </summary>
    /// <param name="update"></param>
    public void SignalRUpdate(DataObjects.SignalRUpdate update)
    {
        NotifySignalRUpdate(update);
    }

    /// <summary>
    /// Gets or sets any startup error messages.
    /// </summary>
    public List<string> StartupErrors {
        get { return _StartupErrors; }
        set {
            if (!ObjectsAreEqual(_StartupErrors, value)) {
                _StartupErrors = value;
                _ModelUpdated = DateTime.UtcNow;
                NotifyDataChanged();
            }
        }
    }

    /// <summary>
    /// Indicates if the application startup has been validated.
    /// </summary>
    public bool StartupValidated {
        get { return _StartupValidated; }
        set {
            if (_StartupValidated != value) {
                _StartupValidated = value;
                _ModelUpdated = DateTime.UtcNow;
                NotifyDataChanged();
            }
        }
    }

    /// <summary>
    /// The CSS class for the sticky menu based on whether or not the menu is pinned.
    /// </summary>
    public string StickyMenuClass {
        get {
            string output = "sticky-menu-unpinned";

            if (_User.UserPreferences.StickyMenus) {
                output = "sticky-menu-pinned";
            }

            return output;
        }
    }

    /// <summary>
    /// The collection of subscribers to the OnChange event.
    /// </summary>
    public List<string> Subscribers_OnChange {
        get { return _Subscribers_OnChange; }
        set {
            if (!ObjectsAreEqual(_Subscribers_OnChange, value)) {
                _Subscribers_OnChange = value;
                _ModelUpdated = DateTime.UtcNow;
                NotifyDataChanged();
            }
        }
    }

    /// <summary>
    /// The collection of subscribers to the OnDotNetHelperHandler event.
    /// </summary>
    public List<string> Subscribers_OnDotNetHelperHandler {
        get { return _Subscribers_OnDotNetHelperHandler; }
        set {
            if (!ObjectsAreEqual(_Subscribers_OnDotNetHelperHandler, value)) {
                _Subscribers_OnDotNetHelperHandler = value;
                _ModelUpdated = DateTime.UtcNow;
                NotifyDataChanged();
            }
        }
    }

    /// <summary>
    /// The collection of subscribers to the OnSignalRUpdate event.
    /// </summary>
    public List<string> Subscribers_OnSignalRUpdate {
        get { return _Subscribers_OnSignalRUpdate; }
        set {
            if (!ObjectsAreEqual(_Subscribers_OnSignalRUpdate, value)) {
                _Subscribers_OnSignalRUpdate = value;
                _ModelUpdated = DateTime.UtcNow;
                NotifyDataChanged();
            }
        }
    }

    // {{ModuleItemStart:Tags}}
    /// <summary>
    /// The list of Tag objects.
    /// </summary>
    public List<DataObjects.Tag> Tags {
        get { return _Tags; }
        set {
            if (!ObjectsAreEqual(_Tags, value)) {
                _Tags = value;
                _ModelUpdated = DateTime.UtcNow;
                NotifyDataChanged();
            }
        }
    }
    // {{ModuleItemEnd:Tags}}

    /// <summary>
    /// The current Tenant object.
    /// </summary>
    public DataObjects.Tenant Tenant {
        get { return _Tenant; }
        set {
            if (!ObjectsAreEqual(_Tenant, value)) {
                _Tenant = value;
                _ModelUpdated = DateTime.UtcNow;
                NotifyDataChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the TenantCode that was in the URL {TenantCode} parameter.
    /// </summary>
    public string? TenantCodeFromUrl {
        get { return _TenantCodeFromUrl; }
        set {
            if (_TenantCodeFromUrl != value) {
                _TenantCodeFromUrl = value;
                _ModelUpdated = DateTime.UtcNow;
                NotifyDataChanged();
            }
        }
    }

    /// <summary>
    /// The current TenantId.
    /// </summary>
    public Guid TenantId {
        get { return _TenantId; }
        set {
            if (_TenantId != value) {
                _TenantId = value;
                _ModelUpdated = DateTime.UtcNow;
                NotifyDataChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the Tenant List
    /// </summary>
    public List<DataObjects.TenantList> TenantList {
        get { return _TenantList; }
        set {
            if (!ObjectsAreEqual(_TenantList, value)) {
                _TenantList = value;
                _ModelUpdated = DateTime.UtcNow;
                NotifyDataChanged();
            }
        }
    }

    /// <summary>
    /// A collection of all Tenants available to the current user.
    /// </summary>
    public List<DataObjects.Tenant> Tenants {
        get { return _Tenants; }
        set {
            if (!ObjectsAreEqual(_Tenants, value)) {
                _Tenants = value;
                _ModelUpdated = DateTime.UtcNow;
                NotifyDataChanged();
            }
        }
    }

    /// <summary>
    /// The current interface theme ("dark", "light", or an empty string for auto).
    /// </summary>
    public string Theme {
        get { return _Theme; }
        set {
            if (_Theme != value) {
                _Theme = value;
                _ModelUpdated = DateTime.UtcNow;
                NotifyDataChanged();
            }
        }
    }

    /// <summary>
    /// The method used to notify pages of data model updates.
    /// </summary>
    public void TriggerUpdate()
    {
        _ModelUpdated = DateTime.UtcNow;
        NotifyDataChanged();
    }

    /// <summary>
    /// Shows a Toast message stating that an unknown error has occurred.
    /// </summary>
    /// <param name="errorMessage">An optional message to show. If not set then the ErrorUnknown language tag is used.</param>
    /// <param name="AutoHide">If true the message will automatically be hidden after 5 seconds.</param>
    /// <param name="RemovePreviousMessages">If true any previous messages will be removed and only this new message will be shown.</param>
    /// <param name="ReplaceLineBreaks">Option to replace any line breaks in the text with HTML an &lt;br /&gt; element.</param>
    public void UnknownError(string errorMessage = "", bool AutoHide = false, bool RemovePreviousMessages = true, bool ReplaceLineBreaks = false)
    {
        if (String.IsNullOrWhiteSpace(errorMessage)) {
            errorMessage = Helpers.Text("ErrorUnknown");
        } else {
            errorMessage = ReplaceLanguageTagsInString(errorMessage);
        }

        ErrorMessage(errorMessage, AutoHide, RemovePreviousMessages, ReplaceLineBreaks, "");
    }

    /// <summary>
    /// Updates a user's photo.
    /// </summary>
    /// <param name="UserId"></param>
    /// <param name="PhotoId"></param>
    public void UpdateUserPhoto(Guid UserId, Guid PhotoId)
    {
        if (_User.UserId == UserId) {
            _User.Photo = PhotoId;

            var users = _Users.Where(x => x.UserId == UserId);
            if (users != null && users.Any()) {
                foreach (var user in users) {
                    user.Photo = PhotoId;
                }
            }
        }
    }

    /// <summary>
    /// Updates the user preferences related to the current view and URL.
    /// </summary>
    private void UpdateUserViewPreferences()
    {
        if (_User.UserPreferences.LastNavigationId != _NavigationId) {
            _User.UserPreferences.LastNavigationId = _NavigationId;
        }

        var currentUrl = Helpers.CurrentUrl;
        if (_User.UserPreferences.LastUrl != currentUrl) {
            _User.UserPreferences.LastUrl = currentUrl;
        }

        if (_User.UserPreferences.LastView != _View) {
            _User.UserPreferences.LastView = _View;
        }
    }



    /// <summary>
    /// The options for a given User-Defined Field.
    /// </summary>
    /// <param name="module">The name of the module (eg: Users)</param>
    /// <param name="item">The individual item.</param>
    /// <returns>A list of strings containing the options for the field.</returns>
    public List<string> UdfFieldOptions(string? module, int item)
    {
        List<string> output = new List<string>();

        string udf = "UDF" + (item < 10 ? "0" : "") + item.ToString();

        if (!String.IsNullOrWhiteSpace(module) && _udfLabels.Any()) {
            string values = String.Empty;

            var match = _udfLabels.FirstOrDefault(x => x.Module != null && x.Module.ToLower() == module.ToLower() && x.udf != null && x.udf.ToUpper() == udf);

            if (match != null && !String.IsNullOrWhiteSpace(match.Label)) {
                values += match.Label;

                if (values.Contains("|")) {
                    var items = values.Split('|');
                    if (items != null && items.Count() > 2) {
                        values = items[2].Trim();
                    }
                    if (!String.IsNullOrWhiteSpace(values)) {
                        var splitItems = values.Split(",");
                        if (splitItems != null && splitItems.Any()) {
                            foreach (var splitItem in splitItems) {
                                if (!String.IsNullOrWhiteSpace(splitItem)) {
                                    output.Add(splitItem.Trim());
                                }
                            }
                        }
                    }
                }
            }
        }

        return output;
    }

    /// <summary>
    /// Indicates the type of field for a given User-Defined Field.
    /// </summary>
    /// <param name="module">The module (eg: Users)</param>
    /// <param name="item">The individual field item.</param>
    /// <returns>The field type.</returns>
    public string UdfFieldType(string? module, int item)
    {
        string output = String.Empty;

        if (!String.IsNullOrWhiteSpace(module) && _udfLabels.Any()) {
            string udf = "UDF" + (item < 10 ? "0" : "") + item.ToString();

            var match = _udfLabels.FirstOrDefault(x => x.Module != null && x.Module.ToLower() == module.ToLower() && x.udf != null && x.udf.ToUpper() == udf);
            if (match != null && !String.IsNullOrWhiteSpace(match.Label)) {
                string label = match.Label.Trim();

                if (label.Contains("|")) {
                    var items = label.Split("|");
                    if (items != null && items.Count() > 1) {
                        output = items[1].Trim();

                        if (String.IsNullOrWhiteSpace(output)) {
                            output = "input";
                        }
                    }
                }
            }
        }

        return output;
    }

    /// <summary>
    /// Filter options for a given User-Defined Field.
    /// </summary>
    /// <param name="module">The module (eg: Users)</param>
    /// <param name="item">The indiviual field item.</param>
    /// <returns>A list of strings containing the options.</returns>
    public List<string> UdfFilterOptions(string? module, int item)
    {
        List<string> output = new List<string>();

        if (!String.IsNullOrWhiteSpace(module) && _udfLabels.Any()) {
            string udf = "UDF" + (item < 10 ? "0" : "") + item.ToString();

            var match = _udfLabels.FirstOrDefault(x => x.Module != null && x.Module.ToLower() == module.ToLower() && x.udf != null && x.udf.ToUpper() == udf);
            if (match != null && match.FilterOptions.Any()) {
                output = match.FilterOptions;
            }
        }

        return output;
    }

    /// <summary>
    /// The label for an individual User-Defined Field.
    /// </summary>
    /// <param name="module">The module (eg: Users)</param>
    /// <param name="item">The indiviual field item.</param>
    /// <returns>The label for the field.</returns>
    public string UdfLabel(string? module, int item)
    {
        string output = String.Empty;

        if (!String.IsNullOrWhiteSpace(module) && _udfLabels.Any()) {
            string udf = "UDF" + (item < 10 ? "0" : "") + item.ToString();

            var match = _udfLabels.FirstOrDefault(x => x.Module != null && x.Module.ToLower() == module.ToLower() && x.udf != null && x.udf.ToUpper() == udf);
            if (match != null && !String.IsNullOrWhiteSpace(match.Label)) {
                output = match.Label.Trim();
                if (output.Contains("|")) {
                    output = output.Substring(0, output.IndexOf("|"));
                }
            }
        }

        return output;
    }

    /// <summary>
    /// Indicates if a given User-Defined Field should be shown as a column.
    /// </summary>
    /// <param name="module">The module (eg: Users)</param>
    /// <param name="item">The indiviual field item.</param>
    /// <returns>True/False</returns>
    public bool UdfShowColumn(string? module, int item)
    {
        bool output = false;

        if (!String.IsNullOrWhiteSpace(module) && _udfLabels.Any()) {
            string udf = "UDF" + (item < 10 ? "0" : "") + item.ToString();

            var match = _udfLabels.FirstOrDefault(x => x.Module != null && x.Module.ToLower() == module.ToLower() && x.udf != null && x.udf.ToUpper() == udf);
            if (match != null && !String.IsNullOrWhiteSpace(match.Label) && match.ShowColumn) {
                output = true;
            }
        }

        return output;
    }

    /// <summary>
    /// Indicates if a given User-Defined Field is in use.
    /// </summary>
    /// <param name="module">The module (eg: Users)</param>
    /// <param name="item">The indiviual field item.</param>
    /// <returns>True/False</returns>
    public bool UdfShowField(string? module, int item)
    {
        string label = UdfLabel(module, item);

        bool output = !String.IsNullOrWhiteSpace(label);

        return output;
    }

    /// <summary>
    /// Indicates if a given User-Defined Field should be shown as a filter option.
    /// </summary>
    /// <param name="module">The module (eg: Users)</param>
    /// <param name="item">The indiviual field item.</param>
    /// <returns>True/False</returns>
    public bool UdfShowInFilter(string? module, int item)
    {
        bool output = false;

        if (!String.IsNullOrWhiteSpace(module) && _udfLabels.Any()) {
            string udf = "UDF" + (item < 10 ? "0" : "") + item.ToString();

            var match = _udfLabels.FirstOrDefault(x => x.Module != null && x.Module.ToLower() == module.ToLower() && x.udf != null && x.udf.ToUpper() == udf);
            if (match != null && !String.IsNullOrWhiteSpace(match.Label) && match.ShowInFilter) {
                output = true;
            }
        }

        return output;
    }

    /// <summary>
    /// Gets or sets the collection of User-Defined Field labels.
    /// </summary>
    public List<DataObjects.udfLabel> udfLabels {
        get { return _udfLabels; }
        set {
            if (!ObjectsAreEqual(_udfLabels, value)) {
                _udfLabels = value;
                _ModelUpdated = DateTime.UtcNow;
                NotifyDataChanged();
            }
        }
    }

    /// <summary>
    /// Indicates if a Custom Authentication Provider has been configured for the Admin account.
    /// </summary>
    public bool UseCustomAuthenticationProviderFromAdminAccount {
        get { return _UseCustomAuthenticationProviderFromAdminAccount; }
        set {
            if (_UseCustomAuthenticationProviderFromAdminAccount != value) {
                _UseCustomAuthenticationProviderFromAdminAccount = value;
                _ModelUpdated = DateTime.Now;
                NotifyDataChanged();
            }
        }
    }

    /// <summary>
    /// The User object for the current user, or an empty User object if no user is logged in.
    /// </summary>
    public DataObjects.User User {
        get { return _User; }
        set {
            if (!ObjectsAreEqual(_User, value)) {
                _User = value;
                _ModelUpdated = DateTime.UtcNow;
                NotifyDataChanged();
            }
        }
    }

    /// <summary>
    /// The collection of all User Group objects.
    /// </summary>
    public List<DataObjects.UserGroup> UserGroups {
        get { return _UserGroups; }
        set {
            if (!ObjectsAreEqual(_UserGroups, value)) {
                _UserGroups = value;
                _ModelUpdated = DateTime.UtcNow;
                NotifyDataChanged();
            }
        }
    }

    /// <summary>
    /// The collection of all User objects for the current user (as a user may have accounts in more than one tenant).
    /// </summary>
    public List<DataObjects.User> Users {
        get { return _Users; }
        set {
            if (!ObjectsAreEqual(_Users, value)) {
                _Users = value;
                _ModelUpdated = DateTime.UtcNow;
                NotifyDataChanged();
            }
        }
    }

    /// <summary>
    /// Returns a collection of ActiveUser objects for any other admin users that are on the same page and NavigationIdas the current user. 
    /// This will always return a non-nullable list.
    /// This is used in the MainLayout to show a warning message when other users are potentially editing the same record.
    /// Since this is only used for admins, it will always return an empty list for regular users.
    /// </summary>
    public List<DataObjects.ActiveUser> UsersOnSamePage {
        get {
            var output = new List<DataObjects.ActiveUser>();

            if (_User.Admin) {
                if (!String.IsNullOrWhiteSpace(_NavigationId)) {
                    var users = _ActiveUsers
                        .Where(x => x.UserPreferences.LastNavigationId == _NavigationId &&
                            x.UserPreferences.LastView == _View &&
                            x.LastAccess > DateTime.UtcNow.AddMinutes(-1) &&
                            x.UserId != _User.UserId &&
                            x.Admin).ToList();

                    if (users != null && users.Any()) {
                        output = users;
                    }
                }
            }

            return output;
        }
    }

    /// <summary>
    /// Indicates if the app is configured to use tenant codes in the URL
    /// </summary>
    public bool UseTenantCodeInUrl {
        get { return _UseTenantCodeInUrl; }
        set {
            if (_UseTenantCodeInUrl != value) {
                _UseTenantCodeInUrl = value;
                _ModelUpdated = DateTime.UtcNow;
                NotifyDataChanged();
            }
        }
    }

    /// <summary>
    /// The current version of the application.
    /// </summary>
    public string Version {
        get { return _Version; }
        set {
            if (_Version != value) {
                _Version = value;
                _ModelUpdated = DateTime.UtcNow;
                NotifyDataChanged();
            }
        }
    }

    /// <summary>
    /// The current view of the application.
    /// </summary>
    public string View {
        get { return _View; }
        set {
            if (_View != value) {
                _View = value;
                _ModelUpdated = DateTime.UtcNow;
                NotifyDataChanged();

                if (_LoggedIn) {
                    UpdateUserViewPreferences();
                }
            }
        }
    }

    /// <summary>
    /// Indicates if the current view is an edit page.
    /// </summary>
    public bool ViewIsEditPage {
        get { return _ViewIsEditPage; }
        set {
            if (value != _ViewIsEditPage) {
                _ViewIsEditPage = value;
                _ModelUpdated = DateTime.UtcNow;
                NotifyDataChanged();
            }
        }
    }

    /// <summary>
    /// The OnChange event that can be subscribed to in a view or component to be notified when this model changes.
    /// </summary>
    public event Action? OnChange;

    /// <summary>
    /// An event that can be subscribed to for updates from the javascript DotNetHelper.
    /// </summary>
    public event Action<List<string>>? OnDotNetHelperHandler;

    /// <summary>
    /// An event that can be subscribed to for SignalR updates.
    /// </summary>
    public event Action<DataObjects.SignalRUpdate>? OnSignalRUpdate;

    /// <summary>
    /// An event that can be subscribed to for when the tenant change has completed.
    /// </summary>
    public event Action? OnTenantChanged;

    /// <summary>
    /// An event that can be subscribed to when the tenant is being changed.
    /// </summary>
    public event Action? OnTenantChanging;

    private void NotifyDataChanged() => OnChange?.Invoke();

    private void NotifyDotNetHelperHandler() => OnDotNetHelperHandler?.Invoke(_DotNetHelperMessages);

    private void NotifySignalRUpdate(DataObjects.SignalRUpdate update)
    {
        if (OnSignalRUpdate != null) {
            OnSignalRUpdate.Invoke(update);
        }
    }

    /// <summary>
    /// The method called when a tenant has changed.
    /// </summary>
    public void NotifyTenantChanged() => OnTenantChanged?.Invoke();

    /// <summary>
    /// The method called when a tenant is changing.
    /// </summary>
    public void NotifyTenantChanging() => OnTenantChanging?.Invoke();
}