namespace CRM;

// Use this file as a place to put any application-specific data access methods.

public partial interface IDataAccess
{
    DataObjects.BooleanResponse YourMethod();
}

public partial class DataAccess
{
    /// <summary>
    /// Use this area to add your custom language tags for your app, or to override built-in language tags (eg: AppTitle).
    /// </summary>
    private Dictionary<string, string> AppLanguage {
        get {
            return new Dictionary<string, string>
            {
                //{ "AppTitle", "My Custom App Title" },
                //{ "YourLanguageItem", "Your Language Item" },
            };
        }
    }

    private void DataAccessAppInit()
    {
        // Add any app-specific initialization logic here.
    }

    /// <summary>
    /// Use this method to delete any pending records for your app-specific tables.
    /// Return true if everything was deleted successfully.
    /// Otherwise, return false and include any error messages in the Messages property.
    /// </summary>
    private async Task<DataObjects.BooleanResponse> DeleteAllPendingDeletedRecordsApp(Guid TenantId, DateTime OlderThan)
    {
        await Task.Delay(0); // Simulate a delay since this method has to be async. This can be removed once you implement your await logic.

        var output = new DataObjects.BooleanResponse();

        output.Result = true;

        return output;
    }

    /// <summary>
    /// Use this method to immediately delete any app-specific records.
    /// Return true if the item was deleted successfully.
    /// Otherwise, return false and include any error messages in the Messages property.
    /// </summary>
    private async Task<DataObjects.BooleanResponse> DeleteRecordImmediatelyApp(string? Type, Guid RecordId, DataObjects.User CurrentUser)
    {
        await Task.Delay(0); // Simulate a delay since this method has to be async. This can be removed once you implement your await logic.

        var output = new DataObjects.BooleanResponse();

        if (!String.IsNullOrWhiteSpace(Type)) {
            switch (Type.ToLower()) {
                case "mytype":
                    //output = await DeleteMyItemType(RecordId, CurrentUser);
                    break;
            }
        }

        // Remove this line once you implement your logic.
        output.Result = true;

        return output;
    }

    /// <summary>
    /// This method is called when the tenant is being deleted to delete app-specific data for the tenant.
    /// </summary>
    private async Task<DataObjects.BooleanResponse> DeleteTenantApp(Guid TenantId)
    {
        await Task.Delay(0); // Simulate a delay since this method has to be async. This can be removed once you implement your await logic.

        var output = new DataObjects.BooleanResponse();

        try {
            // Implement your app-specific tenant deletion logic here to remove records from tables that are specific to your app.
            // Example:
            // data.MyTable.RemoveRange(data.MyTable.Where(x => x.TenantId == TenantId));
            // await data.SaveChangesAsync();
        } catch (Exception ex) {
            output.Messages.Add("An Error Occurred in DeleteTenantApp");
            output.Messages.AddRange(RecurseException(ex));
        }

        output.Result = output.Messages.Count == 0;

        return output;
    }

    /// <summary>
    /// Called by the main GetBlazorDataModel method to load any app-specific data into the Blazor data model.
    /// </summary>
    /// <param name="CurrentUser">The User object for the current user, if loaded.</param>
    /// <returns></returns>
    private async Task<DataObjects.BlazorDataModelLoader> GetBlazorDataModelApp(DataObjects.BlazorDataModelLoader blazorDataModelLoader, DataObjects.User? CurrentUser = null)
    {
        await Task.Delay(0); // Simulate a delay since this method has to be async. This can be removed once you implement your await logic.
        var output = blazorDataModelLoader;

        // Update any app-specific data model properties here.

        return output;
    }

    /// <summary>
    /// This method is called to add any app-specific deleted record counts to the output.
    /// </summary>
    private async Task<DataObjects.DeletedRecordCounts> GetDeletedRecordCountsApp(Guid TenantId, DataObjects.DeletedRecordCounts deletedRecordCounts)
    {
        await Task.Delay(0); // Simulate a delay since this method has to be async. This can be removed once you implement your await logic.

        var output = deletedRecordCounts;

        // Do any lookups for your app-specific deleted record counts here and add them to the output.
        // output.MyCount = await data.MyTable.CountAsync(x => x.TenantId == TenantId && x.Deleted == true);

        return output;
    }

    /// <summary>
    /// This method is called to add any app-specific filter columns to the filter output.
    /// </summary>
    /// <param name="Type">The filter type (eg: Users, Invoices, etc.)</param>
    /// <param name="Position">The position in the column orders (see calling code for details.)</param>
    /// <param name="CurrentUser">The current user object, if one exists</param>
    /// <returns>A list of FilterColumn objects</returns>
    private List<DataObjects.FilterColumn> GetFilterColumnsApp(string Type, string Position, DataObjects.Language Language, DataObjects.User? CurrentUser = null)
    {
        var output = new List<DataObjects.FilterColumn>();
        // Add any app-specific filter columns here.
        // Example:
        // if (Type.ToLower() == "users" && Position.ToLower() == "username") {
        //     output.Add(new DataObjects.FilterColumn { Name = "MyColumn", Type = "string", Title = "My Column", Placeholder = "My Column", Width = 150 });
        // }
        return output;
    }

    /// <summary>
    /// This method is called to add any app-specific deleted records to the output.
    /// </summary>
    private async Task<DataObjects.DeletedRecords> GetDeletedRecordsApp(Guid TenantId, DataObjects.DeletedRecords deletedRecords)
    {
        await Task.Delay(0); // Simulate a delay since this method has to be async. This can be removed once you implement your await logic.

        var output = deletedRecords;

        // Do any lookups for your app-specific deleted records here and add them to the output.

        return output;
    }

    /// <summary>
    /// This method is called to undelete any app-specific records.
    /// </summary>
    private async Task<DataObjects.BooleanResponse> UndeleteRecordApp(string? Type, Guid RecordId, DataObjects.User CurrentUser)
    {
        await Task.Delay(0); // Simulate a delay since this method has to be async. This can be removed once you implement your await logic.

        var output = new DataObjects.BooleanResponse();

        try {
            if (!String.IsNullOrWhiteSpace(Type)) {
                object? obj = null;
                bool sendSignalRUpdate = false;

                switch (Type.ToLower()) {
                    case "this":
                        // Add code to undelete the record of type "this" here.
                        output.Result = true;
                        sendSignalRUpdate = true;
                        break;

                    default:
                        output.Messages.Add("Invalid Delete Record Type '" + Type + "'");
                        break;
                }

                if (sendSignalRUpdate) {
                    await SignalRUpdate(new DataObjects.SignalRUpdate {
                        TenantId = CurrentUser.TenantId,
                        ItemId = RecordId,
                        UpdateType = DataObjects.SignalRUpdateType.Undelete,
                        Message = Type,
                        Object = obj,
                        UserId = CurrentUserId(CurrentUser),
                    });
                }
            }
        } catch (Exception ex) {
            output.Messages.Add("Error Undeleting '" + Type + "' " + RecordId.ToString() + " - " + RecurseExceptionAsString(ex));
        }

        // Do any app-specific undelete logic here. See the main UndeleteRecord method for an example.
        output.Result = true; // Remove this line once you implement your logic.

        return output;
    }

    public DataObjects.BooleanResponse YourMethod()
    {
        return new DataObjects.BooleanResponse { Result = true, Messages = new List<string> { "Your Messages" } };
    }
}