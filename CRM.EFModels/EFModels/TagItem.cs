using System;
using System.Collections.Generic;

namespace CRM.EFModels.EFModels;

public partial class TagItem
{
    public Guid TagItemId { get; set; }

    public Guid TagId { get; set; }

    public Guid TenantId { get; set; }

    public Guid ItemId { get; set; }

    public virtual Tag Tag { get; set; } = null!;
}
