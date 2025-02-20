using Microsoft.AspNetCore.Mvc;

namespace CRM.Server.Controllers
{
    [ApiController]
    public class SetupController : ControllerBase
    {
        private IConfiguration _config;
        private IDataAccess da;
        private IHostApplicationLifetime _hostLifetime;

        public SetupController(IDataAccess daInjection, IConfiguration config, IHostApplicationLifetime hostLifetime)
        {
            _config = config;
            da = daInjection;
            _hostLifetime = hostLifetime;
        }

        [HttpPost]
        [Route("~/api/Setup/SaveConnectionString")]
        public ActionResult<DataObjects.BooleanResponse> SaveConnectionString(DataObjects.ConnectionStringConfig csConfig)
        {
            DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();

            //Try and save the connection string data and redirect to the Index page.
            string cs = String.Empty;

            if (csConfig != null) {
                if (!String.IsNullOrWhiteSpace(csConfig.DatabaseType)) {
                    switch (csConfig.DatabaseType.ToUpper()) {
                        case "INMEMORY":
                            cs = "InMemory";
                            break;

                        case "MYSQL":
                            if (!String.IsNullOrWhiteSpace(csConfig.MySQL_Server) &&
                                !String.IsNullOrWhiteSpace(csConfig.MySQL_Database) &&
                                !String.IsNullOrWhiteSpace(csConfig.MySQL_User) &&
                                !String.IsNullOrWhiteSpace(csConfig.MySQL_Password)
                            ) {
                                cs = "Server=" + csConfig.MySQL_Server +
                                    ";Database=" + csConfig.MySQL_Database +
                                    ";User=" + csConfig.MySQL_User +
                                    ";Password=" + csConfig.MySQL_Password + ";";
                            }

                            break;

                        case "POSTGRESQL":
                            if (!String.IsNullOrWhiteSpace(csConfig.PostgreSql_Host) &&
                                !String.IsNullOrWhiteSpace(csConfig.PostgreSql_Database) &&
                                !String.IsNullOrWhiteSpace(csConfig.PostgreSql_Username) &&
                                !String.IsNullOrWhiteSpace(csConfig.PostgreSql_Password)
                            ) {
                                cs = "Host=" + csConfig.PostgreSql_Host +
                                    ";Database=" + csConfig.PostgreSql_Database +
                                    ";Username=" + csConfig.PostgreSql_Username +
                                    ";Password=" + csConfig.PostgreSql_Password + ";";
                            }

                            break;

                        case "SQLSERVER":
                            if (!String.IsNullOrWhiteSpace(csConfig.SqlServer_Server) &&
                                !String.IsNullOrWhiteSpace(csConfig.SqlServer_Database)) {
                                cs = "Data Source=" + csConfig.SqlServer_Server +
                                    ";Initial Catalog=" + csConfig.SqlServer_Database + ";";
                                if (csConfig.SqlServer_IntegratedSecurity) {
                                    cs += "Integrated Security=true;";
                                }
                                if (csConfig.SqlServer_PersistSecurityInfo) {
                                    cs += "Persist Security Info=True;";
                                }
                                if (csConfig.SqlServer_TrustServerCertificate) {
                                    cs += "TrustServerCertificate=True;";
                                }
                                if (!String.IsNullOrWhiteSpace(csConfig.SqlServer_UserId)) {
                                    cs += "User ID=" + csConfig.SqlServer_UserId + ";";
                                }
                                if (!String.IsNullOrWhiteSpace(csConfig.SqlServer_Password)) {
                                    cs += "Password=" + csConfig.SqlServer_Password + ";";
                                }
                                cs += "MultipleActiveResultSets=True;";
                            }
                            break;

                        case "SQLITE":
                            if (!String.IsNullOrWhiteSpace(csConfig.SQLiteDatabase)) {
                                cs = "Data Source=" + csConfig.SQLiteDatabase + ";";
                            }
                            break;
                    }
                } else {
                    output.Messages.Add("Missing Required Database Type");
                }

                if (!String.IsNullOrEmpty(cs)) {
                    var appSettingsPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "appsettings.json");
                    var json = da.StringValue(System.IO.File.ReadAllText(appSettingsPath));

                    var jsonObject = System.Text.Json.Nodes.JsonNode.Parse(json, new System.Text.Json.Nodes.JsonNodeOptions { PropertyNameCaseInsensitive = true });
                    if (jsonObject != null) {
                        System.Text.Json.Nodes.JsonObject obj = jsonObject.AsObject();

                        var dbType = obj["DatabaseType"];
                        if (dbType != null) {
                            dbType.ReplaceWith<string>(da.StringValue(csConfig.DatabaseType));
                        }

                        var csAppData = obj["ConnectionStrings"]?["AppData"];
                        if (csAppData != null) {
                            csAppData.ReplaceWith<string>(cs);
                        }

                        json = System.Text.Json.JsonSerializer.Serialize(jsonObject, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                    }

                    //var jsonSettings = new JsonSerializerSettings();
                    //jsonSettings.Converters.Add(new ExpandoObjectConverter());
                    //jsonSettings.Converters.Add(new StringEnumConverter());
                    //var config = JsonConvert.DeserializeObject<ExpandoObject>(json, jsonSettings);
                    //if (config != null) {
                    //    dynamic o = (dynamic)config;
                    //    o.ConnectionStrings.AppData = cs;
                    //    o.DatabaseType = csConfig.DatabaseType;
                    //}
                    //json = JsonConvert.SerializeObject(config, Formatting.Indented, jsonSettings);

                    System.IO.File.WriteAllText(appSettingsPath, json);

                    _hostLifetime.StopApplication();

                    output.Result = true;
                } else {
                    output.Messages.Add("Unable to build connection string. Please make sure all required fields are completed.");
                }
            } else {
                output.Messages.Add("Missing Config Object");
            }

            return Ok(output);
        }
    }
}