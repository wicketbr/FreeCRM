using Microsoft.Graph.Models;

namespace CRM;

public partial interface IDataAccess
{
    Task<DataObjects.BooleanResponse> DeleteInvoice(Guid InvoiceId, DataObjects.User? CurrentUser = null, bool ForceDeleteImmediately = false);
    Task<DataObjects.Invoice> GetInvoice(Guid InvoiceId, DataObjects.User? CurrentUser = null, bool IncludeImages = false, bool IncludePDF = false);
    Task<List<DataObjects.Invoice>> GetInvoices(Guid TenantId, bool UnpaidOnly = false, DataObjects.User? CurrentUser = null);
    DataObjects.FilterInvoices GetInvoicesFiltered(DataObjects.FilterInvoices filter, DataObjects.User? CurrentUser = null);
    // {{ModuleItemStart:Appointments}}
    Task<List<DataObjects.Invoice>> GetInvoicesForAppointment(Guid AppointmentId, bool UnpaidOnly = false, DataObjects.User? CurrentUser = null);
    // {{ModuleItemEnd:Appointments}}
    Task<List<DataObjects.Invoice>> GetInvoicesForUser(Guid UserId, bool UnpaidOnly = false, DataObjects.User? CurrentUser = null);
    Task<DataObjects.Invoice> SaveInvoice(DataObjects.Invoice invoice, DataObjects.User? CurrentUser = null);
}

public partial class DataAccess
{
    public async Task<DataObjects.BooleanResponse> DeleteInvoice(Guid InvoiceId, DataObjects.User? CurrentUser = null, bool ForceDeleteImmediately = false)
    {
        var output = new DataObjects.BooleanResponse();

        var rec = await data.Invoices.FirstOrDefaultAsync(x => x.InvoiceId == InvoiceId);
        if (rec == null) {
            output.Messages.Add("Error Deleting Invoice '" + InvoiceId.ToString() + "' - Record No Longer Exists");
            return output;
        }

        var now = DateTime.UtcNow;
        Guid tenantId = GuidValue(rec.TenantId);
        var tenantSettings = GetTenantSettings(tenantId);

        if (ForceDeleteImmediately || tenantSettings.DeletePreference == DataObjects.DeletePreference.Immediate) {
            var deleteAppRecords = await DeleteRecordsApp(rec, CurrentUser);
            if (!deleteAppRecords.Result) {
                output.Messages.AddRange(deleteAppRecords.Messages);
                return output;
            }

            try {
                data.Invoices.Remove(rec);
                await data.SaveChangesAsync();

                output.Result = true;
            } catch (Exception ex) {
                output.Messages.Add("Error Deleting Invoice " + InvoiceId.ToString() + ":");
                output.Messages.AddRange(RecurseException(ex));
            }
        } else {
            try {
                rec.Deleted = true;
                rec.DeletedAt = now;
                rec.LastModified = now;

                if (CurrentUser != null) {
                    rec.LastModifiedBy = CurrentUserIdString(CurrentUser);
                }

                await data.SaveChangesAsync();
                output.Result = true;
            } catch (Exception ex) {
                output.Messages.Add("Error Deleting Invoice " + InvoiceId.ToString() + ":");
                output.Messages.AddRange(RecurseException(ex));
            }
        }

        if (!ForceDeleteImmediately) {
            await SignalRUpdate(new DataObjects.SignalRUpdate {
                TenantId = tenantId,
                ItemId = InvoiceId,
                UpdateType = DataObjects.SignalRUpdateType.Invoice,
                Message = "Deleted",
                UserId = CurrentUserId(CurrentUser),
            });
        }

        return output;
    }

