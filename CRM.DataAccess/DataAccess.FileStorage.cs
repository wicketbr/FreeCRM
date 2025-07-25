namespace CRM;

public partial interface IDataAccess
{
    Task<DataObjects.BooleanResponse> DeleteFileStorage(Guid FileId, DataObjects.User? CurrentUser = null, bool ForceDeleteImmediately = false);
    Task<DataObjects.FileStorage> GetFileStorage(Guid FileId, DataObjects.User? CurrentUser = null);
    Task<List<DataObjects.FileStorage>> GetFileStorageItems(Guid ItemId, bool ImagesOnly, bool ResizeAsThumbnail, DataObjects.User? CurrentUser = null);
    Task<DataObjects.FilterFileStorage> GetFileStorageItems(DataObjects.FilterFileStorage filterFileStorage, DataObjects.User? CurrentUser = null);
    Task<List<DataObjects.FileStorage>> GetFileStorageItems(Guid TenantId);
    Task<List<DataObjects.FileStorage>> GetImageFiles(Guid TenantId);
    Task<List<string>> GetUniqueFileExtensions(Guid TenantId);
    Task<DataObjects.FileStorage> SaveFileStorage(DataObjects.FileStorage fileStorage, DataObjects.User? CurrentUser = null);
    Task<DataObjects.BooleanResponse> UndeleteFileStorage(Guid FileId);
    bool UserCanViewFile(DataObjects.FileStorage file, DataObjects.User CurrentUser);
}

public partial class DataAccess
{
    public async Task<DataObjects.BooleanResponse> DeleteFileStorage(Guid FileId, DataObjects.User? CurrentUser = null, bool ForceDeleteImmediately = false)
    {
        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();

        var now = DateTime.UtcNow;

        var rec = await data.FileStorages.FirstOrDefaultAsync(x => x.FileId == FileId);
        if (rec == null) {
            output.Messages.Add("Error Deleting File Storage " + FileId.ToString() + " - Record No Longer Exists");
        } else {
            Guid tenantId = GuidValue(rec.TenantId);
            var settings = GetTenantSettings(tenantId);

            // If the delete preference is to delete immediately, or if the item is already marked for
            // delete, then delete now.
            if(ForceDeleteImmediately || settings.DeletePreference == DataObjects.DeletePreference.Immediate || rec.Deleted == true) {
                data.FileStorages.Remove(rec);
            } else {
                rec.Deleted = true;
                rec.DeletedAt = now;
                rec.LastModified = now;
                if(CurrentUser != null) {
                    rec.LastModifiedBy = CurrentUserIdString(CurrentUser);
                }
            }
            
            try {
                await data.SaveChangesAsync();
                output.Result = true;

                if (!ForceDeleteImmediately) {
                    await SignalRUpdate(new DataObjects.SignalRUpdate {
                        TenantId = tenantId,
                        ItemId = FileId,
                        UpdateType = DataObjects.SignalRUpdateType.File,
                        Message = "Deleted",
                        UserId = CurrentUserId(CurrentUser),
                    });
                }
            } catch (Exception ex) {
                output.Messages.Add("Error Deleting File Storage " + FileId.ToString() + ":");
                output.Messages.AddRange(RecurseException(ex));
            }
        }

        return output;
    }

    private string FileIdClosestToDate(List<DataObjects.FileStorage> files, string Filename, DateTime? itemTime)
    {
        string output = String.Empty;

        DateTime checkTime = itemTime.HasValue
            ? Convert.ToDateTime(itemTime)
            : DateTime.UtcNow;

        double? TotalSeconds = null;
        var recs = files.Where(x => x.FileName != null && x.FileName.ToLower() == Filename.ToLower());
        if (recs != null && recs.Count() > 0) {
            foreach (var rec in recs) {
                double Seconds = Math.Abs((checkTime - rec.UploadDate).TotalSeconds);
                if (TotalSeconds == null || Seconds < TotalSeconds) {
                    output = rec.FileId.ToString();
                    TotalSeconds = Seconds;
                }
            }
        }

        return output;
    }

