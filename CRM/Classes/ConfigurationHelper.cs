namespace CRM;

public class ConfigurationHelper : IConfigurationHelper
{
    private ConfigurationHelperLoader _loader = new ConfigurationHelperLoader();

    public ConfigurationHelper(ConfigurationHelperLoader ConfigurationLoader)
    {
        _loader = ConfigurationLoader;
    }

    public string? BasePath
    {
        get {
            return _loader.BasePath;
        }
    }

    public ConfigurationHelperConnectionStrings ConnectionStrings
    {
        get {
            return _loader.ConnectionStrings;
        }
    }

    public List<string>? GloballyDisabledModules
    {
        get {
            return _loader.GloballyDisabledModules;
        }
    }
}

public interface IConfigurationHelper
{
    public string? BasePath { get; }
    ConfigurationHelperConnectionStrings ConnectionStrings { get; }
    List<string>? GloballyDisabledModules { get; }
}

public class ConfigurationHelperLoader
{
    public string? BasePath { get; set; }
    public ConfigurationHelperConnectionStrings ConnectionStrings { get; set; } = new ConfigurationHelperConnectionStrings();
    public List<string>? GloballyDisabledModules { get; set; }
}

public class ConfigurationHelperConnectionStrings
{
    public string? AppData { get; set; }
}