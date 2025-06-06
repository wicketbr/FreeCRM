using System;
using System.Collections.Generic;

namespace CRM.EFModels.EFModels;

public partial class Tenant
{
    public Guid TenantId { get; set; }

    public string Name { get; set; } = null!;

    public string TenantCode { get; set; } = null!;

    public bool Enabled { get; set; }

    public DateTime Added { get; set; }

    public string? AddedBy { get; set; }

    public DateTime LastModified { get; set; }

    public string? LastModifiedBy { get; set; }

    // {{ModuleItemStart:Invoices}}
    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    // {{ModuleItemEnd:Invoices}}

    // {{ModuleItemStart:Payments}}
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    // {{ModuleItemEnd:Payments}}

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
