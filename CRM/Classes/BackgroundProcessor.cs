using Microsoft.EntityFrameworkCore;
using Plugins;
using System.Timers;

namespace CRM;

public class BackgroundProcessor : BackgroundService
{
    private List<Plugins.Plugin> _availablePlugins = new List<Plugins.Plugin>();
    private long _iterations = 0;
    private bool _loadedPlugins = false;
    private readonly ILogger<BackgroundProcessor> _logger;
    private List<Plugins.Plugin> _plugins = new List<Plugins.Plugin>();
    private List<Guid> _processingAppTasks = new List<Guid>();
    private int _processingIntervalSeconds = 5;
    private List<Guid> _processingPlugins = new List<Guid>();
    private System.Timers.Timer processorTimer = null!;
    private System.Timers.Timer queueTimer = null!;
    private IServiceProvider _serviceProvider;
    private bool _startOnLoad = false;

    public BackgroundProcessor
    (
        ILogger<BackgroundProcessor> logger,
        IServiceProvider ServiceProvider,
        int ProcessingIntervalSeconds,
        bool StartOnLoad
    ){
        _logger = logger;
        _processingIntervalSeconds = ProcessingIntervalSeconds > 0 ? ProcessingIntervalSeconds : 15;
        _serviceProvider = ServiceProvider;
        _startOnLoad = StartOnLoad;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Background Processor is starting.");

        // Configure the work timer.
        processorTimer = new System.Timers.Timer(TimeSpan.FromMilliseconds(1));
        processorTimer.Elapsed += ProcessTasks;
        processorTimer.AutoReset = false;

        queueTimer = new System.Timers.Timer(TimeSpan.FromSeconds(_processingIntervalSeconds));
        queueTimer.Elapsed += ProcessQueueTimer;
        queueTimer.AutoReset = true;

        if (_startOnLoad || _processingIntervalSeconds < 30) {
            // To prevent the DataAccess library from being invoked before
            // the app has finished starting, we will delay the first execution
            // of the background processor to 30 seconds.
            // This way, the minimum startup interval for the background processor
            // is 30 seconds, even for StartOnLoad = true.
            // Even if StartOnLoad is not true if the timer interval
            // is less than 30 seconds, we will still delay the first execution.
            queueTimer.Interval = TimeSpan.FromSeconds(30).TotalMilliseconds;
        }

        queueTimer.Start();
    }

    private async Task GetTasksToProcess()
    {
        var now = DateTime.UtcNow;
        var da = _serviceProvider.GetRequiredService<IDataAccess>();

        if (!_loadedPlugins) {
            _loadedPlugins = true;
            var allPlugins = da.GetPlugins();
            if (allPlugins.Any(x => x.Type.ToLower() == "backgroundprocess")) {
                _availablePlugins = allPlugins.Where(x => x.Type.ToLower() == "backgroundprocess").ToList();
            }
        }

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
            if (!_processingAppTasks.Contains(tenant.TenantId)) {
                _processingAppTasks.Add(tenant.TenantId);

                var appTasksForTenant = await da.ProcessBackgroundTasksApp(tenant.TenantId, _iterations);
                ProcessTasksMessages(appTasksForTenant);

                _processingAppTasks = _processingAppTasks.Where(x => x != tenant.TenantId).ToList();
            }
        }

        if (_iterations == 1 || _iterations % 100 == 0) {
            // Delete any stale cached compiled Blazor plugins.
            await da.DeleteOldBlazorCachedPluginBinaries();
        }

        // Process any app-specific tasks for specific tenants.
        if (!_processingAppTasks.Contains(Guid.Empty)) {
            _processingAppTasks.Add(Guid.Empty);

            var appTasks = await da.ProcessBackgroundTasksApp(Guid.Empty, _iterations);
            ProcessTasksMessages(appTasks);

            _processingAppTasks = _processingAppTasks.Where(x => x != Guid.Empty).ToList();
        }

        // If there are any plugins that need to be processed, add them to the queue.
        bool startTimer = false;
        foreach (var plugin in _availablePlugins) {
            if (!_processingPlugins.Contains(plugin.Id)) {
                _plugins.Add(plugin);
                startTimer = true;
            }
        }

        if (startTimer) {
            processorTimer.Start();
        }

        // Reset the queue timer interval to the configured processing interval.
        queueTimer.Interval = TimeSpan.FromSeconds(_processingIntervalSeconds).TotalMilliseconds;
        
        if (!queueTimer.Enabled) {
            queueTimer.Start();
        }
    }

    private async void ProcessTasks(Object? source, ElapsedEventArgs e)
    {
        // Get any plugins that need to be processed that are not already processing.
        var pluginsToProcess = _plugins.Where(x => !_processingPlugins.Contains(x.Id)).ToList();

        if (pluginsToProcess != null && pluginsToProcess.Count > 0) {
            foreach (var plugin in pluginsToProcess) {
                _processingPlugins.Add(plugin.Id);

                var pluginExecuteRequest = new PluginExecuteRequest { 
                    Plugin = plugin,
                    Objects = new object[] { _iterations },
                };

                var da = _serviceProvider.GetRequiredService<IDataAccess>();
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