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

    public DataObjects.BooleanResponse YourMethod()
    {
        return new DataObjects.BooleanResponse { Result = true, Messages = new List<string> { "Your Messages" } };
    }
}