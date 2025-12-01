using CRM;
using Plugins;

namespace ExamplePlugin
{
    public class Example1 : IPlugin
    {
        /// <summary>
        /// Properties returned by the plugin.
        /// The minimum properties that should be returned are:
        /// Id (a unique Guid), Author, Name, Type, and Version
        /// </summary>
        public Dictionary<string, object> Properties() =>
            new Dictionary<string, object> {
                { "Id", new Guid("9bbdfb99-80cd-4bbb-8741-6d287437e5f7") },
                { "Author", "Brad Wickett" },
                { "ContainsSensitiveData", false },
                { "Description", "You can write a really long description here that can be shown to users in the application." },
                { "Name", "Plugin Example 1" },
                { "Prompts", new List<PluginPrompt>
                    {
                        new PluginPrompt {
                            Name = "Button1",
                            Description = "A sample of how to use a button to execute code.",
                            ElementClass = "col col-12",
                            Type = PluginPromptType.Button,
                            Function = "Button1",
                            Options = new List<PluginPromptOption> {
                                new PluginPromptOption { Label = "ButtonText",  Value = "Test Button" },
                                new PluginPromptOption { Label = "ButtonClass", Value = "btn btn-success" },
                                new PluginPromptOption { Label = "ButtonIcon",  Value = "fa-regular fa-circle-check" },
                            },
                        },
                        new PluginPrompt {
                            Name = "Checkbox",
                            Description = "Click below to agree.",
                            ElementClass = "col col-3",
                            Required = true,
                            Type = PluginPromptType.Checkbox,
                        },
                        new PluginPrompt {
                            Name = "CheckboxList",
                            Description = "Select one or more options from the list.",
                            ElementClass = "col col-3",
                            Hidden = true,
                            Required = true,
                            Type = PluginPromptType.CheckboxList,
                            Options = new List<PluginPromptOption>
                            {
                                new PluginPromptOption { Label = "Option 1", Value = "1" },
                                new PluginPromptOption { Label = "Option 2", Value = "2" },
                                new PluginPromptOption { Label = "Option 3", Value = "3" },
                            },
                        },
                        new PluginPrompt {
                            Name = "Date",
                            Description = "Select a date from the calendar.",
                            ElementClass = "col col-3",
                            Hidden = true,
                            Type = PluginPromptType.Date,
                            Required = false,
                        },
                        new PluginPrompt {
                            Name = "DateTime",
                            Description = "Select a date and time from the calendar.",
                            ElementClass = "col col-3",
                            Hidden = true,
                            Type = PluginPromptType.DateTime,
                            Required = false,
                        },
                        new PluginPrompt {
                            Name = "File",
                            Description = "Upload a file from your computer.",
                            ElementClass = "col col-6",
                            Hidden = true,
                            Type = PluginPromptType.File,
                            Required = false,
                        },
                        new PluginPrompt {
                            Name = "Files",
                            Description = "Upload one or more files from your computer.",
                            ElementClass = "col col-6",
                            Hidden = true,
                            Type = PluginPromptType.Files,
                            Required = false,
                        },
                        new PluginPrompt {
                            Name = "HTML",
                            ElementClass = "col col-12",
                            Hidden = true,
                            Type = PluginPromptType.HTML,
                            DefaultValue = "<h1>HTML Example</h1><p>This is an example of HTML in a plugin prompt used to just diplay some information.</p>",
                        },
                        new PluginPrompt {
                            Name = "Multiselect",
                            Description = "Select one or more values from the list.",
                            ElementClass = "col col-12",
                            Hidden = true,
                            Type = PluginPromptType.Multiselect,
                            Options = new List<PluginPromptOption>
                            {
                                new PluginPromptOption { Label = "Option 1", Value = "1" },
                                new PluginPromptOption { Label = "Option 2", Value = "2" },
                                new PluginPromptOption { Label = "Option 3", Value = "3" },
                            },
                            Required = false,
                        },
                        new PluginPrompt {
                            Name = "Number",
                            Description = "Enter a number.",
                            ElementClass = "col col-3",
                            Hidden = true,
                            Type = PluginPromptType.Number,
                            Required = false,
                        },
                        new PluginPrompt {
                            Name = "Password",
                            Description = "Enter a password.",
                            ElementClass = "col col-3",
                            Hidden = true,
                            Type = PluginPromptType.Password,
                            Required = false,
                        },
                        new PluginPrompt {
                            Name = "Radio",
                            Description = "Select one option from the list.",
                            ElementClass = "col col-3",
                            Hidden = true,
                            Type = PluginPromptType.Radio,
                            Options = new List<PluginPromptOption>
                            {
                                new PluginPromptOption { Label = "Option 1", Value = "1" },
                                new PluginPromptOption { Label = "Option 2", Value = "2" },
                                new PluginPromptOption { Label = "Option 3", Value = "3" },
                            },
                            Required = false,
                        },
                        new PluginPrompt {
                            Name = "Select",
                            Description = "Select one option from the list.",
                            ElementClass = "col col-3",
                            Hidden = true,
                            Type = PluginPromptType.Select,
                            Options = new List<PluginPromptOption>
                            {
                                new PluginPromptOption { Label = "Option 1", Value = "1" },
                                new PluginPromptOption { Label = "Option 2", Value = "2" },
                                new PluginPromptOption { Label = "Option 3", Value = "3" },
                            },
                            Required = false,
                        },
                        new PluginPrompt { 
                            Name = "Select with Values from Function",
                            Description = "",
                            ElementClass = "col col-3",
                            Hidden = true,
                            Type = PluginPromptType.Select,
                            Options = null,
                            Required = false,
                            Function = "GetPromptValues",
                        },
                        new PluginPrompt { 
                            Name = "Text",
                            Description = "Enter some text.",
                            ElementClass = "col col-3",
                            Hidden = true,
                            Type = PluginPromptType.Text,
                            Required = false,
                        },
                        new PluginPrompt {
                            Name = "Textarea",
                            Description = "Please describe...",
                            ElementClass = "col col-3",
                            Hidden = true,
                            Type = PluginPromptType.Textarea,
                            Required = false,
                        },
                        new PluginPrompt { 
                            Name = "Time",
                            Description = "Enter a time.",
                            ElementClass = "col col-3",
                            Hidden = true,
                            Type = PluginPromptType.Time,
                            Required = false,
                        },
                    }
                },
                { "PromptValuesOnUpdate", "PromptValuesUpdated" },
                { "SortOrder", 0 },
                { "Type", "Example" },
                { "Version", "1.0.0" }
            };

