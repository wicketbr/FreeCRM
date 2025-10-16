using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CRM;

public partial interface IDataAccess
{
    Task<DataObjects.BooleanResponse> DeleteAppointment(Guid AppointmentId, DataObjects.User? CurrentUser = null, bool ForceDeleteImmediately = false);
    Task<DataObjects.BooleanResponse> DeleteAppointmentNote(Guid AppointmentNoteId, DataObjects.User? CurrentUser = null, bool ForceDeleteImmediately = false);
    Task<DataObjects.BooleanResponse> DeleteAppointmentService(Guid AppointmentServiceId, DataObjects.User? CurrentUser = null, bool ForceDeleteImmediately = false);
    Task<DataObjects.Appointment> GetAppointment(Guid AppointmentId, DataObjects.User? CurrentUser = null);
    // {{ModuleItemStart:Invoices}}
    Task<DataObjects.Appointment> GetAppointmentWithInvoices(Guid AppointmentId, DataObjects.User? CurrentUser = null);
    Task<List<DataObjects.Invoice>> GetAppointmentInvoices(Guid AppointmentId);
    // {{ModuleItemEnd:Invoices}}
    Task<DataObjects.AppointmentNote> GetAppointmentNote(Guid AppointmentNoteId);
    Task<List<DataObjects.AppointmentNote>> GetAppointmentNotes(Guid AppointmentId);
    Task<List<DataObjects.Appointment>> GetAppointments(DataObjects.AppoinmentLoader loader, DataObjects.User? CurrentUser = null);
    Task<DataObjects.Appointment> SaveAppointment(DataObjects.Appointment appointment, DataObjects.User? CurrentUser = null);
    // {{ModuleItemStart:Invoices}}
    Task<DataObjects.Appointment> SaveAppointmentInvoices(DataObjects.Appointment appointment, DataObjects.User? CurrentUser = null);
    // {{ModuleItemEnd:Invoices}}
    Task<DataObjects.AppointmentNote> SaveAppointmentNote(DataObjects.AppointmentNote AppointmentNote, DataObjects.User? CurrentUser = null);
    Task<DataObjects.AppointmentAttendanceUpdate> UpdateUserAttendance(DataObjects.AppointmentAttendanceUpdate update);

}

public partial class DataAccess
{
    public async Task<DataObjects.BooleanResponse> DeleteAppointment(Guid AppointmentId, DataObjects.User? CurrentUser = null, bool ForceDeleteImmediately = false)
    {
        var output = new DataObjects.BooleanResponse();

        var rec = await data.Appointments.FirstOrDefaultAsync(x => x.AppointmentId == AppointmentId);
        if(rec == null) {
            output.Messages.Add("Error Deleting Appointment '" + AppointmentId.ToString() + "' - Record No Longer Exists");
            return output;
        }

        var now = DateTime.UtcNow;
        Guid tenantId = GuidValue(rec.TenantId);
        var tenantSettings = GetTenantSettings(tenantId);

        if (ForceDeleteImmediately || tenantSettings.DeletePreference == DataObjects.DeletePreference.Immediate) {
            // First, fix or delete all relational user records
            var deleteAppRecords = await DeleteRecordsApp(rec, CurrentUser);
            if (!deleteAppRecords.Result) {
                output.Messages.AddRange(deleteAppRecords.Messages);
                return output;
            }

            try {
                data.AppointmentNotes.RemoveRange(data.AppointmentNotes.Where(x => x.AppointmentId == AppointmentId));
                data.AppointmentServices.RemoveRange(data.AppointmentServices.Where(x => x.AppointmentId == AppointmentId));
                data.AppointmentUsers.RemoveRange(data.AppointmentUsers.Where(x => x.AppointmentId == AppointmentId));
                // {{ModuleItemStart:Tags}}
                data.TagItems.RemoveRange(data.TagItems.Where(x => x.ItemId == AppointmentId));
                // {{ModuleItemEnd:Tags}}

                await data.SaveChangesAsync();
            }catch (Exception ex) {
                output.Messages.Add("Error Deleting Related Appointment Records for User " + AppointmentId.ToString() + ":");
                output.Messages.AddRange(RecurseException(ex));
                return output;
            }

            try {
                data.Appointments.Remove(rec);
                await data.SaveChangesAsync();

                output.Result = true;
            }catch(Exception ex) {
                output.Messages.Add("Error Deleting Appointment " + AppointmentId.ToString() + ":");
                output.Messages.AddRange(RecurseException(ex));
            }
        } else {
            // Just mark the event for delete
            try {
                rec.Deleted = true;
                rec.DeletedAt = now;
                rec.LastModified = now;

                if (CurrentUser != null) {
                    rec.LastModifiedBy = CurrentUserIdString(CurrentUser);
                }

                await data.SaveChangesAsync();
                output.Result = true;
            } catch( Exception ex ) {
                output.Messages.Add("Error Deleting Appointment " + AppointmentId.ToString() + ":");
                output.Messages.AddRange(RecurseException(ex));
            }
        }

        if (!ForceDeleteImmediately) {
            await SignalRUpdate(new DataObjects.SignalRUpdate {
                TenantId = tenantId,
                ItemId = AppointmentId,
                UpdateType = DataObjects.SignalRUpdateType.Appointment,
                Message = "Deleted",
                UserId = CurrentUserId(CurrentUser),
            });
        }

        return output;
    }

