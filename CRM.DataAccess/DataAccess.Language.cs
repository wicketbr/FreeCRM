namespace CRM;

public partial interface IDataAccess
{
    DataObjects.BooleanResponse AddLanguage(Guid TenantId, string CultureCode);
    Task<DataObjects.BooleanResponse> DeleteLanguage(Guid TenantId, string? CultureCode);
    DataObjects.Language GetDefaultLanguage();
    List<DataObjects.OptionPair> GetLanguageCultureCodes();
    Task<List<string>> GetLanguageCultures(Guid TenantId);
    string GetLanguageItem(string? item, DataObjects.Language? language = null);
    Task<List<DataObjects.Language>> GetTenantLanguages(Guid TenantId);
    Task<DataObjects.BooleanResponse> SaveLanguage(Guid TenantId, DataObjects.Language language, DataObjects.User? CurrentUser = null);
}

public partial class DataAccess
{
    public DataObjects.BooleanResponse AddLanguage(Guid TenantId, string CultureCode)
    {
        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();

        var language = GetTenantLanguage(TenantId, CultureCode);
        output.Result = true;

        return output;
    }

    public async Task<DataObjects.BooleanResponse> DeleteLanguage(Guid TenantId, string? CultureCode)
    {
        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();

        if(!String.IsNullOrWhiteSpace(CultureCode)) {
            var rec = await data.Settings.FirstOrDefaultAsync(x => x.TenantId == TenantId && x.SettingName.ToLower() == "language_" + CultureCode.ToLower());
            if(rec != null) {
                try {
                    data.Settings.Remove(rec);
                    await data.SaveChangesAsync();
                    output.Result = true;

                    await SignalRUpdate(new DataObjects.SignalRUpdate { 
                        TenantId = TenantId,
                        UpdateType = DataObjects.SignalRUpdateType.Language,
                        Message = "Deleted",
                        Object = CultureCode,
                    });
                }catch(Exception ex) {
                    output.Messages.Add("Error Deleting Language '" + CultureCode + "'");
                    output.Messages.AddRange(RecurseException(ex));
                }
            } else {
                output.Messages.Add("Language Record '" + CultureCode + "' No Longer Exists");
            }
        } else {
            output.Messages.Add("Missing Culture Code");
        }

        return output;
    }

