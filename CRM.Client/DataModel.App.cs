namespace CRM.Client;

public partial class BlazorDataModel
{
    private List<string> _MyValues = new List<string>();

    private bool HaveDeletedRecordsApp {
        get {
            bool output = false;

            // Check your app-specific deleted records here.
            //if (DeletedRecordCounts.MyValue > 0 ) {
            //    output = true;
            //}

            return output;
        }
    }

    public bool MyCustomDataModelMethod()
    {
        return true;
    }

    /// <summary>
    /// An example of implementing a custom property in your data model.
    /// </summary>
    public List<string> MyValues {
        get {
            return _MyValues;
        }

        set {
            if (!ObjectsAreEqual(_MyValues, value)) {
                _MyValues = value;
                _ModelUpdated = DateTime.UtcNow;
                NotifyDataChanged();
            }
        }
    }

    /// <summary>
    /// Set this option to true if you wish to make sure all Blazor plugins are precompiled during page load.
    /// If this is set to false then any components that have not yet been cached will take some time to load
    /// in the interface while they are being compiled and cached.
    /// </summary>
    public bool PrecompileBlazorPlugins {
        get {
            return true;
        }
    }
}