    private byte[]? FileStorageToBrowser(byte[] value, string Extension, long? Bytes, bool ImagesOnly, bool ResizeAsThumbnail)
    {
        if (!Bytes.HasValue) { Bytes = 0; }

        byte[]? output = value;
        if (ImagesOnly) {
            switch (Extension.ToUpper()) {
                case ".JPG":
                case ".JPEG":
                case ".PNG":
                case ".GIF":
                    // OK to return these, but in the future these will be resized as thumbnails
                    break;
                default:
                    output = null;
                    break;
            }
        }
        return output;
    }

    public async Task<DataObjects.FileStorage> GetFileStorage(Guid FileId, DataObjects.User? CurrentUser = null)
    {
        DataObjects.FileStorage output = new DataObjects.FileStorage();
        output.ActionResponse = GetNewActionResponse();

        FileStorage? rec = null;

        if(AdminUser(CurrentUser)) {
            rec = await data.FileStorages.FirstOrDefaultAsync(x => x.FileId == FileId);
        } else {
            rec = await data.FileStorages.FirstOrDefaultAsync(x => x.FileId == FileId && x.Deleted != true);
        }
        
        if (rec == null) {
            output.ActionResponse.Messages.Add("File " + FileId.ToString() + " Does Not Exist");
        } else {
            output = new DataObjects.FileStorage {
                ActionResponse = GetNewActionResponse(true),
                TenantId = GuidValue(rec.TenantId),
                Extension = rec.Extension,
                Bytes = rec.Bytes.HasValue ? (long)rec.Bytes : (long?)null,
                Deleted = rec.Deleted,
                DeletedAt = rec.DeletedAt,
                FileId = FileId,
                FileName = rec.FileName,
                ItemId = rec.ItemId,
                LastModified = rec.LastModified,
                LastModifiedBy = LastModifiedDisplayName(rec.LastModifiedBy),
                SourceFileId = rec.SourceFileId,
                UploadDate = rec.UploadDate,
                UploadedBy = LastModifiedDisplayName(rec.UploadedBy),
                UserId = rec.UserId,
                Value = rec.Value != null ? rec.Value.ToArray() : null,
            };
        }

        return output;
    }

    public async Task<List<DataObjects.FileStorage>> GetFileStorageItems(Guid ItemId, bool ImagesOnly, bool ResizeAsThumbnail, DataObjects.User? CurrentUser = null)
    {
        List<DataObjects.FileStorage> output = new List<DataObjects.FileStorage>();

        List<FileStorage>? recs = null;

        if(AdminUser(CurrentUser)) {
            recs = await data.FileStorages.Where(x => x.ItemId == ItemId).OrderBy(x => x.UploadDate).ToListAsync();
        } else {
            recs = await data.FileStorages.Where(x => x.ItemId == ItemId && x.Deleted != true).OrderBy(x => x.UploadDate).ToListAsync();
        }
        
        if (recs != null && recs.Count() > 0) {
            foreach (var rec in recs) {
                long? Bytes = rec.Bytes.HasValue ? (long)rec.Bytes : (long?)null;
                byte[]? value = null;

                if (rec.Value != null && !String.IsNullOrEmpty(rec.Extension)) {
                    value = FileStorageToBrowser(rec.Value.ToArray(), rec.Extension, Bytes, ImagesOnly, ResizeAsThumbnail);
                }

                output.Add(new DataObjects.FileStorage {
                    ActionResponse = GetNewActionResponse(true),
                    Extension = rec.Extension,
                    Bytes = Bytes,
                    Deleted = rec.Deleted,
                    DeletedAt = rec.DeletedAt,
                    FileId = rec.FileId,
                    FileName = rec.FileName,
                    ItemId = rec.ItemId,
                    LastModified = rec.LastModified,
                    LastModifiedBy = LastModifiedDisplayName(rec.LastModifiedBy),
                    TenantId = GuidValue(rec.TenantId),
                    SourceFileId = rec.SourceFileId,
                    UploadDate = rec.UploadDate,
                    UploadedBy = LastModifiedDisplayName(rec.UploadedBy),
                    UserId = rec.UserId,
                    Value = value
                });
            }
        }

        return output;
    }

