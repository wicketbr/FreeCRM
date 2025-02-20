namespace CRM;

public partial interface IDataAccess
{
    bool DatabaseOpen { get; }
    DataObjects.ConnectionStringConfig GetConnectionStringConfig();
}

public partial class DataAccess
{
    private DataObjects.BooleanResponse DatabaseApplyLatestMigrations()
    {
        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();
        output.Messages = new List<string>();

        var appliedMigrations = DatabaseGetAppliedMigrations();

        var migrations = DatabaseGetMigrations();
        if (migrations.Any()) {
            foreach (var migration in migrations) {
                if (!appliedMigrations.Contains(migration.MigrationId)) {
                    if (migration.Migration.Any()) {
                        foreach (var step in migration.Migration) {
                            try {
                                data.Database.ExecuteSqlRaw(step);
                            } catch (Exception ex) {
                                output.Messages.Add("Error Executing Migration " + migration.MigrationId.ToString());
                                output.Messages.AddRange(RecurseException(ex));
                            }
                        }
                    }
                }
            }
        }

        output.Result = output.Messages.Count() == 0;
        return output;
    }

    private List<string> DatabaseGetAppliedMigrations()
    {
        List<string> output = new List<string>();

        try {
            string query = "SELECT MigrationId FROM __EFMigrationsHistory";

            if (_databaseType.ToLower() == "postgresql") {
                query =
                    """
					SELECT "MigrationId"
					FROM public."__EFMigrationsHistory";
					""";
            }

            var recs = data.Database.SqlQueryRaw<string>(query);
            if (recs != null) {
                foreach (var rec in recs) {
                    if (!String.IsNullOrEmpty(rec)) {
                        output.Add(rec);
                    }
                }
            }
        } catch (Exception ex) {
            if (ex != null) { }
        }

        return output;
    }

    private List<DataObjects.DataMigration> DatabaseGetMigrations()
    {
        List<DataObjects.DataMigration> output = new List<DataObjects.DataMigration>();

        var migrations = new DataMigrations();

        switch (_databaseType.ToLower()) {
            case "inmemory":
                break;

            case "mysql":
                output = migrations.GetMigrationsMySQL();
                break;

            case "postgresql":
                output = migrations.GetMigrationsPostgreSQL();
                break;

            case "sqlite":
                output = migrations.GetMigrationsSQLite();
                break;

            case "sqlserver":
                output = migrations.GetMigrationsSqlServer();
                break;
        }

        return output;
    }

    public bool DatabaseOpen {
        get {
            return _open;
        }
    }

    public DataObjects.ConnectionStringConfig GetConnectionStringConfig()
    {
        var output = new DataObjects.ConnectionStringConfig();
        output.ActionResponse = GetNewActionResponse();

        string connectionString = String.Empty;
        try {
            var appSettingsPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "appsettings.json");
            var json = System.IO.File.ReadAllText(appSettingsPath);

            var appSettings = System.Text.Json.JsonDocument.Parse(json, new System.Text.Json.JsonDocumentOptions { CommentHandling = System.Text.Json.JsonCommentHandling.Skip });
            connectionString += appSettings.RootElement.GetProperty("ConnectionStrings").GetProperty("AppData").GetString();
        } catch { }

        if (String.IsNullOrEmpty(connectionString)) {
            connectionString = _connectionString;
        }

        if (!String.IsNullOrEmpty(connectionString)) {
            output.ActionResponse.Result = true;
            output.ConnectionString = connectionString;
            List<string> parts = connectionString.Split(';').ToList();
            if (parts != null && parts.Any()) {
                foreach (var part in parts) {
                    string element = String.Empty;
                    string value = String.Empty;

                    var items = part.Split('=');
                    if (items.Length > 0) {
                        element += items[0];
                        if (items.Length > 1) {
                            value += items[1];
                        }
                    }

                    element = element.Trim();
                    value = value.Trim();

                    if (!String.IsNullOrEmpty(element)) {
                        switch (element.ToUpper()) {
                            case "DATABASE":
                                output.MySQL_Database = value;
                                output.PostgreSql_Database = value;
                                output.SqlServer_Database = value;
                                break;

                            case "DATA SOURCE":
                                output.SQLiteDatabase = value;
                                output.SqlServer_Server = value;
                                break;

                            case "HOST":
                                output.PostgreSql_Host = value;
                                break;

                            case "INITIAL CATALOG":
                                output.SqlServer_Database = value;
                                break;

                            case "INTEGRATED SECURITY":
                                output.SqlServer_IntegratedSecurity = value.ToLower() == "true";
                                break;

                            case "PASSWORD":
                                output.MySQL_Password = value;
                                output.PostgreSql_Password = value;
                                output.SqlServer_Password = value;
                                break;

                            case "PERSIST SECURITY INFO":
                                output.SqlServer_PersistSecurityInfo = value.ToLower() == "true";
                                break;

                            case "SERVER":
                                output.MySQL_Server = value;
                                output.SqlServer_Server = value;
                                break;

                            case "TRUSTSERVERCERTIFICATE":
                                output.SqlServer_TrustServerCertificate = value.ToLower() == "true";
                                break;

                            case "USER":
                                output.MySQL_User = value;
                                break;

                            case "USERNAME":
                                output.PostgreSql_Username = value;
                                break;

                            case "USER ID":
                                output.SqlServer_UserId = value;
                                break;
                        }
                    }
                }
            }
        }

        return output;
    }
}