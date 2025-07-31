namespace CRM;

public partial class DataObjects
{
    public partial class Location : ActionResponseObject
    {
        public Guid LocationId { get; set; }
        public Guid TenantId { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? CalendarBackgroundColor { get; set; }
        public string? CalendarForegroundColor { get; set; }
        public bool Enabled { get; set; }
        public bool DefaultLocation { get; set; }
        public DateTime Added { get; set; }
        public string? AddedBy { get; set; }
        public DateTime LastModified { get; set; }
        public string? LastModifiedBy { get; set; }
        public bool Deleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}