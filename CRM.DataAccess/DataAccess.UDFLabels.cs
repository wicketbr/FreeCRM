//using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CRM;

public partial interface IDataAccess
{
    Task<List<DataObjects.udfLabel>> GetUDFLabels(Guid TenantId, bool includeFilterOptions = true);
    Task<DataObjects.BooleanResponse> SaveUDFLabels(Guid TenantId, List<DataObjects.udfLabel> labels, DataObjects.User? CurrentUser = null);
    bool ShowUDFColumn(string module, int item, List<DataObjects.udfLabel> labels);
}

public partial class DataAccess
{
    private async Task<List<string>> GetUDFLabelFilterOptions(Guid TenantId, string module, string item)
    {
        List<string> output = new List<string>();

        if (!String.IsNullOrWhiteSpace(module)) {
            List<string?>? values = null;

            switch (module.ToUpper()) {
                case "USERS":
                    switch (item.ToUpper()) {
                        case "UDF01":
                            values = await data.Users.Where(x => x.TenantId == TenantId).Select(x => x.UDF01).Distinct().ToListAsync();
                            break;

                        case "UDF02":
                            values = await data.Users.Where(x => x.TenantId == TenantId).Select(x => x.UDF02).Distinct().ToListAsync();
                            break;

                        case "UDF03":
                            values = await data.Users.Where(x => x.TenantId == TenantId).Select(x => x.UDF03).Distinct().ToListAsync();
                            break;

                        case "UDF04":
                            values = await data.Users.Where(x => x.TenantId == TenantId).Select(x => x.UDF04).Distinct().ToListAsync();
                            break;

                        case "UDF05":
                            values = await data.Users.Where(x => x.TenantId == TenantId).Select(x => x.UDF05).Distinct().ToListAsync();
                            break;

                        case "UDF06":
                            values = await data.Users.Where(x => x.TenantId == TenantId).Select(x => x.UDF06).Distinct().ToListAsync();
                            break;

                        case "UDF07":
                            values = await data.Users.Where(x => x.TenantId == TenantId).Select(x => x.UDF07).Distinct().ToListAsync();
                            break;

                        case "UDF08":
                            values = await data.Users.Where(x => x.TenantId == TenantId).Select(x => x.UDF08).Distinct().ToListAsync();
                            break;

                        case "UDF09":
                            values = await data.Users.Where(x => x.TenantId == TenantId).Select(x => x.UDF09).Distinct().ToListAsync();
                            break;

                        case "UDF10":
                            values = await data.Users.Where(x => x.TenantId == TenantId).Select(x => x.UDF10).Distinct().ToListAsync();
                            break;
                    }
                    break;
            }

            if (values != null && values.Count() > 0) {
                foreach (var value in values.OrderBy(x => x)) {
                    if (!String.IsNullOrWhiteSpace(value)) {
                        output.Add(value);
                    }
                }
            }
        }

        return output;
    }

    public async Task<List<DataObjects.udfLabel>> GetUDFLabels(Guid TenantId, bool includeFilterOptions = true)
    {
        List<DataObjects.udfLabel> output = new List<DataObjects.udfLabel>();

        var tenantSettings = GetSettings(TenantId);

        for (int x = 1; x < 11; x++) {
            output.Add(new DataObjects.udfLabel {
                Id = Guid.NewGuid(),
                TenantId = TenantId,
                Module = "Users",
                Label = "",
                ShowColumn = false,
                ShowInFilter = false,
                IncludeInSearch = false,
                udf = "UDF" + x.ToString().PadLeft(2, '0')
            });
        }

        var recs = await data.UDFLabels.Where(x => x.TenantId == TenantId).OrderBy(x => x.Module).ThenBy(x => x.UDF).ToListAsync();
        if (recs != null && recs.Count() > 0) {
            foreach (var rec in recs) {
                var item = output.FirstOrDefault(x => x.Module == rec.Module && x.udf == rec.UDF);
                if (item != null) {
                    item.Id = rec.Id;
                    item.Label = rec.Label;
                    item.ShowColumn = rec.ShowColumn.HasValue ? (bool)rec.ShowColumn : false;
                    item.ShowInFilter = rec.ShowInFilter.HasValue ? (bool)rec.ShowInFilter : false;
                    item.IncludeInSearch = rec.IncludeInSearch.HasValue ? (bool)rec.IncludeInSearch : false;
                    item.LastModified = rec.LastModified;
                    item.LastModifiedBy = LastModifiedDisplayName(rec.LastModifiedBy);

                    // If the ShowInFilter option is checked then get the distinct values for this column
                    if (item.ShowInFilter && includeFilterOptions) {
                        item.FilterOptions = await GetUDFLabelFilterOptions(TenantId, StringValue(item.Module), StringValue(item.udf));
                    }
                }
            }
        } else {
            // Labels do not yet exist in the database, so save them now.
            await SaveUDFLabels(TenantId, output);
        }

        return output;
    }

