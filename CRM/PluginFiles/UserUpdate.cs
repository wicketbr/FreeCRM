using CRM;
using Plugins;

namespace UserUpdatePlugin;

public class UserUpdate : IPluginUserUpdate
{
    public Dictionary<string, object> Properties() =>
        new Dictionary<string, object> {
            { "Id", new Guid("0c5770a0-0dbe-4141-ab16-450bfee850eb") },
            { "Author", "Bradley R. Wickett" },
            { "ContainsSensitiveData", true },
            { "Name", "Update User Information" },
            { "Type", "UserUpdate" },
            { "Version", "1.0.0" },
        };

    public async Task<(bool Result, List<string>? Messages, IEnumerable<object>? Objects)> UpdateUser(
        DataAccess da,
        Plugins.Plugin plugin,
        DataObjects.User? updateUser
    )
    {
        await Task.Delay(0); // Simulate a delay since this method has to be async. This can be removed once you implement your await logic.

        // For testing this plugin is just going to convert the user's email between uppercase and lowercase.
        // In a real-world scenario you would be looking up a user in some external system and updating properties as needed.

        bool result = false;
        var messages = new List<string>();

        if (updateUser != null && !String.IsNullOrWhiteSpace(updateUser.Email)) {
            result = true;

            if (updateUser.Email == updateUser.Email.ToUpper()) {
                updateUser.Email = updateUser.Email.ToLower();
            } else {
                updateUser.Email = updateUser.Email.ToUpper();
            }
        } else {
            messages.Add("Cannot update user without an email address.");
        }

        if (updateUser != null) {
            return (Result: result, Messages: messages, Objects: new object[] { updateUser });
        } else {
            return (Result: result, Messages: messages, Objects: null);
        }
    }
}