    public async Task<DataObjects.FilterFileStorage> GetFileStorageItems(DataObjects.FilterFileStorage filter, DataObjects.User? CurrentUser = null)
    {
        filter.ActionResponse = GetNewActionResponse();
        filter.Records = null;

        var language = GetTenantLanguage(filter.TenantId, StringValue(filter.CultureCode));

        filter.AvailableExtensions = await GetUniqueFileExtensions(filter.TenantId);

        filter.Columns = new List<DataObjects.FilterColumn> { 
            new DataObjects.FilterColumn { 
                Sortable = false,
                Label = "",
                DataElementName = "FileId",
                DataType = "Guid",
            },
            new DataObjects.FilterColumn {
                Sortable = true,
                Label = GetLanguageItem("FileName", language),
                DataElementName = "FileName",
                DataType = "string",
            },
            new DataObjects.FilterColumn {
                Sortable = true,
                Label = GetLanguageItem("Extension", language),
                DataElementName = "Extension",
                DataType = "string",
            },
            new DataObjects.FilterColumn{ 
                Sortable = true,
                Label = GetLanguageItem("LastModified", language),
                DataElementName = "LastModified",
                DataType = "DateTime",
            }
        };

        if(AdminUser(CurrentUser)) {
            filter.Columns.Add(
                new DataObjects.FilterColumn {
                    Align = "center",
                    Sortable = false,
                    Label = GetLanguageItem("Deleted", language),
                    DataElementName = "Deleted",
                    DataType = "bool",
                }
            );
        }

        List<string> excludeSourceTypes = new List<string> { "logo" };

        var recs = data.FileStorages
                .Select(x => 
                    new FileStorage { 
                        TenantId = x.TenantId,
                        Extension = x.Extension,
                        Bytes = x.Bytes,
                        Deleted = x.Deleted,
                        DeletedAt = x.DeletedAt,
                        FileId = x.FileId,
                        FileName = x.FileName,
                        ItemId = x.ItemId,
                        LastModified = x.LastModified,
                        LastModifiedBy = x.LastModifiedBy,
                        SourceFileId = x.SourceFileId,
                        UploadDate = x.UploadDate,
                        UploadedBy = x.UploadedBy,
                        UserId = x.UserId,
                    }
                ).Where(x => 
                    x.TenantId == filter.TenantId && 
                    x.ItemId == null && 
                    x.UserId == null && 
                    !excludeSourceTypes.Contains(x.SourceFileId != null ? x.SourceFileId : ""));

        if (AdminUser(CurrentUser) && filter.IncludeDeletedItems) {
            // Keep deleted items
        } else {
            recs = recs.Where(x => x.Deleted != true);
        }

        if (filter.Start.HasValue) {
            recs = recs.Where(x => x.LastModified >= (DateTime)filter.Start);
        }

        if (filter.End.HasValue) {
            recs = recs.Where(x => x.LastModified <= (DateTime)filter.End);
        }

        if (filter.Extensions != null && filter.Extensions.Any()) {
            // Make sure all extensions are lowercase to compare
            List<string> extensions = new List<string>();
            foreach(var ext in filter.Extensions) {
                extensions.Add(ext.ToLower());
            }

            recs = recs.Where(x => x.Extension != null && extensions.Contains(x.Extension.ToLower()));
        }

        if (!String.IsNullOrWhiteSpace(filter.Keyword)) {
            string keyword = filter.Keyword.ToLower();
            recs = recs.Where(x => 
                (x.FileName != null && x.FileName.ToLower().Contains(keyword)) ||
                (x.Extension != null && x.Extension.ToLower().Contains(keyword)) ||
                (x.SourceFileId != null && x.SourceFileId.ToLower().Contains(keyword))
            );
        }

        if (String.IsNullOrWhiteSpace(filter.Sort)) {
            filter.Sort = "FileName";
            filter.SortOrder = "ASC";
        }

        if (String.IsNullOrWhiteSpace(filter.SortOrder)) {
            switch (filter.Sort.ToUpper()) {
                case "LastModified":
                    filter.SortOrder = "DESC";
                    break;

                default:
                    filter.SortOrder = "ASC";
                    break;
            }
        }

        bool ascending = true;
        if (StringValue(filter.SortOrder).ToUpper() == "DESC") {
            ascending = false;
        }

        switch (StringValue(filter.Sort).ToUpper()) {
            case "FILENAME":
                recs = ascending
                    ? recs.OrderBy(x => x.FileName)
                    : recs.OrderByDescending(x => x.FileName);
                break;

            case "EXTENSION":
                recs = ascending
                    ? recs.OrderBy(x => x.Extension).ThenBy(x => x.FileName)
                    : recs.OrderByDescending(x => x.Extension).ThenByDescending(x => x.FileName);
                break;

            case "LASTMODIFIED":
                recs = ascending
                    ? recs.OrderBy(x => x.LastModified)
                    : recs.OrderByDescending(x => x.LastModified);
                break;
        }

        if (recs != null && recs.Count() > 0) {

            int TotalRecords = recs.Count();
            filter.RecordCount = TotalRecords;

            if (filter.RecordsPerPage > 0) {
                // We are filtering records per page
                if (filter.RecordsPerPage >= TotalRecords) {
                    filter.Page = 1;
                    filter.PageCount = 1;
                } else {
                    // Figure out the page count
                    if (filter.Page < 1) { filter.Page = 1; }
                    if (filter.RecordsPerPage < 1) { filter.RecordsPerPage = 25; }
                    decimal decPages = (decimal)TotalRecords / (decimal)filter.RecordsPerPage;
                    decPages = Math.Ceiling(decPages);
                    filter.PageCount = (int)decPages;

                    if (filter.Page > filter.PageCount) {
                        filter.Page = filter.PageCount;
                    }

                    if (filter.Page > 1) {
                        recs = recs.Skip((filter.Page - 1) * filter.RecordsPerPage).Take(filter.RecordsPerPage);
                    } else {
                        recs = recs.Take(filter.RecordsPerPage);
                    }

                }
            }

            List<DataObjects.FileStorage> records = new List<DataObjects.FileStorage>();

            foreach (var rec in recs) {
                var file = new DataObjects.FileStorage { 
                    ActionResponse = GetNewActionResponse(true),
                    FileId = rec.FileId,
                    Extension = rec.Extension,
                    Bytes = rec.Bytes,
                    Deleted = rec.Deleted,
                    DeletedAt = rec.DeletedAt,
                    FileName = rec.FileName,
                    ItemId = rec.ItemId,
                    LastModified = rec.LastModified,
                    LastModifiedBy = LastModifiedDisplayName(rec.LastModifiedBy),
                    TenantId = GuidValue(rec.TenantId),
                    SourceFileId = rec.SourceFileId,
                    UploadDate = rec.UploadDate,
                    UploadedBy = LastModifiedDisplayName(rec.UploadedBy),
                    UserId = rec.UserId,
                    Value = null,
                };
                records.Add(file);
            }

            filter.Records = records;
        }

        filter.ActionResponse.Result = true;

        return filter;
    }

