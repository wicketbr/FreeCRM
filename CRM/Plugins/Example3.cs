using CRM;
using Plugins;

namespace ExamplePlugin;

public class Example3 : IPlugin
{
    public Dictionary<string, object> Properties() =>
        new Dictionary<string, object> {
            { "Id", new Guid("4dd6cae9-b9a7-4048-8f7c-f338151d46ab") },
            { "Author", "Bradley R. Wickett" },
            { "ContainsSensitiveData", true },
            { "Name", "Plugin Example 3" },
            { "SortOrder", 3 },
            { "Type", "Example" },
            { "Version", "1.0.0" },
        };

    public async Task<(bool Result, List<string>? Messages, IEnumerable<object>? Objects)> Execute(
        DataAccess da,
        Plugins.Plugin plugin,
        DataObjects.User? currentUser
    )
    {
        // Just added this to simulate a delay.
        await System.Threading.Tasks.Task.Delay(10);

        var messages = new List<string>();
        messages.Add("Plugin: " + plugin.Name);
        messages.Add("Plugin Author: " + plugin.Author);
        messages.Add("Plugin Version: " + plugin.Version);
        messages.Add("");
        messages.Add("App Version: " + da.Version);
        messages.Add("Release Date: " + da.Released.ToString());
        messages.Add("");

        if (currentUser != null) {
            messages.Add("Current User: " + currentUser.FirstName + " " + currentUser.LastName);
            messages.Add("Current User Email: " + currentUser.Email);
            messages.Add("Current User Id: " + currentUser.UserId.ToString());
        } else {
            messages.Add("Current User is NULL");
        }

        object[] output = new object[] { "This is an object returned from the plugin." };

        return (Result: true, Messages: messages, Objects: output);
    }
}