    public async Task<DataObjects.BooleanResponse> DeleteAppointmentNote(Guid AppointmentNoteId, DataObjects.User? CurrentUser = null, bool ForceDeleteImmediately = false){
        var output = new DataObjects.BooleanResponse();

        var rec = await data.AppointmentNotes.FirstOrDefaultAsync(x => x.AppointmentNoteId == AppointmentNoteId);
        if(rec == null) {
            output.Messages.Add("Error Deleting Appointment Note '" + AppointmentNoteId.ToString() + "' - Record No Longer Exists");
            return output;
        }

        var now = DateTime.UtcNow;
        Guid tenantId = GuidValue(rec.TenantId);
        var tenantSettings = GetTenantSettings(tenantId);

        if(ForceDeleteImmediately || tenantSettings.DeletePreference == DataObjects.DeletePreference.Immediate) {
            var deleteAppRecords = await DeleteRecordsApp(rec, CurrentUser);
            if (!deleteAppRecords.Result) {
                output.Messages.AddRange(deleteAppRecords.Messages);
                return output;
            }

            try {
                data.AppointmentNotes.Remove(rec);
                await data.SaveChangesAsync();
                output.Result = true;
            }catch(Exception ex ) {
                output.Messages.Add("Error Deleting Appointment Note " + AppointmentNoteId.ToString() + ":");
                output.Messages.AddRange(RecurseException(ex));
            }
        } else {
            try {
                rec.Deleted = true;
                rec.DeletedAt = now;
                rec.LastModified = now;

                if(CurrentUser != null) {
                    rec.LastModifiedBy = CurrentUserIdString(CurrentUser);
                }

                await data.SaveChangesAsync();
                output.Result = true;
            }catch(Exception ex) {
                output.Messages.Add("Error Deleting Appointment Note " + AppointmentNoteId.ToString() + ":");
                output.Messages.AddRange(RecurseException(ex));
            }
        }

        if (!ForceDeleteImmediately) {
            await SignalRUpdate(new DataObjects.SignalRUpdate {
                TenantId = tenantId,
                ItemId = AppointmentNoteId,
                UpdateType = DataObjects.SignalRUpdateType.AppointmentNote,
                Message = "Deleted",
                UserId = CurrentUserId(CurrentUser),
            });
        }

        return output;
    }