    public async Task<List<DataObjects.FileStorage>> GetFileStorageItems(Guid TenantId)
    {
        var output = new List<DataObjects.FileStorage>();

        List<string> excludeSourceTypes = new List<string> { "logo" };

        var recs = await data.FileStorages
            .Select(x => 
                new { 
                    x.TenantId, x.Extension, x.Bytes, x.Deleted, x.DeletedAt, x.FileId, x.FileName, x.ItemId, x.LastModified,
                    x.LastModifiedBy, x.SourceFileId, x.UploadDate, x.UploadedBy, x.UserId 
                })
            .Where(x => x.TenantId == TenantId && x.ItemId == null && x.UserId == null && x.SourceFileId != null && !excludeSourceTypes.Contains(x.SourceFileId)).ToListAsync();
        if(recs != null && recs.Any()) {
            foreach(var rec in recs) {
                output.Add(new DataObjects.FileStorage {
                    ActionResponse = GetNewActionResponse(true),
                    Extension = rec.Extension,
                    Bytes = rec.Bytes.HasValue ? (long)rec.Bytes : (long?)null,
                    Deleted = rec.Deleted,
                    DeletedAt = rec.DeletedAt,
                    FileId = rec.FileId,
                    FileName = rec.FileName,
                    ItemId = rec.ItemId,
                    LastModified = rec.LastModified,
                    LastModifiedBy = LastModifiedDisplayName(rec.LastModifiedBy),
                    TenantId = GuidValue(rec.TenantId),
                    SourceFileId = rec.SourceFileId,
                    UploadDate = rec.UploadDate,
                    UploadedBy = LastModifiedDisplayName(rec.UploadedBy),
                    UserId = rec.UserId,
                    Value = null,
                });
            }
        }

        return output;
    }

