using CRM;

public partial class GraphClient
{
    #region Internals
    private Microsoft.Graph.GraphServiceClient? _client;
    private string _clientId;
    private string _tenantId;
    private string _clientSecret;
    private string? _error;
    private bool _status;

    public GraphClient(string clientId, string tenantId, string clientSecret)
    {
        _clientId = clientId;
        _tenantId = tenantId;
        _clientSecret = clientSecret;
        _status = false;

        try {
            var scopes = new string[] { "https://graph.microsoft.com/.default" };

            var options = new Azure.Identity.TokenCredentialOptions { AuthorityHost = Azure.Identity.AzureAuthorityHosts.AzurePublicCloud };
            var authCodeCredential = new Azure.Identity.ClientSecretCredential(tenantId, clientId, clientSecret, options);
            _client = new Microsoft.Graph.GraphServiceClient(authCodeCredential);
            _status = true;
        } catch (Exception ex) {
            _error = ex.Message;
        }
    }

    public Microsoft.Graph.GraphServiceClient? Client
    {
        get {
            return _client;
        }
    }

    public string? Error
    {
        get {
            return _error;
        }
    }

    public bool Status
    {
        get {
            return _status;
        }
    }
    #endregion

    public async Task<DataObjects.BooleanResponse> SendEmail(DataObjects.EmailMessage message)
    {
        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();

        if (_client != null) {
            var user = _client.Users[message.From];

            if (user != null) {
                List<Microsoft.Graph.Models.Recipient>? to = null;
                List<Microsoft.Graph.Models.Recipient>? cc = null;
                List<Microsoft.Graph.Models.Recipient>? bcc = null;

                if (message.To.Any()) {
                    to = new List<Microsoft.Graph.Models.Recipient>();
                    foreach (var emailAddress in message.To) {
                        to.Add(new Microsoft.Graph.Models.Recipient {
                            EmailAddress = new Microsoft.Graph.Models.EmailAddress {
                                Address = emailAddress,
                            }
                        });
                    }
                }

                if (message.Cc.Any()) {
                    cc = new List<Microsoft.Graph.Models.Recipient>();
                    foreach (var emailAddress in message.Cc) {
                        cc.Add(new Microsoft.Graph.Models.Recipient {
                            EmailAddress = new Microsoft.Graph.Models.EmailAddress {
                                Address = emailAddress,
                            }
                        });
                    }
                }

                if (message.Bcc.Any()) {
                    bcc = new List<Microsoft.Graph.Models.Recipient>();
                    foreach (var emailAddress in message.Bcc) {
                        bcc.Add(new Microsoft.Graph.Models.Recipient {
                            EmailAddress = new Microsoft.Graph.Models.EmailAddress {
                                Address = emailAddress,
                            }
                        });
                    }
                }

                if ((to == null || !to.Any()) && (cc == null || !cc.Any()) && (bcc == null || !bcc.Any())) {
                    output.Messages.Add("At least one recipient is required to send an email.");
                    return output;
                }

                List<Microsoft.Graph.Models.Attachment>? attachments = null;

                if (message.Files != null && message.Files.Any()) {
                    attachments = new List<Microsoft.Graph.Models.Attachment>();

                    foreach (var file in message.Files) {
                        attachments.Add(new Microsoft.Graph.Models.FileAttachment {
                            OdataType = "#microsoft.graph.fileAttachment",
                            ContentBytes = file.Value,
                            ContentId = file.FileId.ToString(),
                            Name = file.FileName,
                        });
                    }
                }

                try {
                    var requestBody = new Microsoft.Graph.Users.Item.SendMail.SendMailPostRequestBody {
                        Message = new Microsoft.Graph.Models.Message {
                            Subject = message.Subject,
                            Body = new Microsoft.Graph.Models.ItemBody {
                                ContentType = Microsoft.Graph.Models.BodyType.Html,
                                Content = message.Body,
                            },
                        },
                        SaveToSentItems = true,
                    };

                    if (to != null && to.Any()) {
                        requestBody.Message.ToRecipients = to;
                    }

                    if (cc != null && cc.Any()) {
                        requestBody.Message.CcRecipients = cc;
                    }

                    if (bcc != null && bcc.Any()) {
                        requestBody.Message.BccRecipients = bcc;
                    }

                    if (attachments != null && attachments.Any()) {
                        requestBody.Message.Attachments = attachments;
                    }

                    await user.SendMail.PostAsync(requestBody);
                    output.Result = true;
                } catch (Exception ex) {
                    output.Messages.Add("Error Sending Graph Email:");
                    output.Messages.Add(ex.Message);
                    if (ex.InnerException != null) {
                        output.Messages.Add(ex.InnerException.Message);
                    }
                }

            } else {
                output.Messages.Add("Unable to access Graph user '" + message.From + "'");
            }
        } else {
            output.Messages.Add("Unable to Create Graph Client");
        }

        return output;
    }
}