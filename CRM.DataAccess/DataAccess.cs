using Microsoft.AspNetCore.Http;
using Plugins;

namespace CRM;

public partial class DataAccess: IDisposable, IDataAccess
{
    private int _accountLockoutMaxAttempts = 5;
    private int _accountLockoutMinutes = 10;
    private string _appName = "freeCRM";
    private DataObjects.AuthenticationProviders? _authenticationProviders;
    private string _connectionString;
    private string _cookiePrefix = "";
    private string _copyright = "Company Name";
    private EFDataModel data;
    private string _databaseType;
    private bool _firstInit = true;
    private Guid _guid1 = new Guid("00000000-0000-0000-0000-000000000001");
    private Guid _guid2 = new Guid("00000000-0000-0000-0000-000000000002");
    private HttpContext? _httpContext;
    private HttpRequest? _httpRequest;
    private HttpResponse? _httpResponse;
    private bool _inMemoryDatabase = false;
    private string _localModeUrl = "";
    private bool _open;
    private DateOnly _released = DateOnly.FromDateTime(Convert.ToDateTime("12/17/2025"));
    private IServiceProvider? _serviceProvider;
    private string _uniqueId = Guid.NewGuid().ToString().Replace("-", "").ToLower();
    private bool _useMigrations = false;
    private bool _useBackgroundService = false;
    private string _version = "2.0.0";

    public DataAccess(
        string ConnectionString = "",
        string DatabaseType = "",
        string LocalModeUrl = "",
        IServiceProvider? serviceProvider = null,
        string CookiePrefix = "",
        bool UseBackgroundService = false
    )
    {
        _cookiePrefix = CookiePrefix;
        _connectionString = ConnectionString;
        _databaseType = DatabaseType;
        _localModeUrl = LocalModeUrl;
        _serviceProvider = serviceProvider;
        _useBackgroundService = UseBackgroundService;

        if (!String.IsNullOrWhiteSpace(_connectionString)) {
            _connectionString = ConnectionString;
        }

        DataAccessAppInit();

        var optionsBuilder = new DbContextOptionsBuilder<EFDataModel>();

        if (StringValue(_databaseType).ToLower() == "inmemory") {
            // A connection string is not required or used for the InMemory option,
            // so just set it to a default value if we are using the InMemory database.
            _connectionString = "InMemory";
        }

        // Both the Connection String and Database Type parameters are required.
        // Otherwise the app will redirect to the page to configure the database connection.
        if (!String.IsNullOrEmpty(_connectionString) && !String.IsNullOrWhiteSpace(_databaseType)) {
            switch (_databaseType.ToLower()) {
                case "inmemory":
                    optionsBuilder.UseInMemoryDatabase("InMemory");
                    _inMemoryDatabase = true;
                    break;

                case "mysql":
                    optionsBuilder.UseMySQL(_connectionString, options => options.EnableRetryOnFailure());
                    break;

                case "postgresql":
                    optionsBuilder.UseNpgsql(_connectionString, options => options.EnableRetryOnFailure());
                    break;

                case "sqlite":
                    optionsBuilder.UseSqlite(_connectionString);
                    break;

                case "sqlserver":
                    optionsBuilder.UseSqlServer(_connectionString, options => options.EnableRetryOnFailure());
                    break;
            }

            data = new EFDataModel(optionsBuilder.Options);

            if (_inMemoryDatabase) {
                // For the In-Memory database this creates the schema in memory.
                data.Database.EnsureCreated();
            }

            if (!GlobalSettings.StartupRun && _firstInit) {
                _firstInit = false;

                if (data.Database.CanConnect()) {
                    _open = true;

                    // See if any migrations need to be applied.
                    if (!_inMemoryDatabase && _useMigrations) {
                        DatabaseApplyLatestMigrations();
                    }
                } else {
                    // Try and create the database using the built-in EF command
                    try {
                        data.Database.EnsureCreated();

                        // See if any migrations need to be applied.
                        if (data.Database.CanConnect()) {
                            if (_useMigrations) {
                                DatabaseApplyLatestMigrations();
                            }

                            _open = true;
                        } else {
                            //throw new Exception("Unable to connect to the database. Please check your connection string.");
                        }
                    } catch (Exception ex) {
                        // This would indicate that the database is not configured correctly, or that
                        // for some reason it is offline.
                        GlobalSettings.StartupErrorMessages = new List<string>();

                        GlobalSettings.StartupError = true;
                        GlobalSettings.StartupErrorCode = "DatabaseOffline";
                        GlobalSettings.StartupErrorMessages.Add(ex.Message);
                        if (ex.InnerException != null && !String.IsNullOrEmpty(ex.InnerException.Message)) {
                            GlobalSettings.StartupErrorMessages.Add(ex.InnerException.Message);
                        }
                        return;
                    }
                }

                // Make sure the default data exists and is up to date.
                SeedTestData();

                GlobalSettings.StartupRun = true;
                GlobalSettings.RunningSince = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            } else {
                _open = true;
            }

            if (PluginsInterface != null && !GlobalSettings.PluginsSavedToCache) {
                GlobalSettings.PluginsSavedToCache = true;
                SavePluginsToCache();
            }
        } else {
            // To prevent errors just use an InMemory copy
            optionsBuilder.UseInMemoryDatabase("InMemory");
            data = new EFDataModel(optionsBuilder.Options);

            if (!GlobalSettings.StartupRun) {
                GlobalSettings.StartupError = true;
                GlobalSettings.StartupErrorCode = "MissingConnectionString";
            } else {
                throw new NullReferenceException("Missing Connection String and/or Database Type");
            }
        }
    }
}