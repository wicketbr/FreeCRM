namespace CRM;

public partial class DataObjects
{
    public partial class EmailTemplate : ActionResponseObject
    {
        public Guid EmailTemplateId { get; set; }
        public Guid TenantId { get; set; }
        public string? Name { get; set; }
        public string? Template { get; set; }
        public bool Enabled { get; set; }
        public DateTime Added { get; set; }
        public string? AddedBy { get; set; }
        public DateTime LastModified { get; set; }
        public string? LastModifiedBy { get; set; }
        public bool Deleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public List<Guid>? Tags { get; set; }
    }

    public partial class EmailTemplateDetails
    {
        public string? From { get; set; }
        public string? ReplyTo { get; set; }
        public string? Subject { get; set; }
        public string? Body { get; set; }
    }
}