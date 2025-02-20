using System;
using System.Collections.Generic;

namespace CRM.EFModels.EFModels;

public partial class AppointmentNote
{
    public Guid AppointmentNoteId { get; set; }

    public Guid AppointmentId { get; set; }

    public Guid TenantId { get; set; }

    public DateTime Added { get; set; }

    public string? AddedBy { get; set; }

    public DateTime LastModified { get; set; }

    public string? LastModifiedBy { get; set; }

    public string? Note { get; set; }

    public bool Deleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Appointment Appointment { get; set; } = null!;
}
