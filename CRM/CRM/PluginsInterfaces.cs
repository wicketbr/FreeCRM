using CRM;

/// <summary>
/// Interface for a basic plugin that just uses an Execute method to execute code.
/// </summary>
public interface IPlugin : IPluginBase
{
    Task<(bool Result, List<string>? Messages, IEnumerable<object>? Objects)> Execute(
        CRM.DataAccess da,
        Plugins.Plugin plugin,
        CRM.DataObjects.User? currentUser
    );
}

/// <summary>
/// Interface for an Auth plugin that implements the Login and Logout methods.
/// </summary>
public interface IPluginAuth : IPluginBase
{
    Task<(bool Result, List<string>? Messages, IEnumerable<object>? Objects)> Login(
        DataAccess da,
        Plugins.Plugin plugin,
        string url,
        Guid tenantId,
        Microsoft.AspNetCore.Http.HttpContext httpContext
    );

    Task<(bool Result, List<string>? Messages, IEnumerable<object>? Objects)> Logout(
        DataAccess da,
        Plugins.Plugin plugin,
        string url,
        Guid tenantId,
        Microsoft.AspNetCore.Http.HttpContext httpContext
    );
}

/// <summary>
/// Interface used for plugins that update user information.
/// </summary>
public interface IPluginUserUpdate : IPluginBase
{
    Task<(bool Result, List<string>? Messages, IEnumerable<object>? Objects)> UpdateUser(
        DataAccess da,
        Plugins.Plugin plugin,
        CRM.DataObjects.User? updateUser
    );
}

/// <summary>
/// Base plugin interface that all plugins should implement.
/// This defines the Properties method that returns a dictionary of properties.
/// </summary>
public interface IPluginBase
{
    public Dictionary<string, object> Properties();
}
