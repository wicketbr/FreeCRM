# FreeCRM 

An open-source CRM solution built in C# Blazor WebAssembly using .NET 10. You can use this project as-is,
or customize it to fit your needs. Or, just grab code that you need to use in your project.

## Step 1 - Rename the Solution

If you want to rename things, you can use the "Rename FreeCRM.exe" console application.
This will rename the project files, create new GUIDs for each project in the solution,
and rename namespaces to match your project name.

The rename tool now supports adding the rename value as a command-line argument. For example:

`"Rename FreeCRM.exe" MyNewProjectName`

## Step 2 - Remove Unneeded Modules

If you want to remove one or more of the optional components from the application
(Appointments, EmailTemplates, Invoices, Locations, Payments, Services, or Tags)
you can use the "Remove Modules from FreeCRM.exe" console application.
This utility may still leave remnants of the removed modules in the code.
If you find any items that are not removed that you believe should be,
please open an issue on GitHub. Please include the name of the file and the line number
where the item is located.

The removal tool now supports two command-line options. They are:

`remove:Module1,Module2,etc.`

This removes any named modules.

`keep:Module1,Module2,etc.`

This removes any modules not named. For example, to remove all optional modules except
the Tags module, you would use:

`keep:Tags`

You can also remove all optional modules without having to list them all by using:

`remove:all`

Do not use any spaces in the command line options. The names of the modules that can be
kept or removed are:

- Appointments
- EmailTemplates
- Invoices
- Locations
- Payments
- Services
- Tags

## Details

### Plugins

FreeCRM supports a plugin architecture that allows you to extend the functionality
of the software. See the example files in the Plugins folder for examples of how
to use the various plugin types. You can easily add support for more plugin types
to support your custom application changes by following the various examples
already built into the application. The types built in out of the box are:
"Auth", "BackgroundProcess", "Example", and "UserUpdate".

Plugins can have a .cs file extension and be included in your project as source code,
or they can have the .plugin extension for files that may provide build conflicts
with your solution. For example, the HelloWorld sample plugin uses an external dll
file that would cause build issues if it were included as a .cs file.
See the .assemblies file used with that plugin to see how external files can be included.
These external references can be paths to files or can be statements that evaluate
to the location of the files at runtime, like:

```
.\HelloWorld\HelloWorld.dll
typeof(SomeNameSpace.SomeProperty).Assembly.Location
```

### Background Service

FreeCRM includes a background service that can be started when your application starts
to perform periodic tasks. This is controlled by the settings in the BackgroundService
section of appsettings.json. By default, the background service is enabled and configured
to run every 60 seconds.

For tenants that are configured to mark records as deleted instead of deleting them immediately,
this background service will take care of permanently deleting those records after the
specified retention period has passed.

If you want detailed logging for the background service then set the LogFilePath with
the path to the folder where you want the logs to be stored. The application will need
to be running under credentials that have write access to that folder.

The StartOnLoad flag indicates if the tasks to be run should be started immediately,
or if they should wait until the first timer interval to start. For example, if this
is set to false and the IntervalSeconds is set to 60, then the first time the tasks
will run is 60 seconds after the application starts. If you have something that needs
to be run as soon as the application starts, then set this to true.

There are two methods to tie into the background service to run your own tasks.
First, you can modify the ProcessBackgroundTasksApp method in the DataAccess.App.cs file
to run methods in there. The second method is to build plugins using the new
"BackgroundProcess" plugin type. See the ExampleBackgroundProcess.cs file in the
Plugins folder for an example of how to build a background process plugin.
The example plugin will simple log a message to the console each time it runs,
and to the logging file if you have one configured.

If you are running in a load balanced environment with multiple instances of the application
and you only want the background service to run on a specific instance, you can set the
LoadBalancingFilter value in the BackgroundService section of appsettings.json to a value
that must be matched in the name of the current System.Environment.MachineName.

### IIS Configuration for the Background Service

If you are hosting on IIS and want to ensure that the background service is always running,
set the Application Pool Start Mode to "AlwaysRunning" and set the Preload Enabled flag
for the application settings to true. The Preload Enabled flag is only available
if you have installed the Application Initialization feature for IIS.