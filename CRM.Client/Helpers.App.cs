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

    public static List<DataObjects.MenuItem> MenuItemsApp {
        get {
            // Add any app-specific top-level menu items here.
            var output = new List<DataObjects.MenuItem>();

            // Sample
            //if (Model.User.Admin) {
            //    output.Add(new DataObjects.MenuItem {
            //        Title = "My Custom Menu Item",
            //        Icon = "Home",
            //        PageNames = new List<string> { "myitems", "editmyitem" },
            //        SortOrder = 1000,
            //        url = Helpers.BuildUrl("MyItems"),
            //        AppAdminOnly = false,
            //    });
            //}

            return output;
        }
    }

    public static List<DataObjects.MenuItem> MenuItemsAdminApp {
        get {
            // Add any app-specific admin menu items here.
            var output = new List<DataObjects.MenuItem>();

            return output;
        }
    }

    private async static Task ReloadModelApp()
    {

    }
}