    public async Task<DataObjects.BooleanResponse> SaveUDFLabels(Guid TenantId, List<DataObjects.udfLabel> labels, DataObjects.User? CurrentUser = null)
    {
        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();

        if (labels != null && labels.Count() > 0) {
            // sort correctly before saving
            var save = labels.OrderBy(x => x.Module).ThenBy(x => x.udf).ToList();

            foreach (var label in save) {
                bool newRecord = false;
                bool modified = false;

                var rec = await data.UDFLabels.FirstOrDefaultAsync(x => x.TenantId == TenantId && x.Id == label.Id);
                if (rec == null) {
                    newRecord = true;
                    modified = true;
                    rec = new UDFLabel();
                    rec.Id = label.Id;
                    rec.TenantId = TenantId;
                    rec.LastModified = DateTime.UtcNow;
                }

                if(rec.Module != MaxStringLength(label.Module, 20)) {
                    rec.Module = MaxStringLength(label.Module, 20);
                    modified = true;
                }

                if(rec.UDF != MaxStringLength(label.udf, 10)) {
                    rec.UDF = MaxStringLength(label.udf, 10);
                    modified = true;
                }

                if (rec.Label != StringValue(label.Label)) {
                    rec.Label = StringValue(label.Label);
                    modified = true;
                }

                if (rec.ShowColumn != label.ShowColumn) {
                    rec.ShowColumn = label.ShowColumn;
                    modified = true;
                }

                if (rec.ShowInFilter != label.ShowInFilter) {
                    rec.ShowInFilter = label.ShowInFilter;
                    modified = true;
                }

                if (rec.IncludeInSearch != label.IncludeInSearch) {
                    rec.IncludeInSearch = label.IncludeInSearch;
                    modified = true;
                }

                if (modified) {
                    rec.LastModified = DateTime.UtcNow;

                    if (CurrentUser != null) {
                        rec.LastModifiedBy = CurrentUserIdString(CurrentUser);
                    }
                }

                try {
                    if (newRecord) {
                        data.UDFLabels.Add(rec);
                    }
                    await data.SaveChangesAsync();

                    ClearTenantCache(TenantId);

                } catch (Exception ex) {
                    output.Messages.Add("Error Saving UDF Label " + label.Id.ToString());
                    output.Messages.AddRange(RecurseException(ex));
                }
            }
        }

        output.Result = output.Messages.Count() == 0;

        if (output.Result) {
            await SignalRUpdate(new DataObjects.SignalRUpdate {
                TenantId = TenantId,
                UpdateType = DataObjects.SignalRUpdateType.Setting,
                Message = "SavedUDFLabels",
                Object = labels
            });
        }

        return output;
    }

    public bool ShowUDFColumn(string module, int item, List<DataObjects.udfLabel> labels)
    {
        bool output = false;

        var label = labels.FirstOrDefault(x => x.Module == module && x.udf == "UDF" + item.ToString().PadLeft(2, '0'));
        if(label != null && !String.IsNullOrEmpty(label.Label)) {
            output = label.ShowColumn;
        }

        //var label = UDFLabel(module, item, labels);
        //bool output = !String.IsNullOrWhiteSpace(label);
        return output;
    }

    private string UDFLabel(string module, string item, List<DataObjects.udfLabel> labels)
    {
        string output = item;

        if (labels != null && labels.Count() > 0) {
            var label = labels.FirstOrDefault(x => x.Module == module && x.udf == item);
            if (label != null && !String.IsNullOrEmpty(label.Label)) {
                output = label.Label;

                if (output.Contains("|")) {
                    output = output.Substring(0, output.IndexOf("|"));
                }
            }
        }

        return output;
    }

    private string UDFLabel(string module, int item, List<DataObjects.udfLabel> labels)
    {
        string output = String.Empty;

        if (labels != null && labels.Count() > 0) {
            var label = labels.FirstOrDefault(x => x.Module == module && x.udf == "UDF" + item.ToString().PadLeft(2, '0'));
            if (label != null && !String.IsNullOrEmpty(label.Label)) {
                output = label.Label;

                if (output.Contains("|")) {
                    output = output.Substring(0, output.IndexOf("|"));
                }
            }
        }

        return output;
    }

    private bool UDFLabelIncludedInSearch(string module, string item, List<DataObjects.udfLabel> labels)
    {
        bool output = false;

        if (labels != null && labels.Count() > 0) {
            var label = labels.FirstOrDefault(x => x.Module == module && x.udf == item);
            if (label != null) {
                output = label.IncludeInSearch;
            }
        }

        return output;
    }
}