namespace CRM;

public partial interface IDataAccess
{
    Task<DataObjects.BooleanResponse> DeleteLocation(Guid LocationId, DataObjects.User? CurrentUser = null, bool ForceDeleteImmediately = false);
    Task<DataObjects.Location> GetLocation(Guid LocationId, DataObjects.User? CurrentUser = null);
    Task<List<DataObjects.Location>> GetLocations(Guid TenantId, DataObjects.User? CurrentUser = null);
    Task<DataObjects.Location> SaveLocation(DataObjects.Location location, DataObjects.User? CurrentUser = null);
}

public partial class DataAccess
{
    public async Task<DataObjects.BooleanResponse> DeleteLocation(Guid LocationId, DataObjects.User? CurrentUser = null, bool ForceDeleteImmediately = false)
    {
        var output = new DataObjects.BooleanResponse();

        var rec = await data.Locations.FirstOrDefaultAsync(x => x.LocationId == LocationId);
        if (rec == null) {
            output.Messages.Add("Error Deleting Location '" + LocationId.ToString() + "' - Record No Longer Exists");
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
                data.Locations.Remove(rec);
                await data.SaveChangesAsync();

                output.Result = true;
            } catch (Exception ex) {
                output.Messages.Add("Error Deleting Location " + LocationId.ToString());
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
                output.Messages.Add("Error Deleting Location " + LocationId.ToString());
                output.Messages.AddRange(RecurseException(ex));
            }
        }

        if (!ForceDeleteImmediately) {
            await SignalRUpdate(new DataObjects.SignalRUpdate {
                TenantId = tenantId,
                ItemId = LocationId,
                UpdateType = DataObjects.SignalRUpdateType.Location,
                Message = "Deleted",
                UserId = CurrentUserId(CurrentUser),
            });
        }

