namespace CRM;

public partial class DataObjects
{
    public partial class Appointment : ActionResponseObject
    {
        public Guid AppointmentId { get; set; }
        public Guid TenantId { get; set; }
        public DateTime Added { get; set; }
        public string? AddedBy { get; set; }
        public bool AllDay { get; set; }
        public bool Deleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime End { get; set; }
        public DateTime LastModified { get; set; }
        public string? LastModifiedBy { get; set; }
        public Guid? LocationId { get; set; }
        public bool Meeting { get; set; }
        public string? Note { get; set; }
        public string? ForegroundColor { get; set; }
        public string? BackgroundColor { get; set; }
        public List<AppointmentNote> Notes { get; set; } = new List<AppointmentNote>();
        public List<AppointmentService> Services { get; set; } = new List<AppointmentService>();
        public DateTime Start { get; set; }
        public string Title { get; set; } = "";
        public string TitleDisplay { get; set; } = "";
        public List<AppointmentUser> Users { get; set; } = new List<AppointmentUser>();
        public List<Guid>? Tags { get; set; }
        // {{ModuleItemStart:Invoices}}
        public List<Invoice> Invoices { get; set; } = new List<Invoice>();
        // {{ModuleItemEnd:Invoices}}
    }

    public partial class AppointmentAttendanceUpdate : ActionResponseObject
    {
        public Guid AppointmentId { get; set; }
        public Guid UserId { get; set; }
        public string AttendanceCode { get; set; } = "invited";
    }

    public partial class AppoinmentLoader
    {
        public Guid TenantId { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
    }

    public partial class AppointmentNote : ActionResponseObject
    {
        public Guid AppointmentNoteId { get; set; }
        public Guid AppointmentId { get; set; }
        public Guid TenantId { get; set; }
        public DateTime Added { get; set; }
        public string? AddedBy { get; set; }
        public DateTime LastModified { get; set; }
        public string? LastModifiedBy { get; set; }
        public string? Note { get; set; }
        public bool Deleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }

    public partial class AppointmentService
    {
        public Guid AppointmentServiceId { get; set; }
        public Guid ServiceId { get; set; }
        public decimal Fee { get; set; }
        public DateTime LastModified { get; set; }
        public string? LastModifiedBy { get; set; }
        public bool Deleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }

    public partial class AppointmentUser
    {
        public Guid UserId { get; set; }
        public string AttendanceCode { get; set; } = "invited";
        public string DisplayName { get; set; } = "";
        public decimal Fees { get; set; }
    }
}