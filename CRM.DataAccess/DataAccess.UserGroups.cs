namespace CRM;

public partial interface IDataAccess
{
    Task<DataObjects.BooleanResponse> DeleteUserGroup(Guid GroupId, DataObjects.User? CurrentUser = null, bool ForceDeleteImmediately = false);
    Task<DataObjects.UserGroup> GetUserGroup(Guid GroupId, bool IncludeUsers = false, DataObjects.User? CurrentUser = null);
    Task<List<DataObjects.UserGroup>> GetUserGroups(Guid TenantId, bool IncludeUsers = false, DataObjects.User? CurrentUser = null);
    Task<DataObjects.UserGroup> SaveUserGroup(DataObjects.UserGroup Group, DataObjects.User? CurrentUser = null);
}

public partial class DataAccess
{
    private async Task<DataObjects.BooleanResponse> AddUserToGroup(Guid UserId, Guid GroupId)
    {
        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();

        if (UserId == Guid.Empty || GroupId == Guid.Empty) {
            output.Messages.Add("You must pass a valid UserId and GroupId");
            return output;
        }

        var group = await GetUserGroup(GroupId);
        if (group.TenantId == Guid.Empty) {
            output.Messages.Add("Unable to find TenantId for Group '" + GroupId.ToString() + "'");
            return output;
        }

        var rec = await data.UserInGroups.FirstOrDefaultAsync(x => x.UserId == UserId && x.GroupId == GroupId && x.TenantId == group.TenantId);
        if (rec != null) {
            output.Messages.Add("User is Already in the Selected Group");
        } else {
            try {
                await data.UserInGroups.AddAsync(new UserInGroup {
                    UserInGroupId = Guid.NewGuid(),
                    UserId = UserId,
                    TenantId = group.TenantId,
                    GroupId = GroupId
                });

                await data.SaveChangesAsync();
                output.Result = true;
            } catch (Exception ex) {
                output.Messages.Add("Error Adding User " + UserId.ToString() + " to Group " + GroupId.ToString());
                output.Messages.AddRange(RecurseException(ex));
            }
        }

        return output;
    }

    public async Task<DataObjects.BooleanResponse> DeleteUserGroup(Guid GroupId, DataObjects.User? CurrentUser = null, bool ForceDeleteImmediately = false)
    {
        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();

        var rec = await data.UserGroups.FirstOrDefaultAsync(x => x.GroupId == GroupId);
        if (rec != null) {
            Guid tenantId = rec.TenantId;
            var tenantSettings = GetTenantSettings(tenantId);

            if(ForceDeleteImmediately || tenantSettings.DeletePreference == DataObjects.DeletePreference.Immediate) {
                var deleteAppRecords = await DeleteRecordsApp(rec, CurrentUser);
                if (!deleteAppRecords.Result) {
                    output.Messages.AddRange(deleteAppRecords.Messages);
                    return output;
                }

                try {
                    data.UserInGroups.RemoveRange(data.UserInGroups.Where(x => x.GroupId == GroupId));
                } catch (Exception ex) {
                    output.Messages.Add("Error Deleting Related Data for User Group " + GroupId.ToString());
                    output.Messages.AddRange(RecurseException(ex));
                    return output;
                }
            }

            try {
                if(ForceDeleteImmediately || tenantSettings.DeletePreference == DataObjects.DeletePreference.Immediate) {
                    data.UserGroups.Remove(rec);
                } else {
                    rec.Deleted = true;
                    rec.DeletedAt = DateTime.UtcNow;
                    rec.LastModified = DateTime.UtcNow;
                    if(CurrentUser != null) {
                        rec.LastModifiedBy = CurrentUserIdString(CurrentUser);
                    }
                }
                
                await data.SaveChangesAsync();
                output.Result = true;
                output.Messages.Add("User Group Deleted");

                if (!ForceDeleteImmediately) {
                    await SignalRUpdate(new DataObjects.SignalRUpdate {
                        TenantId = tenantId,
                        ItemId = GroupId,
                        UpdateType = DataObjects.SignalRUpdateType.UserGroup,
                        Message = "Deleted",
                        UserId = CurrentUserId(CurrentUser),
                    });
                }
            } catch (Exception ex) {
                output.Messages.Add("Error Deleting User Group " + GroupId.ToString());
                output.Messages.AddRange(RecurseException(ex));
            }
        } else {
            output.Messages.Add("User Group '" + GroupId.ToString() + "' No Longer Exists");
        }

        return output;
    }

    public async Task<DataObjects.UserGroup> GetUserGroup(Guid GroupId, bool IncludeUsers = false, DataObjects.User? CurrentUser = null)
    {
        DataObjects.UserGroup output = new DataObjects.UserGroup();

        UserGroup? rec = null;

        if(AdminUser(CurrentUser)) {
            rec = await data.UserGroups
            .Include(x => x.UserInGroups).ThenInclude(x => x.User)
            .FirstOrDefaultAsync(x => x.GroupId == GroupId);
        } else {
            rec = await data.UserGroups
            .Include(x => x.UserInGroups).ThenInclude(x => x.User)
            .FirstOrDefaultAsync(x => x.GroupId == GroupId && x.Deleted != true);
        }

        if (rec != null) {
            var settings = DeserializeObject<DataObjects.UserGroupSettings>(rec.Settings);
            if(settings == null) {
                settings = new DataObjects.UserGroupSettings();
            }

            output = new DataObjects.UserGroup {
                ActionResponse = GetNewActionResponse(true),
                Added = rec.Added,
                AddedBy = LastModifiedDisplayName(rec.AddedBy),
                GroupId = rec.GroupId,
                TenantId = rec.TenantId,
                Name = rec.Name,
                Enabled = rec.Enabled,
                Deleted = BooleanValue(rec.Deleted),
                DeletedAt = rec.DeletedAt,
                LastModified = rec.LastModified,
                LastModifiedBy = LastModifiedDisplayName(rec.LastModifiedBy),
                Settings = settings,
            };

            GetDataApp(rec, output, CurrentUser);

            if(IncludeUsers && rec.UserInGroups != null && rec.UserInGroups.Any()) {
                output.Users = new List<DataObjects.UserListing>();

                foreach(var user in rec.UserInGroups) {
                    output.Users.Add(new DataObjects.UserListing { 
                        UserId = user.UserId,
                        FirstName = user.User.FirstName,
                        LastName = user.User.LastName,
                        Email = user.User.Email,
                        Username = user.User.Username,
                        Location = user.User.Location,
                        Admin = BooleanValue(user.User.Admin),
                        Enabled = BooleanValue(user.User.Enabled),
                    });
                }
            }
        } else {
            output.ActionResponse.Messages.Add("Group '" + GroupId.ToString() + "' Not Found");
        }

        return output;
    }

    public async Task<List<DataObjects.UserGroup>> GetUserGroups(Guid TenantId, bool IncludeUsers = false, DataObjects.User? CurrentUser = null)
    {
        List<DataObjects.UserGroup> output = new List<DataObjects.UserGroup>();

        List<UserGroup>? recs = null;

        if(AdminUser(CurrentUser)) {
            recs = await data.UserGroups.Where(x => x.TenantId == TenantId)
            .Include(x => x.UserInGroups).ThenInclude(x => x.User)
            .OrderBy(x => x.Name).ToListAsync();
        } else {
            recs = await data.UserGroups.Where(x => x.TenantId == TenantId && x.Deleted != true)
            .Include(x => x.UserInGroups).ThenInclude(x => x.User)
            .OrderBy(x => x.Name).ToListAsync();
        }

        if (recs != null && recs.Any()) {
            foreach (var rec in recs) {
                var settings = DeserializeObject<DataObjects.UserGroupSettings>(rec.Settings);
                if (settings == null) {
                    settings = new DataObjects.UserGroupSettings();
                }

                var group = new DataObjects.UserGroup {
                    ActionResponse = GetNewActionResponse(true),
                    Added = rec.Added,
                    AddedBy = LastModifiedDisplayName(rec.AddedBy),
                    GroupId = rec.GroupId,
                    TenantId = rec.TenantId,
                    Name = rec.Name,
                    Enabled = rec.Enabled,
                    Deleted = BooleanValue(rec.Deleted),
                    DeletedAt = rec.DeletedAt,
                    LastModified = rec.LastModified,
                    LastModifiedBy = LastModifiedDisplayName(rec.LastModifiedBy),
                    Settings = settings,
                };

                GetDataApp(rec, group, CurrentUser);

                if (IncludeUsers && rec.UserInGroups != null && rec.UserInGroups.Any()) {
                    group.Users = new List<DataObjects.UserListing>();

                    foreach(var user in rec.UserInGroups) {
                        group.Users.Add(new DataObjects.UserListing {
                            UserId = user.UserId,
                            FirstName = user.User.FirstName,
                            LastName = user.User.LastName,
                            Location = user.User.Location,
                            Email = user.User.Email,
                            Username = user.User.Username,
                            Admin = BooleanValue(user.User.Admin),
                            Enabled = BooleanValue(user.User.Enabled),
                        });
                    }
                }

                output.Add(group);
            }
        } else {
            // There are no groups yet for this customer, so add a new group
            var now = DateTime.UtcNow;

            var newGroup = new UserGroup {
                GroupId = Guid.NewGuid(),
                TenantId = TenantId,
                Name = "All Users (auto-created)",
                Enabled = true,
                Deleted = false,
                Added = now,
                AddedBy = CurrentUserIdString(CurrentUser),
                LastModified = now,
                LastModifiedBy = CurrentUserIdString(CurrentUser),
            };

            await data.UserGroups.AddAsync(newGroup);
            await data.SaveChangesAsync();

            List<DataObjects.UserListing>? groupUsers = null;

            // Now, add all users for this tenant into this new group
            var users = await GetUsers(TenantId);
            if (users != null && users.Any()) {
                if (IncludeUsers) {
                    //groupUsers = new List<Guid>();
                    groupUsers = new List<DataObjects.UserListing>();
                }

                foreach (var user in users) {
                    if (groupUsers != null) {
                        groupUsers.Add(new DataObjects.UserListing { 
                            UserId = user.UserId,
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            Email = user.Email,
                            Username = user.Username,
                            Location = user.Location,
                            Admin = user.Admin,
                            Enabled = user.Enabled,
                        });
                    }

                    await AddUserToGroup(user.UserId, newGroup.GroupId);
                }
            }

            output = new List<DataObjects.UserGroup>();
            output.Add(new DataObjects.UserGroup {
                ActionResponse = GetNewActionResponse(true),
                GroupId = newGroup.GroupId,
                TenantId = newGroup.TenantId,
                Name = newGroup.Name,
                Enabled = newGroup.Enabled,
                Users = groupUsers
            });
        }

        return output;
    }

    public async Task<DataObjects.UserGroup> SaveUserGroup(DataObjects.UserGroup Group, DataObjects.User? CurrentUser = null)
    {
        DataObjects.UserGroup output = Group;
        output.ActionResponse = GetNewActionResponse();
        DateTime now = DateTime.UtcNow;

        bool newRecord = false;
        var rec = await data.UserGroups.FirstOrDefaultAsync(x => x.GroupId == output.GroupId);
        if(rec == null) {
            if(output.GroupId == Guid.Empty) {
                newRecord = true;
                output.GroupId = Guid.NewGuid();
                rec = new UserGroup { 
                    Added = now,
                    AddedBy = CurrentUserIdString(CurrentUser),
                    GroupId = output.GroupId,
                    TenantId = output.TenantId,
                    Deleted = false,
                };
            } else {
                output.ActionResponse.Messages.Add("Group '" + output.GroupId.ToString() + "' No Longer Exists");
                return output;
            }
        }

        try {
            if (String.IsNullOrWhiteSpace(output.Name)) {
                output.Name = "Users";
            }

            output.Name = MaxStringLength(output.Name, 100);

            rec.Name = output.Name;
            rec.Enabled = output.Enabled;
            rec.Settings = SerializeObject(output.Settings);

            rec.LastModified = now;

            if(CurrentUser != null) {
                rec.LastModifiedBy = CurrentUserIdString(CurrentUser);

                if (CurrentUser.Admin) {
                    rec.Deleted = output.Deleted;
                }
            }

            SaveDataApp(rec, output, CurrentUser);

            if (newRecord) {
                await data.UserGroups.AddAsync(rec);
            }
            await data.SaveChangesAsync();
            output.ActionResponse.Result = true;
            output.ActionResponse.Messages.Add(newRecord ? "Group Created" : "Group Updated");

            // Save any users for this group.
            // First, find all users to keep.
            List<Guid> usersInGroup = new List<Guid>();
            if(output.Users != null && output.Users.Any()) {
                usersInGroup = output.Users.Select(x => x.UserId).ToList();
            }

            // Remove any records that should no longer be in the group.
            data.UserInGroups.RemoveRange(data.UserInGroups.Where(x => x.GroupId == output.GroupId && !usersInGroup.Contains(x.UserId)));
            await data.SaveChangesAsync();

            // Make sure all users have a valid record.
            foreach(var userId in usersInGroup) {
                var userInGroup = await data.UserInGroups.FirstOrDefaultAsync(x => x.GroupId == output.GroupId && x.UserId == userId);
                if(userInGroup == null) {
                    await data.UserInGroups.AddAsync(new UserInGroup { 
                        UserInGroupId = Guid.NewGuid(),
                        GroupId = output.GroupId,
                        UserId = userId,
                        TenantId = output.TenantId
                    });
                    await data.SaveChangesAsync();
                }
            }

            await SignalRUpdate(new DataObjects.SignalRUpdate { 
                TenantId = output.TenantId,
                ItemId = output.GroupId,
                UpdateType = DataObjects.SignalRUpdateType.UserGroup,
                Object = output,
                Message = "Saved",
                UserId = CurrentUserId(CurrentUser),
            });

        }catch(Exception ex) {
            output.ActionResponse.Messages.Add("Error Saving Group " + output.GroupId.ToString());
            output.ActionResponse.Messages.AddRange(RecurseException(ex));
        }

        return output;
    }
}