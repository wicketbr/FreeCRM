using System;
using System.Collections.Generic;

namespace CRM.EFModels.EFModels;

public partial class UserGroup
{
    public Guid GroupId { get; set; }

    public Guid TenantId { get; set; }

    public string Name { get; set; } = null!;

    public bool Enabled { get; set; }

    public string? Settings { get; set; }

    public DateTime Added { get; set; }

    public string? AddedBy { get; set; }

    public DateTime LastModified { get; set; }

    public string? LastModifiedBy { get; set; }

    public bool Deleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<UserInGroup> UserInGroups { get; set; } = new List<UserInGroup>();
}
