using Microsoft.EntityFrameworkCore;

namespace CRM.EFModels;

public partial class EFDataModel : DbContext
{
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // MySQL, PostgreSQL, and SQLite store uniqueidentifier fields (GUID) as a string in the EFCore provider.
        // So, if this instance is running one of those providers then override the converter for all Guid
        // values to use strings.
        var providerName = this.Database.ProviderName;
        if (!String.IsNullOrEmpty(providerName)) {
            switch (providerName.ToUpper()) {
                case "MICROSOFT.ENTITYFRAMEWORKCORE.SQLSERVER":
                case "MICROSOFT.ENTITYFRAMEWORKCORE.INMEMORY":
                    break;

                case "MYSQL.ENTITYFRAMEWORKCORE":
                case "NPGSQL.ENTITYFRAMEWORKCORE.POSTGRESQL":
                case "MICROSOFT.ENTITYFRAMEWORKCORE.SQLITE":
                    configurationBuilder
                        .Properties<Guid>()
                        .HaveConversion<Microsoft.EntityFrameworkCore.Storage.ValueConversion.GuidToStringConverter>();
                    break;
            }
        }
    }
}