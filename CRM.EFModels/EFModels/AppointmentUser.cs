using System;
using System.Collections.Generic;

namespace CRM.EFModels.EFModels;

public partial class AppointmentUser
{
    public Guid AppointmentUserId { get; set; }

    public Guid AppointmentId { get; set; }

    public Guid TenantId { get; set; }

    public Guid UserId { get; set; }

    public string? AttendanceCode { get; set; }

    public decimal? Fees { get; set; }

    public virtual Appointment Appointment { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
