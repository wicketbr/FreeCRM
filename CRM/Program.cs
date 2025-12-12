using CRM.Client.Pages;
using CRM.Components;
using CRM.Server.Hubs;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Claims;

namespace CRM
{
    public partial class Program
    {
        public static void Main(string[] args)
        {
            var builder = AppModifyBuilderStart(WebApplication.CreateBuilder(args));

            builder.Services.AddControllersWithViews();

            var isDevelopment = builder.Environment.IsDevelopment();
            if (!isDevelopment) {

            }

            // Try to get the application name.
            string cookiePrefix = String.Empty;
            Assembly assembly = Assembly.GetExecutingAssembly();
            try {
                cookiePrefix += assembly.FullName;

                if (cookiePrefix.Contains(",")) { 
                    cookiePrefix = cookiePrefix.Substring(0, cookiePrefix.IndexOf(",")).Trim();
                }

                if (!String.IsNullOrWhiteSpace(cookiePrefix)) {
                    cookiePrefix = new string(cookiePrefix.Where(Char.IsLetter).ToArray()).ToLower() + "_";
                }
            } catch { }

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
                foreach (var item in usingStatementsFromConfig) {
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

            bool backgroundServiceEnabled = builder.Configuration.GetValue<bool>("BackgroundService:Enabled");
            if (backgroundServiceEnabled) {
                var loadBalancingFilter = String.Empty + builder.Configuration.GetValue<string>("BackgroundService:LoadBalancingFilter");
                if (!String.IsNullOrWhiteSpace(loadBalancingFilter)) {
                    var hostname = (String.Empty + System.Environment.MachineName).ToLower();
                    backgroundServiceEnabled = hostname.Contains(loadBalancingFilter.ToLower());
                }
            }

            string _localModeUrl = String.Empty + builder.Configuration.GetValue<string>("LocalModeUrl");
            string _connectionString = String.Empty + builder.Configuration.GetConnectionString("AppData");
            string _databaseType = String.Empty + builder.Configuration.GetValue<string>("DatabaseType");
            builder.Services.AddTransient<IDataAccess>(x => ActivatorUtilities.CreateInstance<DataAccess>(x, _connectionString, _databaseType, _localModeUrl, x.GetRequiredService<IServiceProvider>(), cookiePrefix, backgroundServiceEnabled));

            if (backgroundServiceEnabled) {
                // Create a logger for the background process and add the hosted service for the background processor.
                var logFilePath = builder.Configuration.GetValue<string>("BackgroundService:LogFilePath");
                string logFile = !String.IsNullOrWhiteSpace(logFilePath) ? System.IO.Path.Combine(logFilePath, "BackgroundService.log") : String.Empty;
                var loggerFactory = LoggerFactory.Create(builder => {
                    builder.AddConsole();
                    if (!String.IsNullOrWhiteSpace(logFile)) {
                        builder.AddFile(logFile);
                    }
                });
                var logger = loggerFactory.CreateLogger<BackgroundProcessor>();

                int processingIntervalSeconds = builder.Configuration.GetValue<int>("BackgroundService:ProcessingIntervalSeconds");
                bool startOnLoad = builder.Configuration.GetValue<bool>("BackgroundService:StartOnLoad");
                builder.Services.AddHostedService<BackgroundProcessor>(x => ActivatorUtilities.CreateInstance<BackgroundProcessor>(x, logger, x.GetRequiredService<IServiceProvider>(), processingIntervalSeconds, startOnLoad));
            }

            var useAuthorization = CustomAuthenticationProviders.UseAuthorization(builder);
            builder.Services.AddTransient<ICustomAuthentication>(x => ActivatorUtilities.CreateInstance<CustomAuthentication>(x, useAuthorization));

            var allowApplicationEmbedding = builder.Configuration.GetValue<bool>("AllowApplicationEmbedding");
            if (allowApplicationEmbedding) {
                builder.Services.AddAntiforgery(x => x.SuppressXFrameOptionsHeader = true);
            }

            // Create DI for supported configuration items.
            var analyticsCode = builder.Configuration.GetValue<string>("AnalyticsCode");

            var basePath = builder.Configuration.GetValue<string>("BasePath");
            if (!String.IsNullOrWhiteSpace(basePath) && !basePath.EndsWith("/")) {
                basePath += "/";
            }

            List<string> disabled = new List<string>();
            var globallyDisabledModules = builder.Configuration.GetSection("GloballyDisabledModules").GetChildren();
            if (globallyDisabledModules != null && globallyDisabledModules.Any()) {
                foreach (var item in globallyDisabledModules.ToArray().Select(c => c.Value).ToList()) {
                    if (!String.IsNullOrWhiteSpace(item)) {
                        disabled.Add(item.ToLower());
                    }
                }
            }

            List<string> enabled = new List<string>();
            var globallyEnabledModules = builder.Configuration.GetSection("GloballyEnabledModules").GetChildren();
            if (globallyEnabledModules != null && globallyEnabledModules.Any()) {
                foreach (var item in globallyEnabledModules.ToArray().Select(c => c.Value).ToList()) {
                    if (!String.IsNullOrWhiteSpace(item)) {
                        enabled.Add(item.ToLower());
                    }
                }
            }

            var configurationHelperLoader = ConfigurationHelpersLoadApp(new ConfigurationHelperLoader {
                AnalyticsCode = analyticsCode,
                CookiePrefix = cookiePrefix,
                BasePath = basePath,
                ConnectionStrings = new ConfigurationHelperConnectionStrings {
                    AppData = builder.Configuration.GetConnectionString("AppData"),
                },
                GloballyDisabledModules = disabled,
                GloballyEnabledModules = enabled,
            }, builder);

            builder.Services.AddTransient<IConfigurationHelper>(x => ActivatorUtilities.CreateInstance<ConfigurationHelper>(x, configurationHelperLoader));

            var policies = new List<string> {
                "AppAdmin",
                "Admin",
                // {{ModuleItemStart:Appointments}}
                "CanBeScheduled",
                "ManageAppointments",
                // {{ModuleItemEnd:Appointments}}
                "ManageFiles",
                "PreventPasswordChange",
            };
            policies.AddRange(AuthenticationPoliciesApp);
            builder.Services.AddAuthorization(options => {
                foreach (var p in policies) {
                    options.AddPolicy(p, policy => policy.RequireClaim(ClaimTypes.Role, p));
                }
            });

            var app = AppModifyStart(AppModifyBuilderEnd(builder).Build());

            bool openIdForceHttps = false;
            try {
                openIdForceHttps = builder.Configuration.GetValue<bool>("AuthenticationProviders:OpenId:ForceHttps");
            } catch { }
            if (openIdForceHttps) {
                app.Use((context, next) => {
                    context.Request.Scheme = "https";
                    context.Response.Headers.Append("Content-Security-Policy", "frame-ancestors 'self' login.wsu.edu cms.em.wsu.edu futurecoug.wsu.edu");
                    return next(context);
                });
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment()) {
                app.UseWebAssemblyDebugging();
            } else {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
            //app.UseHttpsRedirection();

            app.MapStaticAssets();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseAntiforgery();

            app.MapHub<crmHub>("/crmHub", signalRConnctionOptions => {
                signalRConnctionOptions.AllowStatefulReconnects = true;
            });

            app.MapRazorPages();

            app.MapControllers();

            app.MapRazorComponents<App>()
                .AddInteractiveWebAssemblyRenderMode()
                .AddAdditionalAssemblies(typeof(Client._Imports).Assembly);

            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            AppModifyEnd(app).Run();
        }
    }
}