    public DataObjects.Language GetDefaultLanguage()
    {
        DataObjects.Language output = new DataObjects.Language {
            TenantId = Guid.Empty,
            Culture = "en-US",
            Description = CultureCodeDisplay("en-US"),
            Phrases = new List<DataObjects.OptionPair>()
        };

        Dictionary<string, string> language = new Dictionary<string, string> {
            // Module-Specific Language Tags

            // Active Directory
            { "ActiveDirectoryNames", "Active Directory Names" },
            { "ActiveDirectoryNamesInfo", "Bracket-separated names of AD groups to match for this department (eg: {Enrollment IT]{Admissions}, etc.)" },

            // {{ModuleItemStart:Appointments}}
            // Appointments
            { "Appointment", "Appointment" },
            { "AddAppointment", "Add Appointment" },
            { "AllDay", "All Day" },
            { "AllDayEvent", "All Day Event" },
            { "AppointmentAddService", "Add Service" },
            { "AppointmentAttendanceCodeAccept", "Accept" },
            { "AppointmentAttendanceCodeAccepted", "Accepted" },
            { "AppointmentAttendanceCodeDecline", "Decline" },
            { "AppointmentAttendanceCodeDeclined", "Declined" },
            { "AppointmentAttendanceCodeInvited", "Invited" },
            { "AppointmentAttendanceCodeTentative", "Tentative" },
            { "AppointmentAttendees", "Attendees" },
            { "AppointmentEnd", "End" },
            { "AppointmentFees", "Fees" },
            { "AppointmentNote", "Note" },
            { "AppointmentNoteInfo", "This note is public and visible to any person that can view this appointment." },
            { "AppointmentNoteAdd", "Add a Private Note" },
            { "AppointmentNoteAdded", "Added" },
            { "AppointmentNoteAddedBy", "by" },
            { "AppointmentNoteCreated", "Note Created" },
            { "AppointmentNotes", "Appointment Notes" },
            { "AppointmentNotesInfo", "These notes are private and can only be viewed by users that can manage events." },
            { "Appointments", "Appointments" },
            { "AppointmentServices", "Services" },
            { "AppointmentServicesFull", "Appointment Services" },
            { "AppointmentStart", "Start" },
            { "AppointmentStatus", "Status" },
            { "AppointmentTitle", "Title" },
            { "AppointmentType", "Appointment Type" },
            { "AppointmentTypeMeeting", "Meeting" },
            { "AppointmentTypeEvent", "Event" },
            { "BackgroundColor", "Background Color" },
            { "BackgroundColorAbbreviation", "BG" },
            { "CalendarColor", "Calendar Color" },
            { "CalendarStyle", "Style" },
            { "CanBeScheduled", "User Can Be Scheduled to Take Appointments" },
            { "EditAppointment", "Edit Appointment" },
            { "EditAppointmentEvent", "Edit Event" },
            { "EditAppointmentMeeting", "Edit Meeting" },
            { "ForegroundColor", "Foreground Color" },
            { "ForegroundColorAbbreviation", "FG" },
            { "ManageAppointments", "Manage Appointments" },
            { "NoUsersInvited", "No Users Have Been Invited" },
            { "Schedule", "Schedule" },
            { "ScheduleMore", "+ {0} more" },
            { "ScheduleViewDay", "Day" },
            { "ScheduleViewMonth", "Month" },
            { "ScheduleViewToday", "Today" },
            { "ScheduleViewWeek", "Week" },
            { "ScheduleViewYear", "Year" },
            { "Scheduling", "Scheduling" },
            // {{ModuleItemEnd:Appointments}}

            // Departments
            { "Department", "Department" },
            { "DepartmentGroup", "Department Group" },
            { "DepartmentGroupName", "Department Group Name" },
            { "DepartmentGroups", "Department Groups" },
            { "DepartmentName", "Department Name" },
            { "Departments", "Departments" },
            { "EditDepartment", "Edit Department" },
            { "EditDepartmentGroup", "Edit Department Group" },

            // {{ModuleItemStart:EmailTemplates}}
            // Email Templates
            { "AddNewEmailTemplate", "Add a New Email Template" },
            { "EditEmailTemplate", "Edit Email Template" },
            { "EmailTemplateBody", "Body" },
            { "EmailTemplateFrom", "From Email Address" },
            { "EmailTemplateName", "Name" },
            { "EmailTemplateReplyTo", "Reply-to Email Address" },
            { "EmailTemplates", "Email Templates" },
            { "EmailTemplateSubject", "Subject" },
            // {{ModuleItemEnd:EmailTemplates}}

            // Files
            { "Extension", "Extension" },
            { "Extensions", "Extensions" },

            // {{ModuleItemStart:Invoices}}
            // Invoices
            { "AppointmentInvoice", "Appointment Invoice" },
            { "AppointmentInvoices", "Appointment Invoices" },
            { "CreateInvoice", "Create a New Invoice" },
            { "CreateInvoiceForUser", "Create Invoice for User" },
            { "CreateInvoiceInfo", "Use this interface to create an invoice for a user. If you wish to generate an invoice for an event that can be done from the event interface." },
            { "EditInvoice", "Edit Invoice" },
            { "FeatureOptInInvoices", "Use the Invoices Feature" },
            { "Invoice", "Invoice" },
            { "InvoiceAddItem", "Add Item" },
            { "InvoiceClosed", "Closed" },
            { "InvoiceClosedStatus", "Closed Status" },
            { "InvoiceCreated", "Created" },
            { "InvoiceDue", "Due" },
            { "InvoiceErrorRenderingPreview", "Unable to Render Invoice Preview" },
            { "InvoiceForAppointment", "Invoice for Appointment" },
            { "InvoiceItemDescription", "Description" },
            { "InvoiceItemPrice", "Price" },
            { "InvoiceItemQuantity", "Quantity" },
            { "InvoiceItemTotal", "Total" },
            { "InvoiceItems", "Items" },
            { "InvoiceItemsMissingElements", "Missing at least one invoice item element." },
            { "InvoiceNumber", "Invoice Number" },
            { "InvoicePO", "Purchase Order" },
            { "InvoicePreview", "Invoice Preview" },
            { "Invoices", "Invoices" },
            { "InvoiceSendDate", "Send Date" },
            { "InvoiceSent", "Sent" },
            { "InvoiceSentStatus", "Sent Status" },
            { "InvoiceTitle", "Title" },
            { "InvoiceTotal", "Total" },
            { "SearchInvoices", "Search Invoices" },
            { "ViewInvoice", "View Invoice" },
            // {{ModuleItemEnd:Invoices}}

            // {{ModuleItemStart:Locations}}
            // Locations
            { "AddNewLocation", "Add a New Location" },
            { "Address", "Address" },
            { "City", "City" },
            { "PostalCode", "ZIP/Postal Code" },
            { "State", "State" },
            { "EditLocation", "Edit Location" },
            { "Location", "Location" },
            { "LocationName", "Name" },
            { "Locations", "Locations" },
            { "OverrideLocationColors", "Override Colors" },
            { "OverrideLocationColorsInfo", "This location has colors specified, but you can use this option to override those colors." },
            // {{ModuleItemEnd:Locations}}

            // {{ModuleItemStart:Payments}}
            // Payments
            { "Payment", "Payment" },
            { "Payments", "Payments" },
            // {{ModuleItemEnd:Payments}}

            // Plugins
            { "InvalidPlugin", "Invalid Plugin" },
            { "Plugin", "Plugin" },
            { "PluginNotFound", "The plugin with an Id of {0} could not be found." },
            { "TestPlugin", "Test Plugin" },

            // {{ModuleItemStart:Services}}
            // Services
            { "AddNewService", "Add a New Service" },
            { "EditService", "Edit Service" },
            { "Service", "Service" },
            { "ServiceCode", "Code" },
            { "ServiceDefaultAppointmentDuration", "Default Appointment Duration" },
            { "ServiceDefaultService", "Default Service" },
            { "ServiceDescription", "Description" },
            { "ServiceRate", "Rate" },
            { "Services", "Services" },
            // {{ModuleItemEnd:Services}}

            // {{ModuleItemStart:Tags}}
            // Tags
            { "AddNewTag", "Add a New Tag" },
            { "AddTag", "Add a Tag" },
            { "CurrentTags", "Current Tags" },
            { "EditTag", "Edit Tag" },
            { "ManageTags", "Manage Tags" },
            { "NoTagsSelected", "No Tags Have Been Selected" },
            { "SelectTags", "Select Tags" },
            { "SelectTagsToAdd", "Select Tags to Add" },
            { "Tag", "Tag" },
            { "TagCustomStyle", "Or Write Your Own Custom CSS" },
            { "TagModules", "Modules" },
            { "TagMustBeEnabledForAtLeastOneModule", "A tag must be enabled for at least one module." },
            { "TagName", "Name" },
            { "TagPreview", "Preview" },
            { "Tags", "Tags" },
            { "TagSelectColor", "Select a Color" },
            { "TagStyle", "Style" },
            { "TagStyleInfo", "Enter any HTML style info without the style name (eg: background-color:red; color:#fff;)" },
            { "TagUseInAppointments", "Use in Appointments" },
            { "TagUseInEmailTemplates", "Use in Email Templates" },
            { "TagUseInServices", "Use in Services" },
            // {{ModuleItemEnd:Tags}}

            // UDF Labels
            { "FeatureOptInUdf", "Use the User-Defined Fields" },
            { "UDF", "UDF" },
            { "UserDefinedFields", "User-Defined Fields" },
            { "UserDefinedFieldsHelpIntro", "For a simple text input just enter the field name in the Label field.<br /><br />Alternatively, you can specify that the field should be a select element or a list of radio buttons by using the following format:<br /><br />Label|select|options,separated,by,commas<br />Label|radio|options,separated,by,commas" },
            { "UserDefinedFieldsOptions", "Options:" },
            { "UserDefinedFieldsShowConflictNote", "NOTE: While it is possible to use both Show in Filter and Include in Search for an item, this is not recommended as it is a duplication of searching. If the item is shown as a filter you can quickly find those items by clicking the appropriate filter option. Adding unneccesary fields to the Include in Search can affect performance." },

            // User Groups
            { "AddNewUserGroup", "Add a New User Group" },
            { "FeatureOptInUsergroups", "Use User Groups for Users" },
            { "NewUserGroup", "New User Group" },
            { "UserGroup", "User Group" },
            { "UserGroups", "User Groups" },
            { "UsersInGroup", "Users in Group" },

            // General Language Tags
            { "About", "About" },
            { "AccessDenied", "Access Denied" },
            { "Action", "Action" },
            { "Active", "Active" },
            { "ActiveUsers", "Active Users" },
            { "ActiveItemsOnly", "Active Items Only" },
            { "Add", "Add" },
            { "Added", "Added" },
            { "AddedBy", "by" },
            { "AddLanguage", "Add a Language" },
            { "AddLanguageInfo", "Select a new language to add. The default English (en-US) language elements will be copied into a new language set for the selected language. You can then translate the words and phrases into the desired language." },
            { "AddNewDepartment", "Add a New Department" },
            { "AddNewDepartmentGroup", "Add a New Department Group" },
            { "AddNewTenant", "Add a New Tenant" },
            { "AddNewTenantInfo", "To add a new tenant enter the Tenant Name and Tenant Code for this tenant. When saving a new tenant record some default data will be seeded for things like Categories, Priorities, Resolutions, and Statuses. Also, default settings will be set for this tenant." },
            { "AddNewUser", "Add a New User" },
            { "AddUser", "Add a User" },
            { "AddUsersToGroup", "Add Users to Group" },
            { "Admin", "Admin" },
            { "AdminUsersOnly", "Admin Users Only" },
            { "Ago", "Ago" },
            { "All", "All" },
            { "AllItems", "All Items" },
            { "AllowedFileTypes", "Allowed File Types" },
            { "AllowUsersToManageAvatar", "Allow Users to Manage Avatars" },
            { "AllowUsersToManageBasicProfileInfo", "Allow Users to Manage Basic Profile Info" },
            { "AllowUsersToManageBasicProfileInfoElements", "Select the User Profile Elements Users Can Edit" },
            { "AllowUsersToResetLocalPasswordsAtLogin", "Allow Users to Reset Passwords on the Local Login Form" },
            { "AllowUsersToSignUpAtLogin", "Allow Users to Sign Up for an Account on the Local Login Form" },
            { "ApplicationUrl", "Application URL" },
            { "ApplicationUrlInfo", "Enter an optional override base URL for this tenant." },
            { "AppSettings", "Application Settings" },
            { "AppTitle", _appName },
            { "AppUrl", "Application URL" },
            { "AutoCompleteUserLookup", "User Lookup" },
            { "AutoCompleteUserLookupPlaceholder", "Enter a partial name to find a user" },
            { "AutoRefreshActiveUsers", "Auto-Refresh Active Users" },
            { "Back", "Back" },
            { "BackToLogin", "Back to Login" },
            { "Cancel", "Cancel" },
            { "ChangePassword", "Change Password" },
            { "ChangePasswordInstructions", "To change your password enter your current password, your new password, and confirm your new password." },
            { "Clear", "Clear" },
            { "Close", "Close" },
            { "Code", "Code" },
            { "Confirm", "Confirm" },
            { "ConfirmDelete", "Confirm Delete" },
            { "ConfirmDeleteTenant", "Confirm Delete Tenant" },
            { "ConfirmPassword", "Confirm Password" },
            { "CookieDomain", "CookieDomain" },
            { "Copyright", "Copyright" },
            { "CopyrightName", _copyright },
            { "Created", "Created" },
            { "Current", "Current" },
            { "CurrentPassword", "Current Password" },
            { "CustomLoginProvider", "Custom Login Provider" },
            { "CustomLoginProviderAdminTenantWarning", "When the custom login provider is enabled on the admin tenant it is available and will automatically be displayed as a log in option for all tenants and you will not be able to use a separate custom authentication provider in other tenants." },
            { "Day", "Day" },
            { "DayLabelSunday", "Sunday" },
            { "DayLabelMonday", "Monday" },
            { "DayLabelTuesday", "Tuesday" },
            { "DayLabelWednesday", "Wednesday" },
            { "DayLabelThursday", "Thursday" },
            { "DayLabelFriday", "Friday" },
            { "DayLabelSaturday", "Saturday" },
            { "DayLabelSun", "Sun" },
            { "DayLabelMon", "Mon" },
            { "DayLabelTue", "Tue" },
            { "DayLabelWed", "Wed" },
            { "DayLabelThu", "Thu" },
            { "DayLabelFri", "Fri" },
            { "DayLabelSat", "Sat" },
            { "Days", "Days" },
            { "Default", "Default" },
            { "DefaultCultureCode", "Default Culture Code" },
            { "DefaultReplyToAddress", "Default Reply-to Email Address" },
            { "Defaults", "Defaults" },
            { "DefaultTenantCode", "Default Tenant Code" },
            { "Delete", "Delete" },
            { "DeleteAvatar", "Delete Current Avatar Photo" },
            { "Deleted", "Deleted" },
            { "DeleteAll", "Delete All Records" },
            { "DeletedBy", "by" },
            { "DeletedRecords", "Deleted Records" },
            { "DeletedRecordsInfo", "Your account is configured to mark items as deleted instead of deleting items immediately and you currently have records pending permanent deletion. You can review those items in their individual modules and undelete individual items. You can also delete individual records below. Or, you can also choose to purge all of those deleted items immediately." },
            { "DeletedRecordsPurge", "Purge All Deleted Records Immediately" },
            { "DeletePreferences", "Delete Preferences" },
            { "DeletePreferencesDeleteAfterDays", "Delete After Days" },
            { "DeletePreferencesDeleteAfterDaysInfo", "The number of days after a record is marked as deleted until it will be removed from the database. If this number is zero then records will be marked as deleted but not automtically removed." },
            { "DeletePreferencesImmediate", "Delete Immediately" },
            { "DeletePreferencesInfo", "Choose the option you wish to use when deleting records from the application. If you choose the Delete Immediately option then records will be immediately removed from the database when you delete items. If you choose the Mark As Deleted option then items will be marked as deleted in the database with the Deleted flag. You can then choose an optional value for the Delete After Days setting to have those manually removed after that amount of days has passed, or you can create your own process for removing records." },
            { "DeletePreferencesMarkAsDeleted", "Mark As Deleted" },
            { "DeleteTenant", "Delete Tenant" },
            { "DeleteTenantWarning", "WARNING: Your are about to delete a tenant. There is no 'undo' for this operation. To confirm, type 'CONFIRM' below:" },
            { "Deleting", "Deleting" },
            { "DeletingTenant", "Deleting Tenant, Please Wait..." },
            { "DeletingTenantNotification", "Deleting tenant. Do not close this window until complete." },
            { "DeletingWait", "Deleting, Please Wait..." },
            { "Description", "Description" },
            { "DisabledFeatures", "Disabled Features" },
            { "DisabledFeaturesInfo", "Use this option to disable features that might not be used in all tenants. By selecting items below you can hide these features from the application interface." },
            { "DisabledUsersOnly", "Disabled Users Only" },
            { "DownloadPDF", "Download PDF" },
            { "Edit", "Edit" },
            { "EditAll", "Edit All" },
            { "EditHTML", "Edit HTML" },
            { "EditItem", "Edit Item" },
            { "EditLanguage", "Edit Language" },
            { "EditLanguageInfo", "To edit a language item use the lookup field below to find the text you wish to edit. Warning: clicking the Edit All option takes a while to load and is very resource heavy, as there are a lot of language items to load into the editing interface." },
            { "EditNotes", "Edit Notes" },
            { "EditTenant", "Edit Tenant" },
            { "EditUser", "Edit User" },
            { "EditUserGroup", "Edit User Group" },
            { "Email", "Email" },
            { "EmailAddress", "Email Address" },
            { "EmployeeId", "Employee ID" },
            { "Enabled", "Enabled" },
            { "EnabledItemsOnly", "Enabled Items Only" },
            { "EnabledUsersOnly", "Enabled Users Only" },
            { "EncryptionKey", "Encryption Key" },
            { "EncryptionKeyWarning", "<strong>WARNING</strong><br />Modifying the Encryption Key is very dangerous. This is the key used by the application to encrypt all sensitive data.<br /><br />When changing this value you must supply a valid 32-bit key represeted as a byte array string (eg: 0x00,0x01,0x02,etc.).<br /><br />When this value is changed the software will attempt to decrypt all values in the database, the key will be updated, then those values will be re-encrypted. If this fails you will be left with encrypted data that cannot be accessed." },
            { "EndDate", "End Date" },
            { "ErrorOccurred", "The following error has occurred:" },
            { "ErrorsOccurred", "The following errors have occurred:" },
            { "ErrorUnknown", "An unknown error has occurred. Please check the console for any error messages." },
            { "FailedLoginAttempts", "Failed Login Attempts" },
            { "FeatureOptIn", "Optional Features" },
            { "FeatureOptInInfo", "The following feature have been enabled for your account and you can choose which of these you want to use in the application." },
            { "FeatureOptInDepartments", "Use Departments for Users" },
            { "FeatureOptInEmailTemplates", "Use the Email Templates Feature" },
            { "FeatureOptInEmployeeid", "Use the Employee Id Field for Users" },
            { "FeatureOptInFiles", "Use the Files Feature" },
            { "FeatureOptInLocation", "Use the Location Field for Users" },
            { "FeatureOptInScheduling", "Use the Scheduling Module" },
            { "FeatureOptInServices", "Use the Services Feature" },
            { "FeatureOptInTags", "Use the Tags Feature" },
            { "FeatureOptInThemes", "Themes" },
            { "FeatureOptInWorkSchedule", "Work Schedule Hours" },
            { "File", "File" },
            { "FileName", "File Name" },
            { "Files", "Files" },
            { "FileSize", "Size" },
            { "FileUploaded", "Uploaded" },
            { "FilterOptionClear", "--- Clear ---" },
            { "FilterEnd", "End" },
            { "FilterStart", "Start" },
            { "FirstName", "First Name" },
            { "FirstRecord", "First Record" },
            { "ForgotPassword", "Forgot Password" },
            { "ForgotPasswordInfo", "Enter your email address and a new password below. If your email address exists in the application an email will be sent to your email address with a validation code." },
            { "ForgotPasswordValidateInstructions", "To validate your new password, enter the code that was emailed to the address you provided." },
            { "ForUser", "for User" },
            { "From", "From" },
            { "GenerateNewPassword", "Generate a New Password" },
            { "GeneratePassword", "New Password" },
            { "GeneratePasswordLength", "Password Length" },
            { "GeneratePasswordRequireLowercase", "Lowercase" },
            { "GeneratePasswordRequirements", "Password Character Requirements" },
            { "GeneratePasswordRequireNumbers", "Numbers" },
            { "GeneratePasswordRequireSpecialCharacters", "Symbols" },
            { "GeneratePasswordRequireUppercase", "Uppercase" },
            { "GraphAllowSendingFromIndividualEmailAddresses", "Allow Sending from Individual Email Addresses" },
            { "GraphAllowSendingFromIndividualEmailAddressesInfo", "If set emails will be sent from the current user when possible. Otherwise, if not set emails will always come from the default From address. Also, this can affect how the from address and reply-to address in email templates are used. If the mail server does not support sending from other addresses then the specified from address will be used as a reply-to address and the from address will be set back to this default, regardless of tenant settings or email template settings." },
            { "GraphClientId", "Client ID" },
            { "GraphClientSecret", "Client Secret" },
            { "GraphTenantId", "Tenant ID" },
            { "Help", "Help" },
            { "Hide", "Hide" },
            { "HideAbout", "Hide the About Page" },
            { "HideHelp", "Hide Help" },
            { "HideFilter", "Hide Filter" },
            { "HomeMenuText", "Home" },
            { "HomePage", "Home Page" },
            { "Hour", "Hour" },
            { "Hours", "Hours" },
            { "HtmlEditorInSourceViewWarning", "The HTML editor is in Source mode. In order to save your changes you will need to switch back to the WYSIWYG mode." },
            { "HtmlEditorPlaceholder", "Enter Your HTML Here" },
            { "IncludeDeletedItems", "Include Deleted Items" },
            { "IncludeDeletedRecords", "Include Deleted Records" },
            { "IncludeInSearch", "Include in Search" },
            { "IncludeInSearchInfo", "If this option is selected this field will be included when using the keyword search." },
            { "IndicatesRequiredField", "Indicates a Required Field" },
            { "Info", "Info" },
            { "InsertField", "Insert a Field" },
            { "InsertFieldAppointmentDatesAndTimes", "Dates and Times" },
            { "InsertFieldAppointmentEnd", "End" },
            { "InsertFieldAppointmentLocation", "Location" },
            { "InsertFieldAppointmentNote", "Note" },
            { "InsertFieldAppointmentStart", "Start" },
            { "InsertFieldAppointmentTitle", "Title" },
            { "InsertFieldEmployeeId", "Employee ID" },
            { "InsertFieldFirstName", "First Name" },
            { "InsertFieldGroupGeneral", "General Items" },
            { "InsertFieldGroupScheduling", "Scheduling Items" },
            { "InsertFieldGroupServices", "Service Items" },
            { "InsertFieldInfo", "You can insert fields into your email template that will be replaced when sent to a customer. Some fields are specific to sending an email about a certain type of item." },
            { "InsertFieldLastName", "Last Name" },
            { "InsertFieldEmail", "Email Address" },
            { "InsertFieldServiceCode", "Code" },
            { "InsertFieldServiceDescription", "Description" },
            { "InsertFieldServiceRate", "Rate" },
            { "InsertImage", "Insert an Image" },
            { "InvalidImageFileType", "Invalid Image File Type" },
            { "InvalidLogin", "Invalid Login" },
            { "InvalidLoginNoLocalAccount", "Your credentials were valid, but you do not have an account configured in this application." },
            { "InvalidTenantCode", "Invalid Tenant Code" },
            { "InvalidTenantCodeMessage", "Please check your URL and ensure a valid URL is used." },
            { "InvalidUserId", "Invalid UserId" },
            { "InvalidUsernameOrPassword", "Invalid Username or Password" },
            { "Item", "Item" },
            { "JwtRsaPrivateKey", "JSON Web Token RSA Private Key" },
            { "JwtRsaPrivateKeyInfo", "The private RSA key in the PEM format used to encrypt JSON Web Tokens." },
            { "JwtRsaPublicKey", "JSON Web Token RSA Public Key" },
            { "JwtRsaPublicKeyInfo", "The public RSA key in the PEM format used to encrypt JSON Web Tokens." },
            { "Keyword", "Keyword" },
            { "Label", "Label" },
            { "Language", "Language" },
            { "LastAccess", "Last Access" },
            { "LastModified", "Last Modified" },
            { "LastLogin", "Last Login" },
            { "LastName", "Last Name" },
            { "LastRecord", "Last Record" },
            { "Loading", "Loading" },
            { "LoadingWait", "Loading, Please Wait..." },
            { "LoggingOutWait", "Logging Out, Please Wait..." },
            { "Login", "Login" },
            { "LoginIntro", "Select a login option below:" },
            { "LoginOptions", "Login Options" },
            { "LoginRequired", "Login Required" },
            { "LoginTextPassword", "Password" },
            { "LoginTextTenantCode", "Tenant Code" },
            { "LoginTextUsername", "Username" },
            { "LoginTitle", "Login Required" },
            { "LoginTypeApple", "Apple" },
            { "LoginTypeAppleProviderInfo", "Apple Authentication Provider Information" },
            { "LoginTypeAppleProviderInfoDetails", "When using the Apple login option you must set the Apple ClientId, KeyId, and TeamId in the AuthenticationProviders:Apple section of the appsettings.json file. Also, your private key file must exist at the root of the web server and must be named &ldquo;SignInWithAppleKey.p8&rdquo;." },
            { "LoginTypeCustom", "Custom Provider" },
            { "LoginTypeCustomAuthenticationButtonClass", "Custom Authentication Button Class" },
            { "LoginTypeCustomAuthenticationButtonClassInfo", "An optional class that will be applied to the log in button." },
            { "LoginTypeCustomAuthenticationButtonCSharpCode", "Custom Authentication C# Code" },
            { "LoginTypeCustomAuthenticationIcon", "Custom Authentication Icon" },
            { "LoginTypeCustomAuthenticationIconInfo", "An optional icon that will be displayed on the log in button." },
            { "LoginTypeCustomAuthenticationName", "Custom Authentication Name" },
            { "LoginTypeCustomAuthenticationNameInfo", "The text that will be displayed on the log in button." },
            { "LoginTypeCustomInfo", "Custom Authentication Provider Information" },
            { "LoginTypeFacebook", "Facebook" },
            { "LoginTypeFacebookProviderInfo", "Facebook Authentication Provider Information" },
            { "LoginTypeFacebookProviderInfoDetails", "When using the Facebook login option you must set the Facebook AppId and AppSecret in the AuthenticationProviders:Facebook section of the appsettings.json file." },
            { "LoginTypeGoogle", "Google" },
            { "LoginTypeGoogleProviderInfo", "Google Authentication Provider Information" },
            { "LoginTypeGoogleProviderInfoDetails", "When using the Google login option you must set the ClientId and ClientSecret in the AuthenticationProviders:Google section of the appsettings.json file." },
            { "LoginTypeLocal", "Local Login" },
            { "LoginTypeMicrosoftAccount", "Microsoft Account" },
            { "LoginTypeMicrosoftAccountProviderInfo", "Microsoft Account Authentication Provider Information" },
            { "LoginTypeMicrosoftAccountProviderInfoDetails", "When using the Microsoft Account login option you must set the ClientId and ClientSecret in the AuthenticationProviders:MicrosoftAccount section of the appsettings.json file." },
            { "LoginTypeOpenId", "OpenId" },
            { "LoginTypeOpenIdProviderInfo", "OpenId Authentication Provider Information" },
            { "LoginTypeOpenIdProviderInfoDetails", "When using the OpenId login option you must set the ClientId, ClientSecret, and Authority settings in the AuthenticationProviders:OpenId section of the appsettings.json file." },
            { "LoginSuccessMessage", "Logged In, Please Wait..." },
            { "LoginWithEITSSO", "Login with OKTA Single Sign-On" },
            { "LoginWithApple", "Sign in with Apple" },
            { "LoginWithFacebook", "Log in with Facebook" },
            { "LoginWithGoogle", "Log in with Google" },
            { "LoginWithLocalAccount", "Log in with a Local Account" },
            { "LoginWithMicrosoftAccount", "Log in with a Microsoft Account" },
            { "LoginWithOpenId", "Log in with OpenId" },
            { "Log-in", "Log In" },
            { "Logo", "Logo" },
            { "LogoIncludedOnHomePage", "Include the Logo on the home page." },
            { "LogoIncludedOnNavbar", "Include the Logo on the top navigation menu." },
            { "LogoInfo", "Ideally, your logo should be in the SVG format for best scaling. If you do not have an SVG version then a transparent PNG would be the next-best option." },
            { "Logout", "Logout" },
            { "Log-out", "Log Out" },
            { "MailServer", "Mail Server" },
            { "MailServerConfiguration", "Mail Server Configuration" },
            { "MailServerOption", "Mail Server Option" },
            { "MailServerPassword", "Mail Server Password" },
            { "MailServerPort", "Mail Server Port" },
            { "MailServerUsername", "Mail Server Username" },
            { "MailServerUsesSSL", "Mail Server Uses SSL" },
            { "MaintenanceMode", "Maintenance Mode" },
            { "MaintenanceModeAppAdminMessage", "The application is currently in maintenance mode and can only be used by application admin users like you. All other users are currently seeing a message indicating the application is in maintenance mode." },
            { "MaintenanceModeInfo", "When the application is in maintenance mode only application admin users can interact with the application. All other users will see the maintenance mode message." },
            { "MaintenanceModeMessage", "The application is currently offline for maintenance." },
            { "Manage", "Manage" },
            { "ManageAvatar", "Manage Your Avatar" },
            { "ManageAvatarInfo", "Adding or removing a user photo does not require saving the user record." },
            { "ManageAvatarInstructions", "Drag and drop an image here to upload a user image.<br />Or, click on this area to bring up the file selection dialog.<br />Images are limited to a maximum of 5MB." },
            { "ManageAvatarInstructionsAdmin", "Drag and drop an image here to upload a user image. Images are limited to a maximum of 5MB. There is no need to save after uploading the user photo, as the photo is saved automatically." },
            { "ManageFile", "Manage File Details" },
            { "ManageFiles", "Manage Files" },
            { "ManageProfile", "Manage Profile" },
            { "ManageProfileInfo", "Manage Basic Profile Info" },
            { "ManageProfileInfoInstructions", "You can make some basic profile updates below." },
            { "MarkedAsDeleted", "Marked as Deleted" },
            { "Message", "Message" },
            { "Messages", "Messages" },
            { "Minute", "Minute" },
            { "Minutes", "Minutes" },
            { "MissingTenantCode", "Missing Tenant Code" },
            { "MissingTenantCodeInfo", "A tenant code is required in the URL to access this application." },
            { "Modified", "Modified" },
            { "ModifiedItems", "Modified Items" },
            { "MostRecentFirst", "most-recent items listed first" },
            { "MultiTenantUser", "Multi-Tenant User" },
            { "Name", "Name" },
            { "New", "New" },
            { "NewPassword", "New Password" },
            { "NewPasswordAndConfirmDontMatch", "New Password and Confirm Password Don't Match" },
            { "NewTenant", "New Tenant" },
            { "NewUserCreated", "New User Created" },
            { "NextRecord", "Next Record" },
            { "NoItemsToShow", "There are no items to show." },
            { "NonAdminUsersOnly", "Non-Admin Users Only" },
            { "NoPendingDeletedRecords", "No Pending Deleted Records" },
            { "NoResult", "No Result" },
            { "Note", "Note" },
            { "Notes", "Notes" },
            { "Now", "Now" },
            { "Objects", "Objects" },
            { "ObjectsWrittenToConsole", "Objects Written to Console" },
            { "Of", "of" },
            { "Ok", "Ok" },
            { "Option", "Option" },
            { "Password", "Password" },
            { "PasswordChanged", "Password Changed" },
            { "PasswordReset", "Password Reset" },
            { "PasswordResetMessage", "Your password has been updated. You may now log in using your new password." },
            { "PDFViewer", "PDF Viewer" },
            { "PendingDeletedRecords", "Pending Deleted Records" },
            { "PermanentlyDelete", "Permanently Delete" },
            { "PhoneNumber", "Phone Number" },
            { "Photo", "Photo" },
            { "PreventPasswordChange", "Prevent User from Changing Password" },
            { "Preview", "Preview" },
            { "PreviousRecord", "Previous Record" },
            { "ProcessingLoginWait", "Processing Login, Please Wait..." },
            { "ProcessingWait", "Processing, Please Wait..." },
            { "Record", "Record" },
            { "RecordDeleted", "Record Deleted" },
            { "RecordDeletedBy", "Record Deleted by" },
            { "RecordMarkedAsDeletedInfo", "This record has been marked as deleted. Although these records are hidden in the application from regular users, as an Admin user you can still view these records and can choose to undelete this record." },
            { "RecordNavigationFirst", "Go to First Page of Records" },
            { "RecordNavigationLast", "Go to Last Page of Records" },
            { "RecordNavigationNext", "Go to Next Page of Records" },
            { "RecordNavigationPrevious", "Go to Previous Page of Records" },
            { "Records", "Records" },
            { "RecordsPerPage", "Records Per Page" },
            { "RecordUpdated", "Record Updated" },
            { "RecordUpdatedBy", "Record Updated by" },
            { "RedirectingToLogin", "Redirecting to Login" },
            { "Refresh", "Refresh" },
            { "Remove", "Remove" },
            { "RemoveFile", "Remove File" },
            { "ReplaceFile", "Replace File" },
            { "RequiredMissing", "{0} is Required" },
            { "RequirePreExistingAccount", "Require Pre-Existing Account to Log In" },
            { "RequirePreExistingAccountInfo", "If this is set to true then a user cannot login unless a user account already exists in the database. For applications that should allow any user to log in set this to false and a new user account will be created when they log in if there is no user account already in the users table." },
            { "Reset", "Reset" },
            { "ResetDefault", "Reset Default" },
            { "ResetLanguageDefaults", "Reset All Language to Defaults" },
            { "ResetPassword", "Reset Password" },
            { "ResetUserPassword", "Reset User Password" },
            { "Result", "Result" },
            { "Save", "Save" },
            { "Saved", "Saved" },
            { "SavedAt", "Saved at" },
            { "SaveItemChanges", "Save Item Changes" },
            { "Saving", "Saving" },
            { "SavingWait", "Saving, Please Wait..." },
            { "Second", "Second" },
            { "Seconds", "Seconds" },
            { "Search", "Search" },
            { "SearchLanguageTag", "Search for a Language Tag to Edit" },
            { "SearchUsers", "Search Users" },
            { "Select", "Select" },
            { "SelectCulture", "Select a Language Culture" },
            { "SelectFile", "Select a File" },
            { "SelectImageFile", "Select an Image File" },
            { "SelectTenant", "Select a Tenant" },
            { "SelectTenantInfo", "To log in you must first select the tenant account you wish to log in to." },
            { "SelectTheme", "Select Theme" },
            { "SendingWait", "Sending, Please Wait..." },
            { "SendTestEmail", "Send a Test Message" },
            { "Sent", "Sent" },
            { "Settings", "Settings" },
            { "SettingsAuthentication", "Authentication" },
            { "SettingsEmail", "Email" },
            { "SettingsGeneral", "General" },
            { "SettingsOptionalFeatures", "Optional Features" },
            { "SettingsTheme", "Theme" },
            { "SettingsWorkSchedule", "Work Schedule" },
            { "ServerOffline", "Server Offline, Attempting to Reconnect..." },
            { "ServerUpdated", "Server Updated" },
            { "ServerUpdatedMessage", "The server has been updated, refreshing the application..." },
            { "ShowColumn", "Show Column" },
            { "ShowColumnInfo", "Indicates if this field should be show when listing records. Any UDF with a value in the Label field will be shown when editing a record. The Show Column is only used to toggle this column on and off in the records view." },
            { "ShowFilter", "Show Filter" },
            { "ShowHelp", "Show Help" },
            { "ShowInFilter", "Show in Filter" },
            { "ShowInFilterInfo", "If this option is selected this field will be shown in the filter options as a select list. Only use this option on fields with limited values, as every distinct value from the records will be used to create this list." },
            { "Showing", "Showing" },
            { "ShowingAllRecords", "Showing All {0} Records" },
            { "ShowModifiedItemsOnly", "Show Modified Items Only" },
            { "ShowTenantListingWhenMissingTenantCode", "Show Tenant Listing When Missing Tenant Code" },
            { "SignUp", "Sign Up" },
            { "SignUpInstructions", "Complete all of the following fields to sign up for an account. A confirmation code will be emailed to the address you provide to confirm that it is a valid email address. You will then enter that validation code to finish the sign up process." },
            { "SignUpValidateInstructions", "To validate your new account, enter the code that was emailed to the address you provided." },
            { "Sort", "Sort" },
            { "SortOrder", "Sort Order" },
            { "Source", "Source" },
            { "StartDate", "Start Date" },
            { "SupportedFileTypes", "Supported File Types" },
            { "SwitchAccountMessage", "You have access to multiple app instances. You can switch to another instance below:" },
            { "SwitchTenant", "Switch Tenants" },
            { "Tenant", "Tenant" },
            { "TenantCode", "Code" },
            { "TenantCodes", "Tenant Codes" },
            { "TenantId", "TenantId" },
            { "TenantName", "Name" },
            { "Tenants", "Tenants" },
            { "TestCode", "Test Code" },
            { "Theme", "Theme" },
            { "ThemeAuto", "Auto (based on system settings)" },
            { "ThemeBlue", "Blue" },
            { "ThemeCustom", "Custom Theme" },
            { "ThemeCustomCssDefault", "Default CSS" },
            { "ThemeCustomInfo", "Specify your own CSS below to create your custom theme." },
            { "ThemeCyan", "Cyan" },
            { "ThemeDark", "Dark" },
            { "ThemeFont", "Theme Font" },
            { "ThemeFontInfo", "In addition to the standard named web fonts (eg: sans-serif, serif, Arial, etc.) you can specify one of the Google fonts that are already referenced by the application. Those fonts are Roboto, Open Sans, Merriweather, Lora, and PT Sans Narrow. If you wish to use another web font you can enter the entire \"@import()\" statement in the Font CSS Import setting to have that made available." },
            { "ThemeFontCssImport", "Font CSS Import" },
            { "ThemeGray", "Gray" },
            { "ThemeGreen", "Green" },
            { "ThemeIndigo", "Indigo" },
            { "ThemeInfo", "Use these settings to customize the look and feel of the app for your customers. If you leave the Theme setting set to 'Auto' then the standard Light and Dark themes will be available for your customers and the app will adjust dynamically based on their system preferences, or they can force the Light or Dark theme. If you specify a theme then the user will not have the ability to modify the theme and will only get the theme you specify. The colors for these themes are based on the available Bootstrap colors." },
            { "ThemeLight", "Light" },
            { "ThemeOrange", "Orange" },
            { "ThemePink", "Pink" },
            { "ThemePurple", "Purple" },
            { "ThemeRed", "Red" },
            { "ThemeTeal", "Teal" },
            { "ThemeYellow", "Yellow" },
            { "Title", "Title" },
            { "ToggleStickyMenus", "Toggle Sticky Menus" },
            { "Total", "Total" },
            { "Undelete", "Undelete" },
            { "UndeleteRecord", "Undelete Record" },
            { "UnlockUserAccount", "Unlock User Account" },
            { "UpdateAllPasswords", "Update All Passwords" },
            { "UpdateAllPasswordsUserInfo", "You have accounts in multiple tenants. Use this option to update all of your passwords at once." },
            { "UpdateAllPasswordsInfo", "Use this option to reset the password for all of this user's accounts." },
            { "UpdatePassword", "Update Password" },
            { "UpdatingWait", "Updating, Please Wait..." },
            { "Uploaded", "Uploaded" },
            { "UploadFile", "Upload a File" },
            { "UploadFileInstructions", "Drag and drop a file here to upload.<br />Or, click on this area to bring up the file selection dialog." },
            { "UploadFilesInstructions", "Drag and drop one or more files here to upload.<br />Or, click on this area to bring up the file selection dialog" },
            { "UploadFiles", "Upload Files" },
            { "UploadingWait", "Uploading, Please Wait..." },
            { "UserLockedOut", "User account was locked out at {0} due to too many failed login attempts." },
            { "User", "User" },
            { "Username", "Username" },
            { "Users", "Users" },
            { "UserScheduleInfo", "You are scheduled for this event and can update your attendance below." },
            { "UsersOnSamePage", "WARNING: Possible Edit Conflict" },
            { "UsersOnSamePageMessage", "One or more users are editing this same record." },
            { "UseTenantCodeInUrl", "Use Tenant Code in URL" },
            { "ValidateConfirmationCode", "Validate Confirmation Code" },
            { "ValidatingConfirmationCode", "Validating Confirmation Code, Please Wait..." },
            { "ValidationCompleteReturnToLogin", "Your account has been activated. You may now log in." },
            { "ValidatingLogin", "Validating Login, Please Wait..." },
            { "ValidationCode", "Validation Code" },
            { "View", "View" },
            { "ViewFile", "View File" },
            { "ViewImage", "View Image" },
            { "Welcome", "Welcome" },
            { "WillBeDeleted", "Will Be Deleted" },
            { "WorkSchedule", "Work Schedule" },
            { "WorkScheduleAllDay", "All Day" },
            { "WorkScheduleDay", "Day" },
            { "WorkScheduleEnd", "End" },
            { "WorkScheduleStart", "Start" },
        };

        // First, add any items from the app language.
        foreach(var item in AppLanguage) {
            output.Phrases.Add(new DataObjects.OptionPair {
                Id = item.Key,
                Value = item.Value
            });
        }

        // Next, add any items from the default language.
        foreach (var item in language) {
            if (!output.Phrases.Any(x => x.Id != null && x.Id.ToLower() == item.Key.ToLower())) {
                output.Phrases.Add(new DataObjects.OptionPair {
                    Id = item.Key,
                    Value = item.Value
                });
            }
        }

        return output;
    }

    public List<DataObjects.OptionPair> GetLanguageCultureCodes()
    {
        List<DataObjects.OptionPair> output = new List<DataObjects.OptionPair>();

        var ci = System.Globalization.CultureInfo.GetCultures(System.Globalization.CultureTypes.AllCultures);
        if (ci != null && ci.Any()) {
            foreach (var c in ci.Where(x => !String.IsNullOrEmpty(x.Name) && !x.IsNeutralCulture)
                .OrderBy(x => x.DisplayName)) {
                output.Add(new DataObjects.OptionPair {
                    Id = c.Name,
                    Value = c.DisplayName
                });
            }
        }

        return output;
    }

    public async Task<List<string>> GetLanguageCultures(Guid TenantId)
    {
        List<string> output = new List<string>();

        try {
            var recs = await data.Settings
                .Where(x => x.TenantId == TenantId && x.SettingName != null)
                .ToListAsync();

            if (recs != null && recs.Any(x => x.SettingName.ToLower().StartsWith("language_"))) {
                foreach (var rec in recs.Where(x => x.SettingName.ToLower().StartsWith("language_"))) {
                    string culture = StringValue(rec.SettingName).Substring(9);
                    if (!String.IsNullOrWhiteSpace(culture)) {
                        output.Add(culture);
                    }
                }
            }
        } catch {}

        return output;
    }

    public string GetLanguageItem(string? item, DataObjects.Language? language = null)
    {
        string output = String.Empty;

        if (language == null) {
            language = GetDefaultLanguage();
        }

        if (language.Phrases == null || !language.Phrases.Any()) {
            language = GetDefaultLanguage();
        }

        if (language != null && language.Phrases != null && language.Phrases.Any() && !String.IsNullOrEmpty(item)) {
            var phrase = language.Phrases.FirstOrDefault(x => x.Id != null && x.Id.ToLower() == item.ToLower());
            if (phrase != null) {
                output += phrase.Value;
            }
        }

        if (String.IsNullOrEmpty(output) && !String.IsNullOrEmpty(item)) {
            output = item.ToUpper();
        }

        return output;
    }

    public async Task<List<DataObjects.Language>> GetTenantLanguages(Guid TenantId)
    {
        List<DataObjects.Language> output = new List<DataObjects.Language>();

        var cultures = await GetLanguageCultures(TenantId);

        if(!cultures.Any()) {
            cultures.Add("en-US");
        }

        foreach(var culture in cultures) {
            var language = GetTenantLanguage(TenantId, culture);
            output.Add(language);
        }

        return output;
    }

    public async Task<DataObjects.BooleanResponse> SaveLanguage(Guid TenantId, DataObjects.Language language, DataObjects.User? CurrentUser = null)
    {
        if (String.IsNullOrWhiteSpace(language.Culture)) {
            language.Culture = "en-US";
        }

        var output = SaveSetting("Language_" + language.Culture, DataObjects.SettingType.Object, language.Phrases, TenantId, null, "", CurrentUser);

        await SignalRUpdate(new DataObjects.SignalRUpdate { 
            TenantId = TenantId,
            UpdateType = DataObjects.SignalRUpdateType.Language,
            Message = "Saved",
            Object = language.Culture,
        });

        return output;
    }
}