    public async Task<List<DataObjects.FileStorage>> GetImageFiles(Guid TenantId)
    {
        var output = new List<DataObjects.FileStorage>();

        List<string> imageExtensions = new List<string> { ".gif", ".jpg", ".png", ".svg" };

        var recs = await data.FileStorages
            .Where(x => x.TenantId == TenantId && x.UserId == null && x.Deleted != true && imageExtensions.Contains(x.Extension.ToLower()))
            .Select(x => new { x.Extension, x.Bytes, x.FileId, x.SourceFileId, x.FileName, x.ItemId })
            .ToListAsync();

        if(recs != null && recs.Any()) {
            foreach(var rec in recs) {
                output.Add(new DataObjects.FileStorage { 
                    ActionResponse = GetNewActionResponse(true),
                    Extension = rec.Extension,
                    Bytes = rec.Bytes.HasValue ? (long)rec.Bytes : (long?)null,
                    FileId = rec.FileId,
                    FileName = rec.FileName,
                    ItemId = rec.ItemId,
                    TenantId = TenantId,
                    SourceFileId = rec.SourceFileId,
                });
            }
        }

        return output;
    }

    public async Task<List<string>> GetUniqueFileExtensions(Guid TenantId)
    {
        var output = new List<string>();

        var recs = await data.FileStorages
            .Where(x => x.TenantId == TenantId && x.UserId == null && x.Deleted != true)
            .Select(x => x.Extension.ToLower())
            .Distinct()
            .ToListAsync();
        if(recs != null && recs.Any()) {
            output = recs.OrderBy(x => x).ToList();
        }

        return output;
    }

