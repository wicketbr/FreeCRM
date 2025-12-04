namespace CRM;

public partial class ConfigurationHelper : IConfigurationHelper
{
    private ConfigurationHelperLoader _loader = new ConfigurationHelperLoader();

    public ConfigurationHelper(ConfigurationHelperLoader ConfigurationLoader)
    {
        _loader = ConfigurationLoader;
    }

    public string? AnalyticsCode {
        get {
            return _loader.AnalyticsCode;
        }
    }

    public string? BasePath {
        get {
            return _loader.BasePath;
        }
    }

    public ConfigurationHelperConnectionStrings ConnectionStrings {
        get {
            return _loader.ConnectionStrings;
        }
    }

    public string? CookiePrefix {
        get {
            return _loader.CookiePrefix;
        }
    }

    public List<string>? GloballyDisabledModules {
        get {
            return _loader.GloballyDisabledModules;
        }
    }

    public List<string>? GloballyEnabledModules {
        get {
            return _loader.GloballyEnabledModules;
        }
    }
}

public partial interface IConfigurationHelper
{
    public string? AnalyticsCode { get; }
    public string? BasePath { get; }
    ConfigurationHelperConnectionStrings ConnectionStrings { get; }
    public string? CookiePrefix { get; }
    List<string>? GloballyDisabledModules { get; }
    List<string>? GloballyEnabledModules { get; }
}

public partial class ConfigurationHelperLoader
{
    public string? AnalyticsCode { get; set; }
    public string? BasePath { get; set; }
    public ConfigurationHelperConnectionStrings ConnectionStrings { get; set; } = new ConfigurationHelperConnectionStrings();
    public string? CookiePrefix { get; set; }
    public List<string>? GloballyDisabledModules { get; set; }
    public List<string>? GloballyEnabledModules { get; set; }
}

public partial class ConfigurationHelperConnectionStrings
{
    public string? AppData { get; set; }
}