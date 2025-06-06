using System;
using System.Collections.Generic;

namespace CRM.EFModels.EFModels;

public partial class Invoice
{
    public Guid InvoiceId { get; set; }

    public Guid TenantId { get; set; }

    public string? InvoiceNumber { get; set; }

    public string? PONumber { get; set; }

    // {{ModuleItemStart:Appointments}}
    public Guid? AppointmentId { get; set; }
    // {{ModuleItemEnd:Appointments}}

    public Guid? UserId { get; set; }

    public string Title { get; set; } = null!;

    public string Items { get; set; } = null!;

    public string? Notes { get; set; }

    public DateTime? InvoiceCreated { get; set; }

    public DateTime? InvoiceDueDate { get; set; }

    public DateTime? InvoiceSendDate { get; set; }

    public DateTime? InvoiceSent { get; set; }

    public DateTime? InvoiceClosed { get; set; }

    public decimal Total { get; set; }

    public DateTime Added { get; set; }

    public string? AddedBy { get; set; }

    public DateTime LastModified { get; set; }

    public string? LastModifiedBy { get; set; }

    public bool Deleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    // {{ModuleItemStart:Appointments}}
    public virtual Appointment? Appointment { get; set; }
    // {{ModuleItemEnd:Appointments}}

    // {{ModuleItemStart:Payments}}
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    // {{ModuleItemEnd:Payments}}

    public virtual Tenant Tenant { get; set; } = null!;

    public virtual User? User { get; set; }
}