        /// <summary>
        /// An example plugin.
        /// </summary>
        /// <param name="objects">
        /// By convention, when a plugin runs on the server then the first object will contain the DataAccess library.
        /// If the plugin is executed in the Blazor client then the first object will be the Blazor DataModel.
        /// Any other objects that are passed to this particular type of plugin will then follow in the objects array.
        /// </param>
        /// <returns>
        /// Custom plugins must return a Tuple containing a boolean with the result, a nullable list of strings with
        /// any messages returned, and a nullable array of objects that need to be returned, depending on the type of plugin.
        /// </returns>
        public async Task<(bool Result, List<string>? Messages, IEnumerable<object>? Objects)> Execute(
            DataAccess da, 
            Plugins.Plugin plugin,
            DataObjects.User? currentUser
        )
        {
            await Task.Delay(0); // Simulate a delay since this method has to be async. This can be removed once you implement your await logic.

            var messages = new List<string>();

            if (plugin != null) {
                messages.Add("Received Plugin Object: " + plugin.Name);

                if (plugin.Prompts != null && plugin.Prompts.Count > 0 && plugin.PromptValues != null && plugin.PromptValues.Count > 0) {
                    messages.Add("");
                    messages.Add("Prompt Values: " + plugin.Prompts.Count.ToString());
                    messages.Add("");

                    int index = -1;
                    foreach (var prompt in plugin.Prompts) {
                        index++;
                        string value = String.Empty;

                        var promptValue = plugin.PromptValues.FirstOrDefault(p => p.Name.ToLower() == prompt.Name.ToLower());
                        if (promptValue != null && promptValue.Values != null && promptValue.Values.Length > 0) {
                            var values = promptValue.Values.ToList();

                            if (prompt.Type == PluginPromptType.File || prompt.Type == PluginPromptType.Files) {
                                for (int i = 0; i < values.Count; i += 2) {
                                    string fileName = values[i];
                                    string fileBytes = String.Empty;
                                    byte[]? bytes = null;

                                    if (values.Count > i + 1) {
                                        fileBytes = values[i + 1];
                                    }

                                    if (!String.IsNullOrWhiteSpace(fileBytes)) {
                                        try {
                                            bytes = Convert.FromBase64String(fileBytes);
                                        } catch { }
                                    }

                                    if (value != String.Empty) {
                                        value += ", ";
                                    }

                                    value += fileName;

                                    if (bytes != null) {
                                        value += " (" + da.BytesToFileSizeLabel(bytes.Length) + ")";
                                    }
                                }
                            } else if (promptValue.Values.Length == 1) {
                                value += promptValue.Values[0];
                            } else {
                                value = String.Join(", ", promptValue.Values);
                            }
                        }

                        if (!String.IsNullOrWhiteSpace(value)) {
                            messages.Add(prompt.Name + " = \"" + value + "\"");
                        }
                    }
                }
            } else {
                messages.Add("Plugin Object Not Received");
            }

            object[] output = new object[] { "This is an object returned from the plugin." };

            return (Result: true, Messages: messages, Objects: output);
        }

        public (bool Result, List<string>? Messages, IEnumerable<object>? Objects) GetPromptValues(
            DataAccess da,
            Plugins.Plugin plugin,
            DataObjects.User? currentUser
        ) {
            //var values = new List<string> { "Value 1", "Value 2", "Value 3" };
            var options = new List<PluginPromptOption> {
                new PluginPromptOption {
                    Label = "Option 1",
                    Value = "1",
                },
                new PluginPromptOption {
                    Label = "Option 2",
                    Value = "2",
                },
                new PluginPromptOption {
                    Label = "Option 3",
                    Value = "3",
                },
            };

            return (Result: true, Messages: null, Objects: options);
        }

        public (bool Result, List<string>? Messages, IEnumerable<object>? Objects) Button1(
            DataAccess da,
            Plugins.Plugin plugin,
            DataObjects.User? currentUser
        )
        {
            return (Result: true, Messages: null, Objects: new List<string> { "Button 1 Clicked" });
        }

        public (bool Result, List<string>? Messages, IEnumerable<object>? Objects) PromptValuesUpdated(
            DataAccess da,
            Plugins.Plugin plugin,
            DataObjects.User? currentUser
        )
        {
            // If the checkbox named "Checkbox" is checked, then show all fields after that.
            bool hidden = false;
            bool foundCheckbox = false;

            foreach(var prompt in plugin.Prompts) {
                if (foundCheckbox) {
                    prompt.Hidden = hidden;
                }

                if (prompt.Name == "Checkbox") {
                    foundCheckbox = true;

                    var values = plugin.PromptValues.FirstOrDefault(p => p.Name.ToLower() == prompt.Name.ToLower());

                    if (values != null && values.Values != null && values.Values.Length > 0) {
                        var isChecked = values.Values[0].ToLower() == "true";
                        hidden = !isChecked;
                    }
                }
            }

            return (Result: true, Messages: null, Objects: [ plugin ]);
        }
    }
}