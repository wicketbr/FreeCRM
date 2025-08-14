using CRM;
using Plugins;

namespace ExamplePlugin;

public class Example2 : IPlugin
{
    public Dictionary<string, object> Properties() =>
        new Dictionary<string, object> {
            { "Id", new Guid("8507d6b9-deb4-45d6-bd6c-a8267c4a1692") },
            { "Author", "Bradley R. Wickett" },
            { "ContainsSensitiveData", false },
            { "Name", "Plugin Example 2" },
            { "SortOrder", 1 },
            { "Type", "Example" },
            { "Version", "1.0.0" },
            { "LimitToTenants", new List<Guid> { new Guid("00000000-0000-0000-0000-000000000002") }},
        };

    public async Task<(bool Result, List<string>? Messages, IEnumerable<object>? Objects)> Execute(
        DataAccess da,
        Plugins.Plugin plugin,
        DataObjects.User? currentUser
    )
    {
        await Task.Delay(0); // Simulate a delay since this method has to be async. This can be removed once you implement your await logic.

        var messages = new List<string>();
        messages.Add("Plugin: " + plugin.Name);

        object[] output = new object[] { "This is an object returned from the plugin." };

        return (Result: true, Messages: messages, Objects: output);
    }
}