        return output;
    }

    public async Task<DataObjects.Location> GetLocation(Guid LocationId, DataObjects.User? CurrentUser = null)
    {
        var output = new DataObjects.Location();

        Location? rec = null;

        if (AdminUser(CurrentUser)) {
            rec = await data.Locations.FirstOrDefaultAsync(x => x.LocationId == LocationId);
        } else {
            rec = await data.Locations.FirstOrDefaultAsync(x => x.LocationId == LocationId && x.Deleted != true);
        }

        if (rec != null) {
            output = new DataObjects.Location {
                ActionResponse = GetNewActionResponse(true),
                Added = rec.Added,
                AddedBy = LastModifiedDisplayName(rec.AddedBy),
                Address = rec.Address,
                CalendarBackgroundColor = rec.CalendarBackgroundColor,
                CalendarForegroundColor = rec.CalendarForegroundColor,
                City = rec.City,
                DefaultLocation = rec.DefaultLocation,
                Deleted = rec.Deleted,
                DeletedAt = rec.DeletedAt,
                Enabled = rec.Enabled,
                LastModified = rec.LastModified,
                LastModifiedBy = LastModifiedDisplayName(rec.LastModifiedBy),
                LocationId = rec.LocationId,
                Name = rec.Name,
                PostalCode = rec.PostalCode,
                State = rec.State,
                TenantId = rec.TenantId,
            };

            GetDataApp(rec, output, CurrentUser);
        } else {
            output.ActionResponse.Messages.Add("Location '" + LocationId.ToString() + "' No Longer Exists");
        }

        return output;
    }

    public async Task<List<DataObjects.Location>> GetLocations(Guid TenantId, DataObjects.User? CurrentUser = null)
    {
        var output = new List<DataObjects.Location>();

        List<Location>? recs = null;

        if(AdminUser(CurrentUser)) {
            recs = await data.Locations.Where(x => x.TenantId == TenantId).ToListAsync();
        } else {
            recs = await data.Locations.Where(x => x.TenantId == TenantId && x.Deleted != true).ToListAsync();
        }

        if (recs != null && recs.Any()) {
            foreach (var rec in recs) {
                var l = new DataObjects.Location {
                    ActionResponse = GetNewActionResponse(true),
                    Added = rec.Added,
                    AddedBy = LastModifiedDisplayName(rec.AddedBy),
                    Address = rec.Address,
                    CalendarBackgroundColor = rec.CalendarBackgroundColor,
                    CalendarForegroundColor = rec.CalendarForegroundColor,
                    City = rec.City,
                    DefaultLocation = rec.DefaultLocation,
                    Deleted = rec.Deleted,
                    DeletedAt = rec.DeletedAt,
                    Enabled = rec.Enabled,
                    LastModified = rec.LastModified,
                    LastModifiedBy = LastModifiedDisplayName(rec.LastModifiedBy),
                    LocationId = rec.LocationId,
                    Name = rec.Name,
                    PostalCode = rec.PostalCode,
                    State = rec.State,
                    TenantId = rec.TenantId,
                };

                GetDataApp(rec, l, CurrentUser);

                output.Add(l);
            }
        }

        return output;
    }

    public async Task<DataObjects.Location> SaveLocation(DataObjects.Location location, DataObjects.User? CurrentUser = null)
    {
        var output = location;
        output.ActionResponse = GetNewActionResponse();

        bool newRecord = false;
        DateTime now = DateTime.UtcNow;

        var rec = await data.Locations.FirstOrDefaultAsync(x => x.LocationId == output.LocationId);

        if (rec != null && rec.Deleted) {
            if (AdminUser(CurrentUser)) {
                // Ok to edit this record that is marked as deleted.
            } else {
                output.ActionResponse.Messages.Add("Location '" + output.LocationId.ToString() + "' No Longer Exists");
                return output;
            }
        }

        if (rec == null) {
            if (output.LocationId == Guid.Empty) {
                newRecord = true;
                output.LocationId = Guid.NewGuid();

                rec = new Location {
                    LocationId = output.LocationId,
                    TenantId = output.TenantId,
                    Deleted = false,
                    Added = now,
                    AddedBy = CurrentUserIdString(CurrentUser),
                };

            } else {
                output.ActionResponse.Messages.Add("Location '" + output.LocationId.ToString() + "' No Longer Exists");
                return output;
            }
        }

        // If this is being set as the default location and it wasn't previously then update other records.
        if (output.DefaultLocation == true && rec.DefaultLocation != true) {
            await data.Database.ExecuteSqlRawAsync("UPDATE Locations SET DefaultLocation=0 WHERE TenantId={0}", output.TenantId);
        }

        output.Name = MaxStringLength(output.Name, 200);
        output.Address = MaxStringLength(output.Address, 200);
        output.City = MaxStringLength(output.City, 100);
        output.State = MaxStringLength(output.State, 50);
        output.PostalCode = MaxStringLength(output.PostalCode, 50);
        output.CalendarBackgroundColor = MaxStringLength(output.CalendarBackgroundColor, 100);
        output.CalendarForegroundColor = MaxStringLength(output.CalendarForegroundColor, 100);

        rec.Enabled = output.Enabled;
        rec.DefaultLocation = output.DefaultLocation;
        rec.Name = output.Name;
        rec.Address = output.Address;
        rec.City = output.City;
        rec.State = output.State;
        rec.PostalCode = output.PostalCode;
        rec.CalendarBackgroundColor = output.CalendarBackgroundColor;
        rec.CalendarForegroundColor = output.CalendarForegroundColor;

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
                await data.Locations.AddAsync(rec);
            }
            await data.SaveChangesAsync();

            output.ActionResponse.Result = true;

            await SignalRUpdate(new DataObjects.SignalRUpdate { 
                TenantId = output.TenantId,
                ItemId = output.LocationId,
                UpdateType = DataObjects.SignalRUpdateType.Location,
                Message = "Saved",
                UserId = CurrentUserId(CurrentUser),
                Object = output,
            });
        } catch (Exception ex) {
            output.ActionResponse.Messages.Add("Error Saving Location " + output.LocationId.ToString());
            output.ActionResponse.Messages.AddRange(RecurseException(ex));
        }


        return output;
    }
}
