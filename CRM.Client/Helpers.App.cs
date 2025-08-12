namespace CRM.Client;

public static partial class Helpers
{
    public static Dictionary<string, List<string>> AppIcons {
        get {
            Dictionary<string, List<string>> icons = new Dictionary<string, List<string>> {
                { "fa:fa-solid fa-home", new List<string> { "IconName1", "IconName2" }},
            };

            return icons;
        }
    }

    public static bool AppMethod()
    {
        return true;
    }

    private async static Task ReloadModelApp()
    {

    }
}
