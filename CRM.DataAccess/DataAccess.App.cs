namespace CRM;

// Use this file as a place to put any application-specific data access methods.

public partial interface IDataAccess
{
    DataObjects.BooleanResponse YourMethod();
}

public partial class DataAccess
{
    public DataObjects.BooleanResponse YourMethod()
    {
        return new DataObjects.BooleanResponse { Result = true, Messages = new List<string> { "Your Messages" } };
    }
}