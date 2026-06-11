# FreeCRM 

An open-source CRM solution built in C# Blazor WebAssembly using .NET 10. You can use this project as-is,
or customize it to fit your needs. Or, just grab code that you need to use in your project.

## Utilities

The previous utilities "Remove Modules from FreeCRM.exe", "Rename FreeCRM.exe", and
"Upgrade FreeCRM.exe" have now been combined into a new single "Util.exe" utility,
referred to below as Util. This Util console application has an intuitive main menu
with options, as well as a built-in help to show all available commands.
Command in the utility are not case-sensitive.

## Step 1 - Rename the Solution

Using rename with the Util will rename the project files,
create new GUIDs for each project in the solution,
and rename namespaces to match your project name.

The tool supports adding the rename value as a command-line argument. For example:

`Util --rename:MyNewProjectName`

## Step 2 - Remove Unwanted Modules

If you want to remove one or more of the optional components from the application
you can use the Util to do so.
This utility may still leave remnants of the removed modules in the code.
If you find any items that are not removed that you believe should be,
please open an issue on GitHub. Please include the name of the file and the line number
where the item is located.

The removal tool supports two command-line options. They are:

`Util --remove:Module1,Module2,etc.`

This removes any named modules.

`Util --keep:Module1,Module2,etc.`

This removes any modules not named. For example, to remove all optional modules except
the Tags module, you would use:

`Util --keep:Tags`

You can also remove all optional modules without having to list them all by using:

`Util --remove:all`

Do not use any spaces in the command line options. The names of the modules that can be
kept or removed are:

- About
- Appointments
- EmailTemplates
- Invoices
- Locations
- Logo
- Payments
- SamplePages
- SamplePlugins
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
or they can have the .plugin extension for files that may have build conflicts
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

## Upgrade

If you are running an older version of an application based on the FreeCRM
framework and you have already migrated to use .app. files for all of your
app-specific code, then you can use the Util console
application to upgrade your existing application. To do so download a fresh
copy of the FreeCRM files, run the "rename" option in the utility to rename
the application using the exact same namespace as your existing application.
Then, use the "remove" option in the utility to remove any modules
that you won't be using in your application. Finally, run the
"upgrade" option in the utility which will attempt to migrate your existing
code into the new version.

This tool can take a single command-line argument of the path to your
existing application. For example:

`Util --upgrade:"C:\MyExistingApp"`

There are edge cases that cannot be updated with this tool, such as having
additional project in your solution. The tool will copy those projects,
but any references in other projects must be added manually.

The tool will produce a report that will help with any additional steps
that will be required for the migration.

The commands in the Util console application can be chained together to perform
all steps in one call. For example, to rename your app, remove some modules, and
perform the upgrade you could use:

`--Rename:MyNewProjectName --Keep:about,tags --Upgrade:"C:\Path\To\Current\Version"`

## Mac OS

A separate UtilMacOS file is available for use on Macs with Apple silicone.
To make this work you must open a console into the folder where the file
exists and issue the following command:

```chmod +x UtilMacOS```

Then, to run the application issue the following command:

```./UtilMacOS```

Both the Util.exe and UtilMacOS applications are framework-dependent, so
you must have .NET 10 installed to run them.