using System;
using System.Collections.Generic;

namespace CRM.EFModels.EFModels;

public partial class Service
{
    public Guid ServiceId { get; set; }

    public Guid TenantId { get; set; }

    public string? Code { get; set; }

    public bool DefaultService { get; set; }

    public string Description { get; set; } = null!;

    public decimal Rate { get; set; }

    public int DefaultAppointmentDuration { get; set; }

    public bool Enabled { get; set; }

    public DateTime Added { get; set; }

    public string? AddedBy { get; set; }

    public DateTime LastModified { get; set; }

    public string? LastModifiedBy { get; set; }

    public bool Deleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    // {{ModuleItemStart:Appointments}}
    public virtual ICollection<AppointmentService> AppointmentServices { get; set; } = new List<AppointmentService>();
    // {{ModuleItemEnd:Appointments}}
}
