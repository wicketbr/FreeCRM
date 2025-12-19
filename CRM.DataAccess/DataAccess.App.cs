namespace CRM;

// Use this file as a place to put any application-specific data access methods.

public partial interface IDataAccess
{
    Task<DataObjects.BooleanResponse> ProcessBackgroundTasksApp(Guid TenantId, long Iteration);
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
    /// Called from various delete methods to delete any app-specific records before continuing with the delete process.
    /// </summary>
    /// <param name="Rec">The EF record object.</param>
    /// <param name="CurrentUser">The User object for the current user, if loaded.</param>
    /// <returns>A BooleanResponse object.</returns>
    private async Task<DataObjects.BooleanResponse> DeleteRecordsApp(object Rec, DataObjects.User? CurrentUser = null)
    {
        await Task.Delay(0); // Simulate a delay since this method has to be async. This can be removed once you implement your await logic.

        var output = new DataObjects.BooleanResponse();

        try {
            // {{ModuleItemStart:Appointments}}
            if (Rec is EFModels.EFModels.Appointment) {
                var rec = Rec as EFModels.EFModels.Appointment;
                if (rec != null) {
                    // Remove any related records.
                }
            }

            if (Rec is EFModels.EFModels.AppointmentNote) {
                var rec = Rec as EFModels.EFModels.AppointmentNote;
                if (rec != null) {
                    // Remove any related records.
                }
            }

            if (Rec is EFModels.EFModels.AppointmentService) {
                var rec = Rec as EFModels.EFModels.AppointmentService;
                if (rec != null) {
                    // Remove any related records.
                }
            }

            if (Rec is EFModels.EFModels.AppointmentUser) {
                var rec = Rec as EFModels.EFModels.AppointmentUser;
                if (rec != null) {
                    // Remove any related records.
                }
            }
            // {{ModuleItemEnd:Appointments}}

            if (Rec is EFModels.EFModels.Department) {
                var rec = Rec as EFModels.EFModels.Department;
                if (rec != null) {
                    // Remove any related records.
                }
            }

            if (Rec is EFModels.EFModels.DepartmentGroup) {
                var rec = Rec as EFModels.EFModels.DepartmentGroup;
                if (rec != null) {
                    // Remove any related records.
                }
            }

            // {{ModuleItemStart:EmailTemplates}}
            if (Rec is EFModels.EFModels.EmailTemplate) {
                var rec = Rec as EFModels.EFModels.EmailTemplate;
                if (rec != null) {
                    // Remove any related records.
                }
            }
            // {{ModuleItemEnd:EmailTemplates}}

            // {{ModuleItemStart:Invoices}}
            if (Rec is EFModels.EFModels.Invoice) {
                var rec = Rec as EFModels.EFModels.Invoice;
                if (rec != null) {
                    // Remove any related records.
                }
            }
            // {{ModuleItemEnd:Invoices}}

            // {{ModuleItemStart:Locations}}
            if (Rec is EFModels.EFModels.Location) {
                var rec = Rec as EFModels.EFModels.Location;
                if (rec != null) {
                    // Remove any related records.
                }
            }
            // {{ModuleItemEnd:Locations}}

            // {{ModuleItemStart:Payments}}
            if (Rec is EFModels.EFModels.Payment) {
                var rec = Rec as EFModels.EFModels.Payment;
                if (rec != null) {
                    // Remove any related records.
                }
            }
            // {{ModuleItemEnd:Payments}}

            // {{ModuleItemStart:Services}}
            if (Rec is EFModels.EFModels.Service) {
                var rec = Rec as EFModels.EFModels.Service;
                if (rec != null) {
                    // Remove any related records.
                }
            }
            // {{ModuleItemEnd:Services}}

            if (Rec is EFModels.EFModels.Tenant) {
                var rec = Rec as EFModels.EFModels.Tenant;
                if (rec != null) {
                    // Implement your app-specific tenant deletion logic here to remove records from tables that are specific to your app.
                    // Example:
                    // data.MyTable.RemoveRange(data.MyTable.Where(x => x.TenantId == TenantId));
                    // await data.SaveChangesAsync();
                }
            }

            if (Rec is EFModels.EFModels.User) {
                var rec = Rec as EFModels.EFModels.User;
                if (rec != null) {
                    // Remove any related records.
                }
            }

            if (Rec is EFModels.EFModels.UserGroup) {
                var rec = Rec as EFModels.EFModels.UserGroup;
                if (rec != null) {
                    // Remove any related records.
                }
            }
        } catch (Exception ex) {
            output.Messages.Add("An Error Occurred in DeleteUserApp");
            output.Messages.AddRange(RecurseException(ex));
        }

        output.Result = output.Messages.Count == 0;

        return output;
    }

    /// <summary>
    /// Called by the GetApplicationSettings method to load any app-specific settings into the ApplicationSettings object.
    /// </summary>
    /// <param name="settings">The ApplicationSettings object.</param>
    /// <returns>The object with any updates for your app.</returns>
    private DataObjects.ApplicationSettings GetApplicationSettingsApp(DataObjects.ApplicationSettings settings)
    {
        // Add any app-specific settings here.

        return settings;
    }

    /// <summary>
    /// Called by the AppSettings property get load any app-specific settings into the ApplicationSettingsUpdate object.
    /// </summary>
    /// <param name="settings">The current ApplicationSettingsUpdate object.</param>
    /// <returns>The object with any updates from your app.</returns>
    private DataObjects.ApplicationSettingsUpdate GetApplicationSettingsUpdateApp(DataObjects.ApplicationSettingsUpdate settings)
    {
        // Any any app-specific settings for the ApplicationSettingsUpdate object here.
        return settings;
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
    /// This is called by various Get methods to map any app-specific fields from the EF model to the data object.
    /// </summary>
    /// <param name="Rec">The EF record object.</param>
    /// <param name="DataObject">The DataObjects object being updated.</param>
    /// <param name="CurrentUser">The current user object, if one exists.</param>
    private void GetDataApp(object Rec, object DataObject, DataObjects.User? CurrentUser = null)
    {
        try {
            // {{ModuleItemStart:Appointments}}
            if (Rec is EFModels.EFModels.Appointment && DataObject is DataObjects.Appointment) {
                var rec = Rec as EFModels.EFModels.Appointment;
                var appointment = DataObject as DataObjects.Appointment;

                if (rec != null && appointment != null) {
                    //appointment.Property = rec.Property;
                }

                return;
            }

            if (Rec is EFModels.EFModels.AppointmentNote && DataObject is DataObjects.AppointmentNote) {
                var rec = Rec as EFModels.EFModels.AppointmentNote;
                var appointmentNote = DataObject as DataObjects.AppointmentNote;

                if (rec != null && appointmentNote != null) {
                    //appointmentNote.Property = rec.Property;
                }

                return;
            }

            if (Rec is EFModels.EFModels.AppointmentService && DataObject is DataObjects.AppointmentService) {
                var rec = Rec as EFModels.EFModels.AppointmentService;
                var appointmentService = DataObject as DataObjects.AppointmentService;

                if (rec != null && appointmentService != null) {
                    //appointmentService.Property = rec.Property;
                }

                return;
            }

            if (Rec is EFModels.EFModels.AppointmentUser && DataObject is DataObjects.AppointmentUser) {
                var rec = Rec as EFModels.EFModels.AppointmentUser;
                var appointmentUser = DataObject as DataObjects.AppointmentUser;

                if (rec != null && appointmentUser != null) {
                    //appointmentUser.Property = rec.Property;
                }

                return;
            }
            // {{ModuleItemEnd:Appointments}}

            if (Rec is EFModels.EFModels.Department && DataObject is DataObjects.Department) {
                var rec = Rec as EFModels.EFModels.Department;
                var department = DataObject as DataObjects.Department;

                if (rec != null && department != null) {
                    //department.Property = rec.Property;
                }

                return;
            }

            if (Rec is EFModels.EFModels.DepartmentGroup && DataObject is DataObjects.DepartmentGroup) {
                var rec = Rec as EFModels.EFModels.DepartmentGroup;
                var departmentGroup = DataObject as DataObjects.DepartmentGroup;

                if (rec != null && departmentGroup != null) {
                    //departmentGroup.Property = rec.Property;
                }

                return;
            }

            // {{ModuleItemStart:EmailTemplates}}
            if (Rec is EFModels.EFModels.EmailTemplate && DataObject is DataObjects.EmailTemplate) {
                var rec = Rec as EFModels.EFModels.EmailTemplate;
                var emailTemplate = DataObject as DataObjects.EmailTemplate;

                if (rec != null && emailTemplate != null) {
                    //emailTemplate.Property = rec.Property;
                }

                return;
            }
            // {{ModuleItemEnd:EmailTemplates}}

            // {{ModuleItemStart:Invoices}}
            if (Rec is EFModels.EFModels.Invoice && DataObject is DataObjects.Invoice) {
                var rec = Rec as EFModels.EFModels.Invoice;
                var invoice = DataObject as DataObjects.Invoice;

                if (rec != null && invoice != null) {
                    //invoice.Property = rec.Property;
                }

                return;
            }
            // {{ModuleItemEnd:Invoices}}

            // {{ModuleItemStart:Locations}}
            if (Rec is EFModels.EFModels.Location && DataObject is DataObjects.Location) {
                var rec = Rec as EFModels.EFModels.Location;
                var location = DataObject as DataObjects.Location;

                if (rec != null && location != null) {
                    //location.Property = rec.Property;
                }

                return;
            }
            // {{ModuleItemEnd:Locations}}

            // {{ModuleItemStart:Payments}}
            if (Rec is EFModels.EFModels.Payment && DataObject is DataObjects.Payment) {
                var rec = Rec as EFModels.EFModels.Payment;
                var payment = DataObject as DataObjects.Payment;

                if (rec != null && payment != null) {
                    //payment.Property = rec.Property;
                }

                return;
            }
            // {{ModuleItemEnd:Payments}}

            // {{ModuleItemStart:Services}}
            if (Rec is EFModels.EFModels.Service && DataObject is DataObjects.Service) {
                var rec = Rec as EFModels.EFModels.Service;
                var service = DataObject as DataObjects.Service;

                if (rec != null && service != null) {
                    //service.Property = rec.Property;
                }

                return;
            }
            // {{ModuleItemEnd:Services}}

            if (Rec is EFModels.EFModels.User && DataObject is DataObjects.ActiveUser) {
                var rec = Rec as EFModels.EFModels.User;
                var activeUser = DataObject as DataObjects.ActiveUser;

                if (rec != null && activeUser != null) {
                    //activeUser.Property = rec.Property;
                }

                return;
            }

            if (Rec is EFModels.EFModels.User && DataObject is DataObjects.User) {
                var rec = Rec as EFModels.EFModels.User;
                var user = DataObject as DataObjects.User;

                if (rec != null && user != null) {
                    //user.Property = rec.Property;
                }

                return;
            }

            if (Rec is EFModels.EFModels.User && DataObject is DataObjects.UserAccount) {
                var rec = Rec as EFModels.EFModels.User;
                var userAccount = DataObject as DataObjects.UserAccount;

                if (rec != null && userAccount != null) {
                    //userAccount.Property = rec.Property;
                }

                return;
            }

            if (Rec is EFModels.EFModels.User && DataObject is DataObjects.UserListing) {
                var rec = Rec as EFModels.EFModels.User;
                var userListing = DataObject as DataObjects.UserListing;

                if (rec != null && userListing != null) {
                    //userListing.Property = rec.Property;
                }

                return;
            }

            if (Rec is EFModels.EFModels.UserGroup && DataObject is DataObjects.UserGroup) {
                var rec = Rec as EFModels.EFModels.UserGroup;
                var userGroup = DataObject as DataObjects.UserGroup;

                if (rec != null && userGroup != null) {
                    //userGroup.Property = rec.Property;
                }

                return;
            }
        } catch { }
    }

    /// <summary>
    /// Called by the background processor to process any app-specific background tasks.
    /// </summary>
    /// <param name="TenantId">The tenant id being processed.</param>
    /// <returns>A BooleanResponse object.</returns>
    public async Task<DataObjects.BooleanResponse> ProcessBackgroundTasksApp(Guid TenantId, long Iteration)
    {
        var output = new DataObjects.BooleanResponse();

        // Process any background tasks specific to your app here.
        // Return output.Result = true if all tasks were processed successfully.
        // Otherwise, add any error messages to output.Messages and set output.Result = false.
        output.Result = true;

        // You can use the iterations to control various intervals. For example, if you have the ProcessingIntervalSeconds set to 10 seconds
        // in the appsettings.json file, you could make a task run every minute like this:
        //   if (Iteration % 6 == 0) {
        //      your task code here.
        //   }

        // You could also use the settings to store info in the database about the last time a process ran.
        //   var lastRun = GetSetting<DateTime>("MyCustomProcessLastRunDate", DataObjects.SettingType.DateTime);
        //   if (lastRun == default(DateTime) || lastRun < DateTime.UtcNow.AddMinutes(-10)) {
        //       // Run your code
        //       SaveSetting("MyCustomProcessLastRunDate", DataObjects.SettingType.DateTime, DateTime.UtcNow);
        //   }

        return output;
    }

    /// <summary>
    /// Called by the main SaveApplicationSettings method to save any app-specific settings from the ApplicationSettings object.
    /// </summary>
    /// <param name="settings">The ApplicationSettings object.</param>
    /// <param name="CurrentUser">The User object for the current user.</param>
    /// <returns>The updated ApplicationSettings object.</returns>
    private async Task<DataObjects.ApplicationSettings> SaveApplicationSettingsApp(DataObjects.ApplicationSettings settings, DataObjects.User CurrentUser)
    {
        await Task.Delay(0); // Simulate a delay since this method has to be async. This can be removed once you implement your await logic.

        // Add any app-specific settings here.
        return settings;
    }

    /// <summary>
    /// This is called by various Save methods to map any app-specific fields from the data object to the EF model object.
    /// </summary>
    /// <param name="Rec">The EF record object being updated.</param>
    /// <param name="DataObject">The DataObjects object.</param>
    /// <param name="CurrentUser">The current user object, if one exists.</param>
    private void SaveDataApp(object Rec, object DataObject, DataObjects.User? CurrentUser = null)
    {
        try {
            // {{ModuleItemStart:Appointments}}
            if (Rec is EFModels.EFModels.Appointment && DataObject is DataObjects.Appointment) {
                var rec = Rec as EFModels.EFModels.Appointment;
                var appointment = DataObject as DataObjects.Appointment;

                if (rec != null && appointment != null) {
                    //rec.Property = appointment.Property;
                }

                return;
            }

            if (Rec is EFModels.EFModels.AppointmentNote && DataObject is DataObjects.AppointmentNote) {
                var rec = Rec as EFModels.EFModels.AppointmentNote;
                var appointmentNote = DataObject as DataObjects.AppointmentNote;

                if (rec != null && appointmentNote != null) {
                    //rec.Property = appointmentNote.Property;
                }

                return;
            }

            if (Rec is EFModels.EFModels.AppointmentService && DataObject is DataObjects.AppointmentService) {
                var rec = Rec as EFModels.EFModels.AppointmentService;
                var svc = DataObject as DataObjects.AppointmentService;

                if (rec != null && svc != null) {
                    //rec.Property = svc.Property;
                }

                return;
            }

            if (Rec is EFModels.EFModels.AppointmentUser && DataObject is DataObjects.User) {
                var rec = Rec as EFModels.EFModels.AppointmentUser;
                var user = DataObject as DataObjects.User;

                if (rec != null && user != null) {
                    //rec.Property = user.Property;
                }

                return;
            }
            // {{ModuleItemEnd:Appointments}}

            if (Rec is EFModels.EFModels.Department && DataObject is DataObjects.Department) {
                var rec = Rec as EFModels.EFModels.Department;
                var department = DataObject as DataObjects.Department;

                if (rec != null && department != null) {
                    //rec.Property = department.Property;
                }

                return;
            }

            if (Rec is EFModels.EFModels.DepartmentGroup && DataObject is DataObjects.DepartmentGroup) {
                var rec = Rec as EFModels.EFModels.DepartmentGroup;
                var departmentGroup = DataObject as DataObjects.DepartmentGroup;

                if (rec != null && departmentGroup != null) {
                    //rec.Property = departmentGroup.Property;
                }

                return;
            }

            // {{ModuleItemStart:EmailTemplates}}
            if (Rec is EFModels.EFModels.EmailTemplate && DataObject is DataObjects.EmailTemplate) {
                var rec = Rec as EFModels.EFModels.EmailTemplate;
                var emailTemplate = DataObject as DataObjects.EmailTemplate;

                if (rec != null && emailTemplate != null) {
                    //rec.Property = emailTemplate.Property;
                }

                return;
            }
            // {{ModuleItemEnd:EmailTemplates}}

            // {{ModuleItemStart:Invoices}}
            if (Rec is EFModels.EFModels.Invoice && DataObject is DataObjects.Invoice) {
                var rec = Rec as EFModels.EFModels.Invoice;
                var invoice = DataObject as DataObjects.Invoice;

                if (rec != null && invoice != null) {
                    //rec.Property = invoice.Property;
                }

                return;
            }
            // {{ModuleItemEnd:Invoices}}

            // {{ModuleItemStart:Locations}}
            if (Rec is EFModels.EFModels.Location && DataObject is DataObjects.Location) {
                var rec = Rec as EFModels.EFModels.Location;
                var location = DataObject as DataObjects.Location;
                
                if (rec != null && location != null) {
                    //rec.Property = location.Property;
                }
                
                return;
            }
            // {{ModuleItemEnd:Locations}}

            // {{ModuleItemStart:Payments}}
            if (Rec is EFModels.EFModels.Payment && DataObject is DataObjects.Payment) {
                var rec = Rec as EFModels.EFModels.Payment;
                var payment = DataObject as DataObjects.Payment;

                if (rec != null && payment != null) {
                    //rec.Property = payment.Property;
                }

                return;
            }
            // {{ModuleItemEnd:Payments}}

            // {{ModuleItemStart:Services}}
            if (Rec is EFModels.EFModels.Service && DataObject is DataObjects.Service) {
                var rec = Rec as EFModels.EFModels.Service;
                var service = DataObject as DataObjects.Service;

                if (rec != null && service != null) {
                    //rec.Property = service.Property;
                }

                return;
            }
            // {{ModuleItemEnd:Services}}

            if (Rec is EFModels.EFModels.User && DataObject is DataObjects.User) {
                var rec = Rec as EFModels.EFModels.User;
                var user = DataObject as DataObjects.User;

                if (rec != null && user != null) {
                    //rec.Property = user.Property;
                }

                return;
            }

            if (Rec is EFModels.EFModels.UserGroup && DataObject is DataObjects.UserGroup) {
                var rec = Rec as EFModels.EFModels.UserGroup;
                var userGroup = DataObject as DataObjects.UserGroup;

                if (rec != null && userGroup != null) {
                    //rec.Property = userGroup.Property;
                }

                return;
            }
        } catch { }
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

                default:
                    break;
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