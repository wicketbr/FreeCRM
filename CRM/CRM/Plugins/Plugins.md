# Plugin Architecture

## Overview

The plugin architecture in the CRM is designed to allow for developers that are using
this CRM project as the base for a new solution and they want to be able to add
extendibility to the CRM for their end-users without having to give them access
to the entire code solution. Plugins can be developed by to consumed however you
wish inside your application. On application startup and plugins in the /Plugins
folder are loaded into a DI container and are available for use in the application.
The "auth" type plugins are used exlusively for authentication and will be filtered
out of the interface when showing available plugins to the user.

As an example, say you are building out a reports interface. You could create a 
"report" type of plugin and your convention would be that you will pass the current
record, etc., to that plugin. The plugin "Execute" method would receive the default
objects that are passed (more on that below) and then you could pass other objects
to your code, such as the record that you want to format. The plugin code could then
do the work of rendering your report and that report would be one of the objects
returned by your custom code. This would allow you to create some built-in reports
in your code, and then you code could also present the plugins of type "report" to
to users to select when printing a report. That is just one idea, but the possibilities
are really endless. Since the plugin architecture is just a method for executing dynamic
C# code with a standard method of how they will return objects, they can be used
all throughout your solution to provide customization to your end users. Many examples
will be included in this base solution as I continue to built out the CRM.

Plugin files are placed in the Plugins folder at the root of the application.
A plugin file can have either a .cs or .plugin extension.
If a matching file with a .assemblies extension is found, that file will be used
to load any necessary assemblies that are not already loaded as part of the CRM
function to execute dynamic C# code.

Plugins with the .cs extension are useful for files that you wish to develop
inside of your solution and that reference libraries and namespaces that are
available in your solution.
See the HelloWorld.plugin and HelloWorld.assemblies files for an example.
The .assemblies file references the HelloWorld.dll file that was built in
another project. The contents of that file are:

    namespace Hello {
        public static class World {
            public static string SayHello() => "Hello, World!";
        }
    }

Plugins with the .plugin extension are useful for files that you wish to use
that would not compile as part of your solution, perhaps due to referencing
an assembly that is not available in your solution (such as the HelloWorld example.)
These files cannot be in the solution with a .cs extension, as they would
cause build errors. However, creating a plain text file with a .plugin extension
allows for the code to be loaded during startup without causing build errors.

Plugins could be developed directly inside the solution in Visual Studio, or
in a tool such as the great developer tool LINQPad (https://www.linqpad.net/).
In LINQPad you would just add references to the DLL assemblies from the CRM project so
you can get autocomplete and intellisense. You can then write your plugin code
as a .cs file and drop it in the Plugins folder. If you build the solution
and check the CRM\bin\Debug\net8.0 folder you will find the DLL files that you 
need to reference to build against the DataAccess library, the DataObjects, etc.
The main files needed would be: CRM.DataAccess.dll, 
CRM.DataObjects.dll, and CRM.dll. You may also want to reference the
CRM.EFModels.dll file and other libraries if you wish to use methods and
properties from those libraries.

## Plugin Requirements

Plugins must implement a public function named Properties that returns
a dictionary of string keys and object values. The main properties that should be
loaded are:

    public Dictionary<string, object> Properties() =>
        new Dictionary<string, object> {
            { "Id", new Guid("00000000-0000-0000-0000-000000000000") },
            { "Author", "Brad Wickett" },
            { "ContainsSensitiveData", false },
            { "Description", "You can write a really long description here that can be shown to users in the application." },
            { "Name", "Plugin Name" },
            { "SortOrder", 0 },
            { "Type", "Example" },
            { "Version", "1.0.0" }
        };

The Id must be a unique Guid for every plugin. Make sure to generate a new Guid
if you are copying an existing plugin. The SortOrder property is optional and
will default to 0 if not specified. Plugins will sort by SortOrder, then by
Name, to determine the order in which they will be executed if more than one
plugin is going to be executed for a particular type of action.

The optional Prompts property can pass a List&lg;PluginPrompt&gt; objects that will be
used to determine how to prompt for input in the interface. The input type built
in the interface will be determined from the Type enum value. For "auth" plugins you must
have a Username and Password prompt. The Username prompt should have the Type
of PluginPromptType.Text and the Password prompt should have the Type of PluginPromptType.Password.
or PluginPromptType.Text. These will be considered required for "auth" plugins
even if you don't mark them as required, as the "auth" plugin convention requires
both a username and password value to be passed to the PluginAuthentication.cshtml page.

A Blazor component named PluginPrompts can be used in your application to collect the
information from the prompts. Required fields will include the asterisk (*) after the
prompt name to indicate that the field is required. If you set the 
HighlightMissingRequiredFields parameter to True
then any prompts marked as Required will be highlighted with the .m-r class if they are not filled in.
Since the plugins are part of the DI framework, the control will not update the values for the
plugin directly. Instead, see the PluginTesting.razor file for the example of how to keep track
of the values as they change using the OnValuesChange Delegate. You can also add a reference
to the Blazor component so you can access the MissingRequiredValues property to get a
List&lg;string&gt; of the names of the prompts that are required but not filled in.
Each prompt will be rendered in its own div. You can pass in Class parameter to be added
to the div to control the layout. When plugins are executed a copy of the plugin is made
and then the prompt values are updated in the Values property of the plugin object.
This is to prevent the main object that is part of the DI framework from being updated.

For prompt button types you must specifiy a callback handler for button click events.
That callback handler needs to accept the PluginExecuteResult object. See the PluginTesting.razor page
for an example of how this can be done. Also, see the Example1.cs plugin for an example
of how to customize the button text, class, and icon using the Options property.

## Execution

Plugins will call their main Invoker method which will receive some default objects.
Any included non-auth plugins will use the default "Execute" invoker function.
By convention, the array of objects passed to the invoker function for standard
plugins are as follows:

- The DataAccess Library
- The Plugin object
- The User object for the current user (might be null)
- Any additional objects that you have passed from your own code

An example of this is:

    public async Task<(bool Result, List<string>? Messages, IEnumerable<object>? Objects)> Execute(
        DataAccess da, 
        Plugins.Plugin plugin,
        DataObjects.User? currentUser
    )
    {
        var messages = new List<string>();
        messages.Add("Plugin: " + plugin.Name);

        object[] output = new object[] { "This is an object returned from the plugin." };

        return (Result: true, Messages: messages, Objects: output);
    }

Methods can be async on non-async methods.

## Restricting to Specific Tenants

You can limit which plugins will show for which tenants by adding a property named
LimitToTenants which is a List&lt;Guid&gt; of the TenantIds that the plugin should
be shown for. If the property is not present, the plugin will be available for all tenants.
This will allow for you to include custom code solutions for specific tenants.

## Built-In Plugin Conventions

### Interfaces

The built-in plugins use interfaces to help follow the implementation standard.
For standard plugins, the interface is IPlugin. For auth plugins, the interface is IAuthPlugin.
Both interfaces inherit from the IPluginBase interface. The IPluginBase interface
defines the requirement for the Properties method.
The IPlugin interface defines the requirement for the Execute method.
The IPluginAuth interface defines the requirement for the Login and Logout methods.
All interface methods expect async functions. If you wish to use non-async functions
in your plugins you can create them without inheriting from the interfaces, as they
are not required. They are just there to help with implementation.

### Auth

Plugins with a type of "auth" will be used for login only and won't be shown
anywhere else in the interface except as options on the login page.
These plugins must have 2 functions "Login" and "Logout". 
These will only be executed at the server.

The Login and Logout functions will both be passed the following objects in this order:

- the DataAccess library
- the plugin being executed
- a URL (for Login this is the current page, for Logout this is the base URL)
- the TenantId (Guid)
- the current HttpContext

Code Example:

    public async Task<(bool Result, List<string>? Messages, IEnumerable<object>? Objects)> Login(
        DataAccess da,
        Plugins.Plugin plugin,
        string url,
        Guid tenantId,
        Microsoft.AspNetCore.Http.HttpContext httpContext
    ) {
        var messages = new List<string>();
        bool result = false;
        
        object? user = null;
        
        // Process your login here and return a result.
        // Assuming you authenticated your user, update the user object.
        user = new {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@local",
            Username = "john.doe",
            EmployeeId = "12345",
            DepartmentName = "IT",
            Title = "Developer"
        };

        return (Result: result, Messages: messages, Objects: [user]);
    }

The Login function should return an object shaped like the DataObjects.User object
with a minimum of the .Email property filled in.

If the tenant is configured to allow accounts to login without a pre-existing
user account then an account will be created after a valid login. Otherwise,
the login will be denied. If you are going to be creating new accounts from
logins for users that don't already exist in your application, then you should
also return the .FirstName and .LastName properties in the User object along with the
.Email property. It would also be a good idea to populate the .Username, 
.EmployeeId, and .DepartmentName properties if you are going to be using those.

### Examples

Several examples are included in the Plugins folder. There are examples
using both the .cs and .plugin extensions. There is also an example of
using external DLL files in HelloWorld example.