    public async Task<DataObjects.BooleanResponse> DeleteAppointmentService(Guid AppointmentServiceId, DataObjects.User? CurrentUser = null, bool ForceDeleteImmediately = false)
    {
        var output = new DataObjects.BooleanResponse();

        var rec = await data.AppointmentServices.FirstOrDefaultAsync(x => x.AppointmentServiceId == AppointmentServiceId);
        if (rec == null) {
            output.Messages.Add("Error Deleting Appointment Service '" + AppointmentServiceId.ToString() + "' - Record No Longer Exists");
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
                data.AppointmentServices.Remove(rec);
                await data.SaveChangesAsync();
                output.Result = true;
            } catch (Exception ex) {
                output.Messages.Add("Error Deleting Appointment Service " + AppointmentServiceId.ToString() + ":");
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
                output.Messages.Add("Error Deleting Appointment Service " + AppointmentServiceId.ToString() + ":");
                output.Messages.AddRange(RecurseException(ex));
            }
        }

        if (!ForceDeleteImmediately) {
            await SignalRUpdate(new DataObjects.SignalRUpdate {
                TenantId = tenantId,
                ItemId = AppointmentServiceId,
                UpdateType = DataObjects.SignalRUpdateType.AppointmentService,
                Message = "Deleted",
                UserId = CurrentUserId(CurrentUser),
            });
        }

