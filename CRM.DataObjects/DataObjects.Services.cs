namespace CRM;

public partial class DataObjects
{
    public partial class Service : ActionResponseObject
    {
        public Guid ServiceId { get; set; }
        public Guid TenantId { get; set; }
        public string? Code { get; set; }
        public bool DefaultService { get; set; }
        public string? Description { get; set; }
        public decimal Rate { get; set; }
        public int DefaultAppointmentDuration { get; set; }
        public bool Enabled { get; set; }
        public DateTime Added { get; set; }
        public string? AddedBy { get; set; }
        public DateTime LastModified { get; set; }
        public string? LastModifiedBy { get; set; }
        public bool Deleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public List<Guid>? Tags { get; set; }
    }
}