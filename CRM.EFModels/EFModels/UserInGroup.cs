using System;
using System.Collections.Generic;

namespace CRM.EFModels.EFModels;

public partial class UserInGroup
{
    public Guid UserInGroupId { get; set; }

    public Guid UserId { get; set; }

    public Guid TenantId { get; set; }

    public Guid GroupId { get; set; }

    public virtual UserGroup Group { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
