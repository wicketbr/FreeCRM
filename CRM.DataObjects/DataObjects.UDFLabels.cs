namespace CRM;

public partial class DataObjects
{
    public partial class udfLabel
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public string? Module { get; set; }
        public string? udf { get; set; }
        public string? Label { get; set; }
        public bool ShowColumn { get; set; }
        public bool ShowInFilter { get; set; }
        public bool IncludeInSearch { get; set; }
        public List<string> FilterOptions { get; set; } = new List<string>();
        public DateTime LastModified { get; set; }
        public string? LastModifiedBy { get; set; }
    }
}