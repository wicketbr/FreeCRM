using System;
using System.Collections.Generic;

namespace CRM.EFModels.EFModels;

public partial class Setting
{
    public int SettingId { get; set; }

    public string SettingName { get; set; } = null!;

    public string? SettingType { get; set; }

    public string? SettingNotes { get; set; }

    public string? SettingText { get; set; }

    public Guid? TenantId { get; set; }

    public Guid? UserId { get; set; }

    public DateTime LastModified { get; set; }

    public string? LastModifiedBy { get; set; }
}
