using Microsoft.Extensions.Logging.Abstractions;

namespace CRM;

public partial interface IDataAccess
{
    Task<DataObjects.BooleanResponse> DeleteTag(Guid TagId, DataObjects.User? CurrentUser = null, bool ForceDeleteImmediately = false);
    Task<DataObjects.Tag> GetTag(Guid TagId, DataObjects.User? CurrentUser = null);
    Task<List<DataObjects.Tag>> GetTags(Guid TenantId, DataObjects.User? CurrentUser = null);
    Task<DataObjects.Tag> SaveTag(DataObjects.Tag tag, DataObjects.User? CurrentUser = null);
}

public partial class DataAccess
{
    public async Task<DataObjects.BooleanResponse> DeleteTag(Guid TagId, DataObjects.User? CurrentUser = null, bool ForceDeleteImmediately = false)
    {
        var output = new DataObjects.BooleanResponse();

        var rec = await data.Tags.FirstOrDefaultAsync(x => x.TagId == TagId);
        if(rec == null) {
            output.Messages.Add("Error Deleting Tag '" + TagId.ToString() + "' - Record No Longer Exists");
            return output;
        }

        var now = DateTime.UtcNow;
        Guid tenantId = GuidValue(rec.TenantId);
        var tenantSettings = GetTenantSettings(tenantId);

        if (ForceDeleteImmediately || tenantSettings.DeletePreference == DataObjects.DeletePreference.Immediate) {
            try {
                data.Tags.Remove(rec);
                await data.SaveChangesAsync();

                output.Result = true;
            } catch (Exception ex) {
                output.Messages.Add("Error Deleting Tag " + TagId.ToString());
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
                output.Messages.Add("Error Deleting Tag " + TagId.ToString());
                output.Messages.AddRange(RecurseException(ex));
            }
        }

        if (!ForceDeleteImmediately) {
            await SignalRUpdate(new DataObjects.SignalRUpdate {
                TenantId = tenantId,
                ItemId = TagId,
                UpdateType = DataObjects.SignalRUpdateType.Tag,
                Message = "Deleted",
                UserId = CurrentUserId(CurrentUser),
            });
        }

        return output;
    }

    public async Task<DataObjects.Tag> GetTag(Guid TagId, DataObjects.User? CurrentUser = null)
    {
        var output = new DataObjects.Tag();

        Tag? rec = null;

        if(AdminUser(CurrentUser)) {
            rec = await data.Tags.FirstOrDefaultAsync(x => x.TagId == TagId);
        } else {
            rec = await data.Tags.FirstOrDefaultAsync(x => x.TagId == TagId && x.Deleted != true);
        }

        if(rec != null) {
            output = new DataObjects.Tag { 
                ActionResponse = GetNewActionResponse(true),
                TagId = rec.TagId,
                TenantId = rec.TenantId,
                Name = rec.Name,
                Style = rec.Style,
                Enabled = rec.Enabled,
                UseInAppointments = rec.UseInAppointments,
                UseInEmailTemplates = rec.UseInEmailTemplates,
                UseInServices = rec.UseInServices,
                Added = rec.Added,
                AddedBy = LastModifiedDisplayName(rec.AddedBy),
                LastModified = rec.LastModified,
                LastModifiedBy = LastModifiedDisplayName(rec.LastModifiedBy),
                Deleted = rec.Deleted,
                DeletedAt = rec.DeletedAt,
            };
        } else {
            output.ActionResponse.Messages.Add("Tag '" + TagId.ToString() + "' No Longer Exists");
        }

        return output;
    }

    public async Task<List<DataObjects.Tag>> GetTags(Guid TenantId, DataObjects.User? CurrentUser = null)
    {
        var output = new List<DataObjects.Tag>();

        List<Tag>? recs = null;

        if(AdminUser(CurrentUser)) {
            recs = await data.Tags.Where(x => x.TenantId == TenantId).ToListAsync();
        } else {
            recs = await data.Tags.Where(x => x.TenantId == TenantId && x.Deleted != true).ToListAsync();
        }

        if(recs != null && recs.Any()) {
            foreach(var rec in recs) {
                output.Add(new DataObjects.Tag {
                    ActionResponse = GetNewActionResponse(true),
                    TagId = rec.TagId,
                    TenantId = rec.TenantId,
                    Name = rec.Name,
                    Style = rec.Style,
                    Enabled = rec.Enabled,
                    UseInAppointments = rec.UseInAppointments,
                    UseInEmailTemplates = rec.UseInEmailTemplates,
                    UseInServices = rec.UseInServices,
                    Added = rec.Added,
                    AddedBy = LastModifiedDisplayName(rec.AddedBy),
                    LastModified = rec.LastModified,
                    LastModifiedBy = LastModifiedDisplayName(rec.LastModifiedBy),
                    Deleted = rec.Deleted,
                    DeletedAt = rec.DeletedAt,
                });
            }
        }

        return output;
    }

