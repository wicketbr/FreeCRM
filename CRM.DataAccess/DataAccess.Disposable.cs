namespace CRM;

public partial class DataAccess
{
    private bool disposedValue = false; // To detect redundant calls
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue) {
            if (disposing) {
                if (data != null) {
                    data.Dispose();
                    _connectionString = "";
                }
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}