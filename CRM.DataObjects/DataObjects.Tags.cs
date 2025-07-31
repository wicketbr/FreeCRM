namespace CRM;

public partial class DataObjects
{
    public enum TagModule
    {
        Appointment,
        EmailTemplate,
        Service
    }

    public partial class Tag : ActionResponseObject
    {
        public Guid TagId { get; set; }
        public Guid TenantId { get; set; }
        public string? Name { get; set; }
        public string? Style { get; set; }
        public bool Enabled { get; set; }
        public bool UseInAppointments { get; set; }
        public bool UseInEmailTemplates { get; set; }
        public bool UseInServices { get; set; }
        public DateTime Added { get; set; }
        public string? AddedBy { get; set; }
        public DateTime LastModified { get; set; }
        public string? LastModifiedBy { get; set; }
        public bool Deleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}