    public async Task<DataObjects.Invoice> GetInvoice(Guid InvoiceId, DataObjects.User? CurrentUser = null, bool IncludeImages = false, bool IncludePDF = false)
    {
        var output = new DataObjects.Invoice();

        Invoice? rec = null;

        if (AdminUser(CurrentUser)) {
            rec = await data.Invoices.FirstOrDefaultAsync(x => x.InvoiceId == InvoiceId);
        } else {
            rec = await data.Invoices.FirstOrDefaultAsync(x => x.InvoiceId == InvoiceId && x.Deleted != true);
        }

        if (rec != null) {
            var invoiceItems = DeserializeObject<List<DataObjects.InvoiceItem>>(rec.Items);

            output = new DataObjects.Invoice { 
                ActionResponse = GetNewActionResponse(true),
                InvoiceId = rec.InvoiceId,
                TenantId = rec.TenantId,
                InvoiceNumber = rec.InvoiceNumber,
                PONumber = rec.PONumber,
                // {{ModuleItemStart:Appointments}}
                AppointmentId = rec.AppointmentId,
                // {{ModuleItemEnd:Appointments}}
                UserId = rec.UserId,
                Title = rec.Title,
                InvoiceItems = invoiceItems != null && invoiceItems.Any() ? invoiceItems : new List<DataObjects.InvoiceItem>(),
                Notes = rec.Notes,
                InvoiceCreated = rec.InvoiceCreated,
                InvoiceDueDate = rec.InvoiceDueDate,
                InvoiceSendDate = rec.InvoiceSendDate,
                InvoiceSent = rec.InvoiceSent,
                InvoiceClosed = rec.InvoiceClosed,
                Total = rec.Total,
                Added = rec.Added,
                AddedBy = LastModifiedDisplayName(rec.AddedBy),
                LastModified = rec.LastModified,
                LastModifiedBy = LastModifiedDisplayName(rec.LastModifiedBy),
                Deleted = rec.Deleted,
                DeletedAt = rec.DeletedAt,
            };

            GetDataApp(rec, output, CurrentUser);

            if (IncludeImages) {
                output = await GenerateInvoiceImages(output, CurrentUser);
            }

            if (IncludePDF) {
                output = await GenerateInvoicePDF(output, CurrentUser);
            }
        } else {
            output.ActionResponse.Messages.Add("Invoice '" + InvoiceId.ToString() + "' No Longer Exists");
        }

        return output;
    }

    public async Task<List<DataObjects.Invoice>> GetInvoices(Guid TenantId, bool UnpaidOnly = false, DataObjects.User? CurrentUser = null)
    {
        var output = new List<DataObjects.Invoice>();

        var query = data.Invoices.Where(x => x.TenantId == TenantId);

        if (CurrentUser == null || !CurrentUser.Admin) {
            query = query.Where(x => x.Deleted != true);
        }

        if (UnpaidOnly) {
            query = query.Where(x => x.InvoiceClosed != null);
        }

        var recs = await query.ToListAsync();

        if (recs != null && recs.Any()) {
            foreach(var rec in recs) {
                var invoiceItems = DeserializeObject<List<DataObjects.InvoiceItem>>(rec.Items);

                var i = new DataObjects.Invoice {
                    ActionResponse = GetNewActionResponse(true),
                    InvoiceId = rec.InvoiceId,
                    TenantId = rec.TenantId,
                    InvoiceNumber = rec.InvoiceNumber,
                    PONumber = rec.PONumber,
                    // {{ModuleItemStart:Appointments}}
                    AppointmentId = rec.AppointmentId,
                    // {{ModuleItemEnd:Appointments}}
                    UserId = rec.UserId,
                    Title = rec.Title,
                    InvoiceItems = invoiceItems != null && invoiceItems.Any() ? invoiceItems : new List<DataObjects.InvoiceItem>(),
                    Notes = rec.Notes,
                    InvoiceCreated = rec.InvoiceCreated,
                    InvoiceDueDate = rec.InvoiceDueDate,
                    InvoiceSendDate = rec.InvoiceSendDate,
                    InvoiceSent = rec.InvoiceSent,
                    InvoiceClosed = rec.InvoiceClosed,
                    Total = rec.Total,
                    Added = rec.Added,
                    AddedBy = LastModifiedDisplayName(rec.AddedBy),
                    LastModified = rec.LastModified,
                    LastModifiedBy = LastModifiedDisplayName(rec.LastModifiedBy),
                    Deleted = rec.Deleted,
                    DeletedAt = rec.DeletedAt,
                };

                GetDataApp(rec, i, CurrentUser);

                output.Add(i);
            }
        }

        return output;
    }

