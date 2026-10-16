using System;
using System.Collections.Generic;

namespace CRM.EFModels.EFModels;

public partial class Tag
{
    public Guid TagId { get; set; }

    public Guid TenantId { get; set; }

    public string Name { get; set; } = null!;

    public string? Style { get; set; }

    public bool Enabled { get; set; }

    // {{ModuleItemStart:Appointments}}
    public bool UseInAppointments { get; set; }
    // {{ModuleItemEnd:Appointments}}

    // {{ModuleItemStart:EmailTemplates}}
    public bool UseInEmailTemplates { get; set; }
    // {{ModuleItemEnd:EmailTemplates}}

    // {{ModuleItemStart:Services}}
    public bool UseInServices { get; set; }
    // {{ModuleItemEnd:Services}}

    public DateTime Added { get; set; }

    public string? AddedBy { get; set; }

    public DateTime LastModified { get; set; }

    public string? LastModifiedBy { get; set; }

    public bool Deleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<TagItem> TagItems { get; set; } = new List<TagItem>();
}
