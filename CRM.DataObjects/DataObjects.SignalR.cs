namespace CRM;

public partial class DataObjects
{
    public enum SignalRUpdateType
    {
        Appointment,
        AppointmentNote,
        AppointmentService,
        Department,
        DepartmentGroup,
        EmailTemplate,
        File,
        Invoice,
        Language,
        LastAccessTime,
        Location,
        Payment,
        Service,
        Setting,
        Tag,
        Tenant,
        UDF,
        Unknown,
        User,
        UserAttendance,
        UserGroup,
        UserPreferences,
    }

    public class SignalRUpdate
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