    public DataObjects.FilterInvoices GetInvoicesFiltered(DataObjects.FilterInvoices filter, DataObjects.User? CurrentUser = null)
    {
        bool adminUser = AdminUser(CurrentUser);

        var output = filter;
        output.ActionResponse = GetNewActionResponse();
        output.Records = null;

        var language = GetTenantLanguage(output.TenantId, StringValue(output.CultureCode));

        output.Columns = new List<DataObjects.FilterColumn> {
            new DataObjects.FilterColumn { 
                Align = "",
                Label = GetLanguageItem("InvoiceItemDescription", language),
                TipText = "",
                Sortable = true,
                DataElementName = "Title",
                DataType = "text",
            },
            new DataObjects.FilterColumn {
                Align = "",
                Label = GetLanguageItem("InvoiceNumber"),
                TipText = "",
                Sortable = true,
                DataElementName = "InvoiceNumber",
                DataType = "text",
            },
            new DataObjects.FilterColumn {
                Align = "",
                Label = GetLanguageItem("InvoicePO"),
                TipText = "",
                Sortable = true,
                DataElementName = "PONumber",
                DataType = "text",
            },
            new DataObjects.FilterColumn {
                Align = "",
                Label = GetLanguageItem("InvoiceCreated", language),
                TipText = "",
                Sortable = true,
                DataElementName = "InvoiceCreated",
                DataType = "DateTime"
            },
            new DataObjects.FilterColumn {
                Align = "",
                Label = GetLanguageItem("InvoiceDue", language),
                TipText = "",
                Sortable = true,
                DataElementName = "InvoiceDueDate",
                DataType = "Date"
            }
        };

        output.Columns.AddRange(GetFilterColumnsApp("Invoices", "InvoiceDueDate", language, CurrentUser));

        if (adminUser) {
            output.Columns.Add(new DataObjects.FilterColumn {
                Align = "",
                Label = GetLanguageItem("InvoiceSendDate", language),
                TipText = "",
                Sortable = true,
                DataElementName = "InvoiceSendDate",
                DataType = "Date"
            });
        }

        output.Columns.AddRange(GetFilterColumnsApp("Invoices", "InvoiceSendDate", language, CurrentUser));

        output.Columns.Add(new DataObjects.FilterColumn {
            Align = "",
            Label = GetLanguageItem("InvoiceSent", language),
            TipText = "",
            Sortable = true,
            DataElementName = "InvoiceSent",
            DataType = "DateTime"
        });

        output.Columns.AddRange(GetFilterColumnsApp("Invoices", "InvoiceSent", language, CurrentUser));

        output.Columns.Add(new DataObjects.FilterColumn {
            Align = "",
            Label = GetLanguageItem("InvoiceTotal", language),
            TipText = "",
            Sortable = true,
            DataElementName = "Total",
            DataType = "currency"
        });

        output.Columns.AddRange(GetFilterColumnsApp("Invoices", "Total", language, CurrentUser));

        IQueryable<Invoice>? recs = null;

        if(adminUser) {
            recs = data.Invoices
                // {{ModuleItemStart:Appointments}}
                .Include(x => x.Appointment)
                // {{ModuleItemEnd:Appointments}}
                .Include(x => x.User)
                .Where(x => x.TenantId == output.TenantId);

            if (!output.IncludeDeletedItems) {
                recs = recs.Where(x => x.Deleted != true);
            }
        } else {
            recs = data.Invoices
                // {{ModuleItemStart:Appointments}}
                .Include(x => x.Appointment)
                // {{ModuleItemEnd:Appointments}}
                .Include(x => x.User)
                .Where(x => x.TenantId == output.TenantId && x.Deleted != true);
        }

        if (!String.IsNullOrWhiteSpace(output.ClosedStatus)) {
            switch(output.ClosedStatus.ToUpper()) {
                case "CLOSED":
                    recs = recs.Where(x => x.InvoiceClosed != null);
                    break;

                case "OPEN":
                    recs = recs.Where(x => x.InvoiceClosed == null);
                    break;
            }
        }

        if (CurrentUser != null && !adminUser) {
            // Non-admin users can only see their own items, and only items that have been sent.
            output.UserId = CurrentUser.UserId;
            output.SentStatus = "sent";
        }


        if (!String.IsNullOrWhiteSpace(output.SentStatus)) {
            switch (output.SentStatus.ToUpper()) {
                case "SENT":
                    recs = recs.Where(x => x.InvoiceSent != null);
                    break;

                case "UNSENT":
                    recs = recs.Where(x => x.InvoiceSent == null);
                    break;
            }
        }

        // {{ModuleItemStart:Appointments}}
        if (output.AppointmentId.HasValue && output.AppointmentId != Guid.Empty) {
            recs = recs.Where(x => x.AppointmentId == output.AppointmentId);
        }
        // {{ModuleItemEnd:Appointments}}

        if (output.UserId.HasValue && output.UserId != Guid.Empty) {
            recs = recs.Where(x => x.UserId == output.UserId);
        }

        if (String.IsNullOrWhiteSpace(output.Sort)) {
            output.Sort = "InvoiceCreated";
            output.SortOrder = "DESC";
        }

        if (String.IsNullOrWhiteSpace(output.SortOrder)) {
            output.SortOrder = "ASC";
        }

        bool Ascending = true;
        if (StringValue(output.SortOrder).ToUpper() == "DESC") {
            Ascending = false;
        }

        switch (StringValue(output.Sort).ToUpper()) {
            case "TITLE":
                recs = Ascending
                    ? recs.OrderBy(x => x.Title).ThenBy(x => x.InvoiceCreated)
                    : recs.OrderByDescending(x => x.Title).ThenByDescending(x => x.InvoiceCreated);
                break;

            case "INVOICENUMBER":
                recs = Ascending
                    ? recs.OrderBy(x => x.InvoiceNumber).ThenBy(x => x.InvoiceCreated)
                    : recs.OrderByDescending(x => x.InvoiceNumber).ThenByDescending(x => x.InvoiceCreated);
                break;

            case "PONUMBER":
                recs = Ascending
                    ? recs.OrderBy(x => x.PONumber).ThenBy(x => x.InvoiceCreated)
                    : recs.OrderByDescending(x => x.PONumber).ThenByDescending(x => x.InvoiceCreated);
                break;

            case "INVOICECREATED":
                recs = Ascending
                    ? recs.OrderBy(x => x.InvoiceCreated)
                    : recs.OrderByDescending(x => x.InvoiceCreated);
                break;

            case "INVOICEDUEDATE":
                recs = Ascending
                    ? recs.OrderBy(x => x.InvoiceDueDate)
                    : recs.OrderByDescending(x => x.InvoiceDueDate);
                break;

            case "INVOICESENDDATE":
                recs = Ascending
                    ? recs.OrderBy(x => x.InvoiceSendDate)
                    : recs.OrderByDescending(x => x.InvoiceSendDate);
                break;

            case "INVOICESENT":
                recs = Ascending
                    ? recs.OrderBy(x => x.InvoiceSent)
                    : recs.OrderByDescending(x => x.InvoiceSent);
                break;

            case "TOTAL":
                recs = Ascending
                    ? recs.OrderBy(x => x.Total)
                    : recs.OrderByDescending(x => x.Total);
                break;

            // {{ModuleItemStart:Appointments}}
            case "APPOINTMENT":
                recs = Ascending
                    ? recs.OrderBy(x => (x.Appointment != null ? x.Appointment.Title : "")).ThenBy(x => x.InvoiceCreated)
                    : recs.OrderByDescending(x => (x.Appointment != null ? x.Appointment.Title : "")).ThenByDescending(x => x.InvoiceCreated);
                break;
            // {{ModuleItemEnd:Appointments}}

            case "USER":
                recs = Ascending
                    ? recs.OrderBy(x => (x.User != null ? x.User.FirstName : "")).ThenBy(x => (x.User != null ? x.User.LastName : "")).ThenBy(x => x.InvoiceCreated)
                    : recs.OrderByDescending(x => (x.User != null ? x.User.FirstName : "")).ThenByDescending(x => (x.User != null ? x.User.LastName : "")).ThenByDescending(x => x.InvoiceCreated);
                break;
        }

        if (recs != null && recs.Count() > 0) {

            int TotalRecords = recs.Count();
            output.RecordCount = TotalRecords;

            if (output.RecordsPerPage > 0) {
                // We are filtering records per page
                if (output.RecordsPerPage >= TotalRecords) {
                    output.Page = 1;
                    output.PageCount = 1;
                } else {
                    // Figure out the page count
                    if (output.Page < 1) { output.Page = 1; }
                    if (output.RecordsPerPage < 1) { output.RecordsPerPage = 25; }
                    decimal decPages = (decimal)TotalRecords / (decimal)output.RecordsPerPage;
                    decPages = Math.Ceiling(decPages);
                    output.PageCount = (int)decPages;

                    if (output.Page > output.PageCount) {
                        output.Page = output.PageCount;
                    }

                    if (output.Page > 1) {
                        recs = recs.Skip((output.Page - 1) * output.RecordsPerPage).Take(output.RecordsPerPage);
                    } else {
                        recs = recs.Take(output.RecordsPerPage);
                    }

                }
            }

            List<DataObjects.Invoice> records = new List<DataObjects.Invoice>();

            foreach (var rec in recs) {
                var invoiceItems = DeserializeObject<List<DataObjects.InvoiceItem>>(rec.Items);

                var u = new DataObjects.Invoice {
                    ActionResponse = GetNewActionResponse(true),
                    InvoiceId = rec.InvoiceId,
                    TenantId = rec.TenantId,
                    InvoiceNumber = rec.InvoiceNumber,
                    PONumber = rec.PONumber,
                    // {{ModuleItemStart:Appointments}}
                    AppointmentId = rec.AppointmentId,
                    AppointmentDisplay = rec.Appointment != null
                        ? rec.Appointment.Title
                        : String.Empty,
                    // {{ModuleItemEnd:Appointments}}
                    UserId = rec.UserId,
                    UserDisplay = rec.User != null
                        ? rec.User.FirstName + " " + rec.User.LastName
                        : String.Empty,
                    Title = rec.Title,
                    InvoiceItems = invoiceItems != null && invoiceItems.Any() ? invoiceItems : new List<DataObjects.InvoiceItem>(),
                    InvoiceCreated = rec.InvoiceCreated,
                    InvoiceDueDate = rec.InvoiceDueDate,
                    InvoiceSendDate = rec.InvoiceSendDate,
                    InvoiceSent = rec.InvoiceSent,
                    InvoiceClosed = rec.InvoiceClosed,
                    Total = rec.Total,
                    Added = rec.Added,
                    AddedBy = LastModifiedDisplayName(rec.AddedBy),
                    Deleted = rec.Deleted,
                    DeletedAt = rec.DeletedAt,
                    //LastModified = rec.LastModified,
                    //LastModifiedBy = LastModifiedDisplayName(rec.LastModifiedBy),
                };

                GetDataApp(rec, u, CurrentUser);

                records.Add(u);
            }

            output.Records = records;

            // {{ModuleItemStart:Appointments}}
            // Only include the following columns if there is data with values for those items.
            if (records.Any(x => x.AppointmentId != null)) {
                output.Columns.Add(new DataObjects.FilterColumn {
                    Align = "",
                    Label = GetLanguageItem("Appointment", language),
                    TipText = "",
                    Sortable = true,
                    DataElementName = "Appointment",
                    DataType = "string"
                });
            }
            // {{ModuleItemEnd:Appointments}}

            if (adminUser && records.Any(x => x.UserId != null)) {
                output.Columns.Add(new DataObjects.FilterColumn {
                    Align = "",
                    Label = GetLanguageItem("User", language),
                    TipText = "",
                    Sortable = true,
                    DataElementName = "UserDisplay",
                    DataType = "string"
                });
            }

            if (!records.Any(x => !String.IsNullOrWhiteSpace(x.InvoiceNumber))) {
                output.Columns = output.Columns.Where(x => x.DataElementName != "InvoiceNumber").ToList();
            }

            if (!records.Any(x => !String.IsNullOrWhiteSpace(x.PONumber))) {
                output.Columns = output.Columns.Where(x => x.DataElementName != "PONumber").ToList();
            }
        }

        output.ActionResponse.Result = true;

        return output;
    }