    public async Task<DataObjects.FileStorage> SaveFileStorage(DataObjects.FileStorage fileStorage, DataObjects.User? CurrentUser = null)
    {
        var output = fileStorage;
        output.ActionResponse = GetNewActionResponse();

        DateTime now = DateTime.UtcNow;
        bool newRecord = false;

        FileStorage? rec = null;

        if (AdminUser(CurrentUser)) {
            rec = await data.FileStorages.FirstOrDefaultAsync(x => x.FileId == output.FileId);
        } else {
            rec = await data.FileStorages.FirstOrDefaultAsync(x => x.FileId == output.FileId && x.Deleted != true);
        }

        if (rec == null) {
            // See if the record is deleted.
            if (!AdminUser(CurrentUser)) {
                rec = await data.FileStorages.FirstOrDefaultAsync(x => x.FileId == output.FileId && x.Deleted == true);
                if (rec != null) {
                    output.ActionResponse.Messages.Add("File " + output.FileId.ToString() + " Has Been Deleted");
                    return output;
                }
            }

            // See if there is already a file with this same file name
            rec = await data.FileStorages.FirstOrDefaultAsync(x => x.TenantId == output.TenantId && x.ItemId == fileStorage.ItemId &&
                x.UserId == fileStorage.UserId && x.FileName != null && x.FileName.ToLower() == StringValue(output.FileName).ToLower());
        }

        if (rec == null) {
            if (output.FileId == Guid.Empty) {
                newRecord = true;
                output.FileId = Guid.NewGuid();

                rec = new FileStorage {
                    FileId = output.FileId,
                    Deleted = false,
                    UploadDate = now,
                    UploadedBy = CurrentUserIdString(CurrentUser),
                };
            } else {
                output.ActionResponse.Messages.Add("Error Saving File " + output.FileId.ToString() + " - Record No Longer Exists");
                return output;
            }
        } else {
            // Relacing an existing file
            output.FileId = rec.FileId;
            output.Deleted = false;
            output.DeletedAt = null;
        }

        output.FileName = MaxStringLength(output.FileName, 255);
        output.Extension = MaxStringLength(output.Extension, 15);
        output.SourceFileId = MaxStringLength(output.SourceFileId, 100);

        if (output.UploadDate == DateTime.MinValue) {
            output.UploadDate = now;
        }

        rec.TenantId = output.TenantId;
        rec.ItemId = output.ItemId;
        rec.FileName = StringValue(output.FileName);
        rec.Extension = StringValue(output.Extension);

        if (!String.IsNullOrWhiteSpace(output.SourceFileId)) {
            if (output.SourceFileId.Length > 100) {
                output.SourceFileId = output.SourceFileId.Substring(0, 100);
            }
            rec.SourceFileId = output.SourceFileId;
        } else {
            rec.SourceFileId = null;
        }

        rec.Bytes = output.Bytes;
        rec.Value = output.Value;
        rec.UploadDate = output.UploadDate != DateTime.MinValue ? output.UploadDate : now;
        rec.UserId = output.UserId.HasValue ? (Guid)output.UserId : (Guid?)null;

        rec.LastModified = now;
        if(CurrentUser != null) {
            rec.LastModifiedBy = CurrentUserIdString(CurrentUser);
            if (CurrentUser.Admin) {
                rec.Deleted = output.Deleted;
            }
        }

        try {
            if (newRecord) {
                data.FileStorages.Add(rec);
            }
            await data.SaveChangesAsync();
            output.ActionResponse.Result = true;

            await SignalRUpdate(new DataObjects.SignalRUpdate {
                TenantId = output.TenantId,
                ItemId = output.FileId,
                UpdateType = DataObjects.SignalRUpdateType.File,
                Message = "Saved",
                Object = output,
                UserId = CurrentUserId(CurrentUser),
            });
        } catch (Exception ex) {
            output.ActionResponse.Messages.Add("Error Saving File " + output.FileId.ToString() + ":");
            output.ActionResponse.Messages.AddRange(RecurseException(ex));
        }

        return output;
    }

    public async Task<DataObjects.BooleanResponse> UndeleteFileStorage(Guid FileId)
    {
        var output = new DataObjects.BooleanResponse();

        var rec = await data.FileStorages.FirstOrDefaultAsync(x => x.FileId == FileId);
        if(rec != null) {
            rec.Deleted = false;
            await data.SaveChangesAsync();
            output.Result = true;
        } else {
            output.Messages.Add("File No Longer Exists");
        }

        return output;
    }

    public bool UserCanViewFile(DataObjects.FileStorage file, DataObjects.User CurrentUser)
    {
        bool output = false;

        if (CurrentUser.AppAdmin || CurrentUser.Admin || CurrentUser.ManageFiles) {
            output = true;
        } else if (file.UserId.HasValue && file.UserId == CurrentUser.UserId) {
            output = true;
        } else if (file.ItemId.HasValue) {
            // See if this is part of a special item and check access.
        }

        return output;
    }
}