        return output;
    }

    public async Task<DataObjects.Appointment> GetAppointment(Guid AppointmentId, DataObjects.User? CurrentUser = null)
    {
        var output = new DataObjects.Appointment();

        Appointment? rec = null;

        if(AdminUser(CurrentUser)) {
            rec = await data.Appointments
                .Include(x => x.AppointmentUsers).ThenInclude(x => x.User)
                .Include(x => x.AppointmentServices)
                .FirstOrDefaultAsync(x => x.AppointmentId == AppointmentId);
        } else {
            rec = await data.Appointments
                .Include(x => x.AppointmentUsers).ThenInclude(x => x.User)
                .Include(x => x.AppointmentServices)
                .FirstOrDefaultAsync(x => x.AppointmentId == AppointmentId && x.Deleted != true);
        }

        if(rec != null) {
            output = new DataObjects.Appointment { 
                ActionResponse = GetNewActionResponse(true),
                AppointmentId = rec.AppointmentId,
                TenantId = rec.TenantId,
                Added = rec.Added,
                AddedBy = LastModifiedDisplayName(rec.AddedBy),
                AllDay = rec.AllDay,
                BackgroundColor = rec.BackgroundColor,
                Deleted = rec.Deleted,
                DeletedAt = rec.DeletedAt,
                End = rec.End,
                ForegroundColor = rec.ForegroundColor,
                LastModified = rec.LastModified,
                LastModifiedBy = LastModifiedDisplayName(rec.LastModifiedBy),
                LocationId = rec.LocationId,
                Meeting = rec.Meeting,
                Note = rec.Note,
                Notes = await GetAppointmentNotes(AppointmentId),
                Services = new List<DataObjects.AppointmentService>(),
                Start = rec.Start,
                // {{ModuleItemStart:Tags}}
                Tags = await GetTagsForItem(rec.TenantId, AppointmentId),
                // {{ModuleItemEnd:Tags}}
                Title = rec.Title,
                Users = new List<DataObjects.AppointmentUser>(),
            };

            GetDataApp(rec, output, CurrentUser);

            if(rec.AppointmentUsers != null && rec.AppointmentUsers.Any()) {
                foreach(var apptUser in rec.AppointmentUsers) {
                    var u = new DataObjects.AppointmentUser {
                        UserId = apptUser.UserId,
                        AttendanceCode = StringValue(apptUser.AttendanceCode),
                        DisplayName = apptUser.User != null
                            ? apptUser.User.FirstName + " " + apptUser.User.LastName
                            : String.Empty,
                        Fees = apptUser.Fees.HasValue ? (decimal)apptUser.Fees : 0,
                    };

                    GetDataApp(apptUser, u, CurrentUser);

                    output.Users.Add(u);
                }
            }

            if(rec.AppointmentServices != null && rec.AppointmentServices.Any()) {
                foreach(var service in rec.AppointmentServices) {
                    var s = new DataObjects.AppointmentService {
                        AppointmentServiceId = service.AppointmentServiceId,
                        ServiceId = service.ServiceId,
                        Fee = DecimalValue(service.Fee),
                        Deleted = service.Deleted,
                        DeletedAt = service.DeletedAt,
                        LastModified = service.LastModified,
                        LastModifiedBy = LastModifiedDisplayName(rec.LastModifiedBy),
                    };

                    GetDataApp(service, s, CurrentUser);

                    output.Services.Add(s);
                }
            }
        } else {
            output.ActionResponse.Messages.Add("Appointment '" + AppointmentId.ToString() + "' No Longer Exists");
        }

        return output;
    }

    // {{ModuleItemStart:Invoices}}
    public async Task<DataObjects.Appointment> GetAppointmentWithInvoices(Guid AppointmentId, DataObjects.User? CurrentUser = null)
    {
        var output = await GetAppointment(AppointmentId, CurrentUser);

        if (output.ActionResponse.Result) {
            output.Invoices = await GetAppointmentInvoices(output.AppointmentId);
        }

        return output;
    }

    public async Task<List<DataObjects.Invoice>> GetAppointmentInvoices(Guid AppointmentId)
    {
        var output = new List<DataObjects.Invoice>();

        var recs = await data.Invoices
            .Include(x => x.Appointment)
            .Where(x => x.AppointmentId == AppointmentId).ToListAsync();

        if(recs != null && recs.Any()) {
            foreach(var rec in recs) {
                var invoiceItems = DeserializeObject<List<DataObjects.InvoiceItem>>(rec.Items);

                var i = new DataObjects.Invoice {
                    ActionResponse = GetNewActionResponse(true),
                    InvoiceId = rec.InvoiceId,
                    TenantId = rec.TenantId,
                    InvoiceNumber = rec.InvoiceNumber,
                    PONumber = rec.PONumber,
                    AppointmentId = rec.AppointmentId,
                    AppointmentDisplay = rec.Appointment != null
                        ? rec.Appointment.Title
                        : String.Empty,
                    UserId = rec.UserId,
                    UserDisplay = LastModifiedDisplayName(GuidValue(rec.UserId).ToString()),
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

                GetDataApp(rec, i);

                output.Add(i);
            }
        }

        return output;
    }
    // {{ModuleItemEnd:Invoices}}
    public async Task<DataObjects.AppointmentNote> GetAppointmentNote(Guid AppointmentNoteId)
    {
        var output = new DataObjects.AppointmentNote();

        var rec = await data.AppointmentNotes.FirstOrDefaultAsync(x => x.AppointmentNoteId ==  AppointmentNoteId);
        if(rec == null) {
            output.ActionResponse.Messages.Add("Error Loading Appointment Note '" + AppointmentNoteId.ToString() + "' - Record Not Found");
            return output;
        }

        output = new DataObjects.AppointmentNote { 
            ActionResponse = GetNewActionResponse(true),
            AppointmentNoteId = rec.AppointmentNoteId,
            Added = rec.Added,
            AddedBy = LastModifiedDisplayName(rec.AddedBy),
            AppointmentId = rec.AppointmentId,
            Deleted = rec.Deleted,
            DeletedAt = rec.DeletedAt,
            LastModified = rec.LastModified,
            LastModifiedBy = LastModifiedDisplayName(rec.LastModifiedBy),
            Note = rec.Note,
            TenantId = rec.TenantId,
        };

        GetDataApp(rec, output);

        return output;
    }

    public async Task<List<DataObjects.AppointmentNote>> GetAppointmentNotes(Guid AppointmentId)
    {
        List<DataObjects.AppointmentNote> output = new List<DataObjects.AppointmentNote>();

        var recs = await data.AppointmentNotes.Where(x => x.AppointmentId == AppointmentId).ToListAsync();
        if(recs != null && recs.Any()) {
            foreach(var rec in recs) {
                var n = new DataObjects.AppointmentNote {
                    Added = rec.Added,
                    AddedBy = LastModifiedDisplayName(rec.AddedBy),
                    AppointmentId = rec.AppointmentId,
                    AppointmentNoteId = rec.AppointmentNoteId,
                    Deleted = rec.Deleted,
                    DeletedAt = rec.DeletedAt,
                    LastModified = rec.LastModified,
                    LastModifiedBy = LastModifiedDisplayName(rec.LastModifiedBy),
                    Note = rec.Note,
                    TenantId = rec.TenantId,
                };

                GetDataApp(rec, n);

                output.Add(n);
            }
        }

        return output;
    }

    public async Task<List<DataObjects.Appointment>> GetAppointments(DataObjects.AppoinmentLoader loader, DataObjects.User? CurrentUser = null)
    {
        var output = new List<DataObjects.Appointment>();

        DateTime start = loader.Start.HasValue
            ? ((DateTime)loader.Start).AddDays(-2)
            : DateTime.UtcNow.AddDays(-2);

        DateTime end = loader.End.HasValue
            ? ((DateTime)loader.End).AddDays(2)
            : DateTime.UtcNow.AddDays(2);

        var recs = await data.Appointments
            .Include(x => x.AppointmentUsers)
            .Include(x => x.AppointmentServices)
            .Where(x => x.TenantId == loader.TenantId && x.Start >= start && x.End <= end)
            .ToListAsync();

        if(recs != null && recs.Any()) {
            foreach(var rec in recs) {
                var appt = new DataObjects.Appointment { 
                    ActionResponse = GetNewActionResponse(true),
                    AppointmentId = rec.AppointmentId,
                    TenantId = rec.TenantId,
                    Added = rec.Added,
                    AddedBy = rec.AddedBy,
                    AllDay = rec.AllDay,
                    BackgroundColor = rec.BackgroundColor,
                    Deleted = rec.Deleted,
                    DeletedAt = rec.DeletedAt,
                    End = rec.End,
                    ForegroundColor = rec.ForegroundColor,
                    LastModified = rec.LastModified,
                    LastModifiedBy = rec.LastModifiedBy,
                    LocationId = rec.LocationId,
                    Meeting = rec.Meeting,
                    Note = rec.Note,
                    Notes = new List<DataObjects.AppointmentNote>(),
                    Services = new List<DataObjects.AppointmentService>(),
                    Start = rec.Start,
                    // {{ModuleItemStart:Tags}}
                    Tags = await GetTagsForItem(rec.TenantId, rec.AppointmentId),
                    // {{ModuleItemEnd:Tags}}
                    Title = rec.Title,
                    Users = new List<DataObjects.AppointmentUser>(),
                };

                GetDataApp(rec, appt, CurrentUser);

                if (rec.AppointmentUsers != null && rec.AppointmentUsers.Any()) {
                    foreach(var user in rec.AppointmentUsers) {
                        var u = new DataObjects.AppointmentUser {
                            UserId = user.UserId,
                            AttendanceCode = StringValue(user.AttendanceCode),
                            Fees = DecimalValue(user.Fees),
                        };

                        GetDataApp(user, u, CurrentUser);

                        appt.Users.Add(u);
                    }
                }

                if(rec.AppointmentServices != null && rec.AppointmentServices.Any()) {
                    foreach (var service in rec.AppointmentServices) {
                        var s = new DataObjects.AppointmentService {
                            AppointmentServiceId = service.AppointmentServiceId,
                            Deleted = service.Deleted,
                            DeletedAt = service.DeletedAt,
                            Fee = DecimalValue(service.Fee),
                            LastModified = service.LastModified,
                            //LastModifiedBy = LastModifiedDisplayName(service.LastModifiedBy),
                            ServiceId = service.ServiceId,
                        };

                        GetDataApp(service, s, CurrentUser);

                        appt.Services.Add(s);
                    }
                }
                    

                output.Add(appt);
            }
        }

        return output;
    }

    public async Task<DataObjects.Appointment> SaveAppointment(DataObjects.Appointment appointment, DataObjects.User? CurrentUser = null)
    {
        var output = appointment;
        output.ActionResponse = GetNewActionResponse();

        bool newRecord = false;
        DateTime now = DateTime.UtcNow;

        var rec = await data.Appointments.FirstOrDefaultAsync(x => x.AppointmentId == output.AppointmentId);

        if(rec != null && rec.Deleted) {
            if(AdminUser(CurrentUser)) {
                // Ok to edit this record that is marked as deleted.
            } else {
                output.ActionResponse.Messages.Add("Appointment '" + output.AppointmentId.ToString() + "' No Longer Exists");
                return output;
            }
        }

        if(rec == null) {
            if(appointment.AppointmentId == Guid.Empty) {
                newRecord = true;
                output.AppointmentId = Guid.NewGuid();
                rec = new Appointment { 
                    AppointmentId = output.AppointmentId,
                    TenantId = output.TenantId,
                    Deleted = false,
                    Added = now,
                    AddedBy = CurrentUserIdString(CurrentUser),
                };
            } else {
                output.ActionResponse.Messages.Add("Appointment '" + output.AppointmentId.ToString() + "' No Longer Exists");
                return output;
            }
        }

        output.Title = MaxStringLength(output.Title, 200);
        output.ForegroundColor = MaxStringLength(output.ForegroundColor, 100);
        output.BackgroundColor = MaxStringLength(output.BackgroundColor, 100);

        // Convert the dates back to UTC
        output.Start = output.Start.ToUniversalTime();
        output.End = output.End.ToUniversalTime();

        rec.Title = output.Title;
        rec.Start = output.Start;
        rec.End = output.End;
        rec.AllDay = output.AllDay;
        rec.Meeting = output.Meeting;
        rec.Note = output.Note;
        rec.LocationId = output.LocationId;
        rec.LastModified = now;
        rec.LastModifiedBy = CurrentUserIdString(CurrentUser);
        rec.BackgroundColor = output.BackgroundColor;
        rec.ForegroundColor = output.ForegroundColor;

        if (AdminUser(CurrentUser)) {
            rec.Deleted = output.Deleted;
        }

        SaveDataApp(rec, output, CurrentUser);

        try {
            if (newRecord) {
                await data.Appointments.AddAsync(rec);
            }
            await data.SaveChangesAsync();

            output.Users = await SaveAppointmentUsers(output);
            output.Services = await SaveAppointmentServices(output, CurrentUser);

            // {{ModuleItemStart:Tags}}
            await SaveItemTags(output.TenantId, output.AppointmentId, output.Tags);
            // {{ModuleItemEnd:Tags}}

            output.ActionResponse.Result = true;

            var outputObject = await GetAppointment(output.AppointmentId, CurrentUser);

            await SignalRUpdate(new DataObjects.SignalRUpdate {
                TenantId = output.TenantId,
                ItemId = output.AppointmentId,
                UpdateType = DataObjects.SignalRUpdateType.Appointment,
                Message = "Saved",
                UserId = CurrentUserId(CurrentUser),
                Object = outputObject,
            });
        } catch (Exception ex) {
            output.ActionResponse.Messages.Add("Error Saving Appointment " + output.AppointmentId.ToString() + ":");
            output.ActionResponse.Messages.AddRange(RecurseException(ex));
        }


        return output;
    }

    // {{ModuleItemStart:Invoices}}
    public async Task<DataObjects.Appointment> SaveAppointmentInvoices(DataObjects.Appointment appointment, DataObjects.User? CurrentUser = null)
    {
        var output = appointment;
        output.ActionResponse = GetNewActionResponse();

        if (appointment.Invoices.Any()) {
            foreach(var invoice in appointment.Invoices) {
                var saved = await SaveInvoice(invoice, CurrentUser);
                if (!saved.ActionResponse.Result) {
                    if (saved.ActionResponse.Messages.Any()) {
                        foreach(var msg in saved.ActionResponse.Messages) {
                            output.ActionResponse.Messages.Add(msg);
                        }
                    }
                }
            }
        }

        output.ActionResponse.Result = output.ActionResponse.Messages.Count() == 0;

        return output;
    }
    // {{ModuleItemEnd:Invoices}}

    public async Task<DataObjects.AppointmentNote> SaveAppointmentNote(DataObjects.AppointmentNote AppointmentNote, DataObjects.User? CurrentUser = null)
    {
        var output = AppointmentNote;
        output.ActionResponse = GetNewActionResponse();

        bool newRecord = false;
        bool modified = false;
        DateTime now = DateTime.UtcNow;

        var rec = await data.AppointmentNotes.FirstOrDefaultAsync(x => x.AppointmentNoteId == output.AppointmentNoteId);
        if(rec == null) {
            if(output.AppointmentNoteId == Guid.Empty) {
                newRecord = true;

                output.AppointmentNoteId = Guid.NewGuid();

                rec = new AppointmentNote { 
                    AppointmentNoteId = output.AppointmentNoteId,
                    AppointmentId = output.AppointmentId,
                    TenantId = output.TenantId,
                    Added = now,
                    AddedBy = CurrentUserIdString(CurrentUser),
                };
            } else {
                output.ActionResponse.Messages.Add("Error Saving Appointment Note '" + AppointmentNote.AppointmentNoteId.ToString() + "' - Record No Longer Exists");
                return output;
            }
        }

        if(rec.Note != output.Note) {
            rec.Note = output.Note;
            modified = true;
        }

        if(newRecord || modified) {
            rec.LastModified = now;
            rec.LastModifiedBy = CurrentUserIdString(CurrentUser);
        }

        if(CurrentUser != null && CurrentUser.ManageAppointments) {
            rec.Deleted = output.Deleted;
        }

        SaveDataApp(rec, output, CurrentUser);

        try {
            if (newRecord) {
                await data.AppointmentNotes.AddAsync(rec);
            }
            await data.SaveChangesAsync();
            output.ActionResponse.Result = true;

            var outputObject = await GetAppointmentNote(output.AppointmentNoteId);

            await SignalRUpdate(new DataObjects.SignalRUpdate {
                TenantId = output.TenantId,
                ItemId = output.AppointmentNoteId,
                UpdateType = DataObjects.SignalRUpdateType.AppointmentNote,
                Message = "Saved",
                UserId = CurrentUserId(CurrentUser),
                Object = outputObject,
            });
        } catch(Exception ex) {
            output.ActionResponse.Messages.Add("Error Saving Appointment Note " + AppointmentNote.AppointmentNoteId.ToString() + ":");
            output.ActionResponse.Messages.AddRange(RecurseException(ex));
        }

        if (output.ActionResponse.Result) {
            output = await GetAppointmentNote(output.AppointmentNoteId);
        }

        return output;
    }

    protected async Task<List<DataObjects.AppointmentService>> SaveAppointmentServices(DataObjects.Appointment appointment, DataObjects.User? CurrentUser = null)
    {
        List<DataObjects.AppointmentService> output = new List<DataObjects.AppointmentService>();

        // First, remove any records that should no longer be included.
        List<Guid> keep = new List<Guid>();

        if (appointment.Services.Any()) {
            keep = appointment.Services.Select(x => x.AppointmentServiceId).ToList();
        }

        data.AppointmentServices.RemoveRange(data.AppointmentServices.Where(x => x.AppointmentId == appointment.AppointmentId && !keep.Contains(x.AppointmentServiceId)));
        await data.SaveChangesAsync();

        // Now, make sure we have an updated record for every users.
        DateTime now = DateTime.UtcNow;

        foreach (var service in appointment.Services) {
            bool newRecord = false;

            var rec = await data.AppointmentServices.FirstOrDefaultAsync(x => x.AppointmentId == appointment.AppointmentId && x.ServiceId == service.ServiceId);
            if (rec == null) {
                service.AppointmentServiceId = Guid.NewGuid();

                rec = new AppointmentService {
                    AppointmentServiceId = service.AppointmentServiceId,
                    AppointmentId = appointment.AppointmentId,
                    TenantId = appointment.TenantId,
                    Deleted = false,
                };
                newRecord = true;
            }

            rec.ServiceId = service.ServiceId;
            rec.Fee = service.Fee;
            rec.LastModified = now;
            rec.LastModifiedBy = CurrentUserIdString(CurrentUser);

            SaveDataApp(rec, service, CurrentUser);

            if(rec.Deleted != service.Deleted) {
                rec.Deleted = service.Deleted;

                if (service.Deleted) {
                    rec.DeletedAt = now;
                } else {
                    rec.DeletedAt = null;
                }
            }

            if (newRecord) {
                await data.AppointmentServices.AddAsync(rec);
            }

            await data.SaveChangesAsync();

            output.Add(service);
        }

        return output;
    }

    protected async Task<List<DataObjects.AppointmentUser>> SaveAppointmentUsers(DataObjects.Appointment appointment)
    {
        List<DataObjects.AppointmentUser> output = new List<DataObjects.AppointmentUser>();

        // First, remove any records that should no longer be included.
        List<Guid> keep = new List<Guid>();

        if (appointment.Users.Any()) {
            keep = appointment.Users.Select(x => x.UserId).ToList();
        }

        data.AppointmentUsers.RemoveRange(data.AppointmentUsers.Where(x => x.AppointmentId == appointment.AppointmentId && !keep.Contains(x.UserId)));
        await data.SaveChangesAsync();

        // Now, make sure we have an updated record for every users.
        foreach(var user in appointment.Users) {
            bool newRecord = false;


            var rec = await data.AppointmentUsers.FirstOrDefaultAsync(x => x.AppointmentId == appointment.AppointmentId && x.UserId == user.UserId);
            if(rec == null) {
                rec = new AppointmentUser { 
                    AppointmentUserId = Guid.NewGuid(),
                    AppointmentId = appointment.AppointmentId,
                    TenantId = appointment.TenantId,
                    UserId = user.UserId,
                };
                newRecord = true;
            }

            rec.AttendanceCode = user.AttendanceCode;
            rec.Fees = user.Fees;

            SaveDataApp(rec, user);

            if (newRecord) {
                await data.AppointmentUsers.AddAsync(rec);
            }

            await data.SaveChangesAsync();
        }

        return output;
    }

    public async Task<DataObjects.AppointmentAttendanceUpdate> UpdateUserAttendance(DataObjects.AppointmentAttendanceUpdate update)
    {
        DataObjects.AppointmentAttendanceUpdate output = update;
        output.ActionResponse = GetNewActionResponse();

        var rec = await data.AppointmentUsers.FirstOrDefaultAsync(x => x.AppointmentId == output.AppointmentId && x.UserId == output.UserId);
        if(rec != null) {
            try {
                rec.AttendanceCode = output.AttendanceCode;
                await data.SaveChangesAsync();

                output.ActionResponse.Result = true;
            } catch(Exception ex) {
                output.ActionResponse.Messages.Add("Error Updating User Attendance:");
                output.ActionResponse.Messages.AddRange(RecurseException(ex));
            }
        } else {
            output.ActionResponse.Messages.Add("Record Not Found");
        }

        return output;
    }
}