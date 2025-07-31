namespace CRM;

public partial class DataObjects
{
    public partial class Department : ActionResponseObject
    {
        public Guid DepartmentId { get; set; }
        public Guid TenantId { get; set; }
        public string? DepartmentName { get; set; }
        public string? ActiveDirectoryNames { get; set; }
        public bool Enabled { get; set; }
        public DateTime Added { get; set; }
        public string? AddedBy { get; set; }
        public DateTime LastModified { get; set; }
        public string? LastModifiedBy { get; set; }
        public bool Deleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public Guid? DepartmentGroupId { get; set; }
    }

    public partial class DepartmentGroup : ActionResponseObject
    {
        public Guid DepartmentGroupId { get; set; }
        public Guid TenantId { get; set; }
        public string? DepartmentGroupName { get; set; }
        public DateTime Added { get; set; }
        public string? AddedBy { get; set; }
        public DateTime LastModified { get; set; }
        public string? LastModifiedBy { get; set; }
        public bool Deleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}