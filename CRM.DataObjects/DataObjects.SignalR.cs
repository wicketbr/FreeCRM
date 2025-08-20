namespace CRM;

public partial class DataObjects
{
    public enum SignalRUpdateType
    {
        // {{ModuleItemStart:Appointments}}
        Appointment,
        AppointmentNote,
        AppointmentService,
        // {{ModuleItemEnd:Appointments}}
        Department,
        DepartmentGroup,
        // {{ModuleItemStart:EmailTemplates}}
        EmailTemplate,
        // {{ModuleItemEnd:EmailTemplates}}
        File,
        // {{ModuleItemStart:Invoices}}
        Invoice,
        // {{ModuleItemEnd:Invoices}}
        Language,
        LastAccessTime,
        // {{ModuleItemStart:Locations}}
        Location,
        // {{ModuleItemEnd:Locations}}
        // {{ModuleItemStart:Payments}}
        Payment,
        // {{ModuleItemEnd:Payments}}
        // {{ModuleItemStart:Services}}
        Service,
        // {{ModuleItemEnd:Services}}
        Setting,
        // {{ModuleItemStart:Tags}}
        Tag,
        // {{ModuleItemEnd:Tags}}
        Tenant,
        UDF,
        Undelete,
        Unknown,
        User,
        UserAttendance,
        UserGroup,
        UserPreferences,
    }

    public partial class SignalRUpdate
    {
        public Guid? TenantId { get; set; }
        public Guid? ItemId { get; set; }
        public Guid? UserId { get; set; }
        public string? UserDisplayName { get; set; }
        public SignalRUpdateType UpdateType { get; set; }
        public string Message { get; set; } = "";
        public object? Object { get; set; }
        public string? ObjectAsString { get; set; }
    }
}