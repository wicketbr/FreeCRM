namespace CRM;

public partial class DataObjects
{
    public partial class FilterInvoices : Filter
    {
        public List<Invoice>? Records { get; set; }
        // {{ModuleItemStart:Appointments}}
        public Guid? AppointmentId { get; set; }
        // {{ModuleItemEnd:Appointments}}
        public Guid? UserId { get; set; }
        public string? ClosedStatus { get; set; }
        public string? SentStatus { get; set; }
    }

    public partial class Invoice : ActionResponseObject
    {
        public Guid InvoiceId { get; set; }
        public Guid TenantId { get; set; }
        public string? InvoiceNumber { get; set; }
        public string? PONumber { get; set; }
        // {{ModuleItemStart:Appointments}}
        public Guid? AppointmentId { get; set; }
        // {{ModuleItemEnd:Appointments}}
        public string? AppointmentDisplay { get; set; }
        public Guid? UserId { get; set; }
        public string? UserDisplay { get; set; }
        public string? Title { get; set; }
        //public string? Items { get; set; }
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
        public byte[]? PDF { get; set; }
        public List<byte[]>? Images { get; set; }
        public List<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
    }

    public partial class InvoiceItem
    {
        public Guid Id { get; set; }
        public string? Description { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}