    protected async Task<List<Guid>?> GetTagsForItem(Guid TenantId, Guid ItemId)
    {
        List<Guid>? output = null;

        var tags = await data.TagItems
            .Where(x => x.TenantId == TenantId && x.ItemId == ItemId)
            .Select(x => x.TagId)
            .ToListAsync();
        if(tags != null && tags.Any()) {
            output = tags;
        }

        return output;
    }

    protected async Task SaveItemTags(Guid TenantId, Guid ItemId, List<Guid>? Tags)
    {
        List<Guid> keepTags = Tags != null && Tags.Any() 
            ? Tags 
            : new List<Guid>();

        // Remove any items from the table that should no longer be there.
        data.TagItems.RemoveRange(data.TagItems.Where(x => x.TenantId == TenantId && x.ItemId == ItemId && !keepTags.Contains(x.TagId)));
        await data.SaveChangesAsync();

        // Make sure all tags exists
        foreach(var tagId in keepTags) {
            var rec = await data.TagItems.FirstOrDefaultAsync(x => x.TenantId == TenantId && x.ItemId == ItemId && x.TagId == tagId);

            if(rec == null) {
                await data.TagItems.AddAsync(new TagItem { 
                    TagItemId = Guid.NewGuid(),
                    TagId = tagId,
                    TenantId = TenantId,
                    ItemId = ItemId,
                });
                await data.SaveChangesAsync();
            }
        }
    }

    public async Task<DataObjects.Tag> SaveTag(DataObjects.Tag tag, DataObjects.User? CurrentUser = null)
    {
        var output = tag;
        output.ActionResponse = GetNewActionResponse();

        bool newRecord = false;
        DateTime now = DateTime.UtcNow;

        var rec = await data.Tags.FirstOrDefaultAsync(x => x.TagId == output.TagId);

        if(rec != null && rec.Deleted) {
            if(AdminUser(CurrentUser)) {
                // Ok to edit this record that is marked as deleted.
            } else {
                output.ActionResponse.Messages.Add("Tag '" + output.TagId.ToString() + "' No Longer Exists");
                return output;
            }
        }

        if(rec == null) {
            if(output.TagId == Guid.Empty) {
                newRecord = true;
                output.TagId = Guid.NewGuid();

                rec = new Tag { 
                    TagId = output.TagId,
                    TenantId = output.TenantId,
                    Deleted = false,
                    Added = now,
                    AddedBy = CurrentUserIdString(CurrentUser),
                };
            } else {
                output.ActionResponse.Messages.Add("Tag '" + output.TagId.ToString() + "' No Longer Exists");
                return output;
            }
        }

        output.Name = MaxStringLength(output.Name, 200);

        rec.Name = output.Name;
        rec.Style = output.Style;
        rec.Enabled = output.Enabled;
        rec.UseInAppointments = output.UseInAppointments;
        rec.UseInEmailTemplates = output.UseInEmailTemplates;
        rec.UseInServices = output.UseInServices;
        rec.LastModified = now;
        rec.LastModifiedBy = CurrentUserIdString(CurrentUser);

        if(AdminUser(CurrentUser)) {
            rec.Deleted = output.Deleted;

            if (!output.Deleted) {
                rec.DeletedAt = null;
            }
        }

        try {
            if (newRecord) {
                await data.Tags.AddAsync(rec);
            }
            await data.SaveChangesAsync();

            output.ActionResponse.Result = true;

            await SignalRUpdate(new DataObjects.SignalRUpdate { 
                TenantId = output.TenantId,
                ItemId = output.TagId,
                UpdateType = DataObjects.SignalRUpdateType.Tag,
                Message = "Saved",
                UserId = CurrentUserId(CurrentUser),
                Object = output,
            });
        }catch(Exception ex) {
            output.ActionResponse.Messages.Add("Error Saving Tag " + output.TagId.ToString());
            output.ActionResponse.Messages.AddRange(RecurseException(ex));
        }

        return output;
    }
}