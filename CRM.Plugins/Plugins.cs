using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;

namespace Plugins
{
    /// <summary>
    /// The Plugins interface.
    /// </summary>
    public interface IPlugins
    {
        /// <summary>
        /// Gets all plugins from the interface.
        /// </summary>
        public List<Plugin> AllPlugins { get; }

        /// <summary>
        /// Gets all plugins from the interface to store in the cache.
        /// </summary>
        public List<Plugin> AllPluginsForCache { get; }

        /// <summary>
        /// Executes dynamic C# code.
        /// </summary>
        /// <typeparam name="T">The type of object expected to be returned.</typeparam>
        /// <param name="code">The code to execute.</param>
        /// <param name="objects">Any objects to pass to the invoker method.</param>
        /// <param name="additionalAssemblies">Any additional assemblies that should be loaded to execute this code.</param>
        /// <param name="Namespace">The namespace of the code.</param>
        /// <param name="Classname">The class name of the code.</param>
        /// <param name="invokerFunction">The name of the function to invoke in the code.</param>
        /// <returns></returns>
        public T? ExecuteDynamicCSharpCode<T>(string code,
            IEnumerable<object>? objects,
            List<string>? additionalAssemblies,
            string Namespace,
            string Classname,
            string invokerFunction);

        /// <summary>
        /// Loads the plugins into the interface from the Plugins folder on startup.
        /// </summary>
        /// <param name="path">The path to the plugins folder.</param>
        /// <returns>A list of plugin objects to be included in the interface.</returns>
        public List<Plugin> Load(string path);

        /// <summary>
        /// The path to the plugins folder.
        /// </summary>
        public string PluginFolder { get; }

        /// <summary>
        /// Any server references that need to be loaded when executing dynamic code.
        /// </summary>
        public List<string> ServerReferences { get; set; }

        /// <summary>
        /// The using statements loaded from the appSettings.json file during startup.
        /// </summary>
        public List<string> UsingStatements { get; set; }
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public class Plugins : IPlugins
    {
        private byte[] _key = new byte[] { 0xC6, 0x46, 0xB1, 0xA0, 0x62, 0x0C, 0xCF, 0x29, 0xE4, 0xBE, 0x04, 0x38, 0x0F, 0xB8, 0x64, 0xFE, 0xA8, 0x25, 0xF8, 0x54, 0x46, 0x5D, 0xA0, 0xDF, 0x2E, 0xB3, 0x1D, 0xE9, 0x6E, 0xF3, 0xF5, 0xAE };
        private string _pluginFolder = String.Empty;
        private List<Plugin> _plugins = new List<Plugin>();
        private List<string> _serverReferences = new List<string>();
        private List<string> _usingStatements = new List<string>();

        private string AddMissingUsingStatements(string? code)
        {
            var output = String.Empty;

            if (!String.IsNullOrWhiteSpace(code)) {
                output = code;

                string addUsingStatements = String.Empty;

                string codeLower = code.ToLower();

                foreach (var item in _usingStatements) {
                    if (!codeLower.Contains(item.ToLower())) {
                        addUsingStatements += item + Environment.NewLine;
                    }
                }

                if (!String.IsNullOrEmpty(addUsingStatements)) {
                    output = addUsingStatements + Environment.NewLine + code;
                }
            }

            return output;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public List<Plugin> AllPlugins {
            get {
                return _plugins;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public List<Plugin> AllPluginsForCache {
            get {
                var output = new List<Plugin>();

                foreach(var plugin in _plugins) {
                    var copy = DuplicateObject<Plugin>(plugin);

                    if (copy != null) {
                        var code = DecryptCode(copy.Code);

                        if (String.IsNullOrWhiteSpace(code)) {
                            copy.Code = EncryptCode(copy.Code);
                        } else {
                            // Code was already encrypted.
                        }

                        output.Add(copy);
                    }

                    
                }

                return output;
            }
        }

        private string DecryptCode(string? input)
        {
            string output = String.Empty;

            if (!String.IsNullOrEmpty(input)) {
                var e = new Encryption.Encryption(_key);
                var decrypted = e.Decrypt(input);
                if (!String.IsNullOrEmpty(decrypted)) {
                    output = decrypted;
                }
            }

            return output;
        }

        private T? DuplicateObject<T>(object? o) {
            T? output = default(T);

            if (o != null) {
                try {
                    // To make a new copy serialize the object and then deserialize it back to a new object.
                    var serialized = System.Text.Json.JsonSerializer.Serialize(o);
                    if (!String.IsNullOrEmpty(serialized)) {
                        var duplicate = System.Text.Json.JsonSerializer.Deserialize<T>(serialized, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (duplicate != null) {
                            output = duplicate;
                        }
                    }
                } catch { }
            }

            return output;
        }

        private string EncryptCode(string? input)
        {
            string output = String.Empty;

            if (!String.IsNullOrEmpty(input)) {
                var e = new Encryption.Encryption(_key);
                var encrypted = e.Encrypt(input);
                if (!String.IsNullOrEmpty(encrypted)) {
                    output = encrypted;
                }
            }

            return output;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public T? ExecuteDynamicCSharpCode<T>(string code,
            IEnumerable<object>? objects,
            List<string>? additionalAssemblies,
            string Namespace,
            string Classname,
            string invokerFunction)
        {
            T? output = default(T);

            // If this code was encrypted decrypt it now.
            if (code.Contains(",0x") && _plugins != null) {
                code = DecryptCode(code);
            }

            // Add any missing required references to the code
            code = AddMissingUsingStatements(code);

            try {
                // Load all references required by the HelpDesk data project to use the DataAccess library.
                // First, get the base .NET6 references from the Basic.Reference.Assemblies package by jaredpar (https://github.com/jaredpar/basic-reference-assemblies)
                var references = Basic.Reference.Assemblies.Net90.References.All.ToList();

                if (_serverReferences.Count > 0) {
                    foreach (var reference in _serverReferences) {
                        try { references.Add(Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(reference)); } catch { }
                    }
                }

                // Add any user-specified references
                if (additionalAssemblies != null && additionalAssemblies.Any()) {
                    foreach (var assembly in additionalAssemblies) {
                        references.Add(Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(assembly));
                    }
                }

                // Create the C# Syntax Tree from the code block passed in.
                Microsoft.CodeAnalysis.SyntaxTree syntaxTree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(code);

                // Create a compiler instance.
                Microsoft.CodeAnalysis.CSharp.CSharpCompilation compilation = Microsoft.CodeAnalysis.CSharp.CSharpCompilation.Create(
                    Path.GetRandomFileName(),
                    syntaxTrees: new[] { syntaxTree },
                    references: references,
                    options: new Microsoft.CodeAnalysis.CSharp.CSharpCompilationOptions(Microsoft.CodeAnalysis.OutputKind.DynamicallyLinkedLibrary)
                );

                using (var ms = new MemoryStream()) {
                    Microsoft.CodeAnalysis.Emit.EmitResult result = compilation.Emit(ms);

                    if (result.Success) {
                        ms.Seek(0, SeekOrigin.Begin);
                        System.Reflection.Assembly assembly = System.Reflection.Assembly.Load(ms.ToArray());

                        var type = assembly.GetType(Namespace + "." + Classname);
                        if (type != null) {
                            var obj = Activator.CreateInstance(type);

                            // Invoke the function and pass in the necessary objects required by that method
                            var r = type.InvokeMember(invokerFunction,
                                System.Reflection.BindingFlags.Default | System.Reflection.BindingFlags.InvokeMethod,
                                null,
                                obj,
                                objects != null ? objects.ToArray() : null);

                            // See if we received valid results.
                            if (r != null) {
                                var t = r.GetType().ToString();

                                if (t.ToLower().Contains("asynctaskmethodbuilder") || t.ToLower().Contains("system.threading.tasks.task")) {
                                    var taskItem = (Task)r;

                                    int iterations = 0;

                                    while (!taskItem.IsCompleted) {
                                        iterations++;
                                        System.Threading.Thread.Sleep(10);
                                    }

                                    dynamic taskFromResult = Task.FromResult(taskItem);
                                    if (taskFromResult != null) {
                                        output = (T)(object)(taskFromResult.Result.Result);
                                    }
                                } else {
                                    output = (T)r;
                                }
                            }
                        }
                    } else {
                        IEnumerable<Microsoft.CodeAnalysis.Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                            diagnostic.IsWarningAsError ||
                            diagnostic.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);

                        foreach (Microsoft.CodeAnalysis.Diagnostic diagnostic in failures) {
                            Console.WriteLine(diagnostic.Id.ToString() + ": " + diagnostic.GetMessage());
                        }
                    }
                }
            } catch (Exception ex) {
                // When an error is encountered the output object will be null, so create and return a new object.
                //return new DataObjects.DynamicWorkflowResponse { Message = ex.Message };
                Console.WriteLine("Exception: " + ex.Message);
                if (ex.InnerException != null) {
                    Console.WriteLine("Inner Exception: " + ex.InnerException.Message);
                }
            }

            return output;
        }

        private List<string> ExecuteDynamicCSharpCode_GetAssemblyLocationsFromCode(List<string> libraries)
        {
            var output = new List<string>();

            string code =
            "namespace MyCodeNamespace {" + Environment.NewLine +
                "  using System;" + Environment.NewLine +
                "  using System.Collections.Generic;" + Environment.NewLine +
                "  " + Environment.NewLine +
                "  class MyCode {" + Environment.NewLine +
                "    public List<String> MyFunction(List<String> input) {" + Environment.NewLine +
                "      List<String> output = new List<String>(){" + Environment.NewLine;

            foreach (var line in libraries) {
                code += "        " + line + "," + Environment.NewLine;
            }

            code +=
                "      };" + Environment.NewLine +
                "      return output;" + Environment.NewLine +
                "    }" + Environment.NewLine +
                "  }" + Environment.NewLine +
                "}";

            var results = ExecuteDynamicCSharpCode<List<string>>(code, new object[] { libraries }, null, "MyCodeNamespace", "MyCode", "MyFunction");
            if (results != null && results.Any()) {
                output = results;
            }

            return output;
        }

        private string FindFirstLineStartingWith(string code, string startsWith)
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

        private T? GetDictionaryProperty<T>(string key, Dictionary<string, object> properties)
        {
            var output = default(T);

            var item = properties.FirstOrDefault(x => x.Key.ToLower() == key.ToLower());
            if (!String.IsNullOrWhiteSpace(item.Key)) {
                try {
                    output = (T)item.Value;
                } catch { }
            }

            return output;
        }

        private List<string> GetFilesWithExtensions(string path, IEnumerable<string> extensions)
        {
            var output = new List<string>();

            if (System.IO.Path.Exists(path)) {
                if (extensions.Any()) {
                    foreach (var ext in extensions) {
                        var files = System.IO.Directory.EnumerateFiles(path, ext);
                        foreach (var file in files) {
                            if (!output.Contains(file)) {
                                output.Add(file);
                            }
                        }
                    }

                    if (output.Any()) {
                        output = output.Order().ToList();
                    }
                }
            }

            return output;
        }

        private string GetPluginNamespace(string code)
        {
            string output = String.Empty;

            var line = FindFirstLineStartingWith(code, "namespace");
            if (!String.IsNullOrWhiteSpace(line)) {
                output = line.Replace("namespace ", "").Replace(";", "").Replace("{", "").Trim();
            }

            return output;
        }

        private string GetPluginClass(string code)
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
        /// <inheritdoc/>
        /// </summary>
        public List<Plugin> Load(string path)
        {
            _pluginFolder = path;

            var output = new List<Plugin>();

            var files = GetFilesWithExtensions(path, ["*.cs", "*.plugin"]);

            if (files != null && files.Any()) {
                foreach (var file in files) {
                    var code = System.IO.File.ReadAllText(file);

                    // See if this plugin has a .assemblies file that lists additional assemblies required to run the plugin.
                    var additionalAssemblies = new List<string>();
                    var assembliesFile = file
                        .Replace(".cs", ".assemblies")
                        .Replace(".plugin", ".assemblies");

                    if (System.IO.File.Exists(assembliesFile)) {
                        var assembliesText = System.IO.File.ReadAllLines(assembliesFile);
                        if (assembliesText != null && assembliesText.Any()) {
                            List<string> loadAssemblies = new List<string>();
                            foreach (var assembly in assembliesText) {
                                if (!String.IsNullOrWhiteSpace(assembly)) {
                                    var lowerAssembly = assembly.ToLower().Trim();

                                    if (lowerAssembly.EndsWith(".dll")) {
                                        // This is a dll that can be loaded directly.
                                        if (lowerAssembly.StartsWith(".\\") || lowerAssembly.StartsWith("./")) {
                                            var assemblyFile = System.IO.Path.Combine(path, assembly.Substring(2));
                                            additionalAssemblies.Add(assemblyFile);
                                        } else {
                                            additionalAssemblies.Add(assembly);
                                        }
                                    } else if (lowerAssembly.StartsWith("typeof(")) {
                                        loadAssemblies.Add(assembly);
                                    }
                                }
                            }

                            if (loadAssemblies.Any()) {
                                var loaded = ExecuteDynamicCSharpCode_GetAssemblyLocationsFromCode(loadAssemblies);
                                if (loaded.Any()) {
                                    additionalAssemblies.AddRange(loaded);
                                }
                            }
                        }
                    }

                    if (!String.IsNullOrWhiteSpace(code)) {
                        string ns = GetPluginNamespace(code);
                        string c = GetPluginClass(code);

                        var properties = ExecuteDynamicCSharpCode<Dictionary<string, object>>(code, null, additionalAssemblies, ns, c, "Properties");

                        if (properties != null) {
                            var id = GetDictionaryProperty<Guid>("Id", properties);

                            if (id != Guid.Empty && !_plugins.Any(x => x.Id == id)) {
                                // This plugin has an ID and it is unique, so we can use it.

                                //var prompts = GetDictionaryProperty<Dictionary<string, object>>("Prompts", properties);
                                var prompts = GetDictionaryProperty<List<PluginPrompt>>("Prompts", properties);

                                bool containsSensitiveData = GetDictionaryProperty<bool>("ContainsSensitiveData", properties);

                                var limitToTenants = GetDictionaryProperty<List<Guid>>("LimitToTenants", properties);

                                var env = GetDictionaryProperty<string>("Environment", properties);

                                var plugin = new Plugin {
                                    Id = id,
                                    Author = String.Empty + GetDictionaryProperty<string>("Author", properties),
                                    ClassName = c,
                                    Code = containsSensitiveData ? EncryptCode(code) : code,
                                    ContainsSensitiveData = containsSensitiveData,
                                    Description = String.Empty + GetDictionaryProperty<string>("Description", properties),
                                    LimitToTenants = limitToTenants != null ? limitToTenants : new List<Guid>(),
                                    Name = String.Empty + GetDictionaryProperty<string>("Name", properties),
                                    Namespace = ns,
                                    Prompts = prompts != null && prompts.Count > 0 ? prompts : new List<PluginPrompt>(),
                                    PromptValues = new List<PluginPromptValue>(),
                                    PromptValuesOnUpdate = String.Empty + GetDictionaryProperty<string>("PromptValuesOnUpdate", properties),
                                    Properties = properties,
                                    SortOrder = GetDictionaryProperty<int>("SortOrder", properties),
                                    Type = String.Empty + GetDictionaryProperty<string>("Type", properties),
                                    Version = String.Empty + GetDictionaryProperty<string>("Version", properties),
                                    AdditionalAssemblies = additionalAssemblies,
                                };

                                // Set the invoker based on the type of plugin for built-in types.
                                switch (plugin.Type.ToLower()) {
                                    case "auth":
                                        plugin.Invoker = "Login";
                                        break;

                                    case "userupdate":
                                        plugin.Invoker = "UpdateUser";
                                        break;

                                    default:
                                        plugin.Invoker = "Execute";
                                        break;
                                }

                                output.Add(plugin);

                                // Only add to the internal collection if this is a unique new plugin.
                                var existing = _plugins
                                    .FirstOrDefault(x =>
                                        x.Author == plugin.Author &&
                                        x.Code == plugin.Code &&
                                        x.Name == plugin.Name &&
                                        x.Type == plugin.Type &&
                                        x.Version == plugin.Version
                                );

                                if (existing == null) {
                                    _plugins.Add(plugin);
                                }
                            }
                        }
                    }
                }

                output = output.OrderBy(x => x.SortOrder).ThenBy(x => x.Name).ToList();
            }

            return output;
        }

        /// <summary>
        /// The path to the plugins folder.
        /// </summary>
        public string PluginFolder {
            get { return _pluginFolder; }
        }

        private List<string> SplitStringIntoLines(string? input)
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
        /// <inheritdoc/>
        /// </summary>
        public List<string> ServerReferences {
            get { return _serverReferences; }
            set { _serverReferences = value != null && value.Count > 0 ? value : new List<string>(); }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public List<string> UsingStatements {
            get { return _usingStatements; }
            set { _usingStatements = value != null && value.Count > 0 ? value : new List<string>(); }
        }
    }









    public enum PluginPromptType
    {
        Button,
        Checkbox,
        CheckboxList,
        Date,
        DateTime,
        File,
        Files,
        HTML,
        Multiselect,
        Number,
        Password,
        Radio,
        Select,
        Text,
        Textarea,
        Time,
    }

    /// <summary>
    /// Plugin object.
    /// </summary>
    public class Plugin
    {
        /// <summary>
        /// The unique Guid Id for this plugin.
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// The name of the Author of this plugin.
        /// </summary>
        public string Author { get; set; } = "";
        /// <summary>
        /// The name of the class that contains the plugin code. This is detected at startup and should not be set manually.
        /// </summary>
        public string ClassName { get; set; } = "";
        /// <summary>
        /// The code for the plugin. This is read at startup and should not be set manually.
        /// </summary>
        public string Code { get; set; } = "";
        /// <summary>
        /// Flag that indicates if this plugin contains sensitive data.
        /// If true, then the Code will be encrypted before this object is sent to the client browser.
        /// While it is not likely that a client would be able to inspect the source code, this option is here for safety.
        /// </summary>
        public bool ContainsSensitiveData { get; set; }
        /// <summary>
        /// A description of this plugin.
        /// </summary>
        public string Description { get; set; } = "";
        /// <summary>
        /// An option to limit this plugin to specific tenants.
        /// If this is not set, the plugin will be available to all tenants.
        /// Otherwise, the plugin will only be available if the tenant Id matches one of the Ids in this list.
        /// </summary>
        public List<Guid> LimitToTenants { get; set; } = new List<Guid>();
        /// <summary>
        /// The name of this plugin.
        /// </summary>
        public string Name { get; set; } = "";
        /// <summary>
        /// The namespace in which the plugin resides. This is detected at startup and should not be set manually.
        /// </summary>
        public string Namespace { get; set; } = "";
        /// <summary>
        /// The main invoker function for this plugin (defaults to "Execute".)
        /// </summary>
        public string Invoker { get; set; } = "Execute";
        /// <summary>
        /// An optional collection of Prompts that can be used to collect data for this plugin.
        /// </summary>
        public List<PluginPrompt> Prompts { get; set; } = new List<PluginPrompt>();
        /// <summary>
        /// Holds the values for any prompts that have been collected.
        /// </summary>
        public List<PluginPromptValue> PromptValues { get; set; } = new List<PluginPromptValue>();
        /// <summary>
        /// The name of a function to execute when a prompt value is updated.
        /// </summary>
        public string PromptValuesOnUpdate { get; set; } = "";
        /// <summary>
        /// The collection of Properties for this plugin read from the Properties method when the plugin is first loaded.
        /// </summary>
        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
        /// <summary>
        /// The sort order for the plugin. Plugins will sort first by SortOrder, then by name to determine the order in which they are processed.
        /// </summary>
        public int SortOrder { get; set; }
        /// <summary>
        /// The type of plugin.
        /// </summary>
        public string Type { get; set; } = "";
        /// <summary>
        /// Any values for this plugin.
        /// </summary>
        public List<PluginPromptValue> Values { get; set; } = new List<PluginPromptValue>();
        /// <summary>
        /// The version of the plugin.
        /// </summary>
        public string Version { get; set; } = "";
        /// <summary>
        /// Holds the values for any additional assemblies that need to be loaded for this plugin, detected
        /// during startup and should not be set manually.
        /// </summary>
        public List<string> AdditionalAssemblies { get; set; } = new List<string>();
    }

    /// <summary>
    /// Object used to pass a request to execute a plugin.
    /// </summary>
    public class PluginExecuteRequest
    {
        /// <summary>
        /// The plugin to be executed.
        /// </summary>
        public Plugin Plugin { get; set; } = new Plugin();
        /// <summary>
        /// Any objects to pass along to the invoker function.
        /// </summary>
        public object[]? Objects { get; set; }
    }

    /// <summary>
    /// Object that contains the results of executing a plugin.
    /// </summary>
    public class PluginExecuteResult
    {
        /// <summary>
        /// Indicates if the result was a success or failure.
        /// </summary>
        public bool Result { get; set; }
        /// <summary>
        /// Contains any messages returned from the code.
        /// </summary>
        public List<string> Messages { get; set; } = new List<string>();
        /// <summary>
        /// Contains any objects returned from the code.
        /// </summary>
        public List<object> Objects { get; set; } = new List<object>();
    }

    /// <summary>
    /// Defines an individual prompt item for a plugin.
    /// </summary>
    public class PluginPrompt
    {
        /// <summary>
        /// Any default value for this prompt.
        /// </summary>
        public string DefaultValue { get; set; } = "";
        /// <summary>
        /// A description of this prompt. This will be shown above the prompt
        /// when using the built-in PluginPrompts Blazor component.
        /// </summary>
        public string Description { get; set; } = "";
        /// <summary>
        /// A class to add to this individual prompt.
        /// </summary>
        public string ElementClass { get; set; } = "";
        /// <summary>
        /// Indicates if this prompt element should be initially hidden.
        /// Can be updated by using the PromptValuesOnUpdate property of the plugin
        /// to call a function that will be used to update the plugin and/or prompts.
        /// </summary>
        public bool Hidden { get; set; }
        /// <summary>
        /// The type of this prompt.
        /// </summary>
        public PluginPromptType Type { get; set; }
        /// <summary>
        /// The name for this prompt. Must be unique within the plugin prompt collection.
        /// </summary>
        public string Name { get; set; } = "";
        /// <summary>
        /// An optional function to call to load Options for this prompt.
        /// You can either specific Options directly in the Options property or use this
        /// function to load them dynamically with a matching function in your plugin.
        /// </summary>
        public string Function { get; set; } = "";
        /// <summary>
        /// Options for this prompt.
        /// </summary>
        public List<PluginPromptOption>? Options { get; set; }
        /// <summary>
        /// Indicates if this is a required prompt. If it is, then when using the Blazor
        /// PluginPrompts component, this element will be marked as required.
        /// </summary>
        public bool Required { get; set; }
    }

    /// <summary>
    /// An individual plugin prompt option.
    /// </summary>
    public class PluginPromptOption
    {
        /// <summary>
        /// The label for the option.
        /// </summary>
        public string Label { get; set; } = "";
        /// <summary>
        /// The value for the option.
        /// </summary>
        public string Value { get; set; } = "";
    }

    /// <summary>
    /// Holds the values for a plugin prompt.
    /// </summary>
    public class PluginPromptValue
    {
        /// <summary>
        /// The name of the prompt.
        /// </summary>
        public string Name { get; set; } = "";
        /// <summary>
        /// Any values for the prompt.
        /// </summary>
        public string[]? Values { get; set; }
    }
}