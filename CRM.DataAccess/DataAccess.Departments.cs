namespace CRM;

public partial interface IDataAccess
{
    Task<DataObjects.BooleanResponse> DeleteDepartment(Guid DepartmentId, DataObjects.User? CurrentUser = null, bool ForceDeleteImmediately = false);
    Task<DataObjects.BooleanResponse> DeleteDepartmentGroup(Guid DepartmentGroupId, DataObjects.User? CurrentUser = null, bool ForceDeleteImmediately = false);
    Task<Guid> DepartmentIdFromNameAndLocation(Guid TenantId, string? Department, string? Location = "");
    Task<DataObjects.Department> GetDepartment(Guid DepartmentId, DataObjects.User? CurrentUser = null);
    Task<DataObjects.DepartmentGroup> GetDepartmentGroup(Guid DepartmentGroupId, DataObjects.User? CurrentUser = null);
    Task<List<DataObjects.DepartmentGroup>> GetDepartmentGroups(Guid TenantId, DataObjects.User? CurrentUser = null);
    string GetDepartmentName(Guid TenantId, Guid DepartmentId);
    Task<List<DataObjects.Department>> GetDepartments(Guid TenantId, DataObjects.User? CurrentUser = null);
    Task<DataObjects.Department> SaveDepartment(DataObjects.Department department, DataObjects.User? CurrentUser = null);
    Task<DataObjects.DepartmentGroup> SaveDepartmentGroup(DataObjects.DepartmentGroup departmentGroup, DataObjects.User? CurrentUser = null);
    Task<List<DataObjects.Department>> SaveDepartments(List<DataObjects.Department> departments, DataObjects.User? CurrentUser = null);
}

public partial class DataAccess
{
    public async Task<DataObjects.BooleanResponse> DeleteDepartment(Guid DepartmentId, DataObjects.User? CurrentUser = null, bool ForceDeleteImmediately = false)
    {
        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();

        var rec = await data.Departments.FirstOrDefaultAsync(x => x.DepartmentId == DepartmentId);
        if (rec == null) {
            output.Messages.Add("Error Deleting Department " + DepartmentId.ToString() + " - Record No Longer Exists");
        } else {
            Guid tenantId = GuidValue(rec.TenantId);
            var tenantSettings = GetTenantSettings(tenantId);

            // First, delete related data
            if(ForceDeleteImmediately || tenantSettings.DeletePreference == DataObjects.DeletePreference.Immediate) {
                var deleteAppRecords = await DeleteRecordsApp(rec, CurrentUser);
                if (!deleteAppRecords.Result) {
                    output.Messages.AddRange(deleteAppRecords.Messages);
                    return output;
                }

                try {
                    var recs = await data.Users.Where(x => x.DepartmentId == DepartmentId).ToListAsync();
                    if (recs != null && recs.Any()) {
                        foreach (var record in recs) {
                            record.DepartmentId = null;
                        }
                        await data.SaveChangesAsync();
                    }
                } catch (Exception ex) {
                    output.Messages.Add("Error Deleting Related Department Records for Department " + DepartmentId.ToString() + ":");
                    output.Messages.AddRange(RecurseException(ex));
                    return output;
                }
            }

            if(ForceDeleteImmediately || tenantSettings.DeletePreference == DataObjects.DeletePreference.Immediate) {
                data.Departments.Remove(rec);
            } else {
                rec.Deleted = true;
                rec.DeletedAt = DateTime.UtcNow;
                rec.LastModified = DateTime.UtcNow;
                if(CurrentUser != null) {
                    rec.LastModifiedBy = CurrentUserIdString(CurrentUser);
                }
            }

            try {
                await data.SaveChangesAsync();
                output.Result = true;

                if (tenantId != Guid.Empty) {
                    ClearTenantCache(tenantId);
                }

                if (!ForceDeleteImmediately) {
                    await SignalRUpdate(new DataObjects.SignalRUpdate {
                        TenantId = tenantId,
                        ItemId = DepartmentId,
                        UpdateType = DataObjects.SignalRUpdateType.Department,
                        UserId = CurrentUserId(CurrentUser),
                        Message = "Deleted"
                    });
                }
            } catch (Exception ex) {
                output.Messages.Add("Error Deleting Department " + DepartmentId.ToString() + ":");
                output.Messages.AddRange(RecurseException(ex));
            }
        }

        return output;
    }

    public async Task<DataObjects.BooleanResponse> DeleteDepartmentGroup(Guid DepartmentGroupId, DataObjects.User? CurrentUser = null, bool ForceDeleteImmediately = false)
    {
        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();

        var rec = await data.DepartmentGroups.FirstOrDefaultAsync(x => x.DepartmentGroupId == DepartmentGroupId);
        if (rec == null) {
            output.Messages.Add("Error Deleting DepartmentGroup " + DepartmentGroupId.ToString() + " - Record No Longer Exists");
        } else {
            // First, delete related data
            Guid tenantId = GuidValue(rec.TenantId);
            var tenantSettings = GetTenantSettings(GuidValue(rec.TenantId));

            if(ForceDeleteImmediately || tenantSettings.DeletePreference == DataObjects.DeletePreference.Immediate) {
                var deleteAppRecords = await DeleteRecordsApp(rec, CurrentUser);
                if (!deleteAppRecords.Result) {
                    output.Messages.AddRange(deleteAppRecords.Messages);
                    return output;
                }

                try {
                    var recs = await data.Departments.Where(x => x.DepartmentGroupId == DepartmentGroupId).ToListAsync();
                    if (recs != null) {
                        foreach (var r in recs) {
                            r.DepartmentGroupId = null;
                        }
                        await data.SaveChangesAsync();
                    }
                } catch (Exception ex) {
                    output.Messages.Add("Error Deleting Related Department Records for DepartmentGroup " + DepartmentGroupId.ToString() + ":");
                    output.Messages.AddRange(RecurseException(ex));
                    return output;
                }
            }

            if(ForceDeleteImmediately || tenantSettings.DeletePreference == DataObjects.DeletePreference.Immediate) {
                data.DepartmentGroups.Remove(rec);
            } else {
                rec.Deleted = true;
                rec.DeletedAt = DateTime.UtcNow;
                rec.LastModified = DateTime.UtcNow;
                if(CurrentUser != null) {
                    rec.LastModifiedBy = CurrentUserIdString(CurrentUser);
                }
            }
            
            try {
                await data.SaveChangesAsync();
                output.Result = true;

                if (tenantId != Guid.Empty) {
                    ClearTenantCache(tenantId);
                }

                if (!ForceDeleteImmediately) {
                    await SignalRUpdate(new DataObjects.SignalRUpdate {
                        TenantId = tenantId,
                        ItemId = DepartmentGroupId,
                        UpdateType = DataObjects.SignalRUpdateType.DepartmentGroup,
                        UserId = CurrentUserId(CurrentUser),
                        Message = "Deleted"
                    });
                }
            } catch (Exception ex) {
                output.Messages.Add("Error Deleting DepartmentGroup " + DepartmentGroupId.ToString() + ":");
                output.Messages.AddRange(RecurseException(ex));
            }
        }

        return output;
    }

    public async Task<Guid> DepartmentIdFromNameAndLocation(Guid TenantId, string? Department, string? Location = "")
    {
        Guid output = Guid.Empty;

        EFModels.EFModels.Department? rec = null;

        string department = StringValue(Department).ToLower();
        string location = StringValue(Location).ToLower();

        // First, try and match on both Department and Location.
        if (!String.IsNullOrWhiteSpace(department) && !String.IsNullOrWhiteSpace(location)) {
            rec = await data.Departments.AsNoTracking()
                .OrderBy(x => x.DepartmentName)
                .FirstOrDefaultAsync(x => x.TenantId == TenantId &&
                    x.Deleted != true &&
                    x.ActiveDirectoryNames != null &&
                    x.ActiveDirectoryNames.ToLower().Contains("{" + department + "}") &&
                    x.ActiveDirectoryNames.ToLower().Contains("[" + location + "]")
                );
        }

        if (rec == null) {
            if (!String.IsNullOrWhiteSpace(department)) {
                // Try and match on just Department
                rec = await data.Departments.AsNoTracking()
                    .OrderBy(x => x.DepartmentName)
                    .FirstOrDefaultAsync(x => x.TenantId == TenantId &&
                        x.Deleted != true &&
                        x.ActiveDirectoryNames != null &&
                        x.ActiveDirectoryNames.ToLower().Contains("{" + department + "}")
                    );
            }
        }

        if (rec == null) {
            if (!String.IsNullOrWhiteSpace(location)) {
                // Finally, try and match on a group that only has a location tag.
                rec = await data.Departments.AsNoTracking()
                    .OrderBy(x => x.DepartmentName)
                    .FirstOrDefaultAsync(x => x.TenantId == TenantId &&
                        x.Deleted != true &&
                        x.ActiveDirectoryNames != null &&
                        x.ActiveDirectoryNames.ToLower().Contains("[" + location + "]") &&
                        !x.ActiveDirectoryNames.Contains("{")
                    );
            }
        }

        if (rec != null && rec.DepartmentId != Guid.Empty) {
            output = rec.DepartmentId;
        }

        return output;
    }

    public async Task<DataObjects.Department> GetDepartment(Guid DepartmentId, DataObjects.User? CurrentUser = null)
    {
        DataObjects.Department output = new DataObjects.Department();

        Department? rec = null;

        if (AdminUser(CurrentUser)) {
            rec = await data.Departments.FirstOrDefaultAsync(x => x.DepartmentId == DepartmentId);
        } else {
            rec = await data.Departments.FirstOrDefaultAsync(x => x.DepartmentId == DepartmentId && x.Deleted != true);
        }
        
        if (rec == null) {
            output.ActionResponse.Messages.Add("Department " + DepartmentId.ToString() + " Not Found");
        } else {
            output = new DataObjects.Department { 
                ActionResponse = GetNewActionResponse(true),
                ActiveDirectoryNames = rec.ActiveDirectoryNames,
                Added = rec.Added,
                AddedBy = LastModifiedDisplayName(rec.AddedBy),
                Deleted = rec.Deleted,
                DeletedAt = rec.DeletedAt,
                DepartmentGroupId = rec.DepartmentGroupId,
                DepartmentId = rec.DepartmentId,
                DepartmentName = rec.DepartmentName,
                Enabled = rec.Enabled,
                LastModified = rec.LastModified,
                LastModifiedBy = LastModifiedDisplayName(rec.LastModifiedBy),
                TenantId = rec.TenantId,
            };

            GetDataApp(rec, output, CurrentUser);
        }

        return output;
    }

    public async Task<DataObjects.DepartmentGroup> GetDepartmentGroup(Guid DepartmentGroupId, DataObjects.User? CurrentUser = null)
    {
        DataObjects.DepartmentGroup output = new DataObjects.DepartmentGroup();

        DepartmentGroup? rec = null;

        if(AdminUser(CurrentUser)) {
            rec = await data.DepartmentGroups.FirstOrDefaultAsync(x => x.DepartmentGroupId == DepartmentGroupId);
        } else {
            rec = await data.DepartmentGroups.FirstOrDefaultAsync(x => x.DepartmentGroupId == DepartmentGroupId && x.Deleted != true);
        }
        
        if(rec != null) {
            output = new DataObjects.DepartmentGroup { 
                ActionResponse = GetNewActionResponse(true),
                Added = rec.Added,
                AddedBy = LastModifiedDisplayName(rec.AddedBy),
                DepartmentGroupId = rec.DepartmentGroupId,
                DepartmentGroupName = rec.DepartmentGroupName,
                Deleted = rec.Deleted,
                DeletedAt = rec.DeletedAt,
                TenantId = GuidValue(rec.TenantId),
                LastModified = rec.LastModified,
                LastModifiedBy = LastModifiedDisplayName(rec.LastModifiedBy),
            };

            GetDataApp(rec, output, CurrentUser);
        } else {
            output.ActionResponse = GetNewActionResponse(false, "Department Group '" + DepartmentGroupId.ToString() + "' Not Found");
        }

        return output;
    }

    public async Task<List<DataObjects.DepartmentGroup>> GetDepartmentGroups(Guid TenantId, DataObjects.User? CurrentUser = null)
    {
        List<DataObjects.DepartmentGroup> output = new List<DataObjects.DepartmentGroup>();

        List<DepartmentGroup>? recs = null;

        if(AdminUser(CurrentUser)) {
            recs = await data.DepartmentGroups.Where(x => x.TenantId == TenantId)
            .OrderBy(x => x.DepartmentGroupName).ToListAsync();
        } else {
            recs = await data.DepartmentGroups.Where(x => x.TenantId == TenantId && x.Deleted != true)
            .OrderBy(x => x.DepartmentGroupName).ToListAsync();
        }
        
        if (recs != null && recs.Count() > 0) {
            foreach (var rec in recs) {
                var d = new DataObjects.DepartmentGroup {
                    ActionResponse = GetNewActionResponse(true),
                    Added = rec.Added,
                    AddedBy = LastModifiedDisplayName(rec.AddedBy),
                    DepartmentGroupId = rec.DepartmentGroupId,
                    TenantId = TenantId,
                    DepartmentGroupName = rec.DepartmentGroupName,
                    Deleted = rec.Deleted,
                    DeletedAt = rec.DeletedAt,
                    LastModified = rec.LastModified,
                    LastModifiedBy = LastModifiedDisplayName(rec.LastModifiedBy),
                };

                GetDataApp(rec, d, CurrentUser);

                output.Add(d);
            }
        }

        return output;
    }

    public string GetDepartmentName(Guid TenantId, Guid DepartmentId)
    {
        string output = String.Empty;

        var rec = data.Departments.FirstOrDefault(x => x.TenantId == TenantId && x.DepartmentId == DepartmentId && x.Deleted != true);
        if(rec != null && !String.IsNullOrEmpty(rec.DepartmentName)) {
            output = rec.DepartmentName;
        }

        return output;
    }

    public async Task<List<DataObjects.Department>> GetDepartments(Guid TenantId, DataObjects.User? CurrentUser = null)
    {
        List<DataObjects.Department> output = new List<DataObjects.Department>();

        List<Department>? recs = null;

        if(AdminUser(CurrentUser)) {
            recs = await data.Departments.Where(x => x.TenantId == TenantId).OrderBy(x => x.DepartmentName).ToListAsync();
        } else {
            recs = await data.Departments.Where(x => x.TenantId == TenantId && x.Deleted != true).OrderBy(x => x.DepartmentName).ToListAsync();
        }
        
        if (recs != null && recs.Count() > 0) {
            foreach (var rec in recs) {
                var d = new DataObjects.Department {
                    ActionResponse = GetNewActionResponse(true),
                    ActiveDirectoryNames = rec.ActiveDirectoryNames,
                    Added = rec.Added,
                    AddedBy = LastModifiedDisplayName(rec.AddedBy),
                    DepartmentId = rec.DepartmentId,
                    TenantId = GuidValue(rec.TenantId),
                    DepartmentName = rec.DepartmentName,
                    Enabled = rec.Enabled,
                    Deleted = rec.Deleted,
                    DeletedAt = rec.DeletedAt,
                    DepartmentGroupId = rec.DepartmentGroupId,
                    LastModified = rec.LastModified,
                    LastModifiedBy = LastModifiedDisplayName(rec.LastModifiedBy),
                };

                GetDataApp(rec, d, CurrentUser);

                output.Add(d);
            }
        }

        return output;
    }

    public async Task<DataObjects.Department> SaveDepartment(DataObjects.Department department, DataObjects.User? CurrentUser = null)
    {
        var output = department;
        output.ActionResponse = GetNewActionResponse();

        if (GuidValue(output.TenantId) != Guid.Empty) {
            DateTime now = DateTime.UtcNow;

            bool newRecord = false;
            var rec = await data.Departments.FirstOrDefaultAsync(x => x.DepartmentId == output.DepartmentId);
            if (rec == null) {
                if (output.DepartmentId == Guid.Empty) {
                    rec = new Department();
                    output.DepartmentId = Guid.NewGuid();
                    rec.Added = now;
                    rec.AddedBy = CurrentUserIdString(CurrentUser);
                    rec.DepartmentId = output.DepartmentId;
                    rec.Deleted = false;
                    newRecord = true;
                } else {
                    output.ActionResponse.Messages.Add("Error Saving Department " + output.DepartmentId.ToString() + " - Record No Longer Exists");
                    return output;
                }
            }

            output.DepartmentName = MaxStringLength(output.DepartmentName, 100);
            output.ActiveDirectoryNames = MaxStringLength(output.ActiveDirectoryNames, 100);

            rec.TenantId = GuidValue(output.TenantId);
            rec.DepartmentName = StringValue(output.DepartmentName);
            rec.ActiveDirectoryNames = output.ActiveDirectoryNames;
            rec.Enabled = output.Enabled;
            rec.DepartmentGroupId = output.DepartmentGroupId;
            rec.LastModified = now;

            if(CurrentUser != null) {
                rec.LastModifiedBy = CurrentUserIdString(CurrentUser);

                if (CurrentUser.Admin) {
                    rec.Deleted = output.Deleted;
                }
            }

            SaveDataApp(rec, output, CurrentUser);

            try {
                if (newRecord) {
                    data.Departments.Add(rec);
                }
                await data.SaveChangesAsync();
                output.ActionResponse.Result = true;
                ClearTenantCache(output.TenantId);

                await SignalRUpdate(new DataObjects.SignalRUpdate {
                    TenantId = output.TenantId,
                    ItemId = output.DepartmentId,
                    UpdateType = DataObjects.SignalRUpdateType.Department,
                    Message = "Saved",
                    UserId = CurrentUserId(CurrentUser),
                    Object = output
                });
            } catch (Exception ex) {
                output.ActionResponse.Messages.Add("Error Saving Department " + output.DepartmentId.ToString());
                output.ActionResponse.Messages.AddRange(RecurseException(ex));
            }
        } else {
            output.ActionResponse.Messages.Add("Invalid DepartmentId");
        }

        return output;
    }

    public async Task<DataObjects.DepartmentGroup> SaveDepartmentGroup(DataObjects.DepartmentGroup departmentGroup, DataObjects.User? CurrentUser = null)
    {
        var output = departmentGroup;
        output.ActionResponse = GetNewActionResponse();

        if (GuidValue(output.TenantId) != Guid.Empty) {
            DateTime now = DateTime.UtcNow;

            bool newRecord = false;
            var rec = await data.DepartmentGroups.FirstOrDefaultAsync(x => x.DepartmentGroupId == output.DepartmentGroupId);
            if (rec == null) {
                if (output.DepartmentGroupId == Guid.Empty) {
                    rec = new DepartmentGroup();
                    output.DepartmentGroupId = Guid.NewGuid();
                    rec.Added = now;
                    rec.AddedBy = CurrentUserIdString(CurrentUser);
                    rec.DepartmentGroupId = output.DepartmentGroupId;
                    rec.TenantId = output.TenantId;
                    rec.Deleted = false;
                    newRecord = true;
                } else {
                    output.ActionResponse.Messages.Add("Error Saving DepartmentGroup " + output.DepartmentGroupId.ToString() + " - Record No Longer Exists");
                    return output;
                }
            }

            output.DepartmentGroupName = MaxStringLength(output.DepartmentGroupName, 200);

            rec.DepartmentGroupName = output.DepartmentGroupName;
            rec.LastModified = now;

            if(CurrentUser != null) {
                rec.LastModifiedBy = CurrentUserIdString(CurrentUser);

                if (CurrentUser.Admin) {
                    rec.Deleted = output.Deleted;
                }
            }

            SaveDataApp(rec, output, CurrentUser);

            try {
                if (newRecord) {
                    data.DepartmentGroups.Add(rec);
                }
                await data.SaveChangesAsync();
                output.ActionResponse.Result = true;
                ClearTenantCache(output.TenantId);

                await SignalRUpdate(new DataObjects.SignalRUpdate {
                    TenantId = output.TenantId,
                    ItemId = output.DepartmentGroupId,
                    UpdateType = DataObjects.SignalRUpdateType.DepartmentGroup,
                    UserId = CurrentUserId(CurrentUser),
                    Message = "Saved",
                    Object = output
                });
            } catch (Exception ex) {
                output.ActionResponse.Messages.Add("Error Saving DepartmentGroup " + output.DepartmentGroupId.ToString());
                output.ActionResponse.Messages.AddRange(RecurseException(ex));
            }
        } else {
            output.ActionResponse.Messages.Add("Invalid DepartmentId");
        }

        return output;
    }

    public async Task<List<DataObjects.Department>> SaveDepartments(List<DataObjects.Department> departments, DataObjects.User? CurrentUser = null)
    {
        List<DataObjects.Department> output = new List<DataObjects.Department>();
        foreach (var department in departments) {
            var saved = await SaveDepartment(department, CurrentUser);
            output.Add(saved);
        }
        return output;
    }
}