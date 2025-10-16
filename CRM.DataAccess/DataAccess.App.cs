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
    /// This method is called to map any app-specific fields from the EF Appointment model to the Appointment data object.
    /// </summary>
    /// <param name="rec">The Appointment data record.</param>
    /// <param name="appointment">The Appointment data object.</param>
    /// <param name="CurrentUser">The current user object, if one exists.</param>
    /// <returns>An updated Appointment data object.</returns>
    private DataObjects.Appointment GetAppointmentApp(EFModels.EFModels.Appointment rec, DataObjects.Appointment appointment, DataObjects.User? CurrentUser = null)
    {
        //appointment.Property = rec.Property;
        return appointment;
    }

    /// <summary>
    /// This method is called to map any app-specific fields from the EF Department model to the Department data object.
    /// </summary>
    /// <param name="rec">The Department data record.</param>
    /// <param name="department">The Department data object.</param>
    /// <param name="CurrentUser">The current user object, if one exists.</param>
    /// <returns>An updated Department data object.</returns>
    private DataObjects.Department GetDepartmentApp(EFModels.EFModels.Department rec, DataObjects.Department department, DataObjects.User? CurrentUser = null)
    {
        //department.Property = rec.Property;
        return department;
    }

    /// <summary>
    /// This method is called to map any app-specific fields from the EF DepartmentGroup model to the DepartmentGroup data object.
    /// </summary>
    /// <param name="rec">The DepartmentGroup data record.</param>
    /// <param name="departmentGroup">The DepartmentGroup data object.</param>
    /// <param name="CurrentUser">The current user object, if one exists.</param>
    /// <returns>An updated DepartmentGroup data object.</returns>
    private DataObjects.DepartmentGroup GetDepartmentGroupApp(EFModels.EFModels.DepartmentGroup rec, DataObjects.DepartmentGroup departmentGroup, DataObjects.User? CurrentUser = null)
    {
        //departmentGroup.Property = rec.Property;
        return departmentGroup;
    }

    /// <summary>
    /// This method is called to map any app-specific fields from the EF EmailTemplate model to the EmailTemplate data object.
    /// </summary>
    /// <param name="rec">The EmailTemplate data record.</param>
    /// <param name="emailTemplate">The EmailTemplate data object.</param>
    /// <param name="CurrentUser">The current user object, if one exists.</param>
    /// <returns>An updated EmailTemplate data object.</returns>
    private DataObjects.EmailTemplate GetEmailTemplateApp(EFModels.EFModels.EmailTemplate rec, DataObjects.EmailTemplate emailTemplate, DataObjects.User? CurrentUser = null)
    {
        //emailTemplate.Property = rec.Property;
        return emailTemplate;
    }

    /// <summary>
    /// This method is called to map any app-specific fields from the EF Invoice model to the Invoice data object.
    /// </summary>
    /// <param name="rec">The Invoice data record.</param>
    /// <param name="invoice">The Invoice data object.</param>
    /// <param name="CurrentUser">The current user object, if one exists.</param>
    /// <returns>An updated Invoice data object.</returns>
    private DataObjects.Invoice GetInvoiceApp(EFModels.EFModels.Invoice rec, DataObjects.Invoice invoice, DataObjects.User? CurrentUser = null)
    {
        //invoice.Property = rec.Property;
        return invoice;
    }

    /// <summary>
    /// This method is called to map any app-specific fields from the EF Payment model to the Payment data object.
    /// </summary>
    /// <param name="rec">The Payment data record.</param>
    /// <param name="payment">The Payment data object.</param>
    /// <param name="CurrentUser">The current user object, if one exists.</param>
    /// <returns>An updated Payment data object.</returns>
    private DataObjects.Payment GetPaymentApp(EFModels.EFModels.Payment rec, DataObjects.Payment payment, DataObjects.User? CurrentUser = null)
    {
        //payment.Property = rec.Property;
        return payment;
    }

    /// <summary>
    /// This method is called to map any app-specific fields from the EF Service model to the Service data object.
    /// </summary>
    /// <param name="rec">The Service data record.</param>
    /// <param name="service">The Service data object.</param>
    /// <param name="CurrentUser">The current user object, if one exists.</param>
    /// <returns>An updated Service data object.</returns>
    private DataObjects.Service GetServiceApp(EFModels.EFModels.Service rec, DataObjects.Service service, DataObjects.User? CurrentUser = null)
    {
        //service.Property = rec.Property;
        return service;
    }

    /// <summary>
    /// This method is called to map any app-specific fields from the EF User model to the User data object.
    /// </summary>
    /// <param name="rec">The User data record.</param>
    /// <param name="user">The User data object.</param>
    /// <param name="CurrentUser">The current user object, if one exists.</param>
    /// <returns>An updated User data object.</returns>
    private DataObjects.User GetUserApp(EFModels.EFModels.User rec, DataObjects.User user, DataObjects.User? CurrentUser = null)
    {
        //user.Property = rec.Property;

        return user;
    }

    /// <summary>
    /// This method is called to map any app-specific fields from the EF UserGroup model to the UserGroup data object.
    /// </summary>
    /// <param name="rec">The UserGroup data record.</param>
    /// <param name="userGroup">The UserGroup data object.</param>
    /// <param name="CurrentUser">The current user object, if one exists.</param>
    /// <returns>An updated UserGroup data object.</returns>
    private DataObjects.UserGroup GetUserGroupApp(EFModels.EFModels.UserGroup rec, DataObjects.UserGroup userGroup, DataObjects.User? CurrentUser = null)
    {
        //userGroup.Property = rec.Property;
        return userGroup;
    }

    /// <summary>
    /// This method is called to map any app-specific fields from the Appointment data object to the EF Appointment model.
    /// </summary>
    /// <param name="rec">The Appointment data record.</param>
    /// <param name="appointment">The Appointment data object.</param>
    /// <param name="CurrentUser">The current user object, if one exists.</param>
    /// <returns>An updated Appointment data record.</returns>
    private EFModels.EFModels.Appointment SaveAppointmentApp(EFModels.EFModels.Appointment rec, DataObjects.Appointment appointment, DataObjects.User? CurrentUser = null)
    {
        //rec.Property = appointment.Property;
        return rec;
    }

    /// <summary>
    /// This method is called to map any app-specific fields from the Department data object to the EF Department model.
    /// </summary>
    /// <param name="rec">The Department data record.</param>
    /// <param name="department">The Department data object.</param>
    /// <param name="CurrentUser">The current user object, if one exists.</param>
    /// <returns>An updated Department data record.</returns>
    private EFModels.EFModels.Department SaveDepartmentApp(EFModels.EFModels.Department rec, DataObjects.Department department, DataObjects.User? CurrentUser = null)
    {
        //rec.Property = department.Property;
        return rec;
    }

    /// <summary>
    /// This method is called to map any app-specific fields from the DepartmentGroup data object to the EF DepartmentGroup model.
    /// </summary>
    /// <param name="rec">The DepartmentGroup data record.</param>
    /// <param name="departmentGroup">The DepartmentGroup data object.</param>
    /// <param name="CurrentUser">The current user object, if one exists.</param>
    /// <returns>An updated DepartmentGroup data record.</returns>
    private EFModels.EFModels.DepartmentGroup SaveDepartmentGroupApp(EFModels.EFModels.DepartmentGroup rec, DataObjects.DepartmentGroup departmentGroup, DataObjects.User? CurrentUser = null)
    {
        //rec.Property = departmentGroup.Property;
        return rec;
    }

    /// <summary>
    /// This method is called to map any app-specific fields from the EmailTemplate data object to the EF EmailTemplate model.
    /// </summary>
    /// <param name="rec">The EmailTemplate data record.</param>
    /// <param name="emailTemplate">The EmailTemplate data object.</param>
    /// <param name="CurrentUser">The current user object, if one exists.</param>
    /// <returns>An updated EmailTemplate data record.</returns>
    private EFModels.EFModels.EmailTemplate SaveEmailTemplateApp(EFModels.EFModels.EmailTemplate rec, DataObjects.EmailTemplate emailTemplate, DataObjects.User? CurrentUser = null)
    {
        //rec.Property = emailTemplate.Property;
        return rec;
    }

    /// <summary>
    /// This method is called to map any app-specific fields from the Invoice data object to the EF Invoice model.
    /// </summary>
    /// <param name="rec">The Invoice data record.</param>
    /// <param name="invoice">The Invoice data object.</param>
    /// <param name="CurrentUser">The current user object, if one exists.</param>
    /// <returns>An updated Invoice data record.</returns>
    private EFModels.EFModels.Invoice SaveInvoiceApp(EFModels.EFModels.Invoice rec, DataObjects.Invoice invoice, DataObjects.User? CurrentUser = null)
    {
        //rec.Property = invoice.Property;
        return rec;
    }

    /// <summary>
    /// This method is called to map any app-specific fields from the Payment data object to the EF Payment model.
    /// </summary>
    /// <param name="rec">The Payment data record.</param>
    /// <param name="payment">The Payment data object.</param>
    /// <param name="CurrentUser">The current user object, if one exists.</param>
    /// <returns>An updated Payment data record.</returns>
    private EFModels.EFModels.Payment SavePaymentApp(EFModels.EFModels.Payment rec, DataObjects.Payment payment, DataObjects.User? CurrentUser = null)
    {
        //rec.Property = payment.Property;
        return rec;
    }

    /// <summary>
    /// This method is called to map any app-specific fields from the Service data object to the EF Service model.
    /// </summary>
    /// <param name="rec">The Service data record.</param>
    /// <param name="service">The Service data object.</param>
    /// <param name="CurrentUser">The current user object, if one exists.</param>
    /// <returns>An updated Service data record.</returns>
    private EFModels.EFModels.Service SaveServiceApp(EFModels.EFModels.Service rec, DataObjects.Service service, DataObjects.User? CurrentUser = null)
    {
        //rec.Property = service.Property;
        return rec;
    }

    /// <summary>
    /// This method is called to map any app-specific fields from the User data object to the EF User model.
    /// </summary>
    /// <param name="rec">The User data record.</param>
    /// <param name="user">The User data object.</param>
    /// <param name="CurrentUser">The current user object, if one exists.</param>
    /// <returns>An updated User data record.</returns>
    private EFModels.EFModels.User SaveUserApp(EFModels.EFModels.User rec, DataObjects.User user, DataObjects.User? CurrentUser = null)
    {
        //rec.Property = user.Property;

        return rec;
    }

    /// <summary>
    /// This method is called to map any app-specific fields from the UserGroup data object to the EF UserGroup model.
    /// </summary>
    /// <param name="rec">The UserGroup data record.</param>
    /// <param name="userGroup">The UserGroup data object.</param>
    /// <param name="CurrentUser">The current user object, if one exists.</param>
    /// <returns>An updated UserGroup data record.</returns>
    private EFModels.EFModels.UserGroup SaveUserGroupApp(EFModels.EFModels.UserGroup rec, DataObjects.UserGroup userGroup, DataObjects.User? CurrentUser = null)
    {
        //rec.Property = userGroup.Property;
        return rec;
    }

    /// <summary>
    /// Called by the main method to get filtered users to apply any app-specific sorting.
    /// </summary>
    /// <param name="recs">The current records.</param>
    /// <param name="SortBy">The sort field.</param>
    /// <param name="Ascending">The boolean that indicates if we are sorting ascending.</param>
    /// <returns></returns>
    private IQueryable<EFModels.EFModels.User>? SortUsersApp(IQueryable<EFModels.EFModels.User>? recs, string SortBy, bool Ascending)
    {
        if (recs != null) {

            switch (SortBy.ToUpper()) {
                //case "PROPERTY":
                //    recs = Ascending
                //        ? recs.OrderBy(x => x.PROPERTY).ThenBy(x => x.FirstName).ThenBy(x => x.LastName)
                //        : recs.OrderBy(x => x.PROPERTY).ThenBy(x => x.FirstName).ThenBy(x => x.LastName);
                //    break;
            }
        }

        return recs;
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