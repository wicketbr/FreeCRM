using System;
using System.Collections.Generic;

namespace CRM.EFModels.EFModels;

public partial class UDFLabel
{
    public Guid Id { get; set; }

    public string Module { get; set; } = null!;

    public string UDF { get; set; } = null!;

    public string? Label { get; set; }

    public bool? ShowColumn { get; set; }

    public bool? ShowInFilter { get; set; }

    public bool? IncludeInSearch { get; set; }

    public Guid TenantId { get; set; }

    public DateTime LastModified { get; set; }

    public string? LastModifiedBy { get; set; }
}
