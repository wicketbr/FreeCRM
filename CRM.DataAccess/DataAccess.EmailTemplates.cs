namespace CRM;

public partial interface IDataAccess
{
    Task<DataObjects.BooleanResponse> DeleteEmailTemplate(Guid EmailTemplateId, DataObjects.User? CurrentUser = null, bool ForceDeleteImmediately = false);
    Task<DataObjects.EmailTemplate> GetEmailTemplate(Guid EmailTemplateId, DataObjects.User? CurrentUser = null);
    Task<List<DataObjects.EmailTemplate>> GetEmailTemplates(Guid TenantId, DataObjects.User? CurrentUser = null);
    Task<DataObjects.EmailTemplate> SaveEmailTemplate(DataObjects.EmailTemplate template, DataObjects.User? CurrentUser = null);
    DataObjects.BooleanResponse SendTemplateEmail(DataObjects.EmailTemplate template, DataObjects.User CurrentUser, object? obj = null);
    DataObjects.BooleanResponse SendTemplateEmailTest(DataObjects.EmailTemplate template, DataObjects.User CurrentUser);
}

public partial class DataAccess
{
    public async Task<DataObjects.BooleanResponse> DeleteEmailTemplate(Guid EmailTemplateId, DataObjects.User? CurrentUser = null, bool ForceDeleteImmediately = false)
    {
        var output = new DataObjects.BooleanResponse();

        var rec = await data.EmailTemplates.FirstOrDefaultAsync(x => x.EmailTemplateId == EmailTemplateId);
        if (rec == null) {
            output.Messages.Add("Error Deleting Email Template '" + EmailTemplateId.ToString() + "' - Record No Longer Exists");
            return output;
        }

        var now = DateTime.UtcNow;
        Guid tenantId = GuidValue(rec.TenantId);
        var tenantSettings = GetTenantSettings(tenantId);

        if (ForceDeleteImmediately || tenantSettings.DeletePreference == DataObjects.DeletePreference.Immediate) {
            var deleteAppRecords = await DeleteRecordsApp(rec, CurrentUser);
            if (!deleteAppRecords.Result) {
                output.Messages.AddRange(deleteAppRecords.Messages);
                return output;
            }

            try {
                // {{ModuleItemStart:Tags}}
                data.TagItems.RemoveRange(data.TagItems.Where(x => x.ItemId == EmailTemplateId));
                await data.SaveChangesAsync();
                // {{ModuleItemEnd:Tags}}

                data.EmailTemplates.Remove(rec);
                await data.SaveChangesAsync();

                output.Result = true;
            } catch (Exception ex) {
                output.Messages.Add("Error Deleting Email Template " + EmailTemplateId.ToString() + ":");
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
                output.Messages.Add("Error Deleting Email Template " + EmailTemplateId.ToString() + ":");
                output.Messages.AddRange(RecurseException(ex));
            }
        }

        if (!ForceDeleteImmediately) {
            await SignalRUpdate(new DataObjects.SignalRUpdate {
                TenantId = tenantId,
                ItemId = EmailTemplateId,
                UpdateType = DataObjects.SignalRUpdateType.EmailTemplate,
                Message = "Deleted",
                UserId = CurrentUserId(CurrentUser),
            });
        }

        return output;
    }

    public async Task<DataObjects.EmailTemplate> GetEmailTemplate(Guid EmailTemplateId, DataObjects.User? CurrentUser = null)
    {
        var output = new DataObjects.EmailTemplate();

        EmailTemplate? rec = null;

        if(AdminUser(CurrentUser)) {
            rec = await data.EmailTemplates
                .FirstOrDefaultAsync(x => x.EmailTemplateId == EmailTemplateId);
        } else {
            rec = await data.EmailTemplates
                .FirstOrDefaultAsync(x => x.EmailTemplateId == EmailTemplateId && x.Deleted != true);
        }

        if(rec != null) {
            output = new DataObjects.EmailTemplate { 
                ActionResponse = GetNewActionResponse(true),
                EmailTemplateId = rec.EmailTemplateId,
                TenantId = rec.TenantId,
                Name = rec.Name,
                Template = rec.Template,
                Enabled = rec.Enabled,
                Added = rec.Added,
                AddedBy = LastModifiedDisplayName(rec.AddedBy),
                LastModified = rec.LastModified,
                LastModifiedBy = LastModifiedDisplayName(rec.LastModifiedBy),
                Deleted = rec.Deleted,
                DeletedAt = rec.DeletedAt,
                // {{ModuleItemStart:Tags}}
                Tags = await GetTagsForItem(rec.TenantId, EmailTemplateId),
                // {{ModuleItemEnd:Tags}}
            };

            GetDataApp(rec, output, CurrentUser);
        } else {
            output.ActionResponse.Messages.Add("Email Template '" + EmailTemplateId.ToString() + "' No Longer Exists");
        }

        return output;
    }

    public async Task<List<DataObjects.EmailTemplate>> GetEmailTemplates(Guid TenantId, DataObjects.User? CurrentUser = null)
    {
        var output = new List<DataObjects.EmailTemplate>();

        List<EmailTemplate>? recs = null;

        if (AdminUser(CurrentUser)) {
            recs = await data.EmailTemplates
                .Where(x => x.TenantId == TenantId).ToListAsync();
        } else {
            recs = await data.EmailTemplates
                .Where(x => x.TenantId == TenantId && x.Deleted != true).ToListAsync();
        }

        if(recs != null && recs.Any()) {
            foreach(var rec in recs) {
                var t = new DataObjects.EmailTemplate {
                    ActionResponse = GetNewActionResponse(true),
                    EmailTemplateId = rec.EmailTemplateId,
                    TenantId = rec.TenantId,
                    Name = rec.Name,
                    Template = rec.Template,
                    Enabled = rec.Enabled,
                    Added = rec.Added,
                    AddedBy = LastModifiedDisplayName(rec.AddedBy),
                    LastModified = rec.LastModified,
                    LastModifiedBy = LastModifiedDisplayName(rec.LastModifiedBy),
                    Deleted = rec.Deleted,
                    DeletedAt = rec.DeletedAt,
                    // {{ModuleItemStart:Tags}}
                    Tags = await GetTagsForItem(rec.TenantId, rec.EmailTemplateId),
                    // {{ModuleItemEnd:Tags}}
                };

                GetDataApp(rec, t, CurrentUser);

                output.Add(t);
            }
        }

        return output;
    }

    public async Task<DataObjects.EmailTemplate> SaveEmailTemplate(DataObjects.EmailTemplate template, DataObjects.User? CurrentUser = null)
    {
        var output = template;
        output.ActionResponse = GetNewActionResponse();

        bool newRecord = false;
        DateTime now = DateTime.UtcNow;

        var rec = await data.EmailTemplates.FirstOrDefaultAsync(x => x.EmailTemplateId == output.EmailTemplateId);

        if(rec != null && rec.Deleted) {
            if(AdminUser(CurrentUser)) {
                // Ok to edit this record that is marked as deleted.
            } else {
                output.ActionResponse.Messages.Add("Email Template '" + output.EmailTemplateId.ToString() + "' No Longer Exists");
                return output;
            }
        }

        if(rec == null) {
            if(output.EmailTemplateId == Guid.Empty) {
                newRecord = true;
                output.EmailTemplateId = Guid.NewGuid();

                rec = new EmailTemplate { 
                    EmailTemplateId = output.EmailTemplateId,
                    TenantId = output.TenantId,
                    Deleted = false,
                    Added = now,
                    AddedBy = CurrentUserIdString(CurrentUser),
                };
            } else {
                output.ActionResponse.Messages.Add("Email Template '" + output.EmailTemplateId.ToString() + "' No Longer Exists");
                return output;
            }
        }

        output.Name = MaxStringLength(output.Name, 300);
        rec.Name = output.Name;
        rec.Template = output.Template;
        rec.Enabled = output.Enabled;
        rec.LastModified = now;
        rec.LastModifiedBy = CurrentUserIdString(CurrentUser);

        if(AdminUser(CurrentUser)) {
            rec.Deleted = output.Deleted;

            if (!output.Deleted) {
                rec.DeletedAt = null;
            }
        }

        SaveDataApp(rec, output, CurrentUser);

        try {
            if (newRecord) {
                await data.EmailTemplates.AddAsync(rec);
            }
            await data.SaveChangesAsync();

            // {{ModuleItemStart:Tags}}
            await SaveItemTags(output.TenantId, output.EmailTemplateId, output.Tags);
            // {{ModuleItemEnd:Tags}}

            output.ActionResponse.Result = true;

            await SignalRUpdate(new DataObjects.SignalRUpdate { 
                TenantId = output.TenantId,
                ItemId = output.EmailTemplateId,
                UpdateType = DataObjects.SignalRUpdateType.EmailTemplate,
                Message = "Saved",
                UserId = CurrentUserId(CurrentUser),
            });
        }catch (Exception ex) {
            output.ActionResponse.Messages.Add("Error Saving Email Template " + output.EmailTemplateId.ToString() + ":");
            output.ActionResponse.Messages.AddRange(RecurseException(ex));
        }

        return output;
    }

    public DataObjects.BooleanResponse SendTemplateEmail(DataObjects.EmailTemplate template, DataObjects.User CurrentUser, object? obj = null)
    {
        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();

        var _details = DeserializeObject<DataObjects.EmailTemplateDetails>(template.Template);
        if(_details == null) {
            output.Messages.Add("Unable to Deserialize Template Details");
        } else {
            string subject = ReplaceTagsInText(_details.Subject, CurrentUser, obj);
            string body = ReplaceTagsInText(_details.Body, CurrentUser, obj);

            var email = UpdateEmailReplyAddress(template.TenantId, new DataObjects.EmailMessage {
                From = !String.IsNullOrWhiteSpace(_details.From) ? _details.From : DefaultReplyToAddress,
                To = new List<string> { StringValue(CurrentUser.Email) },
                Subject = subject,
                Body = body,
            });

            output = SendEmail(email);
        }

        return output;
    }

    public DataObjects.BooleanResponse SendTemplateEmailTest(DataObjects.EmailTemplate template, DataObjects.User CurrentUser)
    {
        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();

        DataObjects.User user = new DataObjects.User { 
            FirstName = "John",
            LastName = "Doe",
            EmployeeId = "12345",
            Email = "john.doe@local",
        };

        var today = DateTime.Now;


        // {{ModuleItemStart:Appointments}}
        DataObjects.Appointment appt = new DataObjects.Appointment { 
            AppointmentId = Guid.Empty,
            Title = "Test Appointment",
            Note = "Test Note",
            Start = Convert.ToDateTime(today.ToShortDateString() + " 12:00 pm").ToUniversalTime(),
            End = Convert.ToDateTime(today.ToShortDateString() + " 5:00 pm").ToUniversalTime(),
        };
        // {{ModuleItemEnd:Appointments}}

        DataObjects.Service service = new DataObjects.Service { 
            ServiceId = Guid.Empty,
            Code = "TEST-CODE",
            Description = "Test Service",
            Rate = 50,
        };

        var _details = DeserializeObject<DataObjects.EmailTemplateDetails>(template.Template);
        if(_details == null) {
            output.Messages.Add("Unable to Deserialize Template Details");
        } else {
            string subject = ReplaceTagsInText(_details.Subject, user, appt);
            subject = ReplaceTagsInText(subject, null, service);

            string body = ReplaceTagsInText(_details.Body, user, appt);
            body = ReplaceTagsInText(body, null, service);

            var email = UpdateEmailReplyAddress(template.TenantId, new DataObjects.EmailMessage { 
                From = !String.IsNullOrWhiteSpace(_details.From) ? _details.From : DefaultReplyToAddress,
                To = new List<string> { StringValue(CurrentUser.Email) },
                Subject = subject,
                Body = body,
            });

            output = SendEmail(email);
        }

        return output;
    }
}