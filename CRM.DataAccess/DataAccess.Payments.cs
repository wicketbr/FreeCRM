using Microsoft.Graph.Models;

namespace CRM;

public partial interface IDataAccess
{
    Task<DataObjects.BooleanResponse> DeletePayment(Guid PaymentId, DataObjects.User? CurrentUser = null, bool ForceDeleteImmediately = false);
    Task<DataObjects.Payment> GetPayment(Guid PaymentId, DataObjects.User? CurrentUser = null);
    Task<List<DataObjects.Payment>> GetPayments(Guid TenantId, DataObjects.User? CurrentUser = null);
    Task<List<DataObjects.Payment>> GetPaymentsForUser(Guid UserId, DataObjects.User? CurrentUser = null);
    Task<DataObjects.Payment> SavePayment(DataObjects.Payment payment, DataObjects.User? CurrentUser = null);
}

public partial class DataAccess
{
    public async Task<DataObjects.BooleanResponse> DeletePayment(Guid PaymentId, DataObjects.User? CurrentUser = null, bool ForceDeleteImmediately = false)
    {
        var output = new DataObjects.BooleanResponse();

        var rec = await data.Payments.FirstOrDefaultAsync(x => x.PaymentId == PaymentId);
        if (rec == null) {
            output.Messages.Add("Error Deleting Payment '" + PaymentId.ToString() + "' - Record No Longer Exists");
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
                data.Payments.Remove(rec);
                await data.SaveChangesAsync();

                output.Result = true;
            } catch (Exception ex) {
                output.Messages.Add("Error Deleting Payment '" + PaymentId.ToString());
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
                output.Messages.Add("Error Deleting Payment " + PaymentId.ToString());
                output.Messages.AddRange(RecurseException(ex));
            }
        }

        if (!ForceDeleteImmediately) {
            await SignalRUpdate(new DataObjects.SignalRUpdate {
                TenantId = tenantId,
                ItemId = PaymentId,
                UpdateType = DataObjects.SignalRUpdateType.Payment,
                Message = "Deleted",
                UserId = CurrentUserId(CurrentUser),
            });
        }