    // {{ModuleItemStart:Appointments}}
    public async Task<List<DataObjects.Invoice>> GetInvoicesForAppointment(Guid AppointmentId, bool UnpaidOnly = false, DataObjects.User? CurrentUser = null)
    {
        var output = new List<DataObjects.Invoice>();

        var query = data.Invoices.Where(x => x.AppointmentId == AppointmentId);

        if(CurrentUser == null || !CurrentUser.Admin) {
            query = query.Where(x => x.Deleted != true);
        }

        if (UnpaidOnly) {
            query = query.Where(x => x.InvoiceClosed != null);
        }

        var recs = await query.ToListAsync();

        if (recs != null && recs.Any()) {
            foreach (var rec in recs) {
                var invoiceItems = DeserializeObject<List<DataObjects.InvoiceItem>>(rec.Items);

                var i = new DataObjects.Invoice {
                    ActionResponse = GetNewActionResponse(true),
                    InvoiceId = rec.InvoiceId,
                    TenantId = rec.TenantId,
                    InvoiceNumber = rec.InvoiceNumber,
                    PONumber = rec.PONumber,
                    AppointmentId = rec.AppointmentId,
                    UserId = rec.UserId,
                    Title = rec.Title,
                    InvoiceItems = invoiceItems != null && invoiceItems.Any() ? invoiceItems : new List<DataObjects.InvoiceItem>(),
                    Notes = rec.Notes,
                    InvoiceCreated = rec.InvoiceCreated,
                    InvoiceDueDate = rec.InvoiceDueDate,
                    InvoiceSendDate = rec.InvoiceSendDate,
                    InvoiceSent = rec.InvoiceSent,
                    InvoiceClosed = rec.InvoiceClosed,
                    Total = rec.Total,
                    Added = rec.Added,
                    AddedBy = LastModifiedDisplayName(rec.AddedBy),
                    LastModified = rec.LastModified,
                    LastModifiedBy = LastModifiedDisplayName(rec.LastModifiedBy),
                    Deleted = rec.Deleted,
                    DeletedAt = rec.DeletedAt,
                };

                GetDataApp(rec, i, CurrentUser);

                output.Add(i);
            }
        }

        return output;
    }
    // {{ModuleItemEnd:Appointments}}

    public async Task<List<DataObjects.Invoice>> GetInvoicesForUser(Guid UserId, bool UnpaidOnly = false, DataObjects.User? CurrentUser = null)
    {
        var output = new List<DataObjects.Invoice>();

        var query = data.Invoices.Where(x => x.UserId == UserId);

        if (CurrentUser == null || !CurrentUser.Admin) {
            query = query.Where(x => x.Deleted != true);
        }

        if (UnpaidOnly) {
            query = query.Where(x => x.InvoiceClosed != null);
        }

        var recs = await query.ToListAsync();

        if (recs != null && recs.Any()) {
            foreach (var rec in recs) {
                var invoiceItems = DeserializeObject<List<DataObjects.InvoiceItem>>(rec.Items);

                var i = new DataObjects.Invoice {
                    ActionResponse = GetNewActionResponse(true),
                    InvoiceId = rec.InvoiceId,
                    TenantId = rec.TenantId,
                    InvoiceNumber = rec.InvoiceNumber,
                    PONumber = rec.PONumber,
                    // {{ModuleItemStart:Appointments}}
                    AppointmentId = rec.AppointmentId,
                    // {{ModuleItemEnd:Appointments}}
                    UserId = rec.UserId,
                    Title = rec.Title,
                    InvoiceItems = invoiceItems != null && invoiceItems.Any() ? invoiceItems : new List<DataObjects.InvoiceItem>(),
                    Notes = rec.Notes,
                    InvoiceCreated = rec.InvoiceCreated,
                    InvoiceDueDate = rec.InvoiceDueDate,
                    InvoiceSendDate = rec.InvoiceSendDate,
                    InvoiceSent = rec.InvoiceSent,
                    InvoiceClosed = rec.InvoiceClosed,
                    Total = rec.Total,
                    Added = rec.Added,
                    AddedBy = LastModifiedDisplayName(rec.AddedBy),
                    LastModified = rec.LastModified,
                    LastModifiedBy = LastModifiedDisplayName(rec.LastModifiedBy),
                    Deleted = rec.Deleted,
                    DeletedAt = rec.DeletedAt,
                };

                GetDataApp(rec, i, CurrentUser);

                output.Add(i);
            }
        }

        return output;
    }

    public async Task<DataObjects.Invoice> SaveInvoice(DataObjects.Invoice invoice, DataObjects.User? CurrentUser = null)
    {
        var output = invoice;
        output.ActionResponse = GetNewActionResponse();

        bool newRecord = false;
        DateTime now = DateTime.UtcNow;

        var rec = await data.Invoices.FirstOrDefaultAsync(x => x.InvoiceId == output.InvoiceId);

        if (rec != null && rec.Deleted) {
            if (AdminUser(CurrentUser)) {
                // Ok to edit this record that is marked as deleted.
            } else {
                output.ActionResponse.Messages.Add("Invoice '" + output.InvoiceId.ToString() + "' No Longer Exists");
                return output;
            }
        }

        if (rec == null) {
            if (output.InvoiceId == Guid.Empty) {
                newRecord = true;
                output.InvoiceId = Guid.NewGuid();

                rec = new Invoice {
                    InvoiceId = output.InvoiceId,
                    TenantId = output.TenantId,
                    InvoiceCreated = now,
                    Deleted = false,
                    Added = now,
                    AddedBy = CurrentUserIdString(CurrentUser),
                };
            } else {
                output.ActionResponse.Messages.Add("Invoice '" + output.InvoiceId.ToString() + "' No Longer Exists");
                return output;
            }
        }

        output.InvoiceNumber = MaxStringLength(output.InvoiceNumber, 100);
        output.PONumber = MaxStringLength(output.PONumber, 100);
        output.Title = MaxStringLength(output.Title, 255);

        // {{ModuleItemStart:Appointments}}
        rec.AppointmentId = output.AppointmentId;
        // {{ModuleItemEnd:Appointments}}
        rec.InvoiceNumber = output.InvoiceNumber;
        rec.PONumber = output.PONumber;
        rec.UserId = output.UserId;
        rec.Title = output.Title;
        rec.Items = SerializeObject(output.InvoiceItems);
        rec.Notes = output.Notes;
        rec.InvoiceDueDate = output.InvoiceDueDate;
        rec.InvoiceSendDate = output.InvoiceSendDate;
        rec.InvoiceSent = output.InvoiceSent;
        rec.InvoiceClosed = output.InvoiceClosed;
        rec.Total = output.Total;

        rec.LastModified = now;
        rec.LastModifiedBy = CurrentUserIdString(CurrentUser);

        if (AdminUser(CurrentUser)) {
            rec.Deleted = output.Deleted;

            if (!output.Deleted) {
                rec.DeletedAt = null;
            }
        }

        SaveDataApp(rec, output, CurrentUser);

        try {
            if (newRecord) {
                await data.Invoices.AddAsync(rec);
            }
            await data.SaveChangesAsync();

            output.ActionResponse.Result = true;

            await SignalRUpdate(new DataObjects.SignalRUpdate {
                TenantId = output.TenantId,
                ItemId = output.InvoiceId,
                UpdateType = DataObjects.SignalRUpdateType.Invoice,
                Message = "Saved",
                Object = output,
                UserId = CurrentUserId(CurrentUser),
            });
        } catch (Exception ex) {
            output.ActionResponse.Messages.Add("Error Saving Invoice " + output.InvoiceId.ToString() + ":");
            output.ActionResponse.Messages.AddRange(RecurseException(ex));
        }

        return output;
    }
}