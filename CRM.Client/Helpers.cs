using BlazorBootstrap;
using Blazored.LocalStorage;
// {{ModuleItemStart:EmailTemplates}}
using CRM.Client.Pages.Settings.Email;
// {{ModuleItemEnd:EmailTemplates}}
// {{ModuleItemStart:Locations}}
using CRM.Client.Pages.Settings.Locations;
// {{ModuleItemEnd:Locations}}
// {{ModuleItemStart:Services}}
using CRM.Client.Pages.Settings.Services;
// {{ModuleItemEnd:Services}}
// {{ModuleItemStart:Tags}}
using CRM.Client.Pages.Settings.Tags;
// {{ModuleItemEnd:Tags}}
using CRM.Client.Pages.Settings.Users;
using CRM.Client.Shared;
using Humanizer;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Primitives;
using Microsoft.JSInterop;
using MudBlazor.Utilities;
using Plugins;
using Radzen;
using Radzen.Blazor;
using Radzen.Blazor.Markdown;
using Radzen.Blazor.Rendering;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Net.Http.Json;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using static MudBlazor.Colors;

namespace CRM.Client;

public static partial class Helpers
{
    private static Radzen.DialogService DialogService = null!;
    private static HttpClient Http = null!;
    private static bool _initialized = false;
    private static IJSRuntime jsRuntime = null!;
    private static ILocalStorageService LocalStorage = null!;
    private static BlazorDataModel Model = null!;
    private static TooltipService Tooltips = null!;
    private static NavigationManager NavManager = null!;

    private static bool _savingUserPreferences = false;
    private static bool _switchingTenant = false;
    private static bool _validatingUrl = false;

    /// <summary>
    /// Initializes the Helpers static class library by providing the required objects to the library.
    /// </summary>
    /// <param name="jSRuntime">A reference to the IJSRuntime interface.</param>
    /// <param name="model">A reference to the BlazorDataModel.</param>
    /// <param name="httpClient">A reference to the HttpClient.</param>
    /// <param name="localStorage">A reference to the ILocalStorageService interface.</param>
    /// <param name="dialogService">A reference to the Radzen DialogService.</param>
    /// <param name="tooltipService">A reference to the Radzen TooltipService.</param>
    /// <param name="navigationManager">A reference to the NavigationManager interface.</param>
    public static void Init(
        IJSRuntime jSRuntime,
        BlazorDataModel model,
        HttpClient httpClient,
        ILocalStorageService localStorage,
        Radzen.DialogService dialogService,
        Radzen.TooltipService tooltipService,
        NavigationManager navigationManager
    )
    {
        DialogService = dialogService;
        Http = httpClient;
        jsRuntime = jSRuntime;
        LocalStorage = localStorage;
        Model = model;
        Tooltips = tooltipService;
        NavManager = navigationManager;

        _initialized = true;
    }

    /// <summary>
    /// Checks if the file type of the given filename is allowed.
    /// </summary>
    /// <param name="filename">The name of the file to check.</param>
    /// <returns>True if the file type is allowed, false otherwise.</returns>
    public static bool AllowedFileType(string filename)
    {
        bool output = false;

        if (!String.IsNullOrEmpty(filename)) {
            // By default, all file types are allowed if not allowed file types are defined.
            if (Model.Tenant.TenantSettings.AllowedFileTypes.Count() == 0) {
                return true;
            }

            string extension = System.IO.Path.GetExtension(filename).ToLower();
            if (extension.StartsWith(".")) {
                extension = extension.Substring(1);
            }

            if (!String.IsNullOrEmpty(extension)) {
                if (Model.Tenant.TenantSettings.AllowedFileTypes != null && Model.Tenant.TenantSettings.AllowedFileTypes.Any()) {
                    if (Model.Tenant.TenantSettings.AllowedFileTypes.Contains(extension, StringComparer.CurrentCultureIgnoreCase)) {
                        output = true;
                    }
                }
            }
        }

        return output;
    }

    /// <summary>
    /// Determines if the file is an allowed file type and an image file.
    /// </summary>
    /// <param name="filename">The name of th file.</param>
    /// <returns>True if the file is an allowed file type and an image file, otherwise returns false.</returns>
    public static bool AllowedFileTypeImage(string filename)
    {
        bool output = AllowedFileType(filename);

        if (output) {
            string extension = System.IO.Path.GetExtension(filename).ToLower();
            if (extension.StartsWith(".")) {
                extension = extension.Substring(1);
            }

            switch (extension.ToLower()) {
                case "gif":
                case "jpg":
                case "png":
                case "svg":
                    // These are valid image types, so keep the true response.
                    break;

                default:
                    output = false;
                    break;
            }
        }

        return output;
    }

    /// <summary>
    /// Gets the list of allowed file types from the TenantSettings.
    /// </summary>
    /// <returns>A List of strings.</returns>
    public static List<string> AllowedFileTypes {
        get {
            return Model.Tenant.TenantSettings.AllowedFileTypes;
        }
    }

    /// <summary>
    /// The BaseUri from the NavigationManager
    /// </summary>
    public static string BaseUri {
        get {
            return NavManager.BaseUri;
        }
    }

    /// <summary>
    /// Returns a checkbox icon based on a boolean value.
    /// </summary>
    /// <param name="value">The boolean value.</param>
    /// <param name="iconChecked">An optional icon to use instead of the default for the True case.</param>
    /// <param name="iconUnchecked">An optional icon to use instead of the default for the False case.</param>
    /// <returns>The HTML of the icon.</returns>
    public static string BooleanToCheckboxIcons(bool? value, string? iconChecked = "", string? iconUnchecked = "")
    {
        string output = "";

        if (value.HasValue && (bool)value == true) {
            if (!String.IsNullOrWhiteSpace(iconChecked)) {
                // First, see if this is an Icon
                output = Icon(iconChecked, true);
                if (String.IsNullOrWhiteSpace(output)) {
                    output = iconChecked;
                }
            } else {
                output = Icon("Checked", true);
            }
        } else {
            if (!String.IsNullOrWhiteSpace(iconUnchecked)) {
                // First, see if this is an Icon
                output = Icon(iconUnchecked, true);
                if (String.IsNullOrWhiteSpace(output)) {
                    output = iconUnchecked;
                }
            } else {
                output = Icon("Unchecked", true);
            }
        }

        if (output != "") {
            if (!output.Contains("<")) {
                output = "<i class=\"" + output + "\"></i>";
            }
        }

        return output;
    }

    /// <summary>
    /// Returns an icon if the boolean value is true.
    /// </summary>
    /// <param name="value">The boolean value.</param>
    /// <param name="icon">An optional icon to use instead of the default for the True case.</param>
    /// <returns>The HTML of the icon.</returns>
    public static string BooleanToIcon(bool? value, string? icon = "")
    {
        string output = "";

        if (value.HasValue && (bool)value == true) {
            if (!String.IsNullOrWhiteSpace(icon)) {
                // First, see if this is an Icon
                output = Icon(icon, true);
                if (String.IsNullOrWhiteSpace(output)) {
                    output = icon;
                }
            } else {
                output = Icon("Checked", true);
            }

            if (output != "") {
                if (!output.Contains("<")) {
                    output = "<i class=\"" + output + "\"></i>";
                }
            }
        }

        return output;
    }

    /// <summary>
    /// Builds a full URL to an application page, using the tenant code if required.
    /// </summary>
    /// <param name="subUrl">The optional sub-page, otherwise, the URL will be to the root.</param>
    /// <returns>The full URL to the application page.</returns>
    public static string BuildUrl(string? subUrl = "")
    {
        return Model.ApplicationUrlFull + subUrl;
    }

    /// <summary>
    /// Converts bytes to a user-friendly file size.
    /// </summary>
    /// <param name="bytes">The byte length value.</param>
    /// <param name="labels">An optional collection of labels to use in place of the defaults.</param>
    /// <returns>A user-friendly file size.</returns>
    public static string BytesToFileSizeLabel(long? bytes, List<string>? labels = null)
    {
        string output = "";

        if (labels == null || labels.Count() < 4) {
            labels = new List<string> { "b", "kb", "m", "gb" };
        }

        if (bytes > 0) {
            if (bytes < 1024) {
                output = ((int)bytes).ToString() + labels[0];
            } else if (bytes < (1024 * 1024)) {
                output = ((int)(bytes / 1024)).ToString() + labels[1];
            } else if (bytes < (1024 * 1024 * 1024)) {
                output = ((int)(bytes / 1024 / 1024)).ToString() + labels[2];
            } else if (bytes < (1099511627776)) {
                output = ((int)(bytes / 1024 / 1024 / 1024)).ToString() + labels[3];
            }
        }

        return output;
    }

    /// <summary>
    /// Cleans HTML
    /// </summary>
    /// <param name="html">The text that might contain HTML.</param>
    /// <returns>The cleaned HTML.</returns>
    public static string CleanHtml(string? html)
    {
        string output = "";

        if (!String.IsNullOrWhiteSpace(html)) {
            output = html;

            if (!ContainsHtml(output)) {
                return "<pre class=\"non-html\">" + html + "</pre>";
            }

            int bodyStart = output.ToLower().IndexOf("<body");
            int bodyEnd = output.ToLower().IndexOf("</body");

            if (bodyStart > -1 && bodyEnd > bodyStart) {
                // We have body tags, so just return what is between those tags
                bodyStart = output.IndexOf(">", bodyStart) + 1;
                output = output.Substring(bodyStart, bodyEnd - bodyStart);
            }

            // See if we have any style tags and remove them
            int safety = 0;
            int styleStart = output.ToLower().IndexOf("<style");
            while (styleStart > -1) {
                safety++;
                if (safety > 100) {
                    break;
                }

                int styleEnd = output.ToLower().IndexOf("</style", styleStart);
                if (styleEnd > styleStart) {
                    output = output.Substring(0, styleStart) + output.Substring(styleEnd + 8);
                }
                styleStart = output.ToLower().IndexOf("<style");
            }

            output = output.Replace("</div>", "</div>" + Environment.NewLine);
            output = output.Replace("</p>", "</p>" + Environment.NewLine);

            List<string> validLines = new List<string>();
            var lines = output.Split(Environment.NewLine);
            foreach (var line in lines) {
                if (!String.IsNullOrWhiteSpace(line) && line != "&nbsp;" && line != "<" && line.ToLower() != "<p class=\"msonormal\">&nbsp;</p>") {
                    validLines.Add(line);
                }
            }

            if (validLines.Any()) {
                bool stopped = false;
                StringBuilder updatedOutput = new StringBuilder();

                for (int x = 0; x < validLines.Count(); x++) {
                    var line = validLines[x];
                    var lineLower = line.ToLower();
                    string nextLine = "";
                    if (x < validLines.Count() - 1) {
                        nextLine = validLines[x + 1];
                    }

                    if (!stopped) {
                        if (lineLower.Contains("<b>from:</b>") || lineLower.Contains("<b>sent:</b>")) {
                            stopped = true;
                        } else if (lineLower.Contains(">on") && lineLower.Contains("wrote:<") && nextLine.ToLower().Contains("{{help-desk-begin-message}}")) {
                            // Exclude lines the start with "<p>on" and end with "wrote:</p>" if the next line is {{help-desk-begin-message}}
                            // Skip this line, as it is the precursor to an included reply and the next line indicates the start of the reply.
                        } else {
                            updatedOutput.AppendLine(line);
                        }
                    }
                }

                output = updatedOutput.ToString();

                output = output.Replace("{{help-desk-begin-message}}", "", StringComparison.InvariantCultureIgnoreCase);

                output = output.Replace("<pre>", "<pre class=\"non-html\">");
            }

            output = FixLinksInHtml(output);
        }

        return output;
    }

    /// <summary>
    /// Clears the value for a local storage item.
    /// </summary>
    /// <param name="key">The key of the item.</param>
    public static async Task ClearLocalStorageItem(string key)
    {
        if (LocalStorage != null) {
            await LocalStorage.RemoveItemAsync(key);
        }
    }

    /// <summary>
    /// The default text to use for "Cancel" in the confirmation button control.
    /// </summary>
    public static string ConfirmButtonTextCancel {
        get {
            return Helpers.Text("Cancel");
        }
    }

    /// <summary>
    /// The default text to use for "Confirm Delete" in the confirmation button control.
    /// </summary>
    public static string ConfirmButtonTextConfirmDelete {
        get {
            return Helpers.Text("ConfirmDelete");
        }
    }

    /// <summary>
    /// The default text to use for "Delete" in the confirmation button control.
    /// </summary>
    public static string ConfirmButtonTextDelete {
        get {
            return Helpers.Text("Delete");
        }
    }

    /// <summary>
    /// The default text to use for "Delete All" in the confirmation button control.
    /// </summary>
    public static string ConfirmButtonTextDeleteAll {
        get {
            return Helpers.Text("DeleteAll");
        }
    }

    /// <summary>
    /// Writes out objects to the console using jsInterop.
    /// </summary>
    /// <param name="objects">Any objects to write out to the console.</param>
    public static async Task ConsoleLog(params object[] objects)
    {
        if (jsRuntime != null) {
            foreach (var obj in objects) {
                await jsRuntime.InvokeVoidAsync("ConsoleLog", GetObjectType(obj), obj);
            }
        }
    }

    /// <summary>
    /// Writes out a message to the console using jsInterop.
    /// </summary>
    /// <param name="message">The message to display</param>
    public static async Task ConsoleLog(string message)
    {
        if (jsRuntime != null) {
            await jsRuntime.InvokeVoidAsync("ConsoleLog", message);
        }
    }

    /// <summary>
    /// Writes out a message and optional objects to the console using jsInterop.
    /// </summary>
    /// <param name="message">The message to display</param>
    /// <param name="objects">An optional collection of objects.</param>
    public static async Task ConsoleLog(string message, params object[] objects)
    {
        if (jsRuntime != null) {
            if (objects != null) {
                if (objects.Length == 1) {
                    var obj = objects[0];
                    await jsRuntime.InvokeVoidAsync("ConsoleLog", message + ": " + GetObjectType(obj), obj);
                } else {
                    int index = -1;
                    foreach (var obj in objects) {
                        index++;
                        await jsRuntime.InvokeVoidAsync("ConsoleLog", message + index.ToString() + ": " + GetObjectType(obj), obj);
                    }
                }
            } else {
                await jsRuntime.InvokeVoidAsync("ConsoleLog", message);
            }
        }
    }

    /// <summary>
    /// Indicates if a string contains basic HTML tags.
    /// </summary>
    /// <param name="text">The string to check</param>
    /// <returns>True if the string contains some basic HTML tags.</returns>
    public static bool ContainsHtml(string? text)
    {
        bool output = !String.IsNullOrWhiteSpace(text) && text != System.Web.HttpUtility.HtmlEncode(text);
        return output;
    }

    /// <summary>
    /// Converts C# code to a plugin object.
    /// </summary>
    /// <param name="code">The C# code.</param>
    /// <returns>A nullable Plugin object.</returns>
    public static Plugins.Plugin? ConvertCodeToPlugin(string? code)
    {
        Plugins.Plugin? output = null;

        if (!String.IsNullOrWhiteSpace(code)) {
            string ns = GetPluginNamespace(code);
            string c = GetPluginClass(code);

            if (!String.IsNullOrWhiteSpace(ns) && !String.IsNullOrWhiteSpace(c)) {
                output = new Plugin {
                    Id = Guid.Empty,
                    Author = "From Code",
                    ClassName = c,
                    Code = code,
                    ContainsSensitiveData = false,
                    Description = "",
                    LimitToTenants = new List<Guid>(),
                    Name = "From Code",
                    Namespace = ns,
                    Prompts = new List<PluginPrompt>(),
                    PromptValues = new List<PluginPromptValue>(),
                    PromptValuesOnUpdate = "",
                    Properties = new Dictionary<string, object>(),
                    Type = "Code",
                    Version = "X",
                    AdditionalAssemblies = new List<string>(),
                    Invoker = "Execute",
                    Values = new List<PluginPromptValue>(),
                };
            }
        }

        return output;
    }

    /// <summary>
    /// Reads a cookie using jsInterop.
    /// </summary>
    /// <typeparam name="T">The type of value stored in the cookie.</typeparam>
    /// <param name="name">The name of the cookie item.</param>
    /// <returns>A nullable T object.</returns>
    public static async Task<T> CookieRead<T>(string name)
    {
        var output = await jsRuntime.InvokeAsync<T>("CookieRead", name);
        return output;
    }

    /// <summary>
    /// Writes a cookie using jsInterop.
    /// </summary>
    /// <param name="name">The name of the cookie item.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="days">Optional number of days until the cookie expires (defaults to 14.)</param>
    public static async Task CookieWrite(string name, string value, int days = 14)
    {
        await jsRuntime.InvokeVoidAsync("CookieWrite", name, value, days);
    }

    /// <summary>
    /// Copies the value to the clipboard.
    /// </summary>
    /// <param name="value">The value to copy to the clipboard.</param>
    public static async Task CopyToClipboard(string value)
    {
        if (jsRuntime != null) {
            await jsRuntime.InvokeVoidAsync("CopyToClipboard", value);
        }
    }

    /// <summary>
    /// Converts a string with CSV values to a list of strings.
    /// </summary>
    /// <param name="csv">A string that might contain CSV data.</param>
    /// <returns>A List of Strings</returns>
    public static List<string> CsvToListOfString(string? csv)
    {
        var output = new List<string>();

        if (!String.IsNullOrWhiteSpace(csv)) {
            var items = csv.Split(",").Where(x => !String.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList();
            if (items != null && items.Any()) {
                output = items;
            }
        }

        return output;
    }

    /// <summary>
    /// Gets the Uri as a string from the NavigationManager
    /// </summary>
    public static string CurrentUrl {
        get {
            return NavManager.Uri.ToString();
        }
    }

    /// <summary>
    /// Converts a DateOnly? to a DateTime?
    /// </summary>
    /// <param name="dateOnly">The DateOnly? value</param>
    /// <returns>A DateTime? value</returns>
    public static DateTime? DateOnlyToDateTime(DateOnly? dateOnly)
    {
        DateTime? output = null;

        if (dateOnly.HasValue) {
            output = dateOnly.Value.ToDateTime(TimeOnly.Parse("12:00:00 PM"));
        }

        return output;
    }

    /// <summary>
    /// Converts a DateTime? to a DateOnly?
    /// </summary>
    /// <param name="dateTime">The DateTime? value</param>
    /// <returns>A DateOnly? value</returns>
    public static DateOnly? DateTimeToDateOnly(DateTime? dateTime)
    {
        DateOnly? output = null;

        if (dateTime.HasValue) {
            output = DateOnly.FromDateTime(dateTime.Value);
        }

        return output;
    }

    /// <summary>
    /// Converts a string with a DateTime value to a DateOnly?
    /// </summary>
    /// <param name="dateTime">The string with the DateTime value</param>
    /// <returns>A DateOnly? value</returns>
    public static DateOnly? DateTimeToDateOnly(string? dateTime)
    {
        DateOnly? output = null;

        if (!String.IsNullOrWhiteSpace(dateTime)) {
            // See if this is a valid dateTime
            var dt = DateTime.MinValue;

            try {
                dt = Convert.ToDateTime(dateTime);
            } catch { }

            if (dt != DateTime.MinValue) {
                output = DateOnly.FromDateTime(dt);
            }
        }

        return output;
    }

    /// <summary>
    /// UrlDecodes a given value.
    /// </summary>
    /// <param name="input">The value to decode.</param>
    /// <returns>The decoded value.</returns>
    public static string DecodeValue(string input)
    {
        string output = input;

        if (!String.IsNullOrWhiteSpace(output)) {
            output = output.Replace("_", "%");
            output = System.Web.HttpUtility.UrlDecode(output);
        }

        return output;
    }

    /// <summary>
    /// Gets the default language value for a given phrase.
    /// </summary>
    /// <param name="text">The name of the phrase.</param>
    /// <returns>The default language value.</returns>
    public static string DefaultLanguage(string? text)
    {
        string output = String.Empty;

        if (Model != null) {
            if (!String.IsNullOrWhiteSpace(text)) {
                if (Model.DefaultLanguage.Phrases.Any()) {
                    var phrase = Model.DefaultLanguage.Phrases.FirstOrDefault(x => x.Id != null && x.Id.ToUpper() == text.ToUpper());
                    if (phrase != null) {
                        output = !String.IsNullOrWhiteSpace(phrase.Value)
                            ? phrase.Value
                            : String.Empty;
                    }
                }
            }
        }

        return output;
    }

    /// <summary>
    /// Sets the focus to an element as soon as it becomes visible.
    /// </summary>
    /// <param name="elementId">The id of the HTML element.</param>
    public static async Task DelayedFocus(string elementId)
    {
        if (jsRuntime != null) {
            await jsRuntime.InvokeVoidAsync("DelayedFocus", elementId);
        }
    }

    /// <summary>
    /// Sets the focus and selects all text for an element as soon as it becomes visible.
    /// </summary>
    /// <param name="elementId">The id of the HTML element.</param>
    public static async Task DelayedSelect(string elementId)
    {
        if (jsRuntime != null) {
            await jsRuntime.InvokeVoidAsync("DelayedSelect", elementId);
        }
    }

    /// <summary>
    /// Gets the name for a department group from the unique id.
    /// </summary>
    /// <param name="DepartmentGroupId">The unique id of the department group.</param>
    /// <returns>The name of the department group.</returns>
    public static async Task<string> DepartmentGroupName(string? DepartmentGroupId)
    {
        string output = String.Empty;

        if (!String.IsNullOrWhiteSpace(DepartmentGroupId)) {
            if (!Model.DepartmentGroups.Any()) {
                await LoadDepartmentGroups();
            }

            var dept = Model.DepartmentGroups.FirstOrDefault(x => x.DepartmentGroupId.ToString() == DepartmentGroupId);
            if (dept != null) {
                output += dept.DepartmentGroupName;
            }
        }

        return output;
    }

    /// <summary>
    /// Gets the name for a department group from the unique id.
    /// </summary>
    /// <param name="DepartmentGroupId">The unique id of the department group.</param>
    /// <returns>The name of the department group.</returns>
    public static async Task<string> DepartmentGroupName(Guid? DepartmentGroupId)
    {
        string output = String.Empty;

        if (DepartmentGroupId.HasValue) {
            if (!Model.DepartmentGroups.Any()) {
                await LoadDepartmentGroups();
            }

            var dept = Model.DepartmentGroups.FirstOrDefault(x => x.DepartmentGroupId == DepartmentGroupId);
            if (dept != null) {
                output += dept.DepartmentGroupName;
            }
        }

        return output;
    }

    /// <summary>
    /// Gets the name for a department from the unique id
    /// </summary>
    /// <param name="DepartmentId">The unique id of the department.</param>
    /// <returns>The name of the department.</returns>
    public static async Task<string> DepartmentName(string? DepartmentId)
    {
        string output = String.Empty;

        if (!String.IsNullOrWhiteSpace(DepartmentId)) {
            if (!Model.Departments.Any()) {
                await LoadDepartments();
            }

            var dept = Model.Departments.FirstOrDefault(x => x.DepartmentId.ToString() == DepartmentId);
            if (dept != null) {
                output += dept.DepartmentName;
            }
        }

        return output;
    }

    /// <summary>
    /// Gets the name for a department from the unique id
    /// </summary>
    /// <param name="DepartmentId">The unique id of the department.</param>
    /// <returns>The name of the department.</returns>
    public static async Task<string> DepartmentName(Guid? DepartmentId)
    {
        string output = String.Empty;

        if (DepartmentId.HasValue) {
            if (!Model.Departments.Any()) {
                await LoadDepartments();
            }

            var dept = Model.Departments.FirstOrDefault(x => x.DepartmentId == DepartmentId);
            if (dept != null) {
                output += dept.DepartmentName;
            }
        }

        return output;
    }

    /// <summary>
    /// Gets the list of department names from a list of unique ids.
    /// </summary>
    /// <param name="departmentIds">The list of unique ids of the departments.</param>
    /// <param name="orderAlphabetically">Option to order the output alphabetically (Defaults to True).</param>
    /// <returns>A list of strings containing the department names.</returns>
    public static List<string> DepartmentNamesFromListOfGuids(List<Guid>? departmentIds, bool orderAlphabetically = true)
    {
        var output = new List<string>();

        if (departmentIds != null && departmentIds.Any() && Model.Tenant.Departments != null && Model.Tenant.Departments.Any()) {
            foreach (var deptId in departmentIds) {
                var dept = Model.Tenant.Departments.FirstOrDefault(x => x.DepartmentId == deptId);
                if (dept != null && !String.IsNullOrWhiteSpace(dept.DepartmentName)) {
                    output.Add(dept.DepartmentName);
                }
            }

            if (orderAlphabetically) {
                output = output.OrderBy(x => x).ToList();
            }
        }

        return output;
    }

    /// <summary>
    /// Deserializes an object that was serialized as a JSON Document Object back into the object.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="o">The object to be deserialized.</param>
    /// <returns>A nullable object of type T.</returns>
    public static T? DeserializeJsonDocumentObject<T>(object? o)
    {
        var output = default(T);

        if (o != null) {
            try {
                var json = ((System.Text.Json.JsonElement)o).GetRawText();
                var jsonString = json.ToString();

                output = System.Text.Json.JsonSerializer.Deserialize<T>(jsonString, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            } catch { }
        }

        return output;
    }

    /// <summary>
    /// Deserializes an object that was serialized as JSON.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="SerializedObject">The serialized object as a JSON string.</param>
    /// <returns>A nullable object of type T.</returns>
    public static T? DeserializeObject<T>(string? SerializedObject)
    {
        var output = default(T);

        if (!String.IsNullOrWhiteSpace(SerializedObject)) {
            try {
                var d = System.Text.Json.JsonSerializer.Deserialize<T>(SerializedObject, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (d != null) {
                    output = d;
                }
            } catch { }
        }

        return output;
    }

    /// <summary>
    /// Converts a Dictionary&lt;string, object&gt; object that has been serialized with System.Text.Json back into an actual Dictionary object.
    /// </summary>
    /// <param name="json">The json containing the dictionary.</param>
    /// <returns>A Dictionary of string and object pairs.</returns>
    public static Dictionary<string, object> DictionaryFromJson(string? json)
    {
        var output = new Dictionary<string, object>();

        if (!String.IsNullOrWhiteSpace(json)) {
            var des = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (des != null && des.Count > 0) {
                foreach (var element in des) {
                    var key = element.Key;

                    if (!String.IsNullOrWhiteSpace(key)) {
                        var value = element.Value;
                        var v = value.ToString() + String.Empty;

                        var type = (System.Text.Json.JsonElement)value;

                        try {
                            switch (type.ValueKind) {
                                case System.Text.Json.JsonValueKind.Array:
                                    using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(v))) {
                                        var dataContractJsonSerializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(List<object>));
                                        var msObject = dataContractJsonSerializer.ReadObject(memoryStream);
                                        if (msObject != null) {
                                            var a = (List<object>)msObject;

                                            var listOfTypedObjects = ListOfObjectsToTypedList(a);
                                            if (listOfTypedObjects != null) {
                                                output.Add(key, listOfTypedObjects);
                                            } else {
                                                output.Add(key, a);
                                            }
                                        }
                                    }
                                    break;

                                case System.Text.Json.JsonValueKind.False:
                                    output.Add(key, false);
                                    break;

                                case System.Text.Json.JsonValueKind.Null:
                                    output.Add(key, new());
                                    break;

                                case System.Text.Json.JsonValueKind.Number:
                                    if (v.Contains(".") || v.Contains(",")) {
                                        var d = Convert.ToDecimal(v);
                                        output.Add(key, d);
                                    } else {
                                        var i = Convert.ToInt64(v);
                                        output.Add(key, i);
                                    }

                                    break;

                                case System.Text.Json.JsonValueKind.Object:
                                    if (v == "{\"length\":0}") {
                                        // This is a SecureString object
                                        output.Add(key, new System.Security.SecureString());
                                    } else {
                                        // See if this item is a dictionary.
                                        var dict = DictionaryFromJson(v);
                                        if (dict != null && dict.Count > 0) {
                                            output.Add(key, dict);
                                        } else {
                                            output.Add(key, value);
                                        }
                                    }
                                    break;

                                case System.Text.Json.JsonValueKind.String:
                                    output.Add(key, v);
                                    break;

                                case System.Text.Json.JsonValueKind.True:
                                    output.Add(key, true);
                                    break;

                                case System.Text.Json.JsonValueKind.Undefined:
                                    output.Add(key, value);
                                    break;
                            }
                        } catch (Exception ex) {
                            output.Add(key, "Exception: " + ex.Message);
                        }
                    }
                }
            }
        }

        return output;
    }

    /// <summary>
    /// Converts an object that was serialized as JSON back into a Dictionary&lt;string, object&gt; object.
    /// </summary>
    /// <param name="o">A nullable object.</param>
    /// <returns>A dictionary of string, object values.</returns>
    public static Dictionary<string, object> DictionaryFromJsonObject(object? o)
    {
        var output = new Dictionary<string, object>();

        if (o != null) {
            var json = SerializeObject(o);
            if (!String.IsNullOrWhiteSpace(json)) {
                output = DictionaryFromJson(json);
            }
        }

        return output;
    }

    /// <summary>
    /// Builds a user display name from user data.
    /// </summary>
    /// <param name="LastName">The last name of the user.</param>
    /// <param name="FirstName">The first name of the user.</param>
    /// <param name="Email">The email address of the user.</param>
    /// <param name="DepartmentName">The department name of the user.</param>
    /// <param name="Location">The location of the user.</param>
    /// <returns>A formatted user display name.</returns>
    public static string DisplayNameFromLastAndFirst(string? LastName, string? FirstName, string? Email, string? DepartmentName, string? Location)
    {
        string output = String.Empty;

        if (!String.IsNullOrEmpty(FirstName)) {
            output += FirstName;
        }

        if (!String.IsNullOrEmpty(LastName)) {
            if (!String.IsNullOrEmpty(output)) {
                output += " ";
            }
            output += LastName;
        }

        if (String.IsNullOrEmpty(output) && !String.IsNullOrEmpty(Email)) {
            output = Email;
        }

        if (!String.IsNullOrEmpty(DepartmentName) || !String.IsNullOrEmpty(Location)) {
            if (
                Model.FeatureEnabledDepartments
                // {{ModuleItemStart:Locations}}
                || Model.FeatureEnabledLocation
            // {{ModuleItemEnd:Locations}}
            ) {
                output += " [";
                if (
                    !String.IsNullOrEmpty(DepartmentName)
                    && Model.FeatureEnabledDepartments
                    // {{ModuleItemStart:Locations}}
                    && !String.IsNullOrEmpty(Location)
                    && Model.FeatureEnabledLocation
                // {{ModuleItemEnd:Locations}}
                ) {
                    output += Location + "/" + DepartmentName;
                    // {{ModuleItemStart:Locations}}
                } else if (!String.IsNullOrEmpty(Location) && Model.FeatureEnabledLocation) {
                    output += Location;
                    // {{ModuleItemEnd:Locations}}
                } else if (Model.FeatureEnabledDepartments) {
                    output += DepartmentName;
                }

                output += "]";
            }
        }
        return output;
    }

    /// <summary>
    /// Downloads a file to the browser.
    /// </summary>
    /// <param name="FileId">The unqiue id of the file.</param>
    public static async Task DownloadFile(Guid FileId)
    {
        bool error = false;
        List<string> errors = new List<string>();

        var file = await GetOrPost<DataObjects.FileStorage>("api/Data/GetFileStorage/" + FileId.ToString());
        if (file != null) {
            if (file.ActionResponse.Result) {
                await DownloadFileToBrowser(StringValue(file.FileName), file.Value);
            } else {
                error = true;
                if (file.ActionResponse.Messages.Any()) {
                    errors = file.ActionResponse.Messages;
                }
            }
        } else {
            error = true;
        }

        if (error) {
            await ConsoleLog("Error Downloading File: " + FileId.ToString());
            if (errors.Any()) {
                foreach (var msg in errors) {
                    await ConsoleLog(msg);
                }
            }
        }
    }

    /// <summary>
    /// Performs the JS Interop function to download the contents of a file to the browser.
    /// </summary>
    /// <param name="FileName">The name of the file.</param>
    /// <param name="FileData">The byte array data of the file contents.</param>
    public static async Task DownloadFileToBrowser(string FileName, byte[]? FileData)
    {
        if (FileData != null && FileData.Length > 0) {
            MemoryStream fileStream = new MemoryStream(FileData);

            using (var streamRef = new DotNetStreamReference(stream: fileStream)) {
                await jsRuntime.InvokeVoidAsync("DownloadFileFromStream", FileName, streamRef);
            }
        }
    }

    /// <summary>
    /// Creates a duplicate copy of an object when you need a copy that no longer references the original so updates to the object don't update the original.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="o">The object to be duplicated.</param>
    /// <returns>A nullable copy of the object.</returns>
    public static T? DuplicateObject<T>(object? o)
    {
        T? output = default(T);

        if (o != null) {
            // To make a new copy serialize the object and then deserialize it back to a new object.
            var serialized = System.Text.Json.JsonSerializer.Serialize(o);
            if (!String.IsNullOrEmpty(serialized)) {
                try {
                    var duplicate = System.Text.Json.JsonSerializer.Deserialize<T>(serialized, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (duplicate != null) {
                        output = duplicate;
                    }
                } catch { }
            }
        }

        return output;
    }

    /// <summary>
    /// The total seconds between two DateTime objects.
    /// </summary>
    /// <param name="start">The start DateTime object.</param>
    /// <param name="end">The end DateTime object.</param>
    /// <returns>A double of the total seconds between the times. Will always return a positive value, even if the start and end were reversed.</returns>
    public static double Duration(DateTime? start, DateTime? end)
    {
        double output = 0;

        if (start.HasValue) {
            var startDate = (DateTime)start;
            var endDate = end.HasValue ? (DateTime)end : DateTime.UtcNow;

            output = (endDate - startDate).TotalSeconds;

            if (output < 0) {
                output = 0 - output;
            }
        }

        return output;
    }

    /// <summary>
    /// Converts the duration between a start and end DateTime to a user-readable value.
    /// </summary>
    /// <param name="start">The start DateTime object.</param>
    /// <param name="end">The end DateTime object.</param>
    /// <returns>The user-readable value as HH:MM:SS</returns>
    public static string DurationAsHMS(DateTime? start, DateTime? end)
    {
        string output = String.Empty;

        var seconds = Duration(start, end);

        if (seconds > 0) {
            int totalSeconds = (int)seconds;

            if (totalSeconds > 0) {
                int minutes = 0;
                int hours = 0;

                if (totalSeconds >= 3600) {
                    hours = (totalSeconds / 3600);
                    totalSeconds = totalSeconds - (3600 * hours);
                }

                if (totalSeconds > 60) {
                    minutes = (totalSeconds / 60);
                    totalSeconds = totalSeconds - (60 * minutes);
                }

                if (hours > 0) {
                    output += (hours < 10 ? "0" : "") + hours.ToString() + "h";
                }

                if (minutes > 0) {
                    if (output != "") { output += ":"; }
                    output += (minutes < 10 ? "0" : "") + minutes.ToString() + "m";
                }

                if (totalSeconds > 0) {
                    if (output != "") { output += ":"; }
                    output += (totalSeconds < 10 ? "0" : "") + totalSeconds.ToString() + "s";
                }
            }
        }

        return output;
    }

    /// <summary>
    /// Opens a dialog to edit a user.
    /// </summary>
    /// <param name="UserId">The unique user id.</param>
    /// <param name="OnSaved">The callback handler to handle the save method.</param>
    /// <param name="width">The width of the dialog (defaults to "auto".)</param>
    /// <param name="height">The height of the dialog (defaults to "auto".)</param>
    public static async Task EditUser(Guid UserId, Delegate? OnSaved = null, string width = "auto", string height = "auto")
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>();
        parameters.Add("InDialog", true);
        parameters.Add("userid", UserId.ToString());

        if (OnSaved != null) {
            parameters.Add("OnSaved", OnSaved);
        }

        string title = UserId == Guid.Empty ? Text("AddUser") : Text("EditUser");

        string top = String.Empty;

        if (width == "auto") {
            width = "";
        }

        if (height == "auto") {
            height = "";
        }

        if (String.IsNullOrWhiteSpace(width)) {
            width = "95%";
            top = "80px";
        }

        if (String.IsNullOrWhiteSpace(height)) {
            height = "calc(100vh - 120px)";
            top = "80px";
        }

        await DialogService.OpenAsync<CRM.Client.Pages.Settings.Users.EditUser>(title, parameters, new DialogOptions() {
            AutoFocusFirstElement = false,
            Resizable = false,
            Draggable = false,
            Width = width,
            Height = height,
            Top = top,
        });
    }

    // {{ModuleItemStart:EmailTemplates}}
    /// <summary>
    /// Deserializes the template stored as JSON back to an EmailTemplateDetails object.
    /// </summary>
    /// <param name="Template">The template JSON.</param>
    /// <returns>An EmailTemplateDetails object.</returns>
    public static DataObjects.EmailTemplateDetails EmailTemplateDetails(string? Template)
    {
        DataObjects.EmailTemplateDetails output = new DataObjects.EmailTemplateDetails();

        if (!String.IsNullOrWhiteSpace(Template)) {
            var deserialized = DeserializeObject<DataObjects.EmailTemplateDetails>(Template);
            if (deserialized != null) {
                output = deserialized;
            }
        }

        return output;
    }
    // {{ModuleItemEnd:EmailTemplates}}

    /// <summary>
    /// Executes a plugin and returns the results.
    /// </summary>
    /// <param name="plugin">The Plugin object to be executed.</param>
    /// <param name="objects">Any objects being passed to this plugin code.</param>
    /// <returns>A PluginExecuteResult object.</returns>
    public static async Task<PluginExecuteResult> ExecutePlugin(Plugin plugin, object[]? objects = null)
    {
        var output = new PluginExecuteResult {
            Messages = new List<string>(),
            Objects = new List<object>(),
            Result = false,
        };

        var request = new PluginExecuteRequest {
            Plugin = plugin,
            Objects = objects,
        };

        var result = await Helpers.GetOrPost<PluginExecuteResult>("api/Data/ExecutePlugin", request);
        if (result != null) {
            output.Result = result.Result;

            if (result.Messages != null && result.Messages.Count > 0) {
                output.Messages = result.Messages;
            }

            if (result.Objects != null && result.Objects.Count > 0) {
                output.Objects = result.Objects;
            }
        } else {
            output.Messages.Add("An unknown error occurred attempting to execute the plugin '" + plugin.Name + "'.");
        }

        return output;
    }

    /// <summary>
    /// The thumbnail graphic to show for a given file based on the file extension.
    /// </summary>
    /// <param name="extension">The extension of the file with our without a leading period.</param>
    /// <returns>The thumbnail to be used for the file type.</returns>
    public static string FileThumbnail(string? extension)
    {
        string output = String.Empty;

        if (!String.IsNullOrWhiteSpace(extension)) {
            if (extension.StartsWith(".")) {
                extension = extension.Substring(1);
            }

            switch (extension.ToUpper()) {
                case "CSV":
                case "DOC":
                case "DOCX":
                case "EML":
                case "EPS":
                case "MP3":
                case "MP4":
                case "M4V":
                case "PDF":
                case "PPT":
                case "PPTX":
                case "TIF":
                case "TXT":
                case "WAV":
                case "XLS":
                case "XLSX":
                case "ZIP":
                    output = "file_" + extension.ToLower() + ".png";
                    break;

                default:
                    output = "file_unknown.png";
                    break;
            }
        }

        return output;
    }

    /// <summary>
    /// Finds the first line in the code that starts with the given text.
    /// </summary>
    /// <param name="code">The code to check.</param>
    /// <param name="startsWith">The text to find.</param>
    /// <returns>The first line that starts with the given text.</returns>
    public static string FindFirstLineStartingWith(string code, string startsWith)
    {
        string output = String.Empty;

        foreach (var line in SplitStringIntoLines(code)) {
            if (line.Trim().StartsWith(startsWith)) {
                output = line;
                break;
            }
        }

        return output;
    }

    /// <summary>
    /// Sets the 'target="_blank"' attribute on all anchor tags in the HTML.
    /// </summary>
    /// <param name="html">Text that contains HTML.</param>
    /// <returns>The text will all a elements updated to a _blank target.</returns>
    public static string FixLinksInHtml(string? html)
    {
        string output = StringValue(html);

        if (output.Contains("<a")) {
            HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
            document.LoadHtml(output);

            var links = document.DocumentNode.SelectNodes("//a");
            foreach (var link in links) {
                if (link.Attributes["target"] != null) {
                    link.Attributes["target"].Value = "_blank";
                } else {
                    link.Attributes.Add("target", "blank");
                }
            }

            output = document.DocumentNode.OuterHtml;
        }

        return output;
    }

    /// <summary>
    /// Formats a string as a Guid.
    /// </summary>
    /// <param name="input">Formats a string in the Guid format.</param>
    /// <returns>A string formatted in the Guid format.</returns>
    public static string FormatStringAsGuid(string input)
    {
        string output = String.Empty;

        if (!String.IsNullOrEmpty(input)) {
            string guid = input;
            if (guid.Length > 32) {
                guid = input.Substring(0, 32);
            }

            output = guid.Substring(0, 8) + "-" + guid.Substring(8, 4) + "-" + guid.Substring(12, 4) + "-" + guid.Substring(16, 4) + "-" + guid.Substring(20);
        }

        return output;
    }

    /// <summary>
    /// Forces updates to the DataModel that area required for changes to the language.
    /// </summary>
    public static void ForceModelUpdates()
    {
        // Used to ensure updates happen in components.

        DataObjects.Language lang = new DataObjects.Language {
            Culture = Model.Language.Culture,
            Description = Model.Language.Description,
            TenantId = Model.Language.TenantId,
            Phrases = new List<DataObjects.OptionPair>(),
        };

        foreach (var phrase in Model.Language.Phrases) {
            lang.Phrases.Add(new DataObjects.OptionPair {
                Id = phrase.Id,
                Value = phrase.Value,
            });
        }

        Model.Language = lang;
    }

    // {{ModuleItemStart:Locations}}
    /// <summary>
    /// Format an address.
    /// </summary>
    /// <param name="location">The location with an address to format.</param>
    /// <returns>The formatted address.</returns>
    public static string FormatAddress(DataObjects.Location location)
    {
        string output = String.Empty;

        if (!String.IsNullOrWhiteSpace(location.Address)) {
            output += location.Address;
            if (!String.IsNullOrWhiteSpace(location.City)) {
                output += "<br />" + location.City;

                if (!String.IsNullOrWhiteSpace(location.State)) {
                    output += ", " + location.State;

                    if (!String.IsNullOrWhiteSpace(location.PostalCode)) {
                        output += " " + location.PostalCode;
                    }
                }
            }
        }

        return output;
    }
    // {{ModuleItemEnd:Locations}}

    // {{ModuleItemStart:Appointments}}
    /// <summary>
    /// Formats the dates and times for an appointment.
    /// </summary>
    /// <param name="appt">The Appointment object.</param>
    /// <returns>The formatted dates and times.</returns>
    public static string FormatAppointmentDatesAndTimes(DataObjects.Appointment appt)
    {
        string output = String.Empty;

        bool multiDay = appt.Start.ToShortDateString() != appt.End.ToShortDateString();

        if (appt.AllDay) {
            if (multiDay) {
                output = appt.Start.ToString("d") + " - " + appt.End.ToString("d") + " " + Text("AllDay");
            } else {
                output = appt.Start.ToString("d") + " " + Text("AllDay");
            }
        } else {
            // Start with just the main event date and time
            output = appt.Start.ToString("d") + " " + appt.Start.ToString("t");

            // If this is a multi-day event then add the end date and time
            if (multiDay) {
                output += " - " + appt.End.ToString("d") + " " + appt.End.ToString("t");
            } else if (appt.Start.ToString("t") != appt.End.ToString("t")) {
                // Add the end time
                output += " - " + appt.End.ToString("t");
            }
        }

        return output;
    }
    // {{ModuleItemEnd:Appointments}}

    /// <summary>
    /// Formats a value in the currency format.
    /// </summary>
    /// <param name="value">The value to be formatted as a currency.</param>
    /// <returns>The value formatted as a currency.</returns>
    public static string FormatCurrency(string? value)
    {
        return FormatCurrency(value, false);
    }

    /// <summary>
    /// Formats a value in the currency format.
    /// </summary>
    /// <param name="value">The value to be formatted as a currency.</param>
    /// <param name="ReplaceSpaces">Option to replace spaces with non-breaking HTML characters.</param>
    /// <returns>The value formatted as a currency.</returns>
    public static string FormatCurrency(string? value, bool ReplaceSpaces = false)
    {
        string output = "";

        if (!String.IsNullOrWhiteSpace(value)) {
            try {
                decimal v = Convert.ToDecimal(value);
                output = v.ToString("C");

                if (ReplaceSpaces) {
                    output = output.Replace(" ", "&nbsp;");
                }
            } catch { }
        }

        return output;
    }

    /// <summary>
    /// Formats a DateTime as a short date string.
    /// </summary>
    /// <param name="date">The DateTime value to format.</param>
    /// <param name="ReplaceSpaces">Option to replace spaces with non-breaking HTML characters.</param>
    /// <param name="ToLocalTimezone">Option to convert the DateTime from UTC to local time (defaults to true).</param>
    /// <returns>The formatted date.</returns>
    public static string FormatDate(DateTime? date, bool ReplaceSpaces = false, bool ToLocalTimezone = true)
    {
        string output = String.Empty;

        if (date.HasValue) {
            var d = (DateTime)date;

            if (ToLocalTimezone) {
                d = d.ToLocalTime();
            }

            output = d.ToShortDateString();

            if (ReplaceSpaces) {
                output = output.Replace(" ", "&nbsp;");
            }
        }

        return output;
    }

    /// <summary>
    /// Formats a DateTime as a short date string.
    /// </summary>
    /// <param name="value">The string containing a DateTime value to format.</param>
    /// <param name="ReplaceSpaces">Option to replace spaces with non-breaking HTML characters.</param>
    /// <param name="ToLocalTimezone">Option to convert the DateTime from UTC to local time (defaults to true).</param>
    /// <returns>The formatted date.</returns>
    public static string FormatDate(string? value, bool ReplaceSpaces = false, bool ToLocalTimezone = true)
    {
        string output = String.Empty;

        if (!String.IsNullOrWhiteSpace(value)) {
            try {
                var d = Convert.ToDateTime(value);

                if (ToLocalTimezone) {
                    d = d.ToLocalTime();
                }

                output = d.ToShortDateString();

                if (ReplaceSpaces) {
                    output = output.Replace(" ", "&nbsp;");
                }
            } catch { }
        }

        return output;
    }

    /// <summary>
    /// Formats a DateTime as a short date string and short time string.
    /// </summary>
    /// <param name="date">The DateTime to format.</param>
    /// <param name="ReplaceSpaces">Option to replace spaces with non-breaking HTML characters.</param>
    /// <param name="ToLocalTimezone">Option to convert the DateTime from UTC to local time (defaults to true).</param>
    /// <returns>The formatted date and time.</returns>
    public static string FormatDateTime(DateTime? date, bool ReplaceSpaces = false, bool ToLocalTimezone = true)
    {
        string output = String.Empty;

        if (date.HasValue) {
            DateTime d = (DateTime)date;

            if (ToLocalTimezone) {
                d = d.ToLocalTime();
            }

            output = d.ToShortDateString() + " " + d.ToShortTimeString();

            if (ReplaceSpaces) {
                output = output.Replace(" ", "&nbsp;");
            }
        }

        return output;
    }

    /// <summary>
    /// Formats a DateTime as a short date string and short time string.
    /// </summary>
    /// <param name="value">The string containing a DateTime value to format.</param>
    /// <returns>The formatted date and time.</returns>
    public static string FormatDateTime(string? value)
    {
        return FormatDateTime(value, false, true);
    }

    /// <summary>
    /// Formats a DateTime as a short date string and short time string.
    /// </summary>
    /// <param name="value">The string containing a DateTime value to format.</param>
    /// <param name="ReplaceSpaces">Option to replace spaces with non-breaking HTML characters.</param>
    /// <param name="ToLocalTimezone">Option to convert the DateTime from UTC to local time (defaults to true).</param>
    /// <returns>The formatted date and time.</returns>
    public static string FormatDateTime(string? value, bool ReplaceSpaces = false, bool ToLocalTimezone = true)
    {
        string output = String.Empty;

        if (!String.IsNullOrWhiteSpace(value)) {
            try {
                var d = Convert.ToDateTime(value);
                if (ToLocalTimezone) {
                    d = d.ToLocalTime();
                }
                output = d.ToShortDateString() + " " + d.ToShortTimeString();
            } catch { }

            if (ReplaceSpaces) {
                output = output.Replace(" ", "&nbsp;");
            }
        }

        return output;
    }

    /// <summary>
    /// Formats an email address as an email link.
    /// </summary>
    /// <param name="value">The email address.</param>
    /// <returns>An email link element.</returns>
    public static string FormatEmailAddress(string? value)
    {
        return FormatEmailAddress(value, false);
    }

    /// <summary>
    /// Formats an email address as an email link.
    /// </summary>
    /// <param name="value">The email address.</param>
    /// <param name="ReplaceSpaces">Option to replace spaces with non-breaking HTML characters.</param>
    /// <returns>An email link element.</returns>
    public static string FormatEmailAddress(string? value, bool ReplaceSpaces = false)
    {
        string output = "";

        if (!String.IsNullOrWhiteSpace(value)) {
            if (ReplaceSpaces) {
                output = "<a href=\"mailto:" + value + "\">" + value.Replace(" ", "&nbsp;") + "</a>";
            } else {
                output = "<a href=\"mailto:" + value + "\">" + value + "</a>";
            }
        }

        return output;
    }

    /// <summary>
    /// Formats a DateTime in the short time format.
    /// </summary>
    /// <param name="date">The DateTime object to be formatted.</param>
    /// <param name="Compressed">Option to compress the output (remove :00 and change AM to "a" and PM to "p")</param>
    /// <param name="ToLocalTimezone">Option to convert the DateTime from UTC to local time (defaults to true).</param>
    /// <returns>The DateTime in the short time format.</returns>
    public static string FormatTime(DateTime? date, bool Compressed = false, bool ToLocalTimezone = true)
    {
        string output = String.Empty;

        if (date.HasValue) {
            DateTime d = (DateTime)date;

            if (ToLocalTimezone) {
                d = d.ToLocalTime();
            }

            output = d.ToShortTimeString();

            if (Compressed) {
                output = output.Replace(":00", "")
                    .Replace(" ", "")
                    .Replace("AM", "a")
                    .Replace("PM", "p");
            }
        }

        return output;
    }

    /// <summary>
    /// Formats the user display name of a UserListing object.
    /// </summary>
    /// <param name="userListing">The UserListing object to be formatted.</param>
    /// <param name="IncludeLocation">Option to include the Location in the output.</param>
    /// <param name="IncludeEmail">Option to include the email address in the output.</param>
    /// <returns>The formatting user display name.</returns>
    public static string FormatUserDisplayName(DataObjects.UserListing? userListing, bool IncludeLocation = true, bool IncludeEmail = true)
    {
        string output = String.Empty;

        if (userListing != null) {
            output += userListing.FirstName;

            if (!String.IsNullOrWhiteSpace(userListing.LastName)) {
                if (!String.IsNullOrWhiteSpace(output)) {
                    output += " ";
                }
                output += userListing.LastName;
            }

            if (IncludeLocation && !String.IsNullOrWhiteSpace(userListing.Location)) {
                if (!String.IsNullOrWhiteSpace(output)) {
                    output += " ";
                }
                output += "[" + userListing.Location + "]";
            }

            if (IncludeEmail && !String.IsNullOrWhiteSpace(userListing.Email)) {
                if (!String.IsNullOrWhiteSpace(output)) {
                    output += " (" + userListing.Email + ")";
                } else {
                    output += userListing.Email;
                }
            }
        }

        return output;
    }

    /// <summary>
    /// Formats the user display name of a user based on the unique user id.
    /// </summary>
    /// <param name="UserId">The unique user id of the user.</param>
    /// <param name="IncludeLocation">Option to include the Location in the output.</param>
    /// <param name="IncludeEmail">Option to include the email address in the output.</param>
    /// <returns>The formatting user display name.</returns>
    public static string FormatUserDisplayName(string? UserId, bool IncludeLocation = false, bool IncludeEmail = false)
    {
        string output = String.Empty;

        if (!String.IsNullOrWhiteSpace(UserId)) {
            var user = Model.Tenant.Users.FirstOrDefault(x => x.UserId.ToString() == UserId);

            if (user != null) {
                output += user.FirstName;

                if (!String.IsNullOrWhiteSpace(user.LastName)) {
                    if (!String.IsNullOrWhiteSpace(output)) {
                        output += " ";
                    }
                    output += user.LastName;
                }

                if (IncludeLocation && !String.IsNullOrWhiteSpace(user.Location)) {
                    if (!String.IsNullOrWhiteSpace(output)) {
                        output += " ";
                    }
                    output += "[" + user.Location + "]";
                }

                if (IncludeEmail && !String.IsNullOrWhiteSpace(user.Email)) {
                    if (!String.IsNullOrWhiteSpace(output)) {
                        output += " (" + user.Email + ")";
                    } else {
                        output += user.Email;
                    }
                }
            }
        }

        return output;
    }

    /// <summary>
    /// Formats the user display name of a user based on the unique user id.
    /// </summary>
    /// <param name="UserId">The unique user id of the user.</param>
    /// <param name="IncludeLocation">Option to include the Location in the output.</param>
    /// <param name="IncludeEmail">Option to include the email address in the output.</param>
    /// <returns>The formatting user display name.</returns>
    public static string FormatUserDisplayName(Guid? UserId, bool IncludeLocation = false, bool IncludeEmail = false)
    {
        string output = String.Empty;

        if (UserId.HasValue) {
            var user = Model.Tenant.Users.FirstOrDefault(x => x.UserId == UserId);

            if (user != null) {
                output += user.FirstName;

                if (!String.IsNullOrWhiteSpace(user.LastName)) {
                    if (!String.IsNullOrWhiteSpace(output)) {
                        output += " ";
                    }
                    output += user.LastName;
                }

                if (IncludeLocation && !String.IsNullOrWhiteSpace(user.Location)) {
                    if (!String.IsNullOrWhiteSpace(output)) {
                        output += " ";
                    }
                    output += "[" + user.Location + "]";
                }

                if (IncludeEmail && !String.IsNullOrWhiteSpace(user.Email)) {
                    if (!String.IsNullOrWhiteSpace(output)) {
                        output += " (" + user.Email + ")";
                    } else {
                        output += user.Email;
                    }
                }
            }
        }

        return output;
    }

    /// <summary>
    /// Gets the value from a ChangeEventArgs object.
    /// </summary>
    /// <typeparam name="T">The type of object to return.</typeparam>
    /// <param name="e">The ChangeEventArgs object.</param>
    /// <returns>A nullable object of type T with any value from the change event.</returns>
    public static T? GetChangeEventArgValue<T>(ChangeEventArgs e)
    {
        T? output = default(T);

        if (e != null && e.Value != null) {
            try {
                output = (T)e.Value;
            } catch { }
        }

        return output;
    }

    /// <summary>
    /// Uses the CSVHelper library to convert a collection of objects to CSV.
    /// </summary>
    /// <typeparam name="T">The type of the object in the collection.</typeparam>
    /// <param name="records">The collection of objects.</param>
    /// <returns>The CSV data as a byte array.</returns>
    public static byte[]? GetCsvData<T>(IEnumerable<T> records)
    {
        byte[]? output = null;

        using (var memoryStream = new MemoryStream()) {
            using (var streamWriter = new StreamWriter(memoryStream)) {
                using (var csvWriter = new CsvHelper.CsvWriter(streamWriter, culture: new System.Globalization.CultureInfo("en-US"))) {
                    csvWriter.WriteRecords(records);
                    streamWriter.Flush();
                    output = memoryStream.ToArray();
                }
            }
        }

        return output;
    }

    /// <summary>
    /// Converts CSV data back into a collection of objects.
    /// </summary>
    /// <typeparam name="T">The type of objects to return.</typeparam>
    /// <param name="csvData">The CSV data to parse.</param>
    /// <returns>A list of objects of the type specified.</returns>
    public static List<T> GetCsvFromData<T>(string csvData)
    {
        List<T> output = new List<T>();

        if (!String.IsNullOrWhiteSpace(csvData)) {
            var textReader = new StringReader(csvData);

            var config = new CsvHelper.Configuration.CsvConfiguration(new CultureInfo("en-US"));
            config.IgnoreBlankLines = true;
            config.MissingFieldFound = null;

            using (var csvReader = new CsvHelper.CsvReader(textReader, config)) {
                var records = csvReader.GetRecords<T>();
                if (records != null) {
                    output = records.ToList();
                }
            }
        }

        return output;
    }

    /// <summary>
    /// Gets the current user from the user-token stored in the "user-token" cookie.
    /// </summary>
    /// <returns>A User object.</returns>
    public static async Task<DataObjects.User> GetCurrentUser()
    {
        DataObjects.User output = new DataObjects.User();

        var token = await CookieRead<string>("user-token");
        if (!String.IsNullOrWhiteSpace(token)) {
            // We need the fingerprint before we can make this call.
            while (String.IsNullOrWhiteSpace(Model.Fingerprint)) {
                await Task.Delay(100);
            }

            var result = await GetOrPost<DataObjects.User>("api/Data/GetUserFromToken", new DataObjects.SimplePost { SingleItem = token });
            if (result != null && result.ActionResponse.Result && result.Enabled) {
                output = result;
            } else {
                // Clear the token cookie
                await CookieWrite("user-token", "");
            }
        }

        if (!output.ActionResponse.Result) {
            if (!output.ActionResponse.Messages.Any()) {
                output.ActionResponse.Messages.Add("User Not Logged In");
            }
        }

        return output;
    }

    /// <summary>
    /// Gets the count of deleted records for admin users.
    /// </summary>
    public static async Task GetDeletedRecordCount()
    {
        var deletedRecordCounts = await GetOrPost<DataObjects.DeletedRecordCounts>("api/Data/GetDeletedRecordCounts");
        if (deletedRecordCounts != null) {
            Model.DeletedRecordCounts = deletedRecordCounts;
        }
    }

    /// <summary>
    /// Gets the deleted records from the API endpoint.
    /// </summary>
    /// <returns></returns>
    public static async Task<DataObjects.DeletedRecords> GetDeletedRecords()
    {
        var output = new DataObjects.DeletedRecords();

        var result = await GetOrPost<DataObjects.DeletedRecords>("api/Data/GetDeletedRecords");
        if (result != null) {
            output = result;
        }

        // Also update the counts on the Main Model

        // {{ModuleItemStart:Appointments}}
        Model.DeletedRecordCounts.AppointmentNotes = output.AppointmentNotes.Count();
        Model.DeletedRecordCounts.Appointments = output.Appointments.Count();
        Model.DeletedRecordCounts.AppointmentServices = output.AppointmentServices.Count();
        // {{ModuleItemEnd:Appointments}}

        Model.DeletedRecordCounts.DepartmentGroups = output.DepartmentGroups.Count();
        Model.DeletedRecordCounts.Departments = output.Departments.Count();

        // {{ModuleItemStart:EmailTemplates}}
        Model.DeletedRecordCounts.EmailTemplates = output.EmailTemplates.Count();
        // {{ModuleItemEnd:EmailTemplates}}

        Model.DeletedRecordCounts.FileStorage = output.FileStorage.Count();

        // {{ModuleItemStart:Locations}}
        Model.DeletedRecordCounts.Locations = output.Locations.Count();
        // {{ModuleItemEnd:Locations}}

        // {{ModuleItemStart:Services}}
        Model.DeletedRecordCounts.Services = output.Services.Count();
        // {{ModuleItemEnd:Services}}

        // {{ModuleItemStart:Tags}}
        Model.DeletedRecordCounts.Tags = output.Tags.Count();
        // {{ModuleItemEnd:Tags}}

        Model.DeletedRecordCounts.UserGroups = output.UserGroups.Count();
        Model.DeletedRecordCounts.Users = output.Users.Count();

        UpdateModelDeletedRecordCountsForAppItems(output);

        return output;
    }

    /// <summary>
    /// Gets the list of deleted record types.
    /// </summary>
    /// <returns>A list of strings.</returns>
    public static List<string> GetDeletedRecordTypes()
    {
        var output = new List<string> {
            // {{ModuleItemStart:Appointments}}
            "Appointment",
            "AppointmentNote",
            "AppointmentService",
            // {{ModuleItemEnd:Appointments}}
            "Department",
            "DepartmentGroup",
            // {{ModuleItemStart:EmailTemplates}}
            "EmailTemplate",
            // {{ModuleItemEnd:EmailTemplates}}
            "FileStorage",
            // {{ModuleItemStart:Locations}}
            "Location",
            // {{ModuleItemEnd:Locations}}
            // {{ModuleItemStart:Services}}
            "Service",
            // {{ModuleItemEnd:Services}}
            // {{ModuleItemStart:Tags}}
            "Tag",
            // {{ModuleItemEnd:Tags}}
            "User",
            "UserGroup"
        };

        var appDeletedRecordTypes = GetDeletedRecordTypesApp();
        if (appDeletedRecordTypes != null && appDeletedRecordTypes.Any()) {
            output.AddRange(appDeletedRecordTypes);
        }

        output = output.OrderBy(x => x).ToList();

        return output;
    }

    /// <summary>
    /// Gets a value from a dictionary of string, object.
    /// </summary>
    /// <typeparam name="T">The type of value expected.</typeparam>
    /// <param name="key">The name of the dictionary item.</param>
    /// <param name="properties">The collection of string, object items.</param>
    /// <returns>The value from the dictionary or a default(T)</returns>
    public static T? GetDictionaryProperty<T>(string key, Dictionary<string, object> properties)
    {
        var output = default(T);

        var item = properties.FirstOrDefault(x => x.Key.ToLower() == key.ToLower());
        if (!String.IsNullOrWhiteSpace(item.Key) && item.Key.ToLower() == key.ToLower()) {
            try {
                var value = item.Value;

                if (typeof(T) == typeof(String)) {
                    var valueAsString = value.ToString();
                    if (!String.IsNullOrWhiteSpace(valueAsString)) {
                        output = (T)(object)(valueAsString);
                    }
                } else {
                    output = (T)value;
                }


            } catch (Exception ex) {
                if (ex != null) { }
            }
        }

        return output;
    }

    /// <summary>
    /// Gets the unique browser fingerprint.
    /// </summary>
    /// <returns>A string containing the unique browser fingerprint.</returns>
    public static async Task<string> GetFingerprint()
    {
        var output = await jsRuntime.InvokeAsync<string>("GetFingerprint");
        return output;
    }

    /// <summary>
    /// Gets user input.
    /// </summary>
    /// <param name="OnInputAccepted">The delegate that will be invoke with the results of the user input.</param>
    /// <param name="UserInputType">The type of input to get from the user.</param>
    /// <param name="Title">The title of the input dialog.</param>
    /// <param name="Id">A id to add to the input element.</param>
    /// <param name="DefaultValue">The default value to use in input elements that support it.</param>
    /// <param name="Instructions">Any instructions to show before the input element.</param>
    /// <param name="Class">A class to add to the input elements for elements that support it.</param>
    /// <param name="MultiSelectRows">For multiselect elements the number of rows to show.</param>
    /// <param name="PlaceholderText">Placeholder text for elements that support it.</param>
    /// <param name="SetFocus">Option to set the focus to the element.</param>
    /// <param name="UserInputOptions">Any options for input elements that support options. The first element is the value and the second is the label.</param>
    /// <param name="width">A width for the dialog.</param>
    /// <param name="height">A height for the dialog.</param>
    /// <returns>The results of the user input. Depending on the input type this will either be a string or a list of string.</returns>
    public static async Task GetInput(Delegate OnInputAccepted,
        FreeBlazor.GetInput.InputType UserInputType = FreeBlazor.GetInput.InputType.Text,
        string Title = "",
        string Id = "",
        string DefaultValue = "",
        string Instructions = "",
        string Class = "",
        int? MultiSelectRows = null,
        string PlaceholderText = "",
        bool SetFocus = false,
        Dictionary<string, string>? UserInputOptions = null,
        string width = "",
        string height = "")
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>();
        parameters.Add("OnInputAccepted", OnInputAccepted);

        parameters.Add("UserInputType", UserInputType);

        if (!String.IsNullOrWhiteSpace(Id)) {
            parameters.Add("Id", Id);
        }

        if (!String.IsNullOrWhiteSpace(DefaultValue)) {
            parameters.Add("DefaultValue", DefaultValue);
        }

        if (!String.IsNullOrWhiteSpace(Instructions)) {
            parameters.Add("Instructions", Instructions);
        }

        if (!String.IsNullOrWhiteSpace(Class)) {
            parameters.Add("Class", Class);
        }

        if (MultiSelectRows.HasValue) {
            parameters.Add("MultiSelectRows", (int)MultiSelectRows);
        }

        if (!String.IsNullOrWhiteSpace(PlaceholderText)) {
            parameters.Add("PlaceholderText", PlaceholderText);
        }

        parameters.Add("SetFocus", SetFocus);

        if (UserInputOptions != null && UserInputOptions.Any()) {
            parameters.Add("UserInputOptions", UserInputOptions);
        }

        if (width == "auto") {
            width = "";
        }

        if (height == "auto") {
            height = "";
        }

        await DialogService.OpenAsync<GetInputDialog>(Title, parameters, new DialogOptions() {
            AutoFocusFirstElement = false,
            Resizable = false,
            Draggable = false,
            Width = width,
            Height = height,
        });
    }

    /// <summary>
    /// Gets a local storage item.
    /// </summary>
    /// <typeparam name="T">The type of the item.</typeparam>
    /// <param name="key">the key of the item.</param>
    /// <returns>A nullable object of type T.</returns>
    public static async Task<T?> GetLocalStorageItem<T>(string key)
    {
        T? output = default(T);

        if (LocalStorage != null) {
            try {
                var result = await LocalStorage.GetItemAsync<T>(key);
                if (result != null) {
                    output = result;
                }
            } catch { }
        }

        return output;
    }

    // {{ModuleItemStart:Locations}}
    // {{ModuleItemStart:Appointments}}
    /// <summary>
    /// Gets the style for a given location.
    /// </summary>
    /// <param name="LocationId">The unique id of the location.</param>
    /// <param name="appointment">An optional Appointment object.</param>
    /// <returns>A string with the style for the location.</returns>
    public static async Task<string> GetLocationStyle(Guid? LocationId, DataObjects.Appointment? appointment = null)
    {
        string output = String.Empty;

        // See if the event has specific colors
        if (appointment != null) {
            if (!String.IsNullOrWhiteSpace(appointment.BackgroundColor)) {
                output += "background-color:" + appointment.BackgroundColor + ";";
            }
            if (!String.IsNullOrWhiteSpace(appointment.ForegroundColor)) {
                output += "color:" + appointment.ForegroundColor + ";";
            }
        }

        if (String.IsNullOrWhiteSpace(output) && LocationId.HasValue && LocationId != Guid.Empty) {
            if (!Model.Locations.Any()) {
                await LoadLocations();
            }

            var locationItem = Model.Locations.FirstOrDefault(x => x.LocationId == LocationId);
            if (locationItem != null) {
                if (!String.IsNullOrWhiteSpace(locationItem.CalendarBackgroundColor)) {
                    output += "background-color:" + locationItem.CalendarBackgroundColor + ";";
                }

                if (!String.IsNullOrWhiteSpace(locationItem.CalendarForegroundColor)) {
                    output += "color:" + locationItem.CalendarForegroundColor + ";";
                }
            }
        }

        return output;
    }
    // {{ModuleItemEnd:Appointments}}
    // {{ModuleItemEnd:Locations}}

    /// <summary>
    /// Shows a dialog to get a new password.
    /// </summary>
    /// <param name="OnPasswordAccepted">The delegate to be invoked which will receive the new password.</param>
    /// <param name="Title">The title for the dialog.</param>
    /// <param name="Class">An optional class to add to the input element.</param>
    /// <param name="Length">The length for the newly-generated password.</param>
    /// <param name="RequireUpperCase">Option to require uppercase characters in the password.</param>
    /// <param name="RequireLowerCase">Option to require lowercase characters in the password.</param>
    /// <param name="RequireNumbers">Option to require numbers in the password.</param>
    /// <param name="RequireSpecialCharacters">Option to require special characters in the password.</param>
    public static async Task GetNewPassword(
        Action<string> OnPasswordAccepted,
        string Title = "",
        string Class = "",
        int Length = 0,
        bool? RequireUpperCase = null,
        bool? RequireLowerCase = null,
        bool? RequireNumbers = null,
        bool? RequireSpecialCharacters = null
    )
    {
        if (String.IsNullOrWhiteSpace(Title)) {
            Title = Text("GeneratePassword");
        }

        Dictionary<string, object> parameters = new Dictionary<string, object>();
        parameters.Add("OnPasswordAccepted", OnPasswordAccepted);

        if (String.IsNullOrWhiteSpace(Class)) {
            Class = "form-control";
        }
        parameters.Add("Class", Class);

        if (Length > 0) {
            parameters.Add("Length", Length);
        }

        if (RequireUpperCase.HasValue) {
            parameters.Add("RequireUpperCase", (bool)RequireUpperCase);
        }

        if (RequireLowerCase.HasValue) {
            parameters.Add("RequireLowerCase", (bool)RequireLowerCase);
        }

        if (RequireNumbers.HasValue) {
            parameters.Add("RequireNumbers", (bool)RequireNumbers);
        }

        if (RequireSpecialCharacters.HasValue) {
            parameters.Add("RequireSpecialCharacters", (bool)RequireSpecialCharacters);
        }

        await DialogService.OpenAsync<GeneratePasswordDialog>(Title, parameters, new Radzen.DialogOptions() {
            AutoFocusFirstElement = false,
            Resizable = false,
            Draggable = false,
            CloseDialogOnEsc = true,
            ShowClose = false,
        });
    }

    /// <summary>
    /// Gets the typed object for an object that comes back from an API endpoint as a System.Text.Json type of object.
    /// </summary>
    /// <typeparam name="T">The type of object to return.</typeparam>
    /// <param name="obj">The object to cast as a type of T</param>
    /// <returns>If the object is not null then the object is returned as a type of T, otherwise, the default value is returned.</returns>
    public static T? GetObjectAsType<T>(object? obj)
    {
        T? output = default(T);

        if (obj != null) {
            try {
                output = DeserializeObject<T>(SerializeObject(obj));
            } catch { }
        }

        return output;
    }

    /// <summary>
    /// Gets the default for the type of a given object. For IEnumerables, returns a new list of those types.
    /// </summary>
    /// <param name="o">The object.</param>
    /// <returns>The default for the object.</returns>
    public static object? GetObjectDefault(object o)
    {
        var type = o.GetType();

        bool list = false;

        if (type.IsArray) {
            type = type.GetElementType();
            list = true;
        } else if (type.IsGenericType) {
            type = type.GetGenericArguments()[0];
            list = true;
        }

        if (type == typeof(String)) {
            return list ? new List<string>() : String.Empty;
        } else if (type == typeof(bool) || type == typeof(Boolean)) {
            return list ? new List<bool>() : false;
        } else if (type == typeof(int)) {
            return list ? new List<int>() : 0;
        } else if (type == typeof(Int16)) {
            return list ? new List<Int16>() : 0;
        } else if (type == typeof(Int32)) {
            return list ? new List<Int32>() : 0;
        } else if (type == typeof(Int64)) {
            return list ? new List<Int64>() : 0;
        } else if (type == typeof(decimal)) {
            return list ? new List<decimal>() : 0;
        } else if (type == typeof(long)) {
            return list ? new List<long>() : 0;
        } else if (type == typeof(double)) {
            return list ? new List<double>() : 0;
        } else if (type == typeof(float)) {
            return list ? new List<float>() : 0;
        } else if (type == typeof(Single)) {
            return list ? new List<Single>() : 0;
        } else if (type == typeof(SByte)) {
            return list ? new List<SByte>() : 0;
        } else if (type == typeof(Byte)) {
            return list ? new List<Byte>() : 0;
        } else if (type == typeof(UInt16)) {
            return list ? new List<UInt16>() : 0;
        } else if (type == typeof(UInt32)) {
            return list ? new List<UInt32>() : 0;
        } else if (type == typeof(UInt64)) {
            return list ? new List<UInt64>() : 0;
        } else if (type == typeof(IntPtr)) {
            return list ? new List<IntPtr>() : 0;
        } else if (type == typeof(UIntPtr)) {
            return list ? new List<UIntPtr>() : 0;
        } else if (type == typeof(System.Security.SecureString)) {
            return list ? new List<System.Security.SecureString>() : new System.Security.SecureString();
        } else if (type != null) {
            return Activator.CreateInstance(type);
        } else {
            return null;
        }
    }

    /// <summary>
    /// Gets the default base type of a given object. For IEnumerables, returns the base type of the first item.
    /// </summary>
    /// <param name="o">The object.</param>
    /// <returns>The base type of the object.</returns>
    public static object? GetObjectDefaultBaseType(object o)
    {
        var type = o.GetType();

        if (type.IsArray) {
            type = type.GetElementType();
        } else if (type.IsGenericType) {
            type = type.GetGenericArguments()[0];
        }

        if (type == typeof(String)) {
            return String.Empty;
        } else if (type == typeof(bool) || type == typeof(Boolean)) {
            return false;
        } else if (type == typeof(int) || type == typeof(Int16) || type == typeof(Int32) || type == typeof(Int64)) {
            return 0;
        } else if (type == typeof(decimal) || type == typeof(long) || type == typeof(double) || type == typeof(float) || type == typeof(Single)) {
            return 0;
        } else if (type == typeof(SByte) || type == typeof(Byte) || type == typeof(UInt16) || type == typeof(UInt32) || type == typeof(UInt64)) {
            return 0;
        } else if (type == typeof(IntPtr) || type == typeof(UIntPtr)) {
            return 0;
        } else if (type == typeof(System.Security.SecureString)) {
            return new System.Security.SecureString();
        } else if (type != null) {
            return Activator.CreateInstance(type);
        } else {
            return null;
        }
    }

    /// <summary>
    /// Gets the value of a property from an object.
    /// </summary>
    /// <typeparam name="T">The type of the property to return.</typeparam>
    /// <param name="obj">The object to check for the property value.</param>
    /// <param name="property">The name of the property.</param>
    /// <returns>A nullable object of type T containing any value found.</returns>
    public static T? GetObjectPropertyValue<T>(object? obj, string? property)
    {
        T? output = default(T);

        if (obj != null && !String.IsNullOrWhiteSpace(property)) {
            Type type = obj.GetType();

            if (type == typeof(System.Text.Json.JsonElement)) {
                output = GetObjectPropertyValueJson<T>((System.Text.Json.JsonElement)obj, property);
            } else {
                var propertyValue = GetObjectPropertyValueStandard<T>(obj, property);
                if (propertyValue != null) {
                    output = propertyValue;
                }
            }
        }

        return output;
    }

    /// <summary>
    /// Gets the value of a property from a JsonElement object.
    /// </summary>
    /// <typeparam name="T">The type of the property to return.</typeparam>
    /// <param name="element">The JsonElement object to check for the property value.</param>
    /// <param name="property">The name of the property.</param>
    /// <returns>A nullable object of type T containing any value found.</returns>
    private static T? GetObjectPropertyValueJson<T>(System.Text.Json.JsonElement element, string property)
    {
        T? output = default(T);

        string stringValue = String.Empty;

        try {
            // Find the case-sensitive property name
            var document = System.Text.Json.JsonDocument.Parse(element.ToString());
            if (document != null) {
                foreach (var prop in document.RootElement.EnumerateObject()) {
                    if (prop.Name.ToLower() == property.ToLower()) {
                        property = prop.Name;
                    }
                }
            }

            var elem = element.GetProperty(property);
            var type = elem.ValueKind;

            switch (type) {
                case System.Text.Json.JsonValueKind.Array:
                    break;

                case System.Text.Json.JsonValueKind.False:
                    stringValue = "False";
                    break;

                case System.Text.Json.JsonValueKind.Null:
                    break;

                case System.Text.Json.JsonValueKind.Number:
                    stringValue = StringValue(elem.GetRawText());
                    break;

                case System.Text.Json.JsonValueKind.Object:
                    break;

                case System.Text.Json.JsonValueKind.String:
                    stringValue = StringValue(elem.GetString());
                    break;

                case System.Text.Json.JsonValueKind.True:
                    stringValue = "True";
                    break;

                case System.Text.Json.JsonValueKind.Undefined:
                    break;
            }
        } catch (Exception ex) {
            if (ex != null) { }
        }

        if (!String.IsNullOrWhiteSpace(stringValue)) {
            try {
                Type t = typeof(T);
                if (t == typeof(System.Guid)) {
                    Guid g = new Guid(stringValue);
                    output = (T)(object)g;
                } else if (t == typeof(System.DateTime)) {
                    DateTime d = Convert.ToDateTime(stringValue);
                    output = (T)(object)d;
                } else if (t == typeof(System.Boolean)) {
                    bool b = Convert.ToBoolean(stringValue);
                    output = (T)(object)b;
                } else if (t == typeof(System.Double)) {
                    double dbl = Convert.ToDouble(stringValue);
                    output = (T)(object)dbl;
                } else if (t == typeof(System.Decimal)) {
                    decimal dec = Convert.ToDecimal(stringValue);
                    output = (T)(object)dec;
                } else if (t == typeof(System.Int64)) {
                    long l = Convert.ToInt64(stringValue);
                    output = (T)(object)l;
                } else if (t == typeof(System.Int32)) {
                    int i = Convert.ToInt32(stringValue);
                    output = (T)(object)i;
                } else if (t == typeof(System.Int16)) {
                    int i16 = Convert.ToInt16(stringValue);
                    output = (T)(object)i16;
                } else if (t == typeof(System.String)) {
                    output = (T)(object)stringValue;
                } else {
                    // just try to cast it.
                    output = (T)(object)stringValue;
                }

            } catch (Exception ex) {
                if (ex != null) { }
            }

        }

        return output;
    }

    /// <summary>
    /// Gets the value of a property from an object.
    /// </summary>
    /// <typeparam name="T">The type of the property to return.</typeparam>
    /// <param name="o">The object to check for the property value.</param>
    /// <param name="property">The name of the property.</param>
    /// <returns>A nullable object of type T containing any value found.</returns>
    private static T? GetObjectPropertyValueStandard<T>(object? o, string property)
    {
        T? output = default(T);

        if (o != null) {
            foreach (String part in property.Split(".")) {
                Type type = o.GetType();

                System.Reflection.BindingFlags bindingAttrs = System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.Static |
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.IgnoreCase;

                var info = type.GetProperty(part, bindingAttrs);
                if (info != null) {
                    var obj = info.GetValue(o, null);
                    if (obj != null) {
                        return (T)obj;
                    }
                }
            }
        }

        return output;
    }

    /// <summary>
    /// Returns the type for a given object.
    /// </summary>
    /// <param name="o">A nullable object.</param>
    /// <returns>A string containing the object type.</returns>
    public static string GetObjectType(object? o)
    {
        string output = String.Empty;

        if (o != null) {
            output = o.GetType()
                .ToString()
                .Replace("+", ".");
        }

        return output;
    }

    /// <summary>
    /// Gets data from an API endpoint using either the get or post method. If a post object is supplied post will be used. Otherwise, get will be used.
    /// </summary>
    /// <typeparam name="T">The type of the object to return.</typeparam>
    /// <param name="url">The API endpoint URL.</param>
    /// <param name="post">An optional object to post to the endpoint.</param>
    /// <param name="logResults">An option to log the results to the console.</param>
    /// <returns>A nullable object of type T.</returns>
    public static async Task<T?> GetOrPost<T>(string url, object? post = null, bool logResults = false)
    {
        T? output = default(T);

        if (Http != null) {
            try {
                HttpResponseMessage? response = null;

                Http.DefaultRequestHeaders.Clear();

                if (Model != null) {
                    Http.DefaultRequestHeaders.Add("TenantId", Model.User.TenantId.ToString());

                    if (String.IsNullOrWhiteSpace(Model.User.AuthToken)) {
                        Model.User.AuthToken = await Token();
                    }

                    if (!String.IsNullOrWhiteSpace(Model.User.AuthToken)) {
                        Http.DefaultRequestHeaders.Add("Token", Model.User.AuthToken);
                    }

                    if (!String.IsNullOrWhiteSpace(Model.Fingerprint)) {
                        Http.DefaultRequestHeaders.Add("Fingerprint", Model.Fingerprint);
                    }
                }

                if (post != null) {
                    response = await Http.PostAsJsonAsync(url, post);
                } else {
                    response = await Http.GetAsync(url);
                }

                if (response != null) {
                    if (response.IsSuccessStatusCode) {
                        var content = await response.Content.ReadAsStringAsync();

                        if (logResults && url.ToLower() != "api/data/getversioninfo") {
                            await ConsoleLog("GetOrPostResult", url, content);
                        }

                        if (!String.IsNullOrWhiteSpace(content)) {
                            if (content.ToUpper().StartsWith("<!DOCTYPE")) {
                                await ConsoleLog("Not a valid API endpoint - " + url);
                            } else {
                                if (typeof(T) == typeof(string)) {
                                    output = (T)(object)content;
                                } else {
                                    var result = await response.Content.ReadFromJsonAsync<T>();
                                    if (result != null) {
                                        output = result;
                                    }
                                }
                            }
                        }
                    } else {
                        await ConsoleLog("The Server Returned an Error Calling '" + url + "'");
                        await ConsoleLog("Status Code: " + response.StatusCode.ToString());
                        if (!String.IsNullOrWhiteSpace(response.ReasonPhrase)) {
                            Console.Write("Reason Phrase: " + response.ReasonPhrase);
                        }
                    }
                }
            } catch (Exception ex) {
                await ConsoleLog("An Exception Occurred Calling '" + url + "' - " + ex.Message);
            }
        }

        return output;
    }

    /// <summary>
    /// Gets a plugin by it's unique Guid Id.
    /// </summary>
    /// <param name="id">The Guid Id of the plugin.</param>
    /// <returns>A nullable Plugin object.</returns>
    public static Plugin? GetPluginById(Guid id)
    {
        var output = Model.Plugins.FirstOrDefault(x => x.Id == id);
        return output;
    }

    /// <summary>
    /// Gets a plugin by it's unique Guid Id as a string.
    /// </summary>
    /// <param name="id">The Guid Id of the plugin as a string.</param>
    /// <returns>A nullable Plugin object.</returns>
    public static Plugin? GetPluginById(string? id)
    {
        Plugin? output = null;

        if (!String.IsNullOrWhiteSpace(id)) {
            Guid pluginId = Guid.Empty;

            try {
                pluginId = new Guid(id);
            } catch { }

            if (pluginId != Guid.Empty) {
                output = GetPluginById(pluginId);
            }
        }

        return output;
    }

    /// <summary>
    /// Gets the code namespace from the plugin code.
    /// </summary>
    /// <param name="code">The C# code.</param>
    /// <returns>The code namespace.</returns>
    public static string GetPluginNamespace(string code)
    {
        string output = String.Empty;

        var line = FindFirstLineStartingWith(code, "namespace");
        if (!String.IsNullOrWhiteSpace(line)) {
            output = line.Replace("namespace ", "").Replace(";", "").Replace("{", "").Trim();
        }

        return output;
    }

    /// <summary>
    /// Gets the code class from the plugin code.
    /// </summary>
    /// <param name="code">The C# code.</param>
    /// <returns>The code class.</returns>
    public static string GetPluginClass(string code)
    {
        string output = String.Empty;

        var line = FindFirstLineStartingWith(code, "public class ");
        if (String.IsNullOrWhiteSpace(line)) {
            line = FindFirstLineStartingWith(code, "public partial class ");
        }

        if (!String.IsNullOrWhiteSpace(line)) {
            output = line.Replace("public class", "").Replace(";", "").Replace("{", "").Trim();

            if (output.Contains(":")) {
                output = output.Substring(0, output.IndexOf(":")).Trim();
            }
        }


        return output;
    }

    /// <summary>
    /// Gets the default value for a given prompt type.
    /// </summary>
    /// <param name="type">The PluginPromptType enum.</param>
    /// <returns>The default value for the given type.</returns>
    public static string[] GetPromptDefaultValue(PluginPromptType type)
    {
        var output = new string[] { "" };

        switch (type) {
            case PluginPromptType.Checkbox:
                output = new string[] { "False" };
                break;

            case PluginPromptType.CheckboxList:
                break;

            case PluginPromptType.Date:
                break;

            case PluginPromptType.DateTime:
                break;

            case PluginPromptType.File:
                break;

            case PluginPromptType.Multiselect:
                break;

            case PluginPromptType.Number:
                break;

            case PluginPromptType.Password:
                break;

            case PluginPromptType.Radio:
                break;

            case PluginPromptType.Select:
                break;

            case PluginPromptType.Text:
                break;

            case PluginPromptType.Textarea:
                break;

            case PluginPromptType.Time:
                break;
        }

        return output;
    }

    /// <summary>
    /// Gets a prompt item value as a string.
    /// </summary>
    /// <param name="o">A nullable object.</param>
    /// <returns>The value as a string.</returns>
    public static string GetPromptItemValueAsString(object? o)
    {
        string output = String.Empty;

        if (o != null) {
            var type = o.GetType();

            if (type == typeof(String)) {
                output += o.ToString();
            } else if (type == typeof(System.Security.SecureString)) {
                output += GetPasswordFromSecureString(o);
            }
        }

        return output;
    }

    /// <summary>
    /// Gets a value from the querystring.
    /// </summary>
    /// <param name="key">The key of the element.</param>
    /// <returns>The value for the given key.</returns>
    public static string GetQuerystringValue(string? key)
    {
        string output = String.Empty;

        if (!String.IsNullOrWhiteSpace(key)) {
            var uri = NavManager.ToAbsoluteUri(NavManager.Uri);
            output = GetQuerystringValue(uri.ToString(), key);
        }

        if (output.ToLower() == "undefined" || output.ToLower() == "null") {
            output = String.Empty;
        }

        return output;
    }

    /// <summary>
    /// Gets the password from a SecureString object.
    /// </summary>
    /// <param name="secureString">A SecureString object.</param>
    /// <returns>The password.</returns>
    public static string GetPasswordFromSecureString(object? secureString)
    {
        string output = String.Empty;

        if (secureString != null && secureString.GetType() == typeof(System.Security.SecureString)) {
            try {
                output += new System.Net.NetworkCredential("", (System.Security.SecureString)secureString).Password;
            } catch { }
        }

        return output;
    }

    /// <summary>
    /// Gets a value from the querystring of the provided url.
    /// </summary>
    /// <param name="fullUrl">The full url.</param>
    /// <param name="key">The key of the element.</param>
    /// <returns>The value for the given key.</returns>
    public static string GetQuerystringValue(string? fullUrl, string? key)
    {
        string output = String.Empty;

        if (!String.IsNullOrWhiteSpace(fullUrl) && !String.IsNullOrWhiteSpace(key)) {
            var qs = GetQuerystringValues(fullUrl);
            if (qs != null && qs.Any()) {
                var item = qs.FirstOrDefault(x => x.Key.ToLower() == key.ToLower());
                if (!String.IsNullOrWhiteSpace(item.Value)) {
                    output = item.Value;
                }
            }
        }

        if (output.ToLower() == "undefined" || output.ToLower() == "null") {
            output = String.Empty;
        }

        return output;
    }

    /// <summary>
    /// Gets the decoded value from the querystring.
    /// </summary>
    /// <param name="key">The key of the element.</param>
    /// <returns>The value for the given key.</returns>
    public static string GetQuerystringValueDecoded(string? key)
    {
        string output = String.Empty;

        if (!String.IsNullOrWhiteSpace(key)) {
            var uri = NavManager.ToAbsoluteUri(NavManager.Uri);
            output = GetQuerystringValue(uri.ToString(), key);

            if (!String.IsNullOrWhiteSpace(output)) {
                output = DecodeValue(output);
            }
        }

        if (output.ToLower() == "undefined" || output.ToLower() == "null") {
            output = String.Empty;
        }

        return output;
    }

    /// <summary>
    /// Gets all elements from the querystring as a dictionary collection.
    /// </summary>
    /// <param name="fullUrl">The full url.</param>
    /// <returns>A dictionary of string, string where the first element is the key and the second contains any values.</returns>
    public static Dictionary<string, string> GetQuerystringValues(string? fullUrl)
    {
        Dictionary<string, string> output = new Dictionary<string, string>();

        if (!String.IsNullOrWhiteSpace(fullUrl) && fullUrl.Contains("?")) {
            string qs = fullUrl.Substring(fullUrl.IndexOf("?") + 1);
            string[] parts = qs.Split("&");
            foreach (var part in parts) {
                if (part.Contains("=")) {
                    var values = part.Split("=");
                    string key = "";
                    string value = "";

                    try {
                        key = values[0];
                        value = values[1];
                    } catch { }

                    if (!String.IsNullOrWhiteSpace(key) && !String.IsNullOrWhiteSpace(value)) {
                        if (!output.ContainsKey(key)) {
                            output.Add(key, value);
                        }
                    }
                }
            }
        }

        return output;
    }

    // {{ModuleItemStart:Tags}}
    /// <summary>
    /// Gets a Tag by its unique id.
    /// </summary>
    /// <param name="tagId">The unique id of the Tag.</param>
    /// <returns>A nullable Tag object.</returns>
    public static async Task<DataObjects.Tag?> GetTag(Guid? tagId)
    {
        DataObjects.Tag? output = null;

        if (tagId.HasValue) {
            output = Model.Tags.FirstOrDefault(x => x.TagId == tagId);

            if (output == null) {
                output = await GetOrPost<DataObjects.Tag>("api/Data/GetTag/" + tagId.ToString());

                if (output != null) {
                    Model.Tags.Add(output);
                }
            }
        }

        return output;
    }
    // {{ModuleItemEnd:Tags}}

    /// <summary>
    /// Gets a User object for the given unique user id.
    /// </summary>
    /// <param name="UserId">The unique user id.</param>
    /// <returns>A User object.</returns>
    public static async Task<DataObjects.User> GetUser(Guid UserId)
    {
        DataObjects.User output = new DataObjects.User();

        // See if this is already loaded
        if (Model.Users.Any()) {
            var cached = Model.Users.FirstOrDefault(x => x.UserId == UserId);
            if (cached != null) {
                return cached;
            }
        }

        var loadedUser = await GetOrPost<DataObjects.User>("api/Data/GetUser/" + UserId.ToString());
        if (loadedUser != null) {
            output = loadedUser;
        }

        return output;
    }

    /// <summary>
    /// Gets a list of users from a list of user ids.
    /// </summary>
    /// <param name="UserIds">A nullable list of Guids.</param>
    /// <returns>A list of UserListing objects.</returns>
    public static async Task<List<DataObjects.UserListing>> GetUsers(List<Guid>? UserIds)
    {
        var output = new List<DataObjects.UserListing>();

        if (UserIds != null && UserIds.Count > 0) {
            foreach (var userId in UserIds) {
                var user = Model.Tenant.Users.FirstOrDefault(x => x.UserId == userId);
                if (user != null) {
                    output.Add(user);
                } else {
                    var getUser = await GetUser(userId);
                    if (getUser != null) {
                        var userListingItem = new DataObjects.UserListing {
                            UserId = getUser.UserId,
                            FirstName = getUser.FirstName,
                            LastName = getUser.LastName,
                            Email = getUser.Email,
                            Username = getUser.Username,
                            Department = getUser.DepartmentName,
                            Location = getUser.Location,
                            Enabled = getUser.Enabled,
                            Deleted = getUser.Deleted,
                            DeletedAt = getUser.DeletedAt,
                            Admin = getUser.Admin,
                        };

                        Model.Tenant.Users.Add(userListingItem);
                        output.Add(userListingItem);
                    }
                }
            }

            if (output.Count > 0) {
                output = output.OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            }
        }

        return output;
    }

    /// <summary>
    /// Gets a typed value from a string value.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="value">The value.</param>
    /// <returns>The value as a type of T.</returns>
    public static T? GetValueFromString<T>(string? value)
    {
        var output = default(T);

        if (!String.IsNullOrWhiteSpace(value)) {
            try {
                if (typeof(T) == typeof(string)) {
                    output = (T)(object)value;
                } else if (typeof(T) == typeof(bool)) {
                    bool boolValue = value.ToLower() == "true";
                    output = (T)(object)boolValue;
                } else if (typeof(T) == typeof(int)) {
                    output = (T)(object)Convert.ToInt32(value);
                } else if (typeof(T) == typeof(Int32)) {
                    output = (T)(object)Convert.ToInt32(value);
                } else if (typeof(T) == typeof(Int64)) {
                    output = (T)(object)Convert.ToInt64(value);
                } else if (typeof(T) == typeof(long)) {
                    output = (T)(object)Convert.ToInt64(value);
                } else if (typeof(T) == typeof(decimal)) {
                    output = (T)(object)Convert.ToDecimal(value);
                } else {
                    output = (T)(object)value;
                }
            } catch { }
        }

        return output;
    }

    /// <summary>
    /// The value for our built-in Guid1.
    /// </summary>
    public static Guid Guid1 {
        get {
            return new Guid("00000000-0000-0000-0000-000000000001");
        }
    }

    /// <summary>
    /// The value for our built-in Guid2.
    /// </summary>
    public static Guid Guid2 {
        get {
            return new Guid("00000000-0000-0000-0000-000000000002");
        }
    }

    /// <summary>
    /// Converts a number to a Guid padding with leading zeros.
    /// </summary>
    /// <param name="number">The number to format as a Guid.</param>
    /// <returns>A Guid with all leading zeros and ending in the number provided.</returns>
    public static Guid GuidFromNumber(double number)
    {
        Guid output = Guid.Empty;

        string guid = number.ToString().Replace(".", "").Replace("-", "").Replace("+", "").PadLeft(32, '0');
        try {
            output = new Guid(FormatStringAsGuid(guid));
        } catch { }

        return output;
    }

    /// <summary>
    /// Converts a number to a Guid padding with leading zeros.
    /// </summary>
    /// <param name="number">The number to format as a Guid.</param>
    /// <returns>A Guid with all leading zeros and ending in the number provided.</returns>
    public static Guid GuidFromNumber(int number)
    {
        return GuidFromNumber((double)number);
    }

    /// <summary>
    /// Converts a number to a Guid padding with leading zeros.
    /// </summary>
    /// <param name="number">The number to format as a Guid.</param>
    /// <returns>A Guid with all leading zeros and ending in the number provided.</returns>
    public static Guid GuidFromNumber(long number)
    {
        return GuidFromNumber((double)number);
    }

    /// <summary>
    /// Converts a Guid that is in the number format back to a number.
    /// </summary>
    /// <param name="guid">A Guid that might be in the number format.</param>
    /// <returns>The number or zero.</returns>
    public static int GuidToInt(Guid? guid)
    {
        int output = 0;

        if (guid.HasValue) {
            try {
                output = Convert.ToInt32(((Guid)guid).ToString().Replace("-", ""));
            } catch { }
        }

        return output;
    }

    /// <summary>
    /// Gets the Guid value or Guid.Empty if there is no value.
    /// </summary>
    /// <param name="value">A nullable Guid value.</param>
    /// <returns>The Guid value or Guid.Empty if there is no value.</returns>
    public static Guid GuidValue(Guid? value)
    {
        var output = value.HasValue ? value.Value : Guid.Empty;
        return output;
    }

    /// <summary>
    /// Gets the Guid value from a string that might contain a Guid.
    /// </summary>
    /// <param name="value">A string that might be a Guid value.</param>
    /// <returns>The Guid value or Guid.Empty if there was no value.</returns>
    public static Guid GuidValue(string? value)
    {
        Guid output = Guid.Empty;

        if (!String.IsNullOrWhiteSpace(value)) {
            try {
                output = new Guid(value);
            } catch { }
        }

        return output;
    }

    /// <summary>
    /// Hides any menus that are showing.
    /// </summary>
    public static async Task HideMenus()
    {
        await jsRuntime.InvokeVoidAsync("HideMenu");
        Model.QuickAction = "";
    }

    /// <summary>
    /// Highlights HTML elements that have a given class name.
    /// </summary>
    /// <param name="className">The class name to find.</param>
    public static async Task HighlightElementByClass(string className)
    {
        await jsRuntime.InvokeVoidAsync("HighlightElementByClass", className);
    }

    /// <summary>
    /// A popup HTML editor
    /// </summary>
    /// <param name="OnEditCompleted">The required Delegate that will receive the HTML when the OK button is clicked.</param>
    /// <param name="HTML">Optional HTML to set in the editor.</param>
    /// <param name="Title">Optional title to override the default title.</param>
    /// <param name="config">Optional HtmlEditorConfiguration object to override default editor options.</param>
    /// <param name="width">Optional width. Leave empty for the default or set to "auto" for the dialog defaults.</param>
    /// <param name="height">Optional width. Leave empty for the default or set to "auto" for the dialog defaults.</param>
    /// <param name="setFocusOnLoad">Option to set the focus to the editor when it loads.</param>
    /// <param name="instructions">Optional instructions to show above the editor.</param>
    public static async Task HtmlEditor(Delegate OnEditCompleted,
        string? HTML = "",
        string? Title = "",
        FreeBlazor.HtmlEditor.HtmlEditorConfiguration? config = null,
        string width = "",
        string height = "",
        bool setFocusOnLoad = true,
        string? instructions = "")
    {
        if (String.IsNullOrWhiteSpace(Title)) {
            Title = Text("EditHTML");
        }

        Dictionary<string, object> parameters = new Dictionary<string, object>();
        parameters.Add("OnEditCompleted", OnEditCompleted);

        if (!String.IsNullOrWhiteSpace(HTML)) {
            parameters.Add("Value", HTML);
        }

        parameters.Add("SetFocusOnLoad", setFocusOnLoad);

        if (config != null) {
            parameters.Add("Config", config);
        }

        if (!String.IsNullOrWhiteSpace(instructions)) {
            parameters.Add("Instructions", instructions);
        }

        string top = String.Empty;

        if (String.IsNullOrWhiteSpace(width)) {
            width = "95%";
            top = "80px";
        }

        if (String.IsNullOrWhiteSpace(height)) {
            height = "calc(100vh - 120px)";
            top = "80px";
        }

        if (width == "auto") {
            width = "";
        }

        if (height == "auto") {
            height = "";
        }

        await DialogService.OpenAsync<HtmlEditorDialog>(Title, parameters, new DialogOptions() {
            AutoFocusFirstElement = false,
            Resizable = false,
            Draggable = false,
            Width = width,
            Height = height,
            Top = top,
        });
    }

    /// <summary>
    /// Returns the value for a given icon.
    /// </summary>
    /// <param name="name">The name of the icon.</param>
    /// <param name="WrapInElement">Option to wrap the icon in an HTML element for rendering.</param>
    /// <returns>The icon.</returns>
    public static string Icon(string? name, bool WrapInElement = false)
    {
        string output = String.Empty;

        if (!String.IsNullOrWhiteSpace(name)) {
            KeyValuePair<string, List<string>> icon = new KeyValuePair<string, List<string>>();

            icon = AppIcons.FirstOrDefault(x => x.Value.Contains(name.Trim(), StringComparer.InvariantCultureIgnoreCase));
            if (String.IsNullOrWhiteSpace(icon.Key)) {
                icon = Icons.FirstOrDefault(x => x.Value.Contains(name.Trim(), StringComparer.InvariantCultureIgnoreCase));
            }

            if (!String.IsNullOrWhiteSpace(icon.Key)) {
                string key = icon.Key;
                string source = String.Empty;

                if (key.ToLower().StartsWith("google:")) {
                    source = "google";
                    key = key.Substring(7);
                } else if (key.ToLower().StartsWith("fa:")) {
                    source = "fa";
                    key = key.Substring(3);
                } else if (key.ToLower().StartsWith("svg:")) {
                    source = "svg";
                    key = key.Substring(4);
                }

                if (WrapInElement) {
                    switch (source) {
                        case "google":
                            output = "<i class=\"icon material-symbols-outlined\">" + key + "</i>";
                            break;

                        case "fa":
                            output = "<i class=\"icon " + key + "\"></i>";
                            break;

                        case "svg":
                            switch (key) {
                                case "linqpad":
                                    output =
@"<span class=""svg-icon""><svg version=""1.1"" id=""Layer_1"" xmlns=""http://www.w3.org/2000/svg"" xmlns:xlink=""http://www.w3.org/1999/xlink"" x=""0px"" y=""0px""
	 viewBox=""0 0 513.4 536"" style=""enable-background:new 0 0 513.4 536;"" xml:space=""preserve"">
<path fill=""currentColor"" stroke=""currentColor"" d=""M84.5,13.4l147.2,21.2c5,0.7,9.4,3.6,12.1,7.9l186.6,298.8l57.5,24.1c7.6,3.2,11.9,11.4,10.1,19.5l-28.2,127
	c-1.7,7.8-8.7,13.4-16.7,13.4h-99.8c-7.1,0-13.5-4.4-16-11.1l-64.9-171.5L238.7,353L168.3,515c-2.7,6.3-8.9,10.3-15.7,10.3H55.8
	c-7.6,0-14.4-5.1-16.5-12.4l-25.2-87.7c-1.7-5.8-0.2-12,3.9-16.4L201,211.5l-20.1-34.8L123,165.2c-5.8-1.2-10.6-5.2-12.7-10.8
	L66,36.4C62.2,25,69.8,12.1,84.5,13.4z M365,491.1h74.2l22.1-99.6l-49.3-20.7c-3.3-1.4-6-3.7-7.9-6.7L218.9,67.4L108.3,51.5
	l30.8,81.9l56.2,11.2c4.8,1,9,4,11.5,8.2l30.2,52.4c3.8,6.5,2.8,14.7-2.3,20.2l-185,199.6l19,66h72.6l69.1-159
	c2-4.6,5.9-8.1,10.7-9.6l56.4-17.1c8.6-2.6,17.8,1.9,21,10.3L365,491.1z""/>
</svg></span>";
                                    break;
                            }
                            break;
                    }


                } else {
                    output = key;
                }
            }
        }

        return output;
    }

    /// <summary>
    /// Renders both an icon and the text.
    /// </summary>
    /// <param name="name">The name of the language/icon element to render.</param>
    /// <returns>The icon and text.</returns>
    public static string IconAndText(string? name)
    {
        string output = String.Empty;

        string icon = Icon(name, true);
        string text = Text(name);

        if (!String.IsNullOrWhiteSpace(icon)) {
            output += icon;
        }

        if (!String.IsNullOrWhiteSpace(text)) {
            if (!String.IsNullOrEmpty(output)) {
                output += " ";
            }
            output += "<span class=\"icon-text\">" + text + "</span>";
        }

        return output;
    }

    /// <summary>
    /// The collection of icons for the application.
    /// </summary>
    public static Dictionary<string, List<string>> Icons {
        get {
            // Icons names are listed as the first item, and any matching text is included in the List<string> object.
            // Icons can come from various sources, so the first part of the icon name indicates the source,
            // then the second part indicates the source (eg: google:home, fa:fa fa-home, etc.)
            Dictionary<string, List<string>> icons = new Dictionary<string, List<string>> {
                { "fa:fa-regular fa-address-card",               new List<string> { "ManageProfile", "ManageProfileInfo" }},
                // {{ModuleItemStart:Appointments}}
                { "fa:fa-regular fa-calendar",                   new List<string> { "AppointmentTypeEvent", "Schedule", "Scheduling" }},
                { "fa:fa-regular fa-calendar-check",             new List<string> { "Now" }},
                { "fa:fa-regular fa-calendar-plus",              new List<string> { "AddAppointment" }},
                // {{ModuleItemEnd:Appointments}}
                { "fa:fa-regular fa-circle",                     new List<string> { "TenantChange" }},
                { "fa:fa-regular fa-circle-check",               new List<string> { "CurrentCredential", "OK", "Select", "Selected", "UserEnabled" }},
                { "fa:fa-regular fa-circle-dot",                 new List<string> { "TenantCurrent" }},
                { "fa:fa-regular fa-file",                       new List<string> { "Files", "ManageFile" }},
                // {{ModuleItemStart:Appointments}}
                { "fa:fa-regular fa-note-sticky",                new List<string> { "AppointmentNoteAdd" }},
                // {{ModuleItemEnd:Appointments}}
                { "fa:fa-regular fa-square",                     new List<string> { "Unchecked" }},
                { "fa:fa-regular fa-square-check",               new List<string> { "Checked" }},
                { "fa:fa-regular fa-square-plus",                new List<string> { "Add", "AddLanguage", "AddNewEmailTemplate", "AddNewUserGroup", "CreateInvoiceForUser", "InvoiceAddItem" }},
                { "fa:fa-regular fa-sun",                        new List<string> { "Theme", "ThemeLight" }},

                { "fa:fa-solid fa-arrows-rotate",                new List<string> { "Refresh" }},
                // {{ModuleItemStart:Services}}
                { "fa:fa-solid fa-bell-concierge",               new List<string> { "AddNewService", "AppointmentAddService", "EditService", "Service", "Services" }},
                // {{ModuleItemEnd:Services}}
                { "fa:fa-solid fa-broom",                        new List<string> { "Clear", "Reset", "ResetLanguageDefaults" }},
                { "fa:fa-solid fa-building-circle-arrow-right",  new List<string> { "AddNewTenant", "NewTenant" }},
                { "fa:fa-solid fa-building-user",                new List<string> { "EditTenant", "Tenants" }},
                { "fa:fa-solid fa-check-double",                 new List<string> { "ValidateConfirmationCode" }},
                { "fa:fa-solid fa-chevron-down",                 new List<string> { "ShowHelp" }},
                { "fa:fa-solid fa-chevron-left",                 new List<string> { "Back", "BackToLogin" }},
                { "fa:fa-solid fa-chevron-right",                new List<string> { "Continue" }},
                { "fa:fa-solid fa-chevron-up",                   new List<string> { "HideHelp" }},
                { "fa:fa-solid fa-circle-half-stroke",           new List<string> { "ThemeAuto" }},
                { "fa:fa-solid fa-circle-info",                  new List<string> { "About", "Info" }},
                { "fa:fa-solid fa-circle-user",                  new List<string> { "ManageAvatar", "User", "UserMenuIcon" }},
                { "fa:fa-solid fa-code",                         new List<string> { "Code", "HTML", "ThemeCustomCssDefault" }},
                // {{ModuleItemStart:EmailTemplates}}
                { "fa:fa-solid fa-envelopes-bulk",               new List<string> { "EmailTemplate", "EmailTemplates" }},
                // {{ModuleItemEnd:EmailTemplates}}
                // {{ModuleItemStart:Invoices}}
                { "fa:fa-solid fa-file-invoice",                 new List<string> { "CreateInvoice", "EditInvoice", "Invoice", "Invoices", "ViewInvoice" }},
                // {{ModuleItemEnd:Invoices}}
                { "fa:fa-solid fa-file-lines",                   new List<string> { "UserDefinedFields" }},
                { "fa:fa-solid fa-file-pdf",                     new List<string> { "DownloadPDF", "PDF" }},
                { "fa:fa-solid fa-filter",                       new List<string> { "AllItems", "ShowFilter" }},
                { "fa:fa-solid fa-filter-circle-xmark",          new List<string> { "HideFilter", "ModifiedItems", "ShowInFilter" }},
                { "fa:fa-solid fa-floppy-disk",                  new List<string> { "Save", "SavingWait" }},
                { "fa:fa-solid fa-home",                         new List<string> { "AppTitle", "Home", "Welcome" }},
                { "fa:fa-solid fa-hourglass-start",              new List<string> { "LoadingWait", "RedirectingToLogin", "UploadingWait" }},
                { "fa:fa-solid fa-image",                        new List<string> { "InsertImage", "Photo" }},
                { "fa:fa-solid fa-key",                          new List<string> { "ChangePassword", "ForgotPassword", "GeneratePassword", "GenerateNewPassword", "PasswordChanged", "UserAdmin" }},
                { "fa:fa-solid fa-language",                     new List<string> { "Language" }},
                // {{ModuleItemStart:Locations}}
                { "fa:fa-solid fa-location-dot",                 new List<string> { "AddNewLocation", "EditLocation", "Locations" }},
                // {{ModuleItemEnd:Locations}}
                { "fa:fa-solid fa-magnifying-glass",             new List<string> { "IncludeInSearch", "Preview", "Search", "View" }},
                { "fa:fa-solid fa-moon",                         new List<string> { "ThemeDark" }},
                { "fa:fa-solid fa-paper-plane",                  new List<string> { "SendTestEmail" }},
                { "fa:fa-solid fa-pen-to-square",                new List<string> { "Edit", "EditAll", "Manage" }},
                { "fa:fa-solid fa-person-digging",               new List<string> { "MaintenanceMode" }},
                { "fa:fa-solid fa-play",                         new List<string> { "Play", "Process", "Reprocess", "TestCode", "TestPlugin" }},
                { "fa:fa-solid fa-repeat",                       new List<string> { "SwitchTenant" }},
                { "fa:fa-solid fa-screwdriver-wrench",           new List<string> { "AppSettings" }},
                { "fa:fa-solid fa-shield-halved",                new List<string> { "AccessDenied" }},
                { "fa:fa-solid fa-sign-in-alt",                  new List<string> { "Log-In", "Login", "LoginTitle", "LoginWithLocalAccount", "Logout" }},
                { "fa:fa-solid fa-signature",                    new List<string> { "SignUp" }},
                { "fa:fa-solid fa-sitemap",                      new List<string> { "AddNewDepartment", "Departments", "EditDepartment" }},
                { "fa:fa-solid fa-sliders",                      new List<string> { "Admin", "Settings" }},
                { "fa:fa-solid fa-table-columns",                new List<string> { "ShowColumn" }},
                { "fa:fa-solid fa-tags",                         new List<string> { "Tags" }},
                { "fa:fa-solid fa-thumbtack",                    new List<string> { "Pinned" }},
                { "fa:fa-solid fa-thumbtack-slash",              new List<string> { "Unpinned" }},
                { "fa:fa-solid fa-trash",                        new List<string> { "ConfirmDelete", "ConfirmDeleteTenant", "Delete", "DeleteAvatar", "DeleteTenant", "PermanentlyDelete", "Remove", "RemoveFile" }},
                { "fa:fa-solid fa-trash-can-arrow-up",           new List<string> { "DeletedRecords", "NoPendingDeletedRecords", "PendingDeletedRecords", "Undelete", "UndeleteRecord" }},
                { "fa:fa-solid fa-unlock-keyhole",               new List<string> { "UpdateAllPasswords" }},
                { "fa:fa-solid fa-upload",                       new List<string> { "UploadFile", "UploadFiles" }},
                { "fa:fa-solid fa-user",                         new List<string> { "EditUser", "Users" }},
                { "fa:fa-solid fa-user-check",                   new List<string> { "RecordsTableIconEnabled" }},
                { "fa:fa-solid fa-user-lock",                    new List<string> { "ResetPassword", "ResetUserPassword", "UpdatePassword" }},
                { "fa:fa-solid fa-user-plus",                    new List<string> { "AddNewUser" }},
                { "fa:fa-solid fa-user-shield",                  new List<string> { "RecordsTableIconAdmin" }},
                { "fa:fa-solid fa-users",                        new List<string> { "ActiveUsers", "AppointmentTypeMeeting", "EditUserGroup", "NewUserGroup", "UserGroups" }},
                { "fa:fa-solid fa-users-rectangle",              new List<string> { "AddNewDepartmentGroup", "DepartmentGroups", "EditDepartmentGroup" }},
                { "fa:fa-solid fa-xmark",                        new List<string> { "Cancel", "Close", "CloseDialog", "Hide" }},
            };

            return icons;
        }
    }

    /// <summary>
    /// Indicates if this helpers class has been initialized.
    /// </summary>
    public static bool Initialized {
        get {
            return _initialized;
        }
    }

    /// <summary>
    /// Uses Javascript Interop to insert a value at the cursor in a given element.
    /// </summary>
    /// <param name="elementId">The id of the element.</param>
    /// <param name="value">The value to insert.</param>
    public static async Task InsertAtCursor(string? elementId, string? value)
    {
        if (!String.IsNullOrWhiteSpace(elementId) && !String.IsNullOrWhiteSpace(value)) {
            await jsRuntime.InvokeVoidAsync("InsertAtCursor", elementId, value);
        }
    }

    /// <summary>
    /// Indicates if the value contains a valid DateTime.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the value is a valid DateTime.</returns>
    public static bool IsDate(string value)
    {
        bool output = false;

        if (!String.IsNullOrWhiteSpace(value)) {
            try {
                DateTime d = Convert.ToDateTime(value);
                output = true;
            } catch { }
        }

        return output;
    }

    /// <summary>
    /// Indicates if the value contains a valid Guid.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the value is a valid Guid.</returns>
    public static bool IsGuid(string value)
    {
        bool output = false;

        if (!String.IsNullOrWhiteSpace(value)) {
            Guid? g = null;
            try {
                g = new Guid(value);
            } catch { }
            output = g != null;
        }

        return output;
    }

    /// <summary>
    /// Indicates if the FileStorage object is for an image file type.
    /// </summary>
    /// <param name="file">The FileStorage object.</param>
    /// <returns>True if the extension indicates this is an image that can be rendered in the browser.</returns>
    public static bool IsImage(DataObjects.FileStorage file)
    {
        bool output = false;

        switch (Helpers.StringValue(file.Extension).ToUpper()) {
            case ".GIF":
            case ".JPG":
            case ".JPEG":
            case ".PNG":
            case ".SVG":
                output = true;
                break;
        }

        return output;
    }

    /// <summary>
    /// Indicates if the value contains a valid integer.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the value contains a valid integer.</returns>
    public static bool IsInt(string value)
    {
        bool output = false;

        if (!String.IsNullOrWhiteSpace(value) && !value.Contains(".")) {
            int? v = null;

            try {
                v = Convert.ToInt32(value);
            } catch { }

            output = v != null;
        }

        return output;
    }

    /// <summary>
    /// Indicates if the value contains a valid number.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the value contains a valid number.</returns>
    public static bool IsNumeric(string? value)
    {
        bool output = false;

        if (!String.IsNullOrWhiteSpace(value)) {
            if (value.Contains(".")) {
                try {
                    decimal d = Convert.ToDecimal(value);
                    output = true;
                } catch { }
            }

            if (!output) {
                try {
                    int i = Convert.ToInt32(value);
                    output = true;
                } catch { }
            }
        }

        return output;
    }

    /// <summary>
    /// Indicates if the FileStorage object contains a PDF.
    /// </summary>
    /// <param name="file">The FileStorage object.</param>
    /// <returns>True if the file is a PDF file.</returns>
    public static bool IsPDF(DataObjects.FileStorage file)
    {
        bool output = false;

        switch (Helpers.StringValue(file.Extension).ToUpper()) {
            case ".PDF":
                output = true;
                break;
        }

        return output;
    }

    /// <summary>
    /// Indicates if the FileStorage object contains a text file.
    /// </summary>
    /// <param name="file">The FileStorage object.</param>
    /// <returns>True if the file is a text file.</returns>
    public static bool IsTextFile(DataObjects.FileStorage file)
    {
        bool output = false;

        switch (Helpers.StringValue(file.Extension).ToUpper()) {
            case ".TXT":
                output = true;
                break;
        }

        return output;
    }

    /// <summary>
    /// Builds the HTML for the Last Modified element.
    /// </summary>
    /// <param name="Added">The Added DateTime? value.</param>
    /// <param name="AddedBy">The AddedBy string? value.</param>
    /// <param name="LastModified">The LastModified DateTime? value.</param>
    /// <param name="LastModifiedBy">The LastModifiedBy? string value.</param>
    /// <returns>The HTML for the Last Modified element.</returns>
    public static string LastModifiedInfo(DateTime? Added = null, string? AddedBy = "", DateTime? LastModified = null, string? LastModifiedBy = "")
    {
        string output = String.Empty;

        if ((Added.HasValue && Added != DateTime.MinValue) || !String.IsNullOrWhiteSpace(AddedBy)) {
            output +=
                "<span class=\"added-text\">" + Text("Added") + "</span>&nbsp;";


            if (Added.HasValue) {
                output += "<span class=\"datetime-added\">" + FormatDateTime(Added) + "</span>&nbsp;";
            }

            if (!String.IsNullOrWhiteSpace(AddedBy)) {
                output +=
                    "<span class=\"text-addedBy\">" + Text("AddedBy") + "</span>&nbsp;" +
                    "<span class=\"added-by-user\">" + AddedBy + "</span>";
            }
        }

        if ((LastModified.HasValue && LastModified != DateTime.MinValue) || !String.IsNullOrWhiteSpace(LastModifiedBy)) {
            if (!String.IsNullOrWhiteSpace(output)) {
                output += "<span>&nbsp;-&nbsp;</span>";
            }

            output += "<span class=\"text-lastModified\">" + Text("LastModified") + "</span>&nbsp;";

            if (LastModified.HasValue) {
                output += "<span class=\"datetime-lastModified\">" + FormatDateTime(LastModified) + "</span>&nbsp;";
            }

            if (!String.IsNullOrWhiteSpace(LastModifiedBy)) {
                output += "<span class=\"last-modified-by\">" + Text("LastModifiedBy") + LastModifiedBy + "</span>";
            }
        }

        if (!String.IsNullOrWhiteSpace(output)) {
            return "<div class=\"last-modified\">" + output + "</div>";
        } else {
            return String.Empty;
        }
    }

    /// <summary>
    /// Counts the number of lines in a given string.
    /// </summary>
    /// <param name="input">The string to check.</param>
    /// <param name="MinimumLines">Option to always return a minimum value.</param>
    /// <returns>The number of lines in the string, or the minimum value if specified and greater.</returns>
    public static int LinesInString(string? input, int MinimumLines = 0)
    {
        int output = 0;

        if (!String.IsNullOrWhiteSpace(input)) {
            output = Regex.Matches(input, "\n").Count + 1;
        }

        if (output < MinimumLines) {
            output = MinimumLines;
        }

        if (output < 0) {
            output = 0;
        }

        return output;
    }

    /// <summary>
    /// Converts a list of strings containing Guids to a list of Guids
    /// </summary>
    /// <param name="guids">The list of strings containing Guids.</param>
    /// <returns>A list of Guids.</returns>
    public static List<Guid> ListOfGuidStringsToListOfGuid(List<string>? guids)
    {
        List<Guid> output = new List<Guid>();

        if (guids != null && guids.Any()) {
            foreach (var item in guids) {
                Guid? g = null;

                try {
                    g = new Guid(item);
                } catch { }

                if (g != null) {
                    output.Add((Guid)g);
                }
            }
        }

        return output;
    }

    /// <summary>
    /// Converts an object that is a list of objects to a typed list of those objects.
    /// </summary>
    /// <param name="objects">The object that is a list of a typed object.</param>
    /// <returns>A list of the object type.</returns>
    public static object? ListOfObjectsToTypedList(List<object>? objects)
    {
        object? output = null;

        if (objects != null && objects.Count > 0) {
            var type = objects[0].GetType();

            if (type == typeof(String)) {
                var list = new List<string>();
                foreach (var item in objects) {
                    list.Add(String.Empty + item);
                }
                output = list;
            } else if (type == typeof(int) || type == typeof(Int16) || type == typeof(Int32)) {
                var list = new List<Int32>();
                foreach (Int32 item in objects) {
                    list.Add(item);
                }
                output = list;
            } else if (type == typeof(Int64)) {
                var list = new List<Int64>();
                foreach (Int64 item in objects) {
                    list.Add(item);
                }
                output = list;
            } else if (type == typeof(bool) || type == typeof(Boolean)) {
                var list = new List<bool>();
                foreach (bool item in objects) {
                    list.Add(item);
                }
                output = list;
            } else if (type == typeof(decimal)) {
                var list = new List<decimal>();
                foreach (decimal item in objects) {
                    list.Add(item);
                }
                output = list;
            } else if (type == typeof(long)) {
                var list = new List<long>();
                foreach (long item in objects) {
                    list.Add(item);
                }
                output = list;
            } else if (type == typeof(double)) {
                var list = new List<double>();
                foreach (double item in objects) {
                    list.Add(item);
                }
                output = list;
            } else if (type == typeof(float)) {
                var list = new List<float>();
                foreach (float item in objects) {
                    list.Add(item);
                }
                output = list;
            } else if (type == typeof(Single)) {
                var list = new List<Single>();
                foreach (Single item in objects) {
                    list.Add(item);
                }
                output = list;
            }
        }

        return output;
    }

    /// <summary>
    /// Converts a List of strings to a CSV string.
    /// </summary>
    /// <param name="items">A List of strings.</param>
    /// <returns>The list formatted as CSV.</returns>
    public static string ListOfStringsToCsv(List<string>? items)
    {
        string output = String.Empty;

        if (items != null && items.Any()) {
            foreach (var item in items) {
                if (!String.IsNullOrWhiteSpace(item)) {
                    if (output != String.Empty) {
                        output += ", ";
                    }
                    output += item.Trim();
                }
            }
        }

        return output;
    }

    /// <summary>
    /// Converts a nullable list of strings to an array.
    /// </summary>
    /// <param name="values">A nullable list of strings.</param>
    /// <returns>An array with any valid values.</returns>
    public static string[] ListToArray(List<string>? values)
    {
        string[] output = new string[] { };

        if (values != null && values.Any()) {
            output = values.ToArray();
        }

        return output;
    }

    /// <summary>
    /// Loads an ActiveUser item into the Model.ActiveUsers for the given userId.
    /// </summary>
    /// <param name="userId">The unique id of the user.</param>
    public async static Task LoadActiveUser(Guid? userId)
    {
        if (GuidValue(userId) != Guid.Empty) {
            var activeUser = await GetOrPost<DataObjects.ActiveUser>("api/Data/GetActiveUser/" + userId.ToString());
            if (activeUser != null && activeUser.UserId == userId) {
                var activeUsers = Model.ActiveUsers.ToList();
                activeUsers = activeUsers.Where(x => x.UserId != userId).ToList();
                activeUsers.Add(activeUser);
                Model.ActiveUsers = activeUsers.ToList();
            }
        }
    }

    /// <summary>
    /// Loads the list of active users from the API endpoint.
    /// </summary>
    public async static Task LoadActiveUsers()
    {
        var items = await GetOrPost<List<DataObjects.ActiveUser>>("api/Data/GetActiveUsers");
        Model.ActiveUsers = items != null && items.Any() ? items : new List<DataObjects.ActiveUser>();
    }

    /// <summary>
    /// Loads the default language from the API endpoint.
    /// </summary>
    public async static Task LoadDefaultLanguage()
    {
        var language = await Helpers.GetOrPost<DataObjects.Language>("api/Data/GetDefaultLanguage");
        if (language != null) {
            Model.DefaultLanguage = language;
        }
    }

    /// <summary>
    /// Loads the Department Groups from the API endpoint.
    /// </summary>
    public async static Task LoadDepartmentGroups()
    {
        var items = await GetOrPost<List<DataObjects.DepartmentGroup>>("api/Data/GetDepartmentGroups");
        Model.DepartmentGroups = items != null && items.Any() ? items : new List<DataObjects.DepartmentGroup>();
    }

    /// <summary>
    /// Loads the Departments from the API endpoint.
    /// </summary>
    public async static Task LoadDepartments()
    {
        var items = await GetOrPost<List<DataObjects.Department>>("api/Data/GetDepartments");
        Model.Departments = items != null && items.Any() ? items : new List<DataObjects.Department>();
    }

    /// <summary>
    /// Loads the Image Files from the API endpoint.
    /// </summary>
    public async static Task LoadImageFiles()
    {
        var items = await GetOrPost<List<DataObjects.FileStorage>>("api/Data/GetImageFiles");
        Model.ImageFiles = items != null && items.Any() ? items : new List<DataObjects.FileStorage>();
    }

    // {{ModuleItemStart:Locations}}
    /// <summary>
    /// Loads the locations from the API endpoint.
    /// </summary>
    public async static Task LoadLocations()
    {
        var items = await GetOrPost<List<DataObjects.Location>>("api/Data/GetLocations");
        Model.Locations = items != null && items.Any() ? items : new List<DataObjects.Location>();
    }
    // {{ModuleItemEnd:Locations}}

    // {{ModuleItemStart:Services}}
    /// <summary>
    /// Loads the services from the API endpoints.
    /// </summary>
    public async static Task LoadServices()
    {
        var items = await GetOrPost<List<DataObjects.Service>>("api/Data/GetServices");
        Model.Services = items != null && items.Any() ? items : new List<DataObjects.Service>();
    }
    // {{ModuleItemEnd:Services}}

    // {{ModuleItemStart:Tags}}
    /// <summary>
    /// Loads the Tags from the API endpoint.
    /// </summary>
    public async static Task LoadTags()
    {
        var items = await GetOrPost<List<DataObjects.Tag>>("api/Data/GetTags");
        Model.Tags = items != null && items.Any() ? items : new List<DataObjects.Tag>();
    }
    // {{ModuleItemEnd:Tags}}

    /// <summary>
    /// Loads the Tenant List from the API endpoint.
    /// </summary>
    public async static Task LoadTenantList()
    {
        var items = await GetOrPost<List<DataObjects.TenantList>>("api/Data/GetTenantList");
        Model.TenantList = items != null && items.Any() ? items : new List<DataObjects.TenantList>();
    }

    /// <summary>
    /// Loads the User-Defined Field Labels from the API endpoint.
    /// </summary>
    public async static Task LoadUdfLabels()
    {
        var items = await GetOrPost<List<DataObjects.udfLabel>>("api/Data/GetUdfLabels");
        Model.udfLabels = items != null && items.Any() ? items : new List<DataObjects.udfLabel>();
    }

    /// <summary>
    /// Loads the User Groups from the API endpoint.
    /// </summary>
    public async static Task LoadUserGroups()
    {
        var items = await GetOrPost<List<DataObjects.UserGroup>>("api/Data/GetUserGroups");
        Model.UserGroups = items != null && items.Any() ? items : new List<DataObjects.UserGroup>();
    }

    /// <summary>
    /// Trims a string to a maximum length.
    /// </summary>
    /// <param name="input">The string to trim.</param>
    /// <param name="maxLength">The maximum length for the string.</param>
    /// <param name="addEllipses">Option to add ellipses to trimmed strings to indicate they were trimmed.</param>
    /// <returns>The original string if it's under the max length, or a trimmed string to the maximum length.</returns>
    public static string MaxStringLength(string? input, int maxLength = 100, bool addEllipses = true)
    {
        string output = String.Empty;

        if (!String.IsNullOrWhiteSpace(input)) {
            output = input;

            if (output.Length > maxLength) {
                output = output.Substring(0, maxLength);
                if (addEllipses) {
                    output += "...";
                }
            }
        }

        return output;
    }

    /// <summary>
    /// Converts an array of strings to a message.
    /// If a single string is in the array only the text is returned.
    /// If multiple items are in the array an <ul> element is returned with <li> elements for each text item.
    /// </summary>
    /// <param name="messages">The messages collection.</param>
    /// <returns>Returns a single string if the array only contained one string, or a <ul> with <li> elements for each string.</returns>
    public static string MessagesToString(List<string>? messages)
    {
        string output = String.Empty;

        if (messages != null && messages.Any()) {
            if (messages.Count() == 1) {
                output += messages.First();
            } else {
                output += "<ul>";

                foreach (var message in messages) {
                    output += "<li>" + message + "</li>";
                }

                output += "</ul>";
            }
        }

        return output;
    }

    /// <summary>
    /// Converts a MessageType to the Bootstrap badge class for that type.
    /// </summary>
    /// <param name="MessageType">The MessageType</param>
    /// <returns>The Bootstrap badge class.</returns>
    public static string MessageTypeToBadgeClass(MessageType MessageType)
    {
        string output = String.Empty;

        switch (MessageType) {
            case MessageType.Danger:
                output = "badge text-bg-danger";
                break;

            case MessageType.Dark:
                output = "badge text-bg-dark";
                break;

            case MessageType.Info:
                output = "badge text-bg-info";
                break;

            case MessageType.Light:
                output = "badge text-bg-light";
                break;

            case MessageType.Primary:
                output = "badge text-bg-primary";
                break;

            case MessageType.Secondary:
                output = "badge text-bg-secondary";
                break;

            case MessageType.Success:
                output = "badge text-bg-success";
                break;

            case MessageType.Warning:
                output = "badge text-bg-warning";
                break;
        }

        return output;
    }

    /// <summary>
    /// Formats the minutes for an item in a friendly format.
    /// </summary>
    /// <param name="minutes">The minutes as a double.</param>
    /// <returns>The minutes formatting in a friendly format.</returns>
    public static string MinutesToYearsMonthsDaysHoursAndMinutes(double? minutes)
    {
        string output = String.Empty;

        if (minutes.HasValue) {
            output = TimeSpan.FromMinutes((double)minutes).Humanize(maxUnit: TimeUnit.Year, precision: 5);
        }

        return output;
    }

    /// <summary>
    /// The missing required label for a given field.
    /// </summary>
    /// <param name="fieldName">The field name.</param>
    /// <returns>The missing required label for the given field.</returns>
    public static string MissingRequiredField(string fieldName)
    {
        string output = Text("RequiredMissing", false, new List<string> { fieldName });
        return output;
    }

    /// <summary>
    /// The style used to indicate a field is missing a value when applying via a style instead of a class.
    /// </summary>
    /// <returns>The style.</returns>
    public static string MissingRequiredFieldStyle {
        get {
            return "background-color: palevioletred; border-color: darkred; color: #fff;";
        }
    }

    /// <summary>
    /// Returns the class that marks a field as missing if no value is provided.
    /// </summary>
    /// <param name="value">A nullable DateTime value.</param>
    /// <param name="defaultClass">An optional default class to append to the output.</param>
    /// <returns>The class value.</returns>
    public static string MissingValue(DateTime? value, string? defaultClass = "")
    {
        if (String.IsNullOrWhiteSpace(defaultClass)) {
            return !value.HasValue ? MissingValueClass : "";
        } else {
            return !value.HasValue ? MissingValueClass + " " + defaultClass : defaultClass;
        }
    }

    /// <summary>
    /// Returns the class that marks a field as missing if no value is provided.
    /// </summary>
    /// <param name="value">A nullable int value.</param>
    /// <param name="defaultClass">An optional default class to append to the output.</param>
    /// <returns>The class value.</returns>
    public static string MissingValue(decimal? value, string? defaultClass = "")
    {
        if (String.IsNullOrWhiteSpace(defaultClass)) {
            return value.HasValue && value.Value > 0 ? "" : MissingValueClass;
        } else {
            return value.HasValue && value.Value > 0 ? defaultClass : MissingValueClass + " " + defaultClass;
        }
    }

    /// <summary>
    /// Returns the class that marks a field as missing if no value is provided.
    /// </summary>
    /// <param name="value">A nullable Guid value.</param>
    /// <param name="defaultClass">An optional default class to append to the output.</param>
    /// <returns>The class value.</returns>
    public static string MissingValue(Guid? value, string? defaultClass = "")
    {
        if (String.IsNullOrWhiteSpace(defaultClass)) {
            return GuidValue(value) == Guid.Empty ? MissingValueClass : "";
        } else {
            return GuidValue(value) == Guid.Empty ? MissingValueClass + " " + defaultClass : defaultClass;
        }

    }

    /// <summary>
    /// Returns the class that marks a field as missing if no value is provided.
    /// </summary>
    /// <param name="value">A nullable int value.</param>
    /// <param name="defaultClass">An optional default class to append to the output.</param>
    /// <returns>The class value.</returns>
    public static string MissingValue(int? value, string? defaultClass = "")
    {
        if (String.IsNullOrWhiteSpace(defaultClass)) {
            return value.HasValue && value.Value > 0 ? "" : MissingValueClass;
        } else {
            return value.HasValue && value.Value > 0 ? defaultClass : MissingValueClass + " " + defaultClass;
        }
    }

    /// <summary>
    /// Returns the class that marks a field as missing if no value is provided.
    /// </summary>
    /// <param name="value">A nullable string value.</param>
    /// <param name="defaultClass">An optional default class to append to the output.</param>
    /// <returns>The class value.</returns>
    public static string MissingValue(string? value, string? defaultClass = "")
    {
        if (String.IsNullOrWhiteSpace(defaultClass)) {
            return String.IsNullOrWhiteSpace(value) ? MissingValueClass : "";
        } else {
            return String.IsNullOrWhiteSpace(value) ? MissingValueClass + " " + defaultClass : defaultClass;
        }
    }

    /// <summary>
    /// The name of the CSS class that marks a field as missing if no value is provided.
    /// </summary>
    public static string MissingValueClass {
        get {
            return "m-r";
        }
    }

    /// <summary>
    /// Closes the modal dialog.
    /// </summary>
    public static void ModalClose()
    {
        DialogService.Close();
    }

    /// <summary>
    /// Displays a modal message.
    /// </summary>
    /// <param name="message">The message to show.</param>
    /// <param name="title">An optional title for the dialog.</param>
    /// <param name="DisableClose">Option to disable the close button for the dialog.</param>
    /// <param name="width">Optional width for the dialog (defaults to auto-sized.)</param>
    /// <param name="height">Optional height for the dialog (defaults to auto-sized.)</param>
    public async static Task ModalMessage(string message, string title = "", bool DisableClose = false, string width = "auto", string height = "auto")
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>();
        parameters.Add("Message", message);

        if (width == "auto") {
            width = "";
        }

        if (height == "auto") {
            height = "";
        }

        await DialogService.OpenAsync<ModalMessage>(title, parameters, new Radzen.DialogOptions() {
            AutoFocusFirstElement = false,
            Resizable = false,
            Draggable = false,
            CloseDialogOnEsc = !DisableClose,
            ShowClose = !DisableClose,
            Width = width,
            Height = height,
        });
    }

    /// <summary>
    /// Navigates to an application URL.
    /// </summary>
    /// <param name="subUrl">The sub-page to navigate to, otherwise, navigates to the root.</param>
    /// <param name="forceReload">Option to force a full reload on navigate.</param>
    public static void NavigateTo(string subUrl, bool forceReload = false)
    {
        if (subUrl.ToLower().StartsWith("http:") || subUrl.ToLower().StartsWith("https:")) {
            NavManager.NavigateTo(subUrl, forceReload);
        } else {
            NavManager.NavigateTo(Model.ApplicationUrlFull + subUrl, forceReload);
        }
    }

    /// <summary>
    /// Navigates to the login page.
    /// </summary>
    /// <param name="forceReload">Option to force a full reload on navigate.</param>
    public static void NavigateToLogin(bool forceReload = false)
    {
        string loginUrl = String.Empty;

        if (!String.IsNullOrWhiteSpace(Model.Tenant.TenantSettings.ApplicationUrl)) {
            loginUrl = Model.Tenant.TenantSettings.ApplicationUrl;
            if (!loginUrl.EndsWith("/")) {
                loginUrl += "/";
            }

            if (Model.UseTenantCodeInUrl) {
                loginUrl += Model.Tenant.TenantCode + "/";
            }

            loginUrl += "Login";
        }

        if (String.IsNullOrWhiteSpace(loginUrl)) {
            loginUrl = Model.ApplicationUrlFull + "Login";
        }

        NavManager.NavigateTo(loginUrl, forceReload);
    }

    /// <summary>
    /// Navigates to the root of the application.
    /// </summary>
    /// <param name="forceReload">Option to force a full reload on navigate.</param>
    public static void NavigateToRoot(bool forceReload = false)
    {
        _validatingUrl = false;
        NavManager.NavigateTo(Model.ApplicationUrlFull, forceReload);
    }

    /// <summary>
    /// Navigates to the root of the application, regardless of any tenant-specific ApplicationUrl setting.
    /// </summary>
    /// <param name="forceReload">Option to force a full reload on navigate.</param>
    public static void NavigateToRootDefault(bool forceReload = false)
    {
        _validatingUrl = false;
        NavManager.NavigateTo(Model.ApplicationUrlFullDefault, forceReload);
    }

    /// <summary>
    /// Navigates to a given url using javascript.
    /// </summary>
    /// <param name="url">The url to navigate to.</param>
    public async static Task NavigateToViaJavascript(string url)
    {
        await jsRuntime.InvokeVoidAsync("NavigateTo", url);
    }

    /// <summary>
    /// Returns only the numbers found in a given string.
    /// </summary>
    /// <param name="input">The string to parse.</param>
    /// <returns>A string with only numbers from the given string.</returns>
    public static string NumbersOnly(string? input)
    {
        string output = String.Empty;

        if (!String.IsNullOrWhiteSpace(input)) {
            foreach (char c in input) {
                switch (c) {
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        output += c.ToString();
                        break;
                }
            }
        }

        return output;
    }

    /// <summary>
    /// Converts an object to an HTML table.
    /// </summary>
    /// <param name="o">The object to render.</param>
    /// <param name="addClassToTable">An optional class to add to the table.</param>
    /// <param name="addHeaderRowClass">An optional class to add to table rows that contain TH elements.</param>
    /// <param name="sortByPropertyNames">Option to sort the property names alphabetically (defaults to true.)</param>
    /// <returns></returns>
    public static string ObjectToTable(object? o, string addClassToTable = "", string addHeaderRowClass = "", bool sortByPropertyNames = true)
    {
        var output = new System.Text.StringBuilder();

        if (o != null) {
            // If the object is enumerable draw a table with multiple rows.
            // Otherwise, draw rows for each property.
            var type = o.GetType();

            if (o is IEnumerable) {
                if (!String.IsNullOrWhiteSpace(addClassToTable)) {
                    output.AppendLine("<table class=\"" + addClassToTable + "\">");
                } else {
                    output.AppendLine("<table>");
                }

                output.AppendLine("<thead>");

                if (!String.IsNullOrWhiteSpace(addHeaderRowClass)) {
                    output.AppendLine("<tr class=\"" + addHeaderRowClass + "\">");
                } else {
                    output.AppendLine("<tr>");
                }

                var dynamicO = (dynamic)o;
                var firstItem = dynamicO[0];

                var properties = firstItem.GetType().GetProperties();
                var propertyNames = new List<string>();

                foreach (var prop in properties) {
                    propertyNames.Add(prop.Name);
                }

                if (sortByPropertyNames) {
                    propertyNames = propertyNames.OrderBy(x => x).ToList();
                }

                foreach (var prop in propertyNames) {
                    output.AppendLine("<th>" + prop + "</th>");
                }

                output.AppendLine("</tr>");
                output.AppendLine("</thead>");
                output.AppendLine("<tbody>");

                foreach (var row in (IEnumerable)o) {
                    output.AppendLine("<tr>");

                    foreach (var prop in propertyNames) {
                        output.AppendLine("<td>");

                        var value = ObjectToTableGetPropertyValue(row, prop);
                        if (value != null) {
                            var thisType = value.GetType();

                            if (thisType.IsGenericType) {
                                var baseType = thisType.GetGenericArguments()[0];
                                if (!baseType.ToString().ToLower().StartsWith("system.")) {
                                    thisType = baseType;
                                }
                            }

                            var thisTypeString = thisType.ToString().ToLower();

                            if (thisTypeString.StartsWith("system.collections.generic.list")) {
                                output.AppendLine(ObjectToTableListAsDIVs(value));
                            } else if (thisTypeString.StartsWith("system.")) {
                                output.AppendLine(value.ToString());
                            } else if (value is IList || thisType.IsArray) {
                                output.AppendLine(ObjectToTable(value, addClassToTable, addHeaderRowClass, sortByPropertyNames));
                            } else if (value.GetType().GetProperties().Count() > 1) {
                                output.AppendLine(ObjectToTable(value, addClassToTable, addHeaderRowClass, sortByPropertyNames));
                            } else {
                                output.AppendLine(value.ToString());
                            }
                        }

                        output.AppendLine("</td>");
                    }

                    output.AppendLine("</tr>");
                }

                output.AppendLine("</tbody>");
                output.AppendLine("</table>");
            } else {
                if (!String.IsNullOrWhiteSpace(addClassToTable)) {
                    output.AppendLine("<table class=\"" + addClassToTable + "\">");
                } else {
                    output.AppendLine("<table>");
                }

                output.AppendLine("<tbody>");

                foreach (var prop in type.GetProperties()) {
                    output.AppendLine("<tr>");
                    output.AppendLine("<td><b>" + prop.Name + "</b></td>");
                    output.AppendLine("<td>");

                    var value = prop.GetValue(o);

                    if (value != null) {
                        var thisType = value.GetType();

                        if (thisType.IsGenericType) {
                            var baseType = thisType.GetGenericArguments()[0];
                            if (!baseType.ToString().ToLower().StartsWith("system.")) {
                                thisType = baseType;
                            }
                        }

                        var thisTypeString = thisType.ToString().ToLower();

                        if (thisTypeString.StartsWith("system.collections.generic.list")) {
                            output.AppendLine(ObjectToTableListAsDIVs(value));
                        } else if (thisTypeString.StartsWith("system.")) {
                            output.AppendLine(value.ToString());
                        } else if (value is IList || thisType.IsArray) {
                            output.AppendLine(ObjectToTable(value, addClassToTable, addHeaderRowClass, sortByPropertyNames));
                        } else if (value.GetType().GetProperties().Count() > 1) {
                            output.AppendLine(ObjectToTable(value, addClassToTable, addHeaderRowClass, sortByPropertyNames));
                        } else {
                            output.AppendLine(value.ToString());
                        }
                    }

                    output.AppendLine("</td>");
                    output.AppendLine("</tr>");
                }

                output.AppendLine("</tbody>");
                output.AppendLine("</table>");
            }
        }

        return output.ToString();
    }

    /// <summary>
    /// Used by the ObjectToTable function to get the value of an object property.
    /// </summary>
    /// <param name="o">The object.</param>
    /// <param name="property">The property to return.</param>
    /// <returns>The property value if it exists, or null.</returns>
    private static object? ObjectToTableGetPropertyValue(object? o, string property)
    {
        if (o != null) {
            foreach (String part in property.Split(".")) {
                Type type = o.GetType();

                System.Reflection.BindingFlags bindingAttrs = System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.Static |
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.IgnoreCase;

                var info = type.GetProperty(part, bindingAttrs);
                if (info != null) {
                    var obj = info.GetValue(o, null);
                    if (obj != null) {
                        return obj;
                    }
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Used by the ObjectToTable function to convert a list of objects to a list of DIV elements.
    /// </summary>
    /// <param name="o">An object that contains an enumerable collection.</param>
    /// <returns>The items in individual DIV elements.</returns>
    public static string ObjectToTableListAsDIVs(object? o)
    {
        var output = new System.Text.StringBuilder();

        if (o != null) {
            try {
                var type = o.GetType();

                var dyn = (dynamic)o;
                foreach (var item in dyn) {
                    output.AppendLine("<div>" + item + "</div>");
                }
            } catch { }
        }

        return output.ToString();
    }

    /// <summary>
    /// A method that can be used to update and object property when a field changes and passes a value instead of an ChangeEventArgs parameter.
    /// </summary>
    /// <param name="objectToUpdate">The object to be updated.</param>
    /// <param name="propertyToUpdate">The name of the property to update.</param>
    /// <param name="valueToSet">The value to be set in the specific property.</param>
    /// <param name="onChangeComplete">An optional delegate to be called when the update is complete.</param>
    public static void OnChangeHandler(object objectToUpdate, string propertyToUpdate, object? valueToSet, Delegate? onChangeComplete = null)
    {
        SetObjectPropertyValue(objectToUpdate, propertyToUpdate, valueToSet);

        if (onChangeComplete != null) {
            onChangeComplete.DynamicInvoke();
        }
    }

    /// <summary>
    /// A method that can be used to update an object property when a field changes and optionally can fire a method when the change is complete.
    /// </summary>
    /// <param name="objectToUpdate">The object to be updated.</param>
    /// <param name="propertyToUpdate">The name of the property to update.</param>
    /// <param name="e">The ChangeEventArgs from the OnChange event.</param>
    /// <param name="onChangeComplete">An optional delegate to be called when the update is complete.</param>
    public static void OnChangeHandler<T>(object objectToUpdate, string propertyToUpdate, ChangeEventArgs e, Delegate? onChangeComplete = null)
    {
        var value = GetChangeEventArgValue<T>(e);

        SetObjectPropertyValue(objectToUpdate, propertyToUpdate, value);

        if (onChangeComplete != null) {
            onChangeComplete.DynamicInvoke();
        }
    }

    /// <summary>
    /// Gets the current date and time in UTC.
    /// </summary>
    public static DateTime Now {
        get {
            var output = Convert.ToDateTime(DateTime.UtcNow.ToString("o"));
            return output;
        }
    }

    /// <summary>
    /// Parses simple flat XML to get a value.
    /// </summary>
    /// <param name="xml">The XML to search.</param>
    /// <param name="find">The name of the element to find.</param>
    /// <returns>The value between the opening and closing tags for the given element.</returns>
    public static string ParseXML(string? xml, string? find)
    {
        if (string.IsNullOrEmpty(xml) == true) { return String.Empty; }
        if (string.IsNullOrEmpty(find) == true) { return String.Empty; }

        int iStart = xml.ToLower().IndexOf("<" + find.ToLower() + ">");
        if (iStart == -1) { return String.Empty; }
        iStart = xml.ToLower().IndexOf(">", iStart + 1);
        if (iStart == -1) { return String.Empty; }
        int iEnd = xml.ToLower().IndexOf("</" + find.ToLower() + ">", iStart + 1);
        if (iEnd == -1) { return String.Empty; }

        return xml.Substring(iStart + 1, iEnd - iStart - 1);
    }

    /// <summary>
    /// Display a PDF in the PDF Viewer.
    /// </summary>
    /// <param name="FileId">The unique id of the file.</param>
    /// <param name="Title">An optional title to show in the dialog (Defaults to the PDFViewer language element).</param>
    /// <param name="width">An optional width for the dialog.</param>
    /// <param name="height">An optional height for the dialog.</param>
    /// <param name="AllowDownload">An option to show a button to download the PDF.</param>
    public static async Task PdfViewer(Guid FileId, string? Title = "", string width = "", string height = "", bool AllowDownload = true)
    {
        if (String.IsNullOrWhiteSpace(Title)) {
            Title = Text("PDFViewer");
        }

        Dictionary<string, object> parameters = new Dictionary<string, object>();
        parameters.Add("AllowDownload", AllowDownload);
        parameters.Add("FileId", FileId);
        parameters.Add("PdfFile", Model.ApplicationUrl + "File/View/" + FileId.ToString() + "?TenantId=" + Model.TenantId.ToString() + "&Token=" + Model.User.AuthToken);

        string top = String.Empty;

        if (String.IsNullOrWhiteSpace(width)) {
            width = "95%";
            top = "80px";
        }

        if (String.IsNullOrWhiteSpace(height)) {
            height = "calc(100vh - 120px)";
            top = "80px";
        }

        if (width == "auto") {
            width = "";
        }

        if (height == "auto") {
            height = "";
        }

        await DialogService.OpenAsync<PDF_Viewer>(Title, parameters, new DialogOptions() {
            AutoFocusFirstElement = false,
            Resizable = false,
            Draggable = false,
            Width = width,
            Height = height,
            Top = top,
        });
    }

    /// <summary>
    /// Gets the value for a given option value for a prompt.
    /// </summary>
    /// <param name="prompt">The prompt.</param>
    /// <param name="label">The label of the option item.</param>
    /// <returns>The value for the option item, or an empty string.</returns>
    public static string PluginPromptOptionValue(PluginPrompt prompt, string? label)
    {
        string output = String.Empty;

        if (!String.IsNullOrWhiteSpace(label) && prompt.Options != null && prompt.Options.Count > 0) {
            var option = prompt.Options.FirstOrDefault(x => x.Label.ToLower() == label.ToLower());
            if (option != null) {
                output += option.Value;
            }
        }

        return output;
    }

    /// <summary>
    /// Converts the results of executing a plugin to a friendly string.
    /// </summary>
    /// <param name="result">The PluginExecuteResult object.</param>
    /// <param name="includeObjects">An option to include objects in the output (defaults to false.)</param>
    /// <returns>The results of the plugin execution formatted as a string.</returns>
    public static string PluginResultToString(Plugins.PluginExecuteResult result, bool includeObjects = false)
    {
        System.Text.StringBuilder output = new StringBuilder();

        output.AppendLine(Text("Result") + ": " + result.Result.ToString().ToLower());

        if (result.Messages != null && result.Messages.Count > 0) {
            output.AppendLine("");
            output.AppendLine((result.Messages.Count == 1 ? Text("Message") : Text("Messages")) + ":");
            foreach (var msg in result.Messages) {
                output.AppendLine(" - " + msg);
            }
        }

        if (result.Objects != null && result.Objects.Count() > 0) {
            var objects = result.Objects.ToList();
            output.AppendLine("");

            int index = -1;

            if (includeObjects) {
                output.AppendLine(Text("Objects"));

                foreach (var o in objects) {
                    index++;
                    var json = SerializeObject(o);

                    if (!String.IsNullOrWhiteSpace(json) && json != "[]") {
                        output.AppendLine("");
                        output.AppendLine(Text("Object") + " " + index.ToString() + ":");
                        output.AppendLine(json);
                    }
                }
            }
        }

        return output.ToString();
    }

    // {{ModuleItemStart:Invoices}}
    /// <summary>
    /// Gets the preview for an invoice.
    /// </summary>
    /// <param name="invoice">The invoice to preview.</param>
    public async static Task PreviewInvoice(DataObjects.Invoice invoice)
    {
        Model.ClearMessages();
        Model.Message_Loading();

        var preview = await Helpers.GetOrPost<DataObjects.Invoice>("api/Data/GenerateInvoiceImages", invoice);

        Model.ClearMessages();

        if (preview != null) {
            if (preview.ActionResponse.Result) {
                System.Text.StringBuilder html = new System.Text.StringBuilder();

                if (preview.Images != null && preview.Images.Any()) {
                    foreach (var image in preview.Images) {
                        html.AppendLine("<div class=\"mb-2\">");
                        html.AppendLine("  <img src=\"data:image/jpg;base64," + Convert.ToBase64String(image) + "\" style=\"width:100%;\" />");
                        html.AppendLine("</div>");
                    }
                } else {
                    html.AppendLine(Helpers.Text("NoItemsToShow"));
                }

                await Helpers.ModalMessage(html.ToString(), Helpers.Text("Preview"));

            } else {
                Model.ErrorMessages(preview.ActionResponse.Messages);
            }
        } else {
            Model.UnknownError();
        }
    }
    // {{ModuleItemEnd:Invoices}}

    /// <summary>
    /// Opens a quick action slideout.
    /// </summary>
    /// <param name="Action">The name of the action (AddUser or AppointmentNote)</param>
    /// <param name="OnComplete">An optional Delegate to be invoked after the action has completed.</param>
    public async static Task QuickAction(string Action, Delegate? OnComplete = null)
    {
        if (Helpers.StringLower(Action) == "adduser" && Model.FeatureEnabledDepartments && !Model.Departments.Any()) {
            await Helpers.LoadDepartments();
        }

        Model.QuickActionOnComplete = OnComplete;
        Model.QuickAction = Action;
        await jsRuntime.InvokeVoidAsync("ShowQuickActionMenu");

        string focus = String.Empty;

        switch (Action.ToLower()) {
            case "adduser":
                Model.QuickAddUser = new DataObjects.User {
                    UserId = Guid.Empty,
                    TenantId = Model.TenantId,
                    Enabled = true,
                };
                focus = "quickadd-user-FirstName";
                break;

            // {{ModuleItemStart:Appointments}}
            case "appointmentnote":
                focus = "quickadd-appointment-note";
                break;
                // {{ModuleItemEnd:Appointments}}
        }

        if (focus != String.Empty) {
            SetTimeout(async () => await DelayedFocus(focus));
        }
    }

    /// <summary>
    /// Recurses an exception and any inner exceptions.
    /// </summary>
    /// <param name="ex">The exception.</param>
    /// <param name="ShowExceptionType">Option to include the exception type in the results.</param>
    /// <returns>A list of strings containing any exceptions.</returns>
    public static List<string> RecurseException(Exception ex, bool ShowExceptionType = true)
    {
        List<string> output = new List<string>();

        if (ex != null) {
            if (!String.IsNullOrWhiteSpace(ex.Message)) {
                if (ShowExceptionType) {
                    output.Add(ex.GetType().ToString() + ": " + ex.Message);
                } else {
                    output.Add(ex.Message);
                }

            }

            if (ex.InnerException != null) {
                var inner = RecurseException(ex.InnerException, ShowExceptionType);
                if (inner.Any()) {
                    foreach (var message in inner) {
                        output.Add(message);
                    }
                }
            }
        }

        return output;
    }

    // {{ModuleItemStart:Tags}}
    /// <summary>
    /// Renders a tag as HTML.
    /// </summary>
    /// <param name="tag">The tag to render.</param>
    /// <returns>The HTML for the tag.</returns>
    public static string RenderTag(DataObjects.Tag tag)
    {
        string output = String.Empty;

        if (!String.IsNullOrWhiteSpace(tag.Name)) {
            if (!String.IsNullOrWhiteSpace(tag.Style)) {
                string style = tag.Style;

                if (TagColors.Contains(style)) {
                    output += "<div class=\"tag tag-" + style.ToLower() + "\">" + tag.Name + "</div>";
                } else {
                    output += "<div class=\"tag\"" + (!String.IsNullOrWhiteSpace(tag.Style) ? " style=\"" + tag.Style + "\"" : "") + ">" +
                    tag.Name +
                    "</div>";
                }
            } else {
                output += "<div class=\"tag\">" +
                tag.Name +
                "</div>";
            }


        }

        return output;
    }

    /// <summary>
    /// Renders a collection of tags as HTML.
    /// </summary>
    /// <param name="TagIds">The collection of tag ids.</param>
    /// <param name="SortAlphabetically">Option to sort the results alphabetically.</param>
    /// <returns>The rendered HTML for the tags.</returns>
    public static string RenderTags(List<Guid>? TagIds, bool SortAlphabetically = true)
    {
        string output = String.Empty;

        if (SortAlphabetically) {
            TagIds = SortTagList(TagIds);
        }

        if (TagIds != null && TagIds.Any()) {
            List<DataObjects.OptionPair> tags = new List<DataObjects.OptionPair>();

            foreach (var tagId in TagIds) {
                var tag = Model.Tags.FirstOrDefault(x => x.TagId == tagId);
                if (tag != null) {
                    output += RenderTag(tag);
                }
            }
        }

        return output;
    }
    // {{ModuleItemEnd:Tags}}

    /// <summary>
    /// Reloads the entire data model for the selected user id.
    /// </summary>
    public async static Task ReloadModel()
    {
        DataObjects.BlazorDataModelLoader? blazorDataModelLoader = null;

        string currentUrl = Helpers.BaseUri;

        if (Model.User.Enabled && Model.User.UserId != Guid.Empty) {
            blazorDataModelLoader = await GetOrPost<DataObjects.BlazorDataModelLoader>("api/Data/GetBlazorDataModel/" + Model.User.UserId.ToString());
        } else if (!String.IsNullOrWhiteSpace(Model.TenantCodeFromUrl)) {
            blazorDataModelLoader = await GetOrPost<DataObjects.BlazorDataModelLoader>("api/Data/GetBlazorDataModelByTenantCode/" + Model.TenantCodeFromUrl);
        } else {
            blazorDataModelLoader = await GetOrPost<DataObjects.BlazorDataModelLoader>("api/Data/GetBlazorDataModel/");
        }

        if (blazorDataModelLoader != null) {
            string cultureCode = blazorDataModelLoader.CultureCode;
            DataObjects.Language modelLanguage = Model.DefaultLanguage;

            // Preferred or saved culture code.
            bool cultureCodeSet = false;
            var savedCultureCode = await LocalStorage.GetItemAsync<string>("CultureCode");
            if (!String.IsNullOrWhiteSpace(savedCultureCode)) {
                // Only set to this culture if there is a language for this culture.
                if (blazorDataModelLoader.Languages.Any()) {
                    var ccExists = blazorDataModelLoader.Languages.FirstOrDefault(x => x.TenantId == Model.TenantId && x.Culture.ToLower() == savedCultureCode.ToLower());
                    if (ccExists != null) {
                        modelLanguage = ccExists;
                        cultureCode = ccExists.Culture;
                        cultureCodeSet = true;
                    }
                }
            }

            if (!cultureCodeSet) {
                var currentCulture = CultureInfo.CurrentCulture;
                if (currentCulture != null && !String.IsNullOrWhiteSpace(currentCulture.Name)) {
                    // Only set to this culture if there is a language for this culture.
                    if (blazorDataModelLoader.Languages.Any()) {
                        var ccExists = blazorDataModelLoader.Languages.FirstOrDefault(x => x.TenantId == Model.TenantId && x.Culture.ToLower() == currentCulture.Name.ToLower());
                        if (ccExists != null) {
                            cultureCode = ccExists.Culture;
                            modelLanguage = ccExists;
                        }
                    }
                }
            }

            Model.ActiveUsers = blazorDataModelLoader.ActiveUsers;

            Model.AdminCustomLoginProvider = blazorDataModelLoader.AdminCustomLoginProvider;
            Model.AllTenants = blazorDataModelLoader.AllTenants;

            if (Model.ApplicationUrl != blazorDataModelLoader.ApplicationUrl) {
                Model.ApplicationUrl = blazorDataModelLoader.ApplicationUrl;
            }

            if (Model.UseTenantCodeInUrl != blazorDataModelLoader.UseTenantCodeInUrl) {
                Model.UseTenantCodeInUrl = blazorDataModelLoader.UseTenantCodeInUrl;
            }

            if (Model.ShowTenantListingWhenMissingTenantCode != blazorDataModelLoader.AppSettings.ShowTenantListingWhenMissingTenantCode) {
                Model.ShowTenantListingWhenMissingTenantCode = blazorDataModelLoader.AppSettings.ShowTenantListingWhenMissingTenantCode;
            }

            Model.AppSettings = blazorDataModelLoader.AppSettings;
            Model.AuthenticationProviders = blazorDataModelLoader.AuthenticationProviders != null ? blazorDataModelLoader.AuthenticationProviders : new DataObjects.AuthenticationProviders();

            if (Model.CultureCode != cultureCode) {
                Model.CultureCode = cultureCode;
            }

            Model.CultureCodes = blazorDataModelLoader.CultureCodes;
            Model.Language = modelLanguage;
            Model.Languages = blazorDataModelLoader.Languages;


            if (Model.LoggedIn != blazorDataModelLoader.LoggedIn) {
                Model.LoggedIn = blazorDataModelLoader.LoggedIn;
            }

            Model.Plugins = blazorDataModelLoader.Plugins;

            if (Model.Released != blazorDataModelLoader.Released) {
                Model.Released = blazorDataModelLoader.Released;
            }

            Model.Tenants = blazorDataModelLoader.Tenants;

            if (Model.UseCustomAuthenticationProviderFromAdminAccount != blazorDataModelLoader.UseCustomAuthenticationProviderFromAdminAccount) {
                Model.UseCustomAuthenticationProviderFromAdminAccount = blazorDataModelLoader.UseCustomAuthenticationProviderFromAdminAccount;
            }

            Model.User = blazorDataModelLoader.User;
            Model.Users = blazorDataModelLoader.Users;

            if (Model.Version != blazorDataModelLoader.Version) {
                Model.Version = blazorDataModelLoader.Version;
            }

            // Set the current tenant
            var tenant = Model.Tenants.FirstOrDefault(x => x.TenantId == Model.User.TenantId);

            if (tenant == null && !String.IsNullOrWhiteSpace(Model.TenantCodeFromUrl)) {
                tenant = Model.Tenants.FirstOrDefault(x => x.TenantCode.ToLower() == Model.TenantCodeFromUrl.ToLower());
            }

            if (tenant == null) {
                // See if we can match the current tenant based on the current URL and the ApplicationUrl setting for the tenant.
                var tenantMatches = Model.Tenants.Where(x => !String.IsNullOrWhiteSpace(x.TenantSettings.ApplicationUrl) && x.TenantSettings.ApplicationUrl.ToLower() == currentUrl.ToLower()).ToList();
                if (tenantMatches != null && tenantMatches.Count > 0) {
                    if (Model.UseTenantCodeInUrl) {
                        var firstTenantMatch = tenantMatches.FirstOrDefault(x => x.TenantCode.ToLower() == StringLower(Model.TenantCodeFromUrl));
                        if (firstTenantMatch != null) {
                            tenant = firstTenantMatch;
                        }
                    } else if (tenantMatches.Count == 1) {
                        tenant = tenantMatches.First();
                    }
                }
            }

            if (tenant != null) {
                Model.Tenant = tenant;
                if (Model.TenantId != tenant.TenantId) {
                    Model.TenantId = tenant.TenantId;
                }
            } else if (Model.Tenant == null) {
                Model.Tenant = new DataObjects.Tenant();
                Model.TenantId = Guid.Empty;
            }

            await ReloadModelApp(blazorDataModelLoader);

            if (!Model.Loaded) {
                Model.Loaded = true;
            }

            // If the current tenant has a specific ApplicationUrl, and we are not in that URL space, then redirect.
            if (Model.User.ActionResponse.Result && tenant != null && !String.IsNullOrWhiteSpace(tenant.TenantSettings.ApplicationUrl)) {
                string baseUrl = BaseUri.ToLower();

                if (!tenant.TenantSettings.ApplicationUrl.ToLower().Contains(baseUrl)) {
                    var redirectUrl = tenant.TenantSettings.ApplicationUrl;
                    if (!redirectUrl.EndsWith("/")) {
                        redirectUrl += "/";
                    }

                    if (Model.UseTenantCodeInUrl) {
                        redirectUrl += Model.Tenant.TenantCode + "/";
                    }

                    await NavigateToViaJavascript(redirectUrl);
                }
            }
        }
    }

    /// <summary>
    /// Reloads the user listing for the current tenant.
    /// </summary>
    public async static Task ReloadTenantUsers()
    {
        // Only reload this list if this tenant has the users loaded.
        if (Model.Tenant.Users.Any()) {
            var users = await GetOrPost<List<DataObjects.UserListing>>("api/Data/ReloadTenantUsers");
            if (users != null && users.Any()) {
                Model.Tenant.Users = users;

                // Also update the tenant object in Tenants.
                var tenantInTenants = Model.Tenants.FirstOrDefault(x => x.TenantId == Model.TenantId);
                if (tenantInTenants != null) {
                    tenantInTenants.Users = users;
                }
            }
        }
    }

    /// <summary>
    /// Reloads the current user object for the current user.
    /// </summary>
    public async static Task ReloadUser()
    {
        var user = await GetOrPost<DataObjects.User>("api/Data/ReloadUser/" + Model.User.UserId.ToString());
        if (user != null && user.ActionResponse.Result) {
            Model.User = user;
            // Also save the latest token
            await CookieWrite("user-token", StringValue(Model.User.AuthToken));
        }
    }

    /// <summary>
    /// Replaces spaces in a string with non-breaking html characters.
    /// </summary>
    /// <param name="input">The string to replace spaces in.</param>
    /// <returns>A string with any spaces replaced with non-breaking html characters.</returns>
    public static string ReplaceSpaces(string? input)
    {
        string output = String.Empty;

        if (!String.IsNullOrWhiteSpace(input)) {
            output = input.Replace(" ", "&nbsp;");
        }

        return output;
    }

    /// <summary>
    /// Creates a string with a safe file name based on the current date and time.
    /// </summary>
    /// <returns>A string formatted as yyyy.MM.dd.HH.mm.ss.ff</returns>
    public static string SafeFileDate()
    {
        DateTime dt = new DateTime(DateTime.Now.Ticks);
        return string.Format("{0:yyyy.MM.dd.HH.mm.ss.ff}", dt);
    }

    /// <summary>
    /// Saves the user preferences for the current user.
    /// </summary>
    /// <returns>A BooleanResponse object.</returns>
    public static async Task<DataObjects.BooleanResponse> SaveUserPreferences()
    {
        var output = new DataObjects.BooleanResponse();

        if (!_savingUserPreferences) {
            _savingUserPreferences = true;
            output = await SaveUserPreferences(Model.User.UserId, Model.User.UserPreferences);
            _savingUserPreferences = false;
        }

        return output;
    }

    /// <summary>
    /// Saves user preferences for a specific user.
    /// </summary>
    /// <param name="UserId">The unique UserId of the user.</param>
    /// <param name="userPreferences">The UserPreferences object to save for the user.</param>
    /// <returns>A BooleanResponse object.</returns>
    public static async Task<DataObjects.BooleanResponse> SaveUserPreferences(Guid UserId, DataObjects.UserPreferences userPreferences)
    {
        var output = new DataObjects.BooleanResponse();

        var saved = await GetOrPost<DataObjects.BooleanResponse>("api/Data/SaveUserPreferences/" + UserId.ToString(), userPreferences);
        if (saved != null) {
            output = saved;
        }

        return output;
    }

    /// <summary>
    /// Converts seconds to a user-readable friendly time passed string.
    /// </summary>
    /// <param name="seconds">The number of seconds.</param>
    /// <returns>A string in a user-readable format.</returns>
    public static string SecondsToTimePassed(double seconds)
    {
        string output = "";

        int totalSeconds = (int)seconds;

        if (totalSeconds > 0) {
            int minutes = 0;
            int hours = 0;
            int days = 0;

            if (totalSeconds >= 86400) {
                days = (totalSeconds / 86400);
                totalSeconds = totalSeconds - (86400 * days);
            }

            if (totalSeconds >= 3600) {
                hours = (totalSeconds / 3600);
                totalSeconds = totalSeconds - (3600 * hours);
            }

            if (totalSeconds > 60) {
                minutes = (totalSeconds / 60);
                totalSeconds = totalSeconds - (60 * minutes);
            }

            if (days > 0) {
                output += days.ToString() + " " + (days > 1 ? Text("Days") : Text("Day"));
            }

            if (hours > 0) {
                if (output != "") { output += ", "; }
                output += hours.ToString() + " " + (hours > 1 ? Text("Hours") : Text("Hour"));
            }

            if (minutes > 0) {
                if (output != "") { output += ", "; }
                output += minutes.ToString() + " " + (minutes > 1 ? Text("Minutes") : Text("Minute"));
            }

            if (totalSeconds > 0) {
                if (output != "") { output += ", "; }
                output += totalSeconds.ToString() + " " + (totalSeconds > 1 ? Text("Seconds") : Text("Second"));
            }

            output += " " + Text("Ago");
        }

        return output;
    }

    /// <summary>
    /// Shows a dialog to select a file.
    /// </summary>
    /// <param name="OnFileSelected">The Delegate that will be invoked and receive the selected file.</param>
    /// <param name="Title">An optional title for the dialog.</param>
    /// <param name="ImagesOnly">Option to show only image files.</param>
    /// <param name="ShowCancelButton">Option to show a cancel button.</param>
    /// <param name="ShowRefreshButton">Option to show a refresh button.</param>
    public static async Task SelectFile(Action<Guid> OnFileSelected, string Title = "", bool ImagesOnly = false, bool ShowCancelButton = true, bool ShowRefreshButton = true)
    {
        if (String.IsNullOrWhiteSpace(Title)) {
            Title = Text("SelectFile");
        }

        Dictionary<string, object> parameters = new Dictionary<string, object>();
        parameters.Add("OnFileSelected", OnFileSelected);
        parameters.Add("ImagesOnly", ImagesOnly);
        parameters.Add("ShowCancelButton", ShowCancelButton);
        parameters.Add("ShowRefreshButton", ShowRefreshButton);

        await DialogService.OpenAsync<SelectFile>(Title, parameters, new DialogOptions() {
            AutoFocusFirstElement = false,
            Resizable = false,
            Draggable = false,
            Width = "95%",
            Height = "calc(100vh - 120px)",
            Top = "80px",
        });
    }

    // {{ModuleItemStart:Tags}}
    /// <summary>
    /// Shows a dialog to select tags.
    /// </summary>
    /// <param name="OnComplete">The Delegate that will be invoked and received the selected tags.</param>
    /// <param name="Title">An optional title for the dialog.</param>
    /// <param name="ExistingTags">An optional collection of any existing selected tags.</param>
    /// <param name="ShowCurrentTags">An option to show the currently-selected tags.</param>
    /// <param name="PreventDeselctingSelectedTags">An option to prevent the user from deselecting existing tags.</param>
    public static async Task SelectTags(Delegate OnComplete,
        string Title = "",
        DataObjects.TagModule? Module = null,
        List<Guid>? ExistingTags = null,
        bool ShowCurrentTags = true,
        bool PreventDeselctingSelectedTags = false)
    {

        if (String.IsNullOrWhiteSpace(Title)) {
            Title = Text("SelectTags");
        }

        Dictionary<string, object> parameters = new Dictionary<string, object>();
        parameters.Add("OnComplete", OnComplete);

        if (Module != null) {
            parameters.Add("Module", Module);
        }

        if (ExistingTags != null) {
            parameters.Add("SelectedTags", ExistingTags);
        }

        parameters.Add("ShowCurrentTags", ShowCurrentTags);
        parameters.Add("PreventDeselctingSelectedTags", PreventDeselctingSelectedTags);

        await DialogService.OpenAsync<TagSelector>(Title, parameters, new Radzen.DialogOptions() {
            AutoFocusFirstElement = false,
            Resizable = false,
            Draggable = false,
        });
    }
    // {{ModuleItemEnd:Tags}}

    /// <summary>
    /// Serializes an object to JSON using the System.Text.Json.JsonSerializer.
    /// </summary>
    /// <param name="Object">The object to serialize.</param>
    /// <param name="formatOutput">Option to format the output with indenting.</param>
    /// <returns>The JSON of the object.</returns>
    public static string SerializeObject(object? Object, bool formatOutput = false)
    {
        string output = String.Empty;

        if (Object != null) {
            if (formatOutput) {
                output += System.Text.Json.JsonSerializer.Serialize(Object, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            } else {
                output += System.Text.Json.JsonSerializer.Serialize(Object);
            }
        }

        return output;
    }

    /// <summary>
    /// Sets the value of an element using JSInterop.
    /// </summary>
    /// <param name="ElementId">The id of the html element.</param>
    /// <param name="Value">The value to set.</param>
    public static async Task SetElementValue(string ElementId, string Value)
    {
        await jsRuntime.InvokeVoidAsync("SetElementValue", ElementId, Value);
    }

    /// <summary>
    /// Sets a value for an item in the Local Storage.
    /// </summary>
    /// <param name="key">The key of the item.</param>
    /// <param name="value">The value to store.</param>
    public static async Task SetLocalStorageItem(string key, object value)
    {
        if (LocalStorage != null) {
            await LocalStorage.SetItemAsync(key, value);
        }
    }

    /// <summary>
    /// Sets the value for a specific property on an object.
    /// </summary>
    /// <param name="o">The object to have the value set on.</param>
    /// <param name="PropertyName">The name of the property to set.</param>
    /// <param name="Value">The value to set for the property.</param>
    /// <returns>The updated object.</returns>
    public static object SetObjectPropertyValue(object o, string PropertyName, object? Value)
    {
        try {
            System.Reflection.BindingFlags bindingAttrs = System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Static |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.IgnoreCase;

            var prop = o.GetType().GetProperty(PropertyName, bindingAttrs);
            if (prop != null) {
                prop.SetValue(o, Value, null);
            }
        } catch { }

        return o;
    }

    /// <summary>
    /// Calls the jsInterop method to set the theme.
    /// </summary>
    /// <param name="theme">The theme to be set.</param>
    public static async void SetTheme(string theme)
    {
        Model.Theme = theme;
        await jsRuntime.InvokeVoidAsync("SetPreferredColorScheme", theme);
        await LocalStorage.SetItemAsync("Theme", theme);
    }

    /// <summary>
    /// Executes a function after a specific delay.
    /// </summary>
    /// <param name="methodToInvoke">The delegate to the method to invoke.</param>
    /// <param name="millisecondsDelay">The milliseconds to delay before executing (defaults to 100.)</param>
    public static void SetTimeout(Delegate methodToInvoke, int millisecondsDelay = 100)
    {
        System.Threading.Timer? timer = null;
        timer = new System.Threading.Timer((obj) => {
            methodToInvoke.DynamicInvoke();
            timer?.Dispose();
        },
        null, millisecondsDelay, System.Threading.Timeout.Infinite);
    }

    // {{ModuleItemStart:Tags}}
    /// <summary>
    /// Sorts a list of tag by their names.
    /// </summary>
    /// <param name="TagIds">A list of Guids representing tags.</param>
    /// <returns>The list of Guids sorted by the name of the tags.</returns>
    public static List<Guid>? SortTagList(List<Guid>? TagIds)
    {
        var output = TagIds;

        if (TagIds != null && TagIds.Any()) {
            List<DataObjects.OptionPair> items = new List<DataObjects.OptionPair>();
            foreach (var tagId in TagIds) {
                var tag = Model.Tags.FirstOrDefault(x => x.TagId == tagId);
                if (tag != null) {
                    items.Add(new DataObjects.OptionPair {
                        Id = tag.Name,
                        Value = tag.TagId.ToString(),
                    });
                }
            }

            output = items
                .OrderBy(x => x.Id)
                .ToList()
                .Select(x => new Guid(String.Empty + x.Value)).ToList();
        }


        return output;
    }
    // {{ModuleItemEnd:Tags}}

    /// <summary>
    /// Returns a spacer image with a given width.
    /// </summary>
    /// <param name="width">The width of the spacer image.</param>
    /// <returns>A image element.</returns>
    public static string SpacerImage(int width)
    {
        string output = "<img src=\"" + Model.ApplicationUrl + "spacer.png\" width=\"" + width.ToString() + "\" height=\"1\" alt=\"\" />";
        return output;
    }

    /// <summary>
    /// Splits a string based on newline characters.
    /// </summary>
    /// <param name="input">The string to split.</param>
    /// <returns>A list of strings.</returns>
    public static List<string> SplitStringIntoLines(string? input)
    {
        var output = new List<string>();

        if (!String.IsNullOrWhiteSpace(input)) {
            var lines = input.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            if (lines != null && lines.Any()) {
                output = lines.ToList();
            }
        }

        return output;
    }

    /// <summary>
    /// Converts a string to lower case.
    /// </summary>
    /// <param name="input">The string to convert.</param>
    /// <returns>The string as lower case.</returns>
    public static string StringLower(string? input)
    {
        string output = !String.IsNullOrWhiteSpace(input) ? input.ToLower() : String.Empty;
        return output;
    }

    /// <summary>
    /// Gets the value of a nullable string.
    /// </summary>
    /// <param name="input">A nullable string.</param>
    /// <returns>The original string or an empty string if the input was null.</returns>
    public static string StringValue(string? input)
    {
        string output = !String.IsNullOrWhiteSpace(input) ? input : String.Empty;
        return output;
    }

    /// <summary>
    /// Switches between tenants for a user that has access to multiple tenants.
    /// </summary>
    /// <param name="TenantId">The unique id of the tenant.</param>
    public static async Task SwitchTenant(Guid TenantId)
    {
        if (_switchingTenant) {
            return;
        }

        _switchingTenant = true;

        // Find the user for this tenant.
        var user = Model.Users.FirstOrDefault(x => x.TenantId == TenantId);
        var tenant = Model.Tenants.FirstOrDefault(x => x.TenantId == TenantId);

        Model.NotifyTenantChanging();

        Model.TenantId = TenantId;

        string tenantUrl = String.Empty;

        if (tenant != null) {
            Model.Tenant = tenant;

            if (!String.IsNullOrWhiteSpace(tenant.TenantSettings.ApplicationUrl)) {
                tenantUrl = tenant.TenantSettings.ApplicationUrl;

                if (!tenantUrl.EndsWith("/")) {
                    tenantUrl += "/";
                }

                if (Model.UseTenantCodeInUrl) {
                    tenantUrl += tenant.TenantCode + "/";
                }
            } else {
                tenantUrl = Model.ApplicationUrlDefault;
                if (!tenantUrl.EndsWith("/")) {
                    tenantUrl += "/";
                }

                if (Model.UseTenantCodeInUrl) {
                    tenantUrl += tenant.TenantCode + "/";
                }
            }
        }

        DataObjects.Language? language = null;

        if (user != null && Model.Languages.Any()) {
            language = Model.Languages.FirstOrDefault(x => x.TenantId == user.TenantId && (x.Culture.ToLower() == Model.CultureCode.ToLower() || x.Culture == "en-US"));
        }

        Model.Language = language != null ? language : Model.DefaultLanguage;

        if (user != null) {
            Model.User = user;

            if (!String.IsNullOrWhiteSpace(user.AuthToken)) {
                await CookieWrite("user-token", user.AuthToken);
            } else {
                await CookieWrite("user-token", "");
            }
        } else {
            Model.User = new DataObjects.User();
            await CookieWrite("user-token", "");
        }

        await Helpers.CookieWrite("requested-url", "");

        //ForceModelUpdates();

        if (user != null) {
            if (!String.IsNullOrWhiteSpace(tenantUrl)) {
                // Need to do a full redirect to this tenant's URL.
                await NavigateToViaJavascript(tenantUrl);
            } else {
                NavigateToRootDefault(true);
            }
        }

        Model.NotifyTenantChanged();

        _switchingTenant = false;
    }

    // {{ModuleItemStart:Tags}}
    /// <summary>
    /// The list of colors for tags.
    /// </summary>
    public static List<string> TagColors {
        get {
            return new List<string> {
                "LIGHTCORAL", "SALMON", "DARKSALMON", "LIGHTSALMON", "CRIMSON",
                "RED", "FIREBRICK", "DARKRED", "PINK", "LIGHTPINK", "HOTPINK",
                "DEEPPINK", "MEDIUMVIOLETRED", "PALEVIOLETRED",
                "CORAL", "TOMATO", "ORANGERED", "DARKORANGE", "ORANGE",
                "GOLD", "YELLOW", "LIGHTYELLOW", "LEMONCHIFFON",
                "LIGHTGOLDENRODYELLOW", "PAPAYAWHIP", "MOCCASIN", "PEACHPUFF",
                "PALEGOLDENROD", "KHAKI", "DARKKHAKI", "LAVENDER", "THISTLE",
                "PLUM", "VIOLET", "ORCHID", "FUCHSIA", "MAGENTA", "MEDIUMORCHID",
                "MEDIUMPURPLE", "REBECCAPURPLE", "BLUEVIOLET", "DARKVIOLET",
                "DARKORCHID", "DARKMAGENTA", "PURPLE", "INDIGO", "SLATEBLUE",
                "DARKSLATEBLUE", "MEDIUMSLATEBLUE", "GREENYELLOW", "CHARTREUSE",
                "LAWNGREEN", "LIME", "LIMEGREEN", "PALEGREEN", "LIGHTGREEN",
                "MEDIUMSPRINGGREEN", "SPRINGGREEN", "MEDIUMSEAGREEN", "SEAGREEN",
                "FORESTGREEN", "GREEN", "DARKGREEN", "YELLOWGREEN", "OLIVEDRAB",
                "OLIVE", "DARKOLIVEGREEN", "MEDIUMAQUAMARINE", "DARKSEAGREEN",
                "LIGHTSEAGREEN", "DARKCYAN", "TEAL", "AQUA", "CYAN", "LIGHTCYAN",
                "PALETURQUOISE", "AQUAMARINE", "TURQUOISE", "MEDIUMTURQUOISE",
                "DARKTURQUOISE", "CADETBLUE", "STEELBLUE", "LIGHTSTEELBLUE",
                "POWDERBLUE", "LIGHTBLUE", "SKYBLUE", "LIGHTSKYBLUE", "DEEPSKYBLUE",
                "DODGERBLUE", "CORNFLOWERBLUE", "ROYALBLUE",
                "BLUE", "MEDIUMBLUE", "DARKBLUE", "NAVY", "MIDNIGHTBLUE",
                "CORNSILK", "BLANCHEDALMOND", "BISQUE", "NAVAJOWHITE", "WHEAT",
                "BURLYWOOD", "TAN", "ROSYBROWN", "SANDYBROWN", "GOLDENROD",
                "DARKGOLDENROD", "PERU", "CHOCOLATE", "SADDLEBROWN", "SIENNA",
                "BROWN", "MAROON", "MISTYROSE", "GAINSBORO", "LIGHTGRAY",
                "SILVER", "DARKGRAY", "GRAY", "DIMGRAY", "LIGHTSLATEGRAY",
                "SLATEGRAY", "DARKSLATEGRAY", "BLACK" };
        }
    }

    /// <summary>
    /// Converts a CSV list of tag ids to a list of tag names.
    /// </summary>
    /// <param name="TagsAsCsvString">A string containing a CSV list of tag ids.</param>
    /// <returns>A list of tag names.</returns>
    public static async Task<string> TagsListFromIds(string? TagsAsCsvString)
    {
        string output = String.Empty;

        if (!String.IsNullOrWhiteSpace(TagsAsCsvString)) {
            var list = TagsAsCsvString.Split(',').Select(x => x.Trim()).ToList();
            output = await TagsListFromIds(list);
        }

        return output;
    }

    /// <summary>
    /// Converts a list of strings containing tag id to a list of tag names.
    /// </summary>
    /// <param name="Tags">The list of strings of tag ids.</param>
    /// <returns>A list of tag names.</returns>
    public static async Task<string> TagsListFromIds(List<string>? Tags)
    {
        string output = String.Empty;

        if (Tags != null && Tags.Any()) {
            if (!Model.Tags.Any()) {
                await LoadTags();
            }

            foreach (var tagId in Tags) {
                var tag = Model.Tags.FirstOrDefault(x => x.TagId.ToString() == tagId);
                if (tag != null && !String.IsNullOrWhiteSpace(tag.Name)) {
                    if (!String.IsNullOrEmpty(output)) {
                        output += ", ";
                    }
                    output += tag.Name;
                }
            }
        }

        return output;
    }

    /// <summary>
    /// Converts a list of tag ids to a list of tag names.
    /// </summary>
    /// <param name="Tags">The list of tag ids.</param>
    /// <returns>A list of tag names.</returns>
    public static async Task<string> TagsListFromIds(List<Guid>? Tags)
    {
        string output = String.Empty;

        if (Tags != null && Tags.Any()) {
            if (!Model.Tags.Any()) {
                await LoadTags();
            }

            foreach (var tagId in Tags) {
                var tag = Model.Tags.FirstOrDefault(x => x.TagId == tagId);
                if (tag != null && !String.IsNullOrWhiteSpace(tag.Name)) {
                    if (!String.IsNullOrEmpty(output)) {
                        output += ", ";
                    }
                    output += tag.Name;
                }
            }
        }

        return output;
    }
    // {{ModuleItemEnd:Tags}}

    /// <summary>
    /// Gets the name of a tenant from the unique id.
    /// </summary>
    /// <param name="TenantId">The unique id of the tenant.</param>
    /// <returns>The name of the tenant.</returns>
    public static string TenantNameFromId(Guid TenantId)
    {
        string output = String.Empty;

        var tenant = Model.Tenants.FirstOrDefault(x => x.TenantId == TenantId);
        if (tenant == null) {
            tenant = Model.AllTenants.FirstOrDefault(x => x.TenantId == TenantId);
        }


        if (tenant != null) {
            output += tenant.Name;
        }

        return output;
    }

    /// <summary>
    /// Gets the language item text for a given language item.
    /// </summary>
    /// <param name="text">The name of the language item.</param>
    /// <param name="ReplaceSpaces">Option to replace spaces with non-breaking html characters.</param>
    /// <param name="ReplaceValues">Optional list of replacement values for language items that contain replacement items (eg {0}, {1}, etc.)</param>
    /// <param name="MarkUndefinedStrings">An option to mark undefined strings (those not included in your custom language definitions) by converting them to uppercase</param>
    /// <param name="textCase">An option to override the default text case for the text formatting.</param>
    /// <returns>The language item.</returns>
    public static string Text(string? text,
        bool ReplaceSpaces = false,
        List<string>? ReplaceValues = null,
        bool MarkUndefinedStrings = true,
        TextCase textCase = TextCase.Normal)
    {
        string output = !String.IsNullOrWhiteSpace(text) ? text : "";

        bool foundTag = false;

        if (Model.Language.Phrases.Any()) {
            // Find the item.
            var phrase = Model.Language.Phrases.FirstOrDefault(x => x.Id != null && x.Id.ToLower() == output.ToLower());
            if (phrase != null) {
                foundTag = true;
                output = !String.IsNullOrWhiteSpace(phrase.Value)
                    ? phrase.Value
                    : String.Empty;
            }
        }

        if (!foundTag && Model.DefaultLanguage.Phrases.Any()) {
            var phrase = Model.DefaultLanguage.Phrases.FirstOrDefault(x => x.Id != null && x.Id.ToLower() == output.ToLower());
            if (phrase != null) {
                foundTag = true;
                output = !String.IsNullOrWhiteSpace(phrase.Value)
                    ? phrase.Value
                    : String.Empty;
            }
        } else if (textCase != TextCase.Normal) {
            switch (textCase) {
                case TextCase.Lowercase:
                    output = output.ToLower();
                    break;

                case TextCase.Uppercase:
                    output = output.ToUpper();
                    break;

                case TextCase.Sentence:
                    output = output.Humanize(LetterCasing.Sentence);
                    break;

                case TextCase.Title:
                    output = output.Humanize(LetterCasing.Title);
                    break;
            }
        }

        if (!foundTag) {
            // This language tag wasn't found, so use Humanizer to convert the text to title case.
            if (MarkUndefinedStrings) {
                output = output.Humanize(LetterCasing.AllCaps);
            } else {
                output = output.Humanize(LetterCasing.Title);
            }
        }

        if (ReplaceValues != null && ReplaceValues.Any()) {
            int index = 0;
            foreach (var value in ReplaceValues) {
                string replaceWith = Text(value);
                if (String.IsNullOrWhiteSpace(replaceWith)) {
                    replaceWith = value;
                }

                output = output.Replace("{" + index.ToString() + "}", replaceWith);
                index++;
            }
        }

        if (ReplaceSpaces && !String.IsNullOrWhiteSpace(output)) {
            output = output.Replace(" ", "&nbsp;");
        }

        // There are a few items that may need to be shown in cases before
        // the models have completed loaded, so check for those here.
        switch (output) {
            case "APPTITLE":
            case "APP TITLE":
                output = ""; // In cases where we don't have the AppTitle, just return an empty string.
                break;

            case "LOADINGWAIT":
                output = "Loading, Please Wait...";
                break;
        }

        return output;
    }

    /// <summary>
    /// Calls the ToAbsoluteUri method for the given relativeUri using the NavigationManager.
    /// </summary>
    /// <param name="uri">The relativeUrl.</param>
    /// <returns>An abolute Uri.</returns>
    public static Uri ToAbsoluteUri(string uri)
    {
        return NavManager.ToAbsoluteUri(uri);
    }

    /// <summary>
    /// Gets the "user-token" cookie value.
    /// </summary>
    /// <returns>The "user-token" from the cookie.</returns>
    public static async Task<string> Token()
    {
        string output = String.Empty;

        var token = await CookieRead<string>("user-token");
        if (!String.IsNullOrWhiteSpace(token)) {
            output = token;
        }

        return output;
    }

    /// <summary>
    /// Shows a tooltip.
    /// </summary>
    /// <param name="element">The ElementReference for the object.</param>
    /// <param name="html">The html of the tooltip.</param>
    /// <param name="options">Any TooltipOptions to override the defaults.</param>
    public static void Tooltip(ElementReference element, string html, TooltipOptions? options = null)
    {
        if (options == null) {
            options = new TooltipOptions {
                Duration = 5000,
            };
        }

        Tooltips.Open(element, html, options);
    }

    /// <summary>
    /// Updates column titles for a PagedRecordset to replace icons with their icons.
    /// </summary>
    /// <param name="filter">The Filter object.</param>
    /// <returns>The updated Fileter object with any columns containing icons now using the app icon.</returns>
    public static DataObjects.Filter UpdatePagedRecordsetColumnIcons(DataObjects.Filter filter)
    {
        var output = filter;

        if (output.Columns != null && output.Columns.Any()) {
            foreach (var column in output.Columns) {
                if (!String.IsNullOrWhiteSpace(column.BooleanIcon)) {
                    string booleanIcon = Icon(column.BooleanIcon, true);
                    if (!String.IsNullOrWhiteSpace(booleanIcon)) {
                        column.BooleanIcon = booleanIcon;
                    }
                }

                if (!String.IsNullOrWhiteSpace(column.Label) && column.Label.ToLower().StartsWith("icon:")) {
                    string label = column.Label.Substring(5);
                    string icon = Icon(label, true);
                    if (!String.IsNullOrWhiteSpace(icon)) {
                        column.Label = icon;
                        if (String.IsNullOrWhiteSpace(column.BooleanIcon)) {
                            column.BooleanIcon = column.Label;
                        }
                    } else {
                        column.Label = label;
                    }
                }
            }
        }

        return output;
    }

    /// <summary>
    /// Shows a dialog allowing users to upload one or more files.
    /// </summary>
    /// <param name="OnUploadComplete">The Delegate that will be invoked when the upload completes and will receive any a single file or a collection of files.</param>
    /// <param name="Title">An optional title for the dialog to override the default language item of UploadFile.</param>
    /// <param name="UploadInstructions">Any upload instructions to show before the upload control.</param>
    /// <param name="SupportedFileTypes">A list of extensions if you wish to limit the upload types allowed.</param>
    /// <param name="AllowMultipleUploads">Option to indicate if the user can upload only a single file or multiple files.</param>
    public static async Task UploadFile(Delegate OnUploadComplete, string Title = "",
        string UploadInstructions = "", List<string>? SupportedFileTypes = null, bool AllowMultipleUploads = false)
    {
        if (String.IsNullOrWhiteSpace(Title)) {
            Title = Text("UploadFile");
        }

        Dictionary<string, object> parameters = new Dictionary<string, object>();
        parameters.Add("OnUploadComplete", OnUploadComplete);
        parameters.Add("InDialog", true);

        if (!String.IsNullOrWhiteSpace(UploadInstructions)) {
            parameters.Add("UploadInstructions", UploadInstructions);
        }
        if (SupportedFileTypes != null && SupportedFileTypes.Any()) {
            parameters.Add("SupportedFileTypes", SupportedFileTypes);
        }
        if (AllowMultipleUploads) {
            parameters.Add("AllowMultipleUploads", true);
        }

        if (AllowMultipleUploads) {
            await DialogService.OpenAsync<UploadFile<IReadOnlyList<IBrowserFile>>>(Title, parameters, new Radzen.DialogOptions() {
                AutoFocusFirstElement = false,
                Resizable = false,
                Draggable = false,
            });
        } else {
            await DialogService.OpenAsync<UploadFile<IBrowserFile>>(Title, parameters, new Radzen.DialogOptions() {
                AutoFocusFirstElement = false,
                Resizable = false,
                Draggable = false,
            });
        }
    }

    /// <summary>
    /// Uploads a collection of files and returns the updated FileStorage objects.
    /// </summary>
    /// <param name="files">A list of FileStorage objects.</param>
    /// <returns>An updated list of FileStorage objects.</returns>
    public static async Task<List<DataObjects.FileStorage>> UploadFiles(List<DataObjects.FileStorage> files)
    {
        var output = new List<DataObjects.FileStorage>();

        foreach (var file in files) {
            var uploaded = await GetOrPost<DataObjects.FileStorage>("api/Data/SaveFile", file);
            if (uploaded != null) {
                output.Add(uploaded);
            }
        }

        return output;
    }

    /// <summary>
    /// URL decodes the given input.
    /// </summary>
    /// <param name="url">The input to be decoded.</param>
    /// <returns>The UrlDecoded text.</returns>
    public static string UrlDecode(string? url)
    {
        string output = String.Empty;

        if (!String.IsNullOrWhiteSpace(url)) {
            output = System.Web.HttpUtility.UrlDecode(url);
        }

        return output;
    }

    /// <summary>
    /// URL encodes the given input.
    /// </summary>
    /// <param name="url">The input to be encoded.</param>
    /// <returns>The UrlEncoded text.</returns>
    public static string UrlEncode(string? url)
    {
        string output = String.Empty;

        if (!String.IsNullOrWhiteSpace(url)) {
            output = System.Web.HttpUtility.UrlEncode(url);
        }

        return output;
    }

    /// <summary>
    /// Gets the Uri from the NavManager.
    /// </summary>
    public static string Uri {
        get {
            return NavManager.Uri;
        }
    }

    /// <summary>
    /// Gets the name of a User Group based on the unique id.
    /// </summary>
    /// <param name="GroupId">The unique id of the User Group.</param>
    /// <returns>The name of the User Group.</returns>
    public static async Task<string> UserGroupName(Guid? GroupId)
    {
        string output = String.Empty;

        if (GroupId.HasValue) {
            if (!Model.UserGroups.Any()) {
                await LoadUserGroups();
            }

            var userGroup = Model.UserGroups.FirstOrDefault(x => x.GroupId == GroupId);
            if (userGroup != null) {
                output += userGroup.Name;
            }
        }

        return output;
    }

    /// <summary>
    /// Gets either the user photo or the default user icon.
    /// </summary>
    /// <param name="photoId">The unique id of the user's photo file.</param>
    /// <returns>Either the user's photo or the default user icon.</returns>
    public static string UserIcon(Guid? photoId)
    {
        string output = "";

        if (photoId.HasValue && photoId != Guid.Empty) {
            output = "<img class=\"user-menu-icon\" src=\"" + BaseUri + "File/View/" + ((Guid)photoId).ToString() + "\" />";
        } else {
            output = Icon("User", true);
        }

        return output;
    }

    /// <summary>
    /// The results of a user lookup.
    /// </summary>
    /// <param name="search">The search text.</param>
    /// <param name="ExcludeUsers">A list of unique user ids for users to exclude from the results, such as when excluding already-selected users.</param>
    /// <returns>A list of tuple values.</returns>
    public static async Task<List<(string key, string label)>?> UserLookupResults(string search, List<Guid>? ExcludeUsers = null)
    {
        List<(string key, string label)>? output = null;

        if (!String.IsNullOrWhiteSpace(search)) {
            // If this Tenant has the Users complete, search locally.
            if (Model.Tenant.Users.Any()) {
                string lastName = String.Empty;
                string firstName = String.Empty;
                string[] names;

                var recs = Model.Tenant.Users;

                search = search.ToLower();

                if (search.Contains(",")) {
                    // Check "Last, First"
                    names = search.Split(',');
                    try {
                        lastName += names[0];
                        firstName += names[1];
                    } catch { }
                    lastName = lastName.Trim().ToLower();
                    firstName = firstName.Trim().ToLower();

                    if (!String.IsNullOrWhiteSpace(lastName) && !String.IsNullOrWhiteSpace(firstName)) {
                        recs = recs.Where(x => x.LastName != null && x.LastName.ToLower().StartsWith(lastName) && x.FirstName != null && x.FirstName.ToLower().StartsWith(firstName) && x.Username?.ToLower() != "admin").ToList();
                    } else {
                        recs = recs.Where(x => x.LastName != null && x.LastName.ToLower().StartsWith(lastName) && x.Username?.ToLower() != "admin").ToList();
                    }
                } else if (search.Contains(" ")) {
                    // Check "First Last"
                    names = search.Split(' ');
                    try {
                        firstName += names[0];
                        lastName += names[1];
                    } catch { }
                    lastName = lastName.Trim().ToLower();
                    firstName = firstName.Trim().ToLower();

                    if (!String.IsNullOrWhiteSpace(lastName) && !String.IsNullOrWhiteSpace(firstName)) {
                        recs = recs.Where(x => x.LastName != null && x.LastName.ToLower().StartsWith(lastName) && x.FirstName != null && x.FirstName.ToLower().StartsWith(firstName) && x.Username?.ToLower() != "admin").ToList();
                    } else {
                        recs = recs.Where(x => x.FirstName != null && x.FirstName.ToLower().StartsWith(firstName) && x.Username?.ToLower() != "admin").ToList();
                    }
                } else {
                    recs = recs.Where(x =>
                            (x.Username != null ? x.Username : "").ToLower() != "admin"
                            &&
                            (
                                (x.FirstName != null && x.FirstName.ToLower().Contains(search)) ||
                                (x.LastName != null && x.LastName.ToLower().Contains(search)) ||
                                (x.Email != null && x.Email.ToLower().Contains(search))
                            )
                        ).ToList();
                }

                if (ExcludeUsers != null && ExcludeUsers.Any()) {
                    recs = recs.Where(x => !ExcludeUsers.Contains(x.UserId)).ToList();
                }

                // Convert the output to the correct data type
                if (recs.Any()) {
                    output = new List<(string key, string label)>();
                    foreach (var rec in recs) {
                        output.Add((
                            rec.UserId.ToString(),
                            rec.FirstName + " " + rec.LastName + (!String.IsNullOrWhiteSpace(rec.Email) ? " (" + rec.Email + ")" : "")
                        ));
                    }
                    output = output.OrderBy(x => x.label).ToList();
                }
            } else {
                var searchResults = await GetOrPost<DataObjects.AjaxLookup>("api/Data/AjaxUserSearch",
                new DataObjects.AjaxLookup { Search = search, TenantId = Model.TenantId });

                if (searchResults != null && searchResults.Results != null && searchResults.Results.Any()) {
                    output = new List<(string key, string label)>();

                    string guidEmpty = Guid.Empty.ToString();

                    foreach (var item in searchResults.Results) {
                        bool showItem = true;

                        if (ExcludeUsers != null && ExcludeUsers.Any() && !String.IsNullOrWhiteSpace(item.value)) {
                            Guid userId = Guid.Empty;
                            try {
                                userId = new Guid(item.value);
                            } catch { }
                            showItem = !ExcludeUsers.Contains(userId);
                        }

                        if (showItem) {
                            string itemValue = !String.IsNullOrWhiteSpace(item.value) ? item.value : "";
                            if (itemValue == guidEmpty) {
                                itemValue = "";
                            }

                            output.Add((
                                itemValue,
                                !String.IsNullOrWhiteSpace(item.label) ? item.label : ""
                            ));
                        }
                    }
                }
            }
        }

        return output;
    }

    /// <summary>
    /// Validates the current URL based on the UseTenantCodeInUrl setting. If the app is configured to use a Tenant Code in the
    /// URL, but no Tenant Code is present, then the default code is used if configured. Otherwise, the user is redirected to 
    /// a page to let them know a Tenant Code is required. If a Tenant Code was included in the URL but the app is not configured
    /// to use Tenant Codes in the URL, the user is redirected back to the home page with no Tenant Code in the URL.
    /// </summary>
    /// <param name="TenantCode">The TenantCode from the URL passed by the page calling this method.</param>
    /// <param name="AutoRedirect">Option to automatically redirect to the root if the TenantCode is empty.</param>
    public static async Task ValidateUrl(string? TenantCode, bool AutoRedirect = false)
    {
        if (_validatingUrl) {
            return;
        }

        _validatingUrl = true;

        if (Model.UseTenantCodeInUrl) {
            if (String.IsNullOrWhiteSpace(TenantCode)) {
                if (AutoRedirect) {
                    // See if we already have a valid tenant code. If we do the user will be redirected back to the home page with a Tenant Code in the URL.
                    if (!String.IsNullOrWhiteSpace(Model.Tenant.TenantCode)) {
                        NavigateToRoot();
                        return;
                    }
                }

                // We are not auto-redirecting to the home page, but we are missing the required Tenant Code.
                // If a DefaultTenantCode is configured then redirect to the default.
                // Otherwise, redirect to the MissingTenantCode page.
                if (!String.IsNullOrWhiteSpace(Model.AppSettings.DefaultTenantCode)) {
                    NavManager.NavigateTo(Model.ApplicationUrl + Model.AppSettings.DefaultTenantCode, true);
                } else {
                    NavManager.NavigateTo(Model.ApplicationUrl + "MissingTenantCode");
                }
            } else {
                // A Tenant Code was passed in so make sure this is a valid tenant code
                if (!Model.TenantList.Any()) {
                    await LoadTenantList();
                }

                var tenant = Model.TenantList.FirstOrDefault(x => x.TenantCode.ToLower() == TenantCode.ToLower());
                if (tenant == null) {
                    // The Tenant Code was not a valid Tenant Code, so redirect to the InvalidTenantCode page.
                    NavManager.NavigateTo(Model.ApplicationUrl + "InvalidTenantCode");
                } else {
                    // It's a valid tenant code, so make sure it's the current tenant.
                    if (Helpers.StringValue(Model.Tenant.TenantCode).ToLower() != TenantCode.ToLower()) {
                        await SwitchTenant(tenant.TenantId);
                    }
                }
            }
        } else {
            // The application is not configured for Tenant Codes in the URL, so if one was passed in we need to navigate back to the home
            // page with no Tenant Code in the URL, forcing a full reload of the page.
            if (!String.IsNullOrWhiteSpace(TenantCode)) {
                NavManager.NavigateTo(Model.ApplicationUrl, true);
            }
        }

        _validatingUrl = false;
    }

    /// <summary>
    /// Shows a file viewer for supported file types, or downloads the file if no viewer is available.
    /// </summary>
    /// <param name="file">The FileStorage object for the file.</param>
    /// <param name="title">An optional title for the dialog.</param>
    public static async Task ViewOrDownloadFile(DataObjects.FileStorage file, string? title = "")
    {
        if (IsImage(file)) {
            await ViewImage(file, title);
        } else if (IsPDF(file)) {
            await Helpers.PdfViewer(file.FileId, "", "", "", Model.User.Admin);
            //} else if (IsOfficeFile(file)) {
            //    await Helpers.PdfViewer(file.FileId, "", "", "", Model.User.Admin || Model.User.Purchaser);
        } else if (IsTextFile(file)) {
            await ViewTextFile(file, title);
        } else {
            await DownloadFile(file.FileId);
        }
    }

    /// <summary>
    /// Shows an image in the image viewer.
    /// </summary>
    /// <param name="file">The FileStorage object for the file.</param>
    /// <param name="title">An optional title for the dialog.</param>
    public static async Task ViewImage(DataObjects.FileStorage file, string? title = "")
    {
        if (IsImage(file)) {
            if (String.IsNullOrWhiteSpace(title)) {
                title = Text("ViewImage");
            }

            string image = String.Empty;

            if (file.Value != null) {
                string extension = Helpers.StringValue(file.Extension).Replace(".", "").ToLower();
                string imageData = "data:image/" + extension + "; base64," + Convert.ToBase64String(file.Value);

                image = "<img src=\"" + imageData + "\" />";
            } else {
                image = "<img src=\"" + Model.ApplicationUrl + "File/Embed/" + file.FileId.ToString() + "\" class=\"image-preview\" />";
            }

            string content = "<div class=\"mt-2 center\">" + image + "</div>";
            await ModalMessage(content, title, false, "80%");
        }
    }

    /// <summary>
    /// Shows a text file in the viewer.
    /// </summary>
    /// <param name="file">The FileStorage object for the file.</param>
    /// <param name="title">An optional title for the dialog.</param>
    public static async Task ViewTextFile(DataObjects.FileStorage file, string? title = "")
    {
        if (IsTextFile(file)) {
            if (String.IsNullOrWhiteSpace(title)) {
                title = Text("ViewFile");
            }

            var fileStorage = await GetOrPost<DataObjects.FileStorage>("api/Data/GetFileStorage/" + file.FileId.ToString());
            if (fileStorage != null) {
                if (fileStorage.ActionResponse.Result) {
                    if (fileStorage.Value != null) {
                        var fileContents = System.Text.Encoding.Default.GetString(fileStorage.Value);
                        if (!String.IsNullOrWhiteSpace(fileContents)) {
                            string html = "<div class='view-text-file'><pre>" + fileContents + "</pre></div>";
                            await ModalMessage(html, title, false, "98%", "auto");
                        }
                    }
                } else {
                    Model.ErrorMessages(fileStorage.ActionResponse.Messages);
                }
            } else {
                Model.UnknownError();
            }
        }
    }
}