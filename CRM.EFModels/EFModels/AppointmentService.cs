using System;
using System.Collections.Generic;

namespace CRM.EFModels.EFModels;

public partial class AppointmentService
{
    public Guid AppointmentServiceId { get; set; }

    public Guid AppointmentId { get; set; }

    public Guid TenantId { get; set; }

    public Guid ServiceId { get; set; }

    public decimal? Fee { get; set; }

    public bool Deleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public DateTime LastModified { get; set; }

    public string? LastModifiedBy { get; set; }

    public virtual Appointment Appointment { get; set; } = null!;

    // {{ModuleItemStart:Services}}
    public virtual Service Service { get; set; } = null!;
    // {{ModuleItemEnd:Services}}
}
