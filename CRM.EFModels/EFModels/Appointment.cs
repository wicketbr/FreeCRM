using System;
using System.Collections.Generic;

namespace CRM.EFModels.EFModels;

public partial class Appointment
{
    public Guid AppointmentId { get; set; }

    public Guid TenantId { get; set; }

    public string Title { get; set; } = null!;

    public DateTime Start { get; set; }

    public DateTime End { get; set; }

    public bool AllDay { get; set; }

    public bool Meeting { get; set; }

    public Guid? LocationId { get; set; }

    public DateTime Added { get; set; }

    public string? AddedBy { get; set; }

    public DateTime LastModified { get; set; }

    public string? LastModifiedBy { get; set; }

    public bool Deleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public string? Note { get; set; }

    public string? ForegroundColor { get; set; }

    public string? BackgroundColor { get; set; }

    public virtual ICollection<AppointmentNote> AppointmentNotes { get; set; } = new List<AppointmentNote>();

    // {{ModuleItemStart:Appointments}}
    public virtual ICollection<AppointmentService> AppointmentServices { get; set; } = new List<AppointmentService>();

    public virtual ICollection<AppointmentUser> AppointmentUsers { get; set; } = new List<AppointmentUser>();
    // {{ModuleItemEnd:Appointments}}

    // {{ModuleItemStart:Invoices}}
    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    // {{ModuleItemEnd:Invoices}}

    // {{ModuleItemStart:Locations}}
    public virtual Location? Location { get; set; }
    // {{ModuleItemEnd:Locations}}
}
