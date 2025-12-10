using CRM;
using Plugins;

namespace ExamplePlugin
{
    public class ExampleBackgroundProcess : IPluginBackgroundProcess
    {
        /// <summary>
        /// Properties returned by the plugin.
        /// The minimum properties that should be returned are:
        /// Id (a unique Guid), Author, Name, Type, and Version
        /// </summary>
        public Dictionary<string, object> Properties() =>
            new Dictionary<string, object> {
                { "Id", new Guid("3961b30f-0c33-474b-a14c-a73174058f47") },
                { "Author", "Brad Wickett" },
                { "ContainsSensitiveData", false },
                { "Description", "An example of a plugin for the background process engine." },
                { "Name", "Plugin Example Background Process" },
                { "SortOrder", 0 },
                { "Type", "BackgroundProcess" },
                { "Version", "1.0.0" }
            };

        /// <summary>
        /// An example plugin.
        /// </summary>
        /// <param name="objects">
        /// This will receive the DataAccess object and the Plugin object.
        /// </param>
        /// <returns>
        /// Custom plugins must return a Tuple containing a boolean with the result, a nullable list of strings with
        /// any messages returned, and a nullable array of objects that need to be returned, depending on the type of plugin.
        /// </returns>
        public async Task<(bool Result, List<string>? Messages, IEnumerable<object>? Objects)> Execute(
            DataAccess da, 
            Plugins.Plugin plugin,
            long iteration
        )
        {
            // Custom plugins will be called every time the background process runs, which is based on the interval
            // set in the appsettings.json file. If you need to only have this happen on specific days, or during
            // specific hours, you will need to add that logic here.
            // Since you have access to the DataAccess library, you can save settings in the database and read
            // them back in here to keep track of the last time this plugin ran, etc.

            await Task.Delay(0); // Simulate a delay since this method has to be async. This can be removed once you implement your await logic.

            var messages = new List<string>();

            if (plugin != null) {
                messages.Add("Received Plugin Object: " + plugin.Name + ", iteration: " + iteration.ToString());
            } else {
                messages.Add("Plugin Object Not Received");
            }

            return (Result: true, Messages: messages, Objects: null);
        }
    }
}