using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.DependencyInjection;
using MySqlX.XDevAPI.Common;
using Org.BouncyCastle.Utilities;
using Plugins;

namespace CRM;

public partial interface IDataAccess
{
    PluginExecuteResult ExecutePlugin(PluginExecuteRequest request, DataObjects.User? CurrentUser = null);
    List<Plugins.Plugin> GetPlugins();
    IPlugins? PluginsInterface { get; }
}

public partial class DataAccess
{
    public PluginExecuteResult ExecutePlugin(PluginExecuteRequest request, DataObjects.User? CurrentUser = null)
    {
        var output = new PluginExecuteResult { 
            Messages = new List<string>(),
            Objects = new List<object>(),
            Result = false,
        };

        var code = request.Plugin.Code;
        if (String.IsNullOrWhiteSpace(code)) {
            var plugins = GetPlugins();
            var plugin = plugins.FirstOrDefault(x => x.Id == request.Plugin.Id && x.Version == request.Plugin.Version);
            if (plugin != null) {
                code += plugin.Code;
            }
        }

        if (!String.IsNullOrWhiteSpace(code)) {
            object[] objectArguments = new object[] { this, request.Plugin, CurrentUser != null ? CurrentUser : new DataObjects.User() };

            // Auth types don't include the CurrentUser object.
            if (request.Plugin.Type.ToLower() == "auth") {
                objectArguments = new object[] { this, request.Plugin };
            }

            if (request.Objects != null) {
                objectArguments = objectArguments.Concat(request.Objects).ToArray();
            }

            var additionalAssemblies = request.Plugin.AdditionalAssemblies;

            // Add the assemblies required for running on the server.
            additionalAssemblies.Add(typeof(DataAccess).Assembly.Location);
            additionalAssemblies.Add(typeof(DataObjects.BooleanResponse).Assembly.Location);
            additionalAssemblies.Add(typeof(CRM.EFModels.EFModels.User).Assembly.Location);
            additionalAssemblies.Add(typeof(JWT.JwtEncoder).Assembly.Location);
            additionalAssemblies.Add(typeof(JWT.Algorithms.RS256Algorithm).Assembly.Location);
            additionalAssemblies.Add(typeof(JWT.Serializers.JsonNetSerializer).Assembly.Location);
            additionalAssemblies.Add(typeof(Microsoft.EntityFrameworkCore.DbContext).Assembly.Location);
            additionalAssemblies.Add(typeof(System.Net.Http.HttpClient).Assembly.Location);
            additionalAssemblies.Add(typeof(Microsoft.AspNetCore.Http.HttpContext).Assembly.Location);
            additionalAssemblies.Add(typeof(Microsoft.AspNetCore.Http.IQueryCollection).Assembly.Location);
            additionalAssemblies.Add(typeof(Microsoft.Extensions.Primitives.StringValues).Assembly.Location);

            // If there are any additional assemblies to load, we also need to load them into memory.
            foreach (var assembly in additionalAssemblies) {
                try {
                    System.Reflection.Assembly.LoadFrom(assembly);
                } catch (Exception ex) {
                    if (ex != null) { }
                }
            }

            // Execute the plugin code. This will return a tuple of boolean Result, a List of Messages, and possibly an array of Objects.
            if (PluginsInterface != null) {
                var result = PluginsInterface.ExecuteDynamicCSharpCode<(bool Result, List<string>? Messages, IEnumerable<object>? Objects)>(
                    code, 
                    objectArguments, 
                    additionalAssemblies, 
                    request.Plugin.Namespace, 
                    request.Plugin.ClassName, 
                    request.Plugin.Invoker
                );

                output.Result = result.Result;

                if (result.Messages != null) {
                    output.Messages = result.Messages;
                }

                if (result.Objects != null) {
                    output.Objects = result.Objects.ToList();
                }
            }
        } else {
            output.Messages.Add("Plugin contains no code.");
        }

        return output;
    }

    public List<Plugins.Plugin> GetPlugins()
    {
        var output = new List<Plugins.Plugin>();

        // See if the cache contains the plugins.
        if (CacheStore.ContainsKey(Guid.Empty, "Plugins")) {
            var cached = CacheStore.GetCachedItem<List<Plugins.Plugin>>(Guid.Empty, "Plugins");
            if (cached != null) {
                output = cached;
            }
        }

        if (!output.Any()) {
            // If the items weren't in the cache try and get them from the plugins DI object.
            if (PluginsInterface != null) {
                output = PluginsInterface.AllPlugins;
            }
        }

        return output;
    }

    private List<Plugins.Plugin> GetPluginsByType(string? type)
    {
        var output = GetPlugins();

        if (!String.IsNullOrWhiteSpace(type)) {
            var filtered = output.Where(x => x.Type.ToLower() == type.ToLower()).ToList();
            output = filtered != null && filtered.Any() ? filtered : new List<Plugin>();
        }

        return output;
    }

    public List<Plugins.Plugin> GetPluginsWithoutCode()
    {
        var output = new List<Plugins.Plugin>();

        var allPlugins = GetPlugins();
        if (allPlugins.Count > 0) {
            var duplicate = DuplicateObject<List<Plugins.Plugin>>(allPlugins);
            if (duplicate != null) {
                foreach (var item in duplicate) {
                    item.Code = String.Empty;
                }
                output = duplicate;
            }
        }

        return output;
    }

    private List<Plugins.Plugin> GetPlugins_UserUpdate()
    {
        return GetPluginsByType("UserUpdate");
    }

    public IPlugins? PluginsInterface {
        get {
            IPlugins? output = null;

            if (_serviceProvider != null) {
                output = _serviceProvider.GetRequiredService<IPlugins>();
            }

            return output;
        }
    }

    private void SavePluginsToCache() {
        if (PluginsInterface != null) {
            // First, mark all records as StillExists = false;

            var recs = data.PluginCaches.ToList();

            if (recs != null && recs.Any()) {
                foreach (var rec in recs) {
                    rec.StillExists = false;
                }

                data.SaveChanges();
            }

            // Now, add or update the records that still exists.
            if (PluginsInterface.AllPluginsForCache.Any()) {
                foreach (var plugin in PluginsInterface.AllPluginsForCache) {
                    bool newRecord = false;

                    var rec = data.PluginCaches.FirstOrDefault(x => x.Id == plugin.Id && x.Version == plugin.Version);
                    if (rec == null) {
                        rec = new PluginCache {
                            RecordId = Guid.NewGuid(),
                            Id = plugin.Id,
                            Version = plugin.Version,
                        };
                        newRecord = true;
                    }

                    var code = plugin.Code;

                    // Always store the plugin code in the encrypted format.
                    if (!String.IsNullOrWhiteSpace(code)) {
                        if (!code.Contains(",0x")) {
                            
                        }
                    }

                    rec.Author = plugin.Author;
                    rec.Name = plugin.Name;
                    rec.Type = plugin.Type;
                    rec.Version = plugin.Version;
                    rec.Properties = SerializeObject(plugin.Properties);
                    rec.Namespace = plugin.Namespace;
                    rec.ClassName = plugin.ClassName;
                    rec.Code = plugin.Code;
                    rec.AdditionalAssemblies = SerializeObject(plugin.AdditionalAssemblies);
                    rec.StillExists = true;

                    if (newRecord) {
                        data.PluginCaches.Add(rec);
                    }
                }

                data.SaveChanges();
            }
        }
    }
}