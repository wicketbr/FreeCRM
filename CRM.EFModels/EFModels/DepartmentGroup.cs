using System;
using System.Collections.Generic;

namespace CRM.EFModels.EFModels;

public partial class DepartmentGroup
{
    public Guid DepartmentGroupId { get; set; }

    public string? DepartmentGroupName { get; set; }

    public Guid TenantId { get; set; }

    public DateTime Added { get; set; }

    public string? AddedBy { get; set; }

    public DateTime LastModified { get; set; }

    public string? LastModifiedBy { get; set; }

    public bool Deleted { get; set; }

    public DateTime? DeletedAt { get; set; }
}
