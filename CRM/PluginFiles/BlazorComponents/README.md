# Blazor Components

## Info

Blazor plugins are a new feature to the CRM framework built with a combination of the code from 
TryMudBlazor (https://github.com/MudBlazor/TryMudBlazor)
and SpawnDev.BlazorJS.CodeRunner (https://github.com/LostBeard/SpawnDev.BlazorJS.CodeRunner).

## Getting Started

Blazor plugins must live in the PluginFiles/BlazorComponents folder and
the filename must must end with either .blazor or .razor and must follow
a naming convention used to determine the type of component and where in
the application it will be used.

If the component is a button type meant to be shown on a button menu area on a given page,
then the filename must start with "button_", followed by the name of the page it will be used on,
followed by the unique name of the component. For example:

```
Button_Index_MyCustomButton.blazor
Button_EditUser_AnotherButton.blazor
```

If the item is not a button, then the filename must start with the area on the page
that the component will be used, followed by the name of the page, followed by the unique
name of the component. For example:

```
Top_Index_MyTopComponent.blazor
Bottom_EditUser_MyFooterComponent.blazor
```
For pages that have a tabbed interface, the top and bottom of the tab can also be
targeted by using the prefix of the tab name. For example, on the settings page:

```
TabGeneralTop_Settings_MyGeneralTopComponent.blazor
TabThemeBottom_Settings_MyThemeBottomComponent.blazor
```

There are a few other conventions to be aware of when building Blazor component plugins.

First, a standard parameter array will be passed to the component when it is rendered.
One property that will always be included is the "PluginName" property, which will contain
the name of the plugin, so you must include a parameter for PluginName.

If the plugin receives any data from the calling page, the name of the data parameter
for the object will be Value. For example, if this is a plugin on a user edit page, the
DataObjects.User object would be passed to the component as a parameter named Value.
When passing a Value object you must also include an EventCallback of the same type
as the object being passed, named "ValueChanged". See the example for how this is implemented.

Another standard convention is that if you wish to be notified
when the component has been initialized, you can implement the parameter named
"OnInitializedCallback" which is an ```EventCallback<string>``` object method.
This event callback will receive the string PluginName as its only parameter.
See the example component for how this can be implemented.

## Optional Configuration Files

An optional configuration file with a .json extension and the same filename as the component
(without the .blazor or .razor extension) can be included alongside the component file.
This json file can contain the parameters Id (a unique Guid), Author, Description,
LimitToTenants (an array of Guids), SortOrder, and Version. See the example json
file for more information.

If no configuration file is provided the SortOrder will be 0 for the
component and the component will be available to all tenants.
When components are rendered in built-in sections they are sorted first
by SortOrder then by Name. So, if you want to control the sorting of your
components you should include a json configuration file with the SortOrder
property specified, or make sure the names will sort in the order you want.