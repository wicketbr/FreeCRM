using System;
using System.Collections.Generic;

namespace CRM.EFModels.EFModels;

public partial class Department
{
    public Guid DepartmentId { get; set; }

    public string DepartmentName { get; set; } = null!;

    public string? ActiveDirectoryNames { get; set; }

    public bool Enabled { get; set; }

    public Guid? DepartmentGroupId { get; set; }

    public Guid TenantId { get; set; }

    public DateTime Added { get; set; }

    public string? AddedBy { get; set; }

    public DateTime LastModified { get; set; }

    public string? LastModifiedBy { get; set; }

    public bool Deleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
