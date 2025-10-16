namespace CRM;
public partial interface IDataAccess
{
    Task<DataObjects.BooleanResponse> DeleteService(Guid ServiceId, DataObjects.User? CurrentUser = null, bool ForceDeleteImmediately = false);
    Task<DataObjects.Service> GetService(Guid ServiceId, DataObjects.User? CurrentUser = null);
    Task<List<DataObjects.Service>> GetServices(Guid TenantId, DataObjects.User? CurrentUser = null);
    Task<DataObjects.Service> SaveService(DataObjects.Service service, DataObjects.User? CurrentUser = null);
}

public partial class DataAccess
{
    public async Task<DataObjects.BooleanResponse> DeleteService(Guid ServiceId, DataObjects.User? CurrentUser = null, bool ForceDeleteImmediately = false)
    {
        var output = new DataObjects.BooleanResponse();

        var rec = await data.Services.FirstOrDefaultAsync(x => x.ServiceId == ServiceId);
        if (rec == null) {
            output.Messages.Add("Error Deleting Service '" + ServiceId.ToString() + "' - Record No Longer Exists");
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
                // {{ModuleItemStart:Appointments}}
                data.AppointmentServices.RemoveRange(data.AppointmentServices.Where(x => x.ServiceId == ServiceId));
                // {{ModuleItemEnd:Appointments}}
                // {{ModuleItemStart:Tags}}
                data.TagItems.RemoveRange(data.TagItems.Where(x => x.ItemId == ServiceId));
                // {{ModuleItemEnd:Tags}}

                await data.SaveChangesAsync();
            } catch (Exception ex) {
                output.Messages.Add("Error Deleting Related Service Records for User " + ServiceId.ToString());
                output.Messages.AddRange(RecurseException(ex));
                return output;
            }

            try {
                data.Services.Remove(rec);
                await data.SaveChangesAsync();

                output.Result = true;
            } catch (Exception ex) {
                output.Messages.Add("Error Deleting Service " + ServiceId.ToString());
                output.Messages.AddRange(RecurseException(ex));
            }
        } else {
            // First, mark these as deleted in any ServiceAppointments
            await data.Database.ExecuteSqlRawAsync("UPDATE AppointmentServices SET Deleted=1 WHERE ServiceId={0}", ServiceId);

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
                output.Messages.Add("Error Deleting Service " + ServiceId.ToString());
                output.Messages.AddRange(RecurseException(ex));
            }
        }

        if (!ForceDeleteImmediately) {
            await SignalRUpdate(new DataObjects.SignalRUpdate {
                TenantId = tenantId,
                ItemId = ServiceId,
                UpdateType = DataObjects.SignalRUpdateType.Service,
                Message = "Deleted",
                UserId = CurrentUserId(CurrentUser),
            });
        }

        return output;
    }

    public async Task<DataObjects.Service> GetService(Guid ServiceId, DataObjects.User? CurrentUser = null)
    {
        var output = new DataObjects.Service();

        Service? rec = null;

        if (AdminUser(CurrentUser)) {
            rec = await data.Services
                .FirstOrDefaultAsync(x => x.ServiceId == ServiceId);
        } else {
            rec = await data.Services
                .FirstOrDefaultAsync(x => x.ServiceId == ServiceId && x.Deleted != true);
        }

        if (rec != null) {
            output = new DataObjects.Service {
                ActionResponse = GetNewActionResponse(true),
                Added = rec.Added,
                AddedBy = LastModifiedDisplayName(rec.AddedBy),
                Code = rec.Code,
                DefaultAppointmentDuration = rec.DefaultAppointmentDuration,
                DefaultService = rec.DefaultService,
                Deleted = rec.Deleted,
                DeletedAt = rec.DeletedAt,
                Description = rec.Description,
                Enabled = rec.Enabled,
                LastModified = rec.LastModified,
                LastModifiedBy = LastModifiedDisplayName(rec.LastModifiedBy),
                Rate = rec.Rate,
                ServiceId = rec.ServiceId,
                // {{ModuleItemStart:Tags}}
                Tags = await GetTagsForItem(rec.TenantId, ServiceId),
                // {{ModuleItemEnd:Tags}}
                TenantId = rec.TenantId,
            };

            GetDataApp(rec, output, CurrentUser);
        } else {
            output.ActionResponse.Messages.Add("Service '" + ServiceId.ToString() + "' No Longer Exists");
        }

        return output;
    }

    public async Task<List<DataObjects.Service>> GetServices(Guid TenantId, DataObjects.User? CurrentUser = null)
    {
        var output = new List<DataObjects.Service>();

        List<Service>? recs = null;

        if(AdminUser(CurrentUser)) {
            recs = await data.Services
                .Where(x => x.TenantId == TenantId).ToListAsync();
        } else {
            recs = await data.Services
                .Where(x => x.TenantId == TenantId && x.Deleted != true).ToListAsync();
        }

        if (recs != null && recs.Any()) {
            foreach (var rec in recs) {
                var s = new DataObjects.Service {
                    ActionResponse = GetNewActionResponse(true),
                    Added = rec.Added,
                    AddedBy = LastModifiedDisplayName(rec.AddedBy),
                    Code = rec.Code,
                    DefaultAppointmentDuration = rec.DefaultAppointmentDuration,
                    DefaultService = rec.DefaultService,
                    Deleted = rec.Deleted,
                    DeletedAt = rec.DeletedAt,
                    Description = rec.Description,
                    Enabled = rec.Enabled,
                    LastModified = rec.LastModified,
                    LastModifiedBy = LastModifiedDisplayName(rec.LastModifiedBy),
                    Rate = rec.Rate,
                    ServiceId = rec.ServiceId,
                    // {{ModuleItemStart:Tags}}
                    Tags = await GetTagsForItem(TenantId, rec.ServiceId),
                    // {{ModuleItemEnd:Tags}}
                    TenantId = rec.TenantId,
                };

                GetDataApp(rec, s, CurrentUser);

                output.Add(s);
            }
        }

        return output;
    }

    public async Task<DataObjects.Service> SaveService(DataObjects.Service service, DataObjects.User? CurrentUser = null)
    {
        var output = service;
        output.ActionResponse = GetNewActionResponse();

        bool newRecord = false;
        DateTime now = DateTime.UtcNow;

        var rec = await data.Services.FirstOrDefaultAsync(x => x.ServiceId == output.ServiceId);

        if (rec != null && rec.Deleted) {
            if (AdminUser(CurrentUser)) {
                // Ok to edit this record that is marked as deleted.
            } else {
                output.ActionResponse.Messages.Add("Service '" + output.ServiceId.ToString() + "' No Longer Exists");
                return output;
            }
        }

        if (rec == null) {
            if (output.ServiceId == Guid.Empty) {
                newRecord = true;
                output.ServiceId = Guid.NewGuid();

                rec = new Service { 
                    ServiceId = output.ServiceId,
                    TenantId = output.TenantId,
                    Deleted = false,
                    Added = now,
                    AddedBy = CurrentUserIdString(CurrentUser),
                };

            } else {
                output.ActionResponse.Messages.Add("Service '" + output.ServiceId.ToString() + "' No Longer Exists");
                return output;
            }
        }

        output.Code = MaxStringLength(output.Code, 50);
        output.Description = MaxStringLength(output.Description, 200);

        rec.Code = output.Code;
        
        // If this is being set as the default service and it wasn't previously then update other records.
        if(output.DefaultService == true && rec.DefaultService != true) {
            await data.Database.ExecuteSqlRawAsync("UPDATE Services SET DefaultService=0 WHERE TenantId={0}", output.TenantId);
        }

        output.Code = MaxStringLength(output.Code, 50);
        output.Description = MaxStringLength(output.Description, 200);

        rec.DefaultService = output.DefaultService;
        rec.Description = output.Description;
        rec.Enabled = output.Enabled;
        rec.Rate = output.Rate;
        rec.DefaultAppointmentDuration = output.DefaultAppointmentDuration;

        rec.LastModified = now;
        rec.LastModifiedBy = CurrentUserIdString(CurrentUser);

        if (AdminUser(CurrentUser)) {
            rec.Deleted = output.Deleted;
        }

        SaveDataApp(rec, output, CurrentUser);

        try {
            if (newRecord) {
                await data.Services.AddAsync(rec);
            }
            await data.SaveChangesAsync();

            // {{ModuleItemStart:Tags}}
            await SaveItemTags(output.TenantId, output.ServiceId, output.Tags);
            // {{ModuleItemEnd:Tags}}

            output.ActionResponse.Result = true;

            await SignalRUpdate(new DataObjects.SignalRUpdate { 
                TenantId = output.TenantId,
                ItemId = output.ServiceId,
                UpdateType = DataObjects.SignalRUpdateType.Service,
                Message = "Saved",
                UserId = CurrentUserId(CurrentUser),
                Object = output,
            });
        } catch (Exception ex) {
            output.ActionResponse.Messages.Add("Error Saving Service " + output.ServiceId.ToString());
            output.ActionResponse.Messages.AddRange(RecurseException(ex));
        }


        return output;
    }
}
