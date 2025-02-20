using CRM.Components;
using CRM.Server.Controllers;
using CRM.Server.Hubs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Builder;
using System.Security.Claims;

namespace CRM
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();

            var isDevelopment = builder.Environment.IsDevelopment();
            if (!isDevelopment) {
                
            }

            // Attempts to read the AzureSignalRurl setting from appsettings.json.
            string azureSignalRUrl = String.Empty + builder.Configuration.GetValue<string>("AzureSignalRurl");
            if (String.IsNullOrWhiteSpace(azureSignalRUrl)) {
                // Fall back to local SignalR when the setting isn't specified.
                builder.Services.AddSignalR();
            } else {
                // Include the parameter to tell Azure SignalR what endpoint to use.
                builder.Services.AddSignalR().AddAzureSignalR("Endpoint=" + azureSignalRUrl);
            }

            builder.Services.AddRazorComponents()
                .AddInteractiveWebAssemblyComponents();

            builder.Services.AddRazorPages();
            builder.Services.AddHttpContextAccessor();

            #region Plugin Configuration and DI Creation

            // Load any plugins from the Plugins folder.
            // Plugins will get injected into the DataAccess DI object.
            // They will also be available as their own DI object.
            var plugins = new Plugins.Plugins();

            // Setup the server references required for executing dynamic code in plugins.
            var serverReferences = new List<string>();
            try { serverReferences.Add(typeof(DataAccess).Assembly.Location); } catch { }
            try { serverReferences.Add(typeof(DataObjects.BooleanResponse).Assembly.Location); } catch { }
            try { serverReferences.Add(typeof(CRM.EFModels.EFModels.User).Assembly.Location); } catch { }
            try { serverReferences.Add(typeof(JWT.JwtEncoder).Assembly.Location); } catch { }
            try { serverReferences.Add(typeof(JWT.Algorithms.RS256Algorithm).Assembly.Location); } catch { }
            try { serverReferences.Add(typeof(JWT.Serializers.JsonNetSerializer).Assembly.Location); } catch { }
            try { serverReferences.Add(typeof(Microsoft.EntityFrameworkCore.DbContext).Assembly.Location); } catch { }
            try { serverReferences.Add(typeof(System.Net.Http.HttpClient).Assembly.Location); } catch { }
            try { serverReferences.Add(typeof(CRM.Client.BlazorDataModel).Assembly.Location); } catch { }
            try { serverReferences.Add(typeof(Microsoft.AspNetCore.Http.HttpContext).Assembly.Location); } catch { }
            try { serverReferences.Add(typeof(Microsoft.AspNetCore.Http.IQueryCollection).Assembly.Location); } catch { }
            try { serverReferences.Add(typeof(Microsoft.Extensions.Primitives.StringValues).Assembly.Location); } catch { }
            try { serverReferences.Add(typeof(Plugins.Plugin).Assembly.Location); } catch { }
            try { serverReferences.Add(typeof(IPlugin).Assembly.Location); } catch { }
            plugins.ServerReferences = serverReferences;

            // Get the using statements from the appsettings.json file.
            // These will be used to ensure all required namespaces for this app are used when executing dynamic code in plugins.
            var usingStatements = new List<string>();
            var usingStatementsFromConfig = builder.Configuration.GetSection("PluginUsingStatements").GetChildren().Select(c => c.Value).ToArray();
            if (usingStatementsFromConfig != null && usingStatementsFromConfig.Length > 0) {
                foreach(var item in usingStatementsFromConfig) {
                    if (!String.IsNullOrWhiteSpace(item)) {
                        usingStatements.Add(item);
                    }
                }
            }
            plugins.UsingStatements = usingStatements;

            string pluginsPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
            plugins.Load(pluginsPath);
            builder.Services.AddTransient<Plugins.IPlugins>(x => plugins);

            #endregion

            builder.Services.AddSingleton<IServiceProvider>(provider => provider);

            string _localModeUrl = String.Empty + builder.Configuration.GetValue<string>("LocalModeUrl");
            string _connectionString = String.Empty + builder.Configuration.GetConnectionString("AppData");
            string _databaseType = String.Empty + builder.Configuration.GetValue<string>("DatabaseType");
            builder.Services.AddTransient<IDataAccess>(x => ActivatorUtilities.CreateInstance<DataAccess>(x, _connectionString, _databaseType, _localModeUrl, x.GetRequiredService<IServiceProvider>()));

            var useAuthorization = CustomAuthenticationProviders.UseAuthorization(builder);
            builder.Services.AddTransient<ICustomAuthentication>(x => ActivatorUtilities.CreateInstance<CustomAuthentication>(x, useAuthorization));

            var allowApplicationEmbedding = builder.Configuration.GetValue<bool>("AllowApplicationEmbedding");
            if (allowApplicationEmbedding) {
                builder.Services.AddAntiforgery(x => x.SuppressXFrameOptionsHeader = true);
            }

            // Create DI for supported configuration items.
            var basePath = builder.Configuration.GetValue<string>("BasePath");
            if (!String.IsNullOrWhiteSpace(basePath) && !basePath.EndsWith("/")) {
                basePath += "/";
            }

            List<string> disabled = new List<string>();
            var globallyDisabledModules = builder.Configuration.GetSection("GloballyDisabledModules").GetChildren();
            if(globallyDisabledModules != null && globallyDisabledModules.Any()) {
                foreach(var item in globallyDisabledModules.ToArray().Select(c => c.Value).ToList()) {
                    if (!String.IsNullOrWhiteSpace(item)) {
                        disabled.Add(item.ToLower());
                    }
                }
            }

            var configurationHelperLoader = new ConfigurationHelperLoader {
                BasePath = basePath,
                ConnectionStrings = new ConfigurationHelperConnectionStrings {
                    AppData = builder.Configuration.GetConnectionString("AppData"),
                },
                GloballyDisabledModules = disabled,
            };
            builder.Services.AddTransient<IConfigurationHelper>(x => ActivatorUtilities.CreateInstance<ConfigurationHelper>(x, configurationHelperLoader));

            builder.Services.AddAuthorization(options => {
                options.AddPolicy("AppAdmin", policy => policy.RequireClaim(ClaimTypes.Role, "AppAdmin"));
                options.AddPolicy("Admin", policy => policy.RequireClaim(ClaimTypes.Role, "Admin"));
                options.AddPolicy("CanBeScheduled", policy => policy.RequireClaim(ClaimTypes.Role, "CanBeScheduled"));
                options.AddPolicy("ManageAppointments", policy => policy.RequireClaim(ClaimTypes.Role, "ManageAppointments"));
                options.AddPolicy("ManageFiles", policy => policy.RequireClaim(ClaimTypes.Role, "ManageFiles"));
                options.AddPolicy("PreventPasswordChange", policy => policy.RequireClaim(ClaimTypes.Role, "PreventPasswordChange"));
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment()) {
                app.UseWebAssemblyDebugging();
            } else {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            
            //app.UseStaticFiles(); Replaced by the newer MapStaticAssets middleware.
            app.MapStaticAssets();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseAntiforgery();

            app.MapHub<crmhub>("/crmhub", signalRConnctionOptions => {
                signalRConnctionOptions.AllowStatefulReconnects = true;
            });

            app.MapRazorPages();

            app.MapControllers();

            app.MapRazorComponents<App>()
                .AddInteractiveWebAssemblyRenderMode()
                .AddAdditionalAssemblies(typeof(CRM.Client.Pages.Index).Assembly);

            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            app.Run();
        }
    }
}
