namespace CRM;

public partial class DataObjects
{
    public partial class SignalRUpdateType
    {
        // {{ModuleItemStart:Appointments}}
        public const string Appointment = "Appointment";
        public const string AppointmentNote = "AppointmentNote";
        public const string AppointmentService = "AppointmentService";
        // {{ModuleItemEnd:Appointments}}
        public const string Department = "Department";
        public const string DepartmentGroup = "DepartmentGroup";
        // {{ModuleItemStart:EmailTemplates}}
        public const string EmailTemplate = "EmailTemplate";
        // {{ModuleItemEnd:EmailTemplates}}
        public const string File = "File";
        // {{ModuleItemStart:Invoices}}
        public const string Invoice = "Invoice";
        // {{ModuleItemEnd:Invoices}}
        public const string Language = "Language";
        public const string LastAccessTime = "LastAccessTime";
        // {{ModuleItemStart:Locations}}
        public const string Location = "Location";
        // {{ModuleItemEnd:Locations}}
        // {{ModuleItemStart:Payments}}
        public const string Payment = "Payment";
        // {{ModuleItemEnd:Payments}}
        // {{ModuleItemStart:Services}}
        public const string Service = "Service";
        // {{ModuleItemEnd:Services}}
        public const string Setting = "Setting";
        // {{ModuleItemStart:Tags}}
        public const string Tag = "Tag";
        // {{ModuleItemEnd:Tags}}
        public const string Tenant = "Tenant";
        public const string UDF = "UDF";
        public const string Undelete = "Undelete";
        public const string Unknown = "Unknown";
        public const string User = "User";
        public const string UserAttendance = "UserAttendance";
        public const string UserGroup = "UserGroup";
        public const string UserPreferences = "UserPreferences";
    }

    //public enum SignalRUpdateType
    //{
    //    // {{ModuleItemStart:Appointments}}
    //    Appointment,
    //    AppointmentNote,
    //    AppointmentService,
    //    // {{ModuleItemEnd:Appointments}}
    //    Department,
    //    DepartmentGroup,
    //    // {{ModuleItemStart:EmailTemplates}}
    //    EmailTemplate,
    //    // {{ModuleItemEnd:EmailTemplates}}
    //    File,
    //    // {{ModuleItemStart:Invoices}}
    //    Invoice,
    //    // {{ModuleItemEnd:Invoices}}
    //    Language,
    //    LastAccessTime,
    //    // {{ModuleItemStart:Locations}}
    //    Location,
    //    // {{ModuleItemEnd:Locations}}
    //    // {{ModuleItemStart:Payments}}
    //    Payment,
    //    // {{ModuleItemEnd:Payments}}
    //    // {{ModuleItemStart:Services}}
    //    Service,
    //    // {{ModuleItemEnd:Services}}
    //    Setting,
    //    // {{ModuleItemStart:Tags}}
    //    Tag,
    //    // {{ModuleItemEnd:Tags}}
    //    Tenant,
    //    UDF,
    //    Undelete,
    //    Unknown,
    //    User,
    //    UserAttendance,
    //    UserGroup,
    //    UserPreferences,
    //}

    public partial class SignalRUpdate
    {
        public Guid? TenantId { get; set; }
        public Guid? ItemId { get; set; }
        public Guid? UserId { get; set; }
        public string? UserDisplayName { get; set; }
        //public SignalRUpdateType UpdateType { get; set; }
        public string UpdateType { get; set; } = "Unknown";
        public string Message { get; set; } = "";
        public object? Object { get; set; }
        public string? ObjectAsString { get; set; }
    }
}