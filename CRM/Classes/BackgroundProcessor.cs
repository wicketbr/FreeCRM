using Microsoft.EntityFrameworkCore;
using MySqlX.XDevAPI.Common;
using Plugins;
using System.Timers;

namespace CRM;

public class BackgroundProcessor : BackgroundService
{
    private List<Plugins.Plugin> _availablePlugins = new List<Plugins.Plugin>();
    private IDataAccess da;
    private long _iterations = 0;
    private readonly ILogger<BackgroundProcessor> _logger;
    private List<Plugins.Plugin> _plugins = new List<Plugins.Plugin>();
    private int _processingIntervalSeconds = 5;
    private List<Guid> _processingPlugins = new List<Guid>();
    private System.Timers.Timer processorTimer = null!;
    private System.Timers.Timer queueTimer = null!;
    private IServiceProvider _serviceProvider;
    private bool _startOnLoad = false;

    public BackgroundProcessor(
        ILogger<BackgroundProcessor> logger,
        IServiceProvider ServiceProvider,
        int ProcessingIntervalSeconds,
        bool StartOnLoad
    ) {
        _logger = logger;
        _processingIntervalSeconds = ProcessingIntervalSeconds > 0 ? ProcessingIntervalSeconds : 15;
        _serviceProvider = ServiceProvider;
        _startOnLoad = StartOnLoad;

        da = ServiceProvider.GetRequiredService<IDataAccess>();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        _logger.LogInformation("Background Processor is starting.");

        var allPlugins = da.GetPlugins();
        if (allPlugins.Any(x => x.Type.ToLower() == "backgroundprocess")) {
            _availablePlugins = allPlugins.Where(x => x.Type.ToLower() == "backgroundprocess").ToList();
        }

        // Configure the work timer.
        processorTimer = new System.Timers.Timer(TimeSpan.FromMilliseconds(1));
        processorTimer.Elapsed += ProcessTasks;
        processorTimer.AutoReset = false;

        queueTimer = new System.Timers.Timer(TimeSpan.FromSeconds(_processingIntervalSeconds));
        queueTimer.Elapsed += ProcessQueueTimer;
        queueTimer.AutoReset = true;

        if (_startOnLoad) {
            await GetTasksToProcess();
        } else {
            queueTimer.Start();
        }
    }

    private async Task GetTasksToProcess()
    {
        var now = DateTime.UtcNow;
        var tenants = await da.GetTenants();
        _iterations++;

        foreach (var tenant in tenants.Where(x => x.Enabled == true)) {
            // Process any pending deleted records, but only do this every 100 iterations to reduce load.
            if (_iterations == 1 || _iterations % 100 == 0) {
                if (tenant.TenantSettings.DeletePreference == DataObjects.DeletePreference.MarkAsDeleted) {
                    if (tenant.TenantSettings.DeleteMarkedRecordsAfterDays > 0) {
                        var olderThan = now.AddDays(-tenant.TenantSettings.DeleteMarkedRecordsAfterDays);

                        var deletedRecords = await da.DeleteAllPendingDeletedRecords(tenant.TenantId, olderThan);
                        ProcessTasksMessages(deletedRecords);
                    }
                }
            }

            // Process any app-specific tasks.
            var appTasksForTenant = await da.ProcessBackgroundTasksApp(tenant.TenantId, _iterations);
            ProcessTasksMessages(appTasksForTenant);
        }

        // Process any app-specific tasks for specific tenants.
        var appTasks = await da.ProcessBackgroundTasksApp(Guid.Empty, _iterations);
        ProcessTasksMessages(appTasks);

        // If there are any plugins that need to be processed, add them to the queue.
        bool startTimer = false;
        foreach(var plugin in _availablePlugins) {
            if (!_processingPlugins.Contains(plugin.Id)) {
                _plugins.Add(plugin);
                startTimer = true;
            }
        }

        if (startTimer) {
            processorTimer.Start();
        }

        if (!queueTimer.Enabled) {
            queueTimer.Start();
        }
    }

    private async void ProcessTasks(Object? source, ElapsedEventArgs e)
    {
        // Get any plugins that need to be processed that are not already processing.
        var pluginsToProcess = _plugins.Where(x => !_processingPlugins.Contains(x.Id)).ToList();

        if (pluginsToProcess != null && pluginsToProcess.Count > 0) {
            foreach(var plugin in pluginsToProcess) {
                _processingPlugins.Add(plugin.Id);

                var pluginExecuteRequest = new PluginExecuteRequest { 
                    Plugin = plugin,
                    Objects = new object[] { _iterations },
                };

                var executed = da.ExecutePlugin(pluginExecuteRequest);

                ProcessTasksMessages(new DataObjects.BooleanResponse {
                    Result = executed.Result,
                    Messages = executed.Messages,
                });

                _processingPlugins = _processingPlugins.Where(x => x != plugin.Id).ToList();
                _plugins = _plugins.Where(x => x.Id != plugin.Id).ToList();
            }
        }
    }

    private async void ProcessQueueTimer(Object? source, ElapsedEventArgs e)
    {
        await GetTasksToProcess();
    }

    protected void ProcessTasksMessages(DataObjects.BooleanResponse? response)
    {
        if (response != null && response.Messages.Count > 0) {
            if (response.Result) {
                foreach (var message in response.Messages) {
                    _logger.LogInformation(message);
                }
            } else {
                foreach (var message in response.Messages) {
                    _logger.LogError(message);
                }
            }
        }
    }
}
