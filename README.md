# FreeCRM 

An open-source CRM solution built in C# Blazor WebAssembly using .NET 9. You can use this project as-is,
or customize it to fit your needs. Or, just grab code that you need to use in your project.

If you want to rename things, you can use the "Rename FreeCRM.exe" console application.
This will rename the project files, create new GUIDs for each project in the solution,
and rename namespaces to match your project name.

The rename tool now supports adding the rename value as a command-line argument. For example:

`"Rename FreeCRM.exe" MyNewProjectName`

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
