namespace CRM;

public partial class Program
{
    public static WebApplicationBuilder AppModifyBuilderEnd(WebApplicationBuilder builder)
    {
        var output = builder;
        // Add any app-specific modifications to the builder here.
        return output;
    }

    public static WebApplicationBuilder AppModifyBuilderStart(WebApplicationBuilder builder)
    {
        var output = builder;
        // Add any app-specific modifications to the builder here.
        return output;
    }

    public static WebApplication AppModifyEnd(WebApplication app)
    {
        var output = app;
        // Add any app-specific modifications to the app here.
        return output;
    }

    public static WebApplication AppModifyStart(WebApplication app)
    {
        var output = app;
        // Add any app-specific modifications to the app here.
        return output;
    }

    public static List<string> AuthenticationPoliciesApp {
        get {
            var output = new List<string>();
            
            // Add any app-specific authentication policies here.
            // Example:
            //output.Add("PolicyName");
            
            return output;
        }
    }

    public static ConfigurationHelperLoader ConfigurationHelpersLoadApp(ConfigurationHelperLoader loader, WebApplicationBuilder builder)
    {
        var output = loader;

        //output.MyProperty = builder.Configuration.GetValue<string>("MyProperty"); ;

        return output;
    }
}