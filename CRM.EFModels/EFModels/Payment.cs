using System;
using System.Collections.Generic;

namespace CRM.EFModels.EFModels;

public partial class Payment
{
    public Guid PaymentId { get; set; }

    public Guid TenantId { get; set; }

    public Guid InvoiceId { get; set; }

    public Guid? UserId { get; set; }

    public string? Notes { get; set; }

    public DateTime PaymentDate { get; set; }

    public decimal Amount { get; set; }

    public decimal? Refunded { get; set; }

    public string? RefundedBy { get; set; }

    public DateTime? RefundDate { get; set; }

    public DateTime Added { get; set; }

    public string? AddedBy { get; set; }

    public DateTime LastModified { get; set; }

    public string? LastModifiedBy { get; set; }

    public bool Deleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    // {{ModuleItemStart:Invoices}}
    public virtual Invoice Invoice { get; set; } = null!;
    // {{ModuleItemEnd:Invoices}}

    public virtual Tenant Tenant { get; set; } = null!;

    public virtual User? User { get; set; }
}