        return output;
    }

    public async Task<DataObjects.Payment> GetPayment(Guid PaymentId, DataObjects.User? CurrentUser = null)
    {
        var output = new DataObjects.Payment();

        Payment? rec = null;

        if (AdminUser(CurrentUser)) {
            rec = await data.Payments.FirstOrDefaultAsync(x => x.PaymentId == PaymentId);
        } else {
            rec = await data.Payments.FirstOrDefaultAsync(x => x.PaymentId == PaymentId && x.Deleted != true);
        }

        if (rec != null) {
            output = new DataObjects.Payment {
                ActionResponse = GetNewActionResponse(true),
                PaymentId = rec.PaymentId,
                TenantId = rec.TenantId,
                InvoiceId = rec.InvoiceId,
                UserId = rec.UserId,
                Notes = rec.Notes,
                PaymentDate = rec.PaymentDate,
                Amount = rec.Amount,
                Refunded = rec.Refunded,
                RefundedBy = rec.RefundedBy,
                RefundDate = rec.RefundDate,
                Added = rec.Added,
                AddedBy = LastModifiedDisplayName(rec.AddedBy),
                LastModified = rec.LastModified,
                LastModifiedBy = LastModifiedDisplayName(rec.LastModifiedBy),
                Deleted = rec.Deleted,
                DeletedAt = rec.DeletedAt,
            };

            GetDataApp(rec, output, CurrentUser);
        } else {
            output.ActionResponse.Messages.Add("Payment '" + PaymentId.ToString() + "' No Longer Exists");
        }

        return output;
    }

    public async Task<List<DataObjects.Payment>> GetPayments(Guid TenantId, DataObjects.User? CurrentUser = null)
    {
        var output = new List<DataObjects.Payment>();

        List<Payment>? recs = null;

        if(AdminUser(CurrentUser)) {
            recs = await data.Payments.Where(x => x.TenantId == TenantId).ToListAsync();
        } else {
            recs = await data.Payments.Where(x => x.TenantId == TenantId && x.Deleted != true).ToListAsync();
        }

        if (recs != null && recs.Any()) {
            foreach (var rec in recs) {
                var p = new DataObjects.Payment {
                    ActionResponse = GetNewActionResponse(true),
                    PaymentId = rec.PaymentId,
                    TenantId = rec.TenantId,
                    InvoiceId = rec.InvoiceId,
                    UserId = rec.UserId,
                    Notes = rec.Notes,
                    PaymentDate = rec.PaymentDate,
                    Amount = rec.Amount,
                    Refunded = rec.Refunded,
                    RefundedBy = rec.RefundedBy,
                    RefundDate = rec.RefundDate,
                    Added = rec.Added,
                    AddedBy = LastModifiedDisplayName(rec.AddedBy),
                    LastModified = rec.LastModified,
                    LastModifiedBy = LastModifiedDisplayName(rec.LastModifiedBy),
                    Deleted = rec.Deleted,
                    DeletedAt = rec.DeletedAt,
                };

                GetDataApp(rec, p, CurrentUser);

                output.Add(p);
            }
        }

        return output;
    }

    public async Task<List<DataObjects.Payment>> GetPaymentsForUser(Guid UserId, DataObjects.User? CurrentUser = null)
    {
        var output = new List<DataObjects.Payment>();

        List<Payment>? recs = null;

        if (AdminUser(CurrentUser)) {
            recs = await data.Payments.Where(x => x.UserId == UserId).ToListAsync();
        } else {
            recs = await data.Payments.Where(x => x.UserId == UserId && x.Deleted != true).ToListAsync();
        }

        if (recs != null && recs.Any()) {
            foreach (var rec in recs) {
                var p = new DataObjects.Payment {
                    ActionResponse = GetNewActionResponse(true),
                    PaymentId = rec.PaymentId,
                    TenantId = rec.TenantId,
                    InvoiceId = rec.InvoiceId,
                    UserId = rec.UserId,
                    Notes = rec.Notes,
                    PaymentDate = rec.PaymentDate,
                    Amount = rec.Amount,
                    Refunded = rec.Refunded,
                    RefundedBy = rec.RefundedBy,
                    RefundDate = rec.RefundDate,
                    Added = rec.Added,
                    AddedBy = LastModifiedDisplayName(rec.AddedBy),
                    LastModified = rec.LastModified,
                    LastModifiedBy = LastModifiedDisplayName(rec.LastModifiedBy),
                    Deleted = rec.Deleted,
                    DeletedAt = rec.DeletedAt,
                };

                GetDataApp(rec, p, CurrentUser);

                output.Add(p);
            }
        }

        return output;
    }

    public async Task<DataObjects.Payment> SavePayment(DataObjects.Payment payment, DataObjects.User? CurrentUser = null)
    {
        var output = payment;
        output.ActionResponse = GetNewActionResponse();

        bool newRecord = false;
        DateTime now = DateTime.UtcNow;

        var rec = await data.Payments.FirstOrDefaultAsync(x => x.PaymentId == output.PaymentId);

        if (rec != null && rec.Deleted) {
            if (AdminUser(CurrentUser)) {
                // Ok to edit this record that is marked as deleted.
            } else {
                output.ActionResponse.Messages.Add("Payment '" + output.PaymentId.ToString() + "' No Longer Exists");
                return output;
            }
        }

        if (rec == null) {
            if (output.PaymentId == Guid.Empty) {
                newRecord = true;
                output.PaymentId = Guid.NewGuid();

                rec = new Payment {
                    PaymentId = output.PaymentId,
                    TenantId = output.TenantId,
                    InvoiceId = output.InvoiceId,
                    Deleted = false,
                    Added = now,
                    AddedBy = CurrentUserIdString(CurrentUser),
                };
            } else {
                output.ActionResponse.Messages.Add("Payment '" + output.PaymentId.ToString() + "' No Longer Exists");
                return output;
            }
        }

        rec.UserId = output.UserId;
        rec.Notes = output.Notes;
        rec.PaymentDate = output.PaymentDate;
        rec.Amount = output.Amount;
        rec.Refunded = output.Refunded;
        rec.RefundedBy = output.RefundedBy;
        rec.RefundDate = output.RefundDate;

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
                await data.Payments.AddAsync(rec);
            }
            await data.SaveChangesAsync();

            output.ActionResponse.Result = true;

            await SignalRUpdate(new DataObjects.SignalRUpdate {
                TenantId = output.TenantId,
                ItemId = output.PaymentId,
                UpdateType = DataObjects.SignalRUpdateType.Payment,
                Message = "Saved",
                Object = output,
                UserId = CurrentUserId(CurrentUser),
            });
        } catch (Exception ex) {
            output.ActionResponse.Messages.Add("Error Saving Payment '" + output.PaymentId.ToString() + "'");
            output.ActionResponse.Messages.AddRange(RecurseException(ex));
        }

        return output;
    }
}