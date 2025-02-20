namespace CRM;

public partial interface IDataAccess
{
    DataObjects.ActiveDirectoryUserInfo? GetActiveDirectoryInfo(Guid TenantId, string Lookup, DataObjects.UserLookupType Type);
}

public partial class DataAccess
{
    private DataObjects.ActiveDirectoryUserInfo? AuthenticateWithLDAP(Guid TenantId, string Username, string Password)
    {
        DataObjects.ActiveDirectoryUserInfo? output = null;

        var settings = GetTenantSettings(TenantId);
        var ldapLookupRoot = settings.LdapLookupRoot;
        var ldapLookupSearchBase = settings.LdapLookupSearchBase;
        var ldapLookupLocationAttribute = settings.LdapLookupLocationAttribute;
        int ldapLookupPort = settings.LdapLookupPort;

        if (Username.Contains("@")) {
            output = GetActiveDirectoryInfo(Username, DataObjects.UserLookupType.Email, ldapLookupRoot, ldapLookupSearchBase,
                ldapLookupPort, Username, Password, ldapLookupLocationAttribute);
        } else {
            output = GetActiveDirectoryInfo(Username, DataObjects.UserLookupType.Username, ldapLookupRoot, ldapLookupSearchBase,
                ldapLookupPort, Username, Password, ldapLookupLocationAttribute);
        }

        return output;
    }

    public DataObjects.ActiveDirectoryUserInfo? GetActiveDirectoryInfo(Guid TenantId, string Lookup, DataObjects.UserLookupType Type)
    {
        // Get the LDAP settings from the tenant
        var settings = GetTenantSettings(TenantId);
        var ldapLookupRoot = settings.LdapLookupRoot;
        var ldapLookupUsername = settings.LdapLookupUsername;
        var ldapLookupPassword = settings.LdapLookupPassword;
        var ldapLookupSearchBase = settings.LdapLookupSearchBase;
        var ldapLookupLocationAttribute = settings.LdapLookupLocationAttribute;
        int ldapLookupPort = settings.LdapLookupPort;

        var output = GetActiveDirectoryInfo(Lookup, Type, ldapLookupRoot, ldapLookupSearchBase,
            ldapLookupPort, ldapLookupUsername, ldapLookupPassword, ldapLookupLocationAttribute);

        return output;
    }

    private DataObjects.ActiveDirectoryUserInfo? GetActiveDirectoryInfo(string Lookup, DataObjects.UserLookupType Type,
        string? LdapRoot, string? SearchBase, int? LdapPort, string? LdapQueryUsername, string? LdapQueryPassword, string? LdapLocationAttribute)
    {
        if (String.IsNullOrWhiteSpace(LdapLocationAttribute)) {
            LdapLocationAttribute = "physicalDeliveryOfficeName";
        }

        DataObjects.ActiveDirectoryUserInfo? output = null;

        string searchFilter = String.Empty;
        switch (Type) {
            case DataObjects.UserLookupType.Email:
                searchFilter = "(&(objectClass=user)(objectCategory=person)(mail=" + Lookup + "))";
                break;

            case DataObjects.UserLookupType.EmployeeId:
                searchFilter = "(&(objectClass=user)(objectCategory=person)(employeeid=" + Lookup + "))";
                break;

            case DataObjects.UserLookupType.Username:
                searchFilter = "(&(objectClass=user)(objectCategory=person)(samaccountname=" + Lookup + "))";
                break;
        }

        // Make sure all required parameters are included. If not, just return the null object.
        if (String.IsNullOrWhiteSpace(Lookup) || String.IsNullOrWhiteSpace(searchFilter) || String.IsNullOrWhiteSpace(LdapRoot) ||
            String.IsNullOrWhiteSpace(SearchBase) || String.IsNullOrWhiteSpace(LdapQueryUsername) || String.IsNullOrEmpty(LdapQueryPassword)) {
            return output;
        }

        int ldapPort = LdapPort.HasValue ? (int)LdapPort : Novell.Directory.Ldap.LdapConnection.DefaultPort;
        if (ldapPort < 1) {
            ldapPort = Novell.Directory.Ldap.LdapConnection.DefaultPort;
        }

        var ldapConn = new Novell.Directory.Ldap.LdapConnection();
        ldapConn.Connect(LdapRoot, ldapPort);

        try {
            ldapConn.Bind(LdapQueryUsername, LdapQueryPassword);
        } catch {
            return output;
        }

        string[] attributes = new string[] {
            "sAMAccountName", "employeeid", "givenName", "sn", "mail",
            "title", "telephoneNumber", "department",
            "name", LdapLocationAttribute, "objectGUID"
        };

        try {
            var searchResults = ldapConn.Search(SearchBase, 2, searchFilter, attributes, false);

            if (searchResults != null) {
                foreach (var result in searchResults) {
                    if (output == null) {
                        // Only use the first result.
                        var user = new DataObjects.ActiveDirectoryUserInfo();

                        // First, get the GUID
                        Guid objectGUID = Guid.Empty;
                        try {
                            var bytes = result.GetAttribute("objectGUID").ByteValue;
                            if (bytes != null) {
                                objectGUID = new Guid(bytes);
                            }
                        } catch { }

                        if (objectGUID != Guid.Empty) {
                            user.UserId = objectGUID;
                        }

                        // Try to add each remaining property. If properties are missing they throw
                        // an error, so try/catch each attribute.
                        try { user.Department = result.GetAttribute("department").StringValue; } catch { }
                        try { user.Username = result.GetAttribute("sAMAccountName").StringValue; } catch { }
                        try { user.FirstName = result.GetAttribute("givenName").StringValue; } catch { }
                        try { user.LastName = result.GetAttribute("sn").StringValue; } catch { }
                        try { user.Email = result.GetAttribute("mail").StringValue; } catch { }
                        try { user.Phone = result.GetAttribute("telephoneNumber").StringValue; } catch { }
                        try { user.EmployeeId = result.GetAttribute("employeeid").StringValue; } catch { }
                        try { user.Title = result.GetAttribute("title").StringValue; } catch { }
                        try { user.Location = result.GetAttribute(LdapLocationAttribute).StringValue; } catch { }

                        output = user;
                    }
                }
            }
        } catch { }

        return output;
    }

    private List<DataObjects.ActiveDirectoryUserInfo>? GetActiveDirectorySearchResults(Guid TenantId, string SearchText,
        int MaxResults,
        List<string>? excludeEmails)
    {
        // Get the LDAP settings from the tenant
        var settings = GetTenantSettings(TenantId);
        var ldapLookupRoot = settings.LdapLookupRoot;
        var ldapLookupUsername = settings.LdapLookupUsername;
        var ldapLookupPassword = settings.LdapLookupPassword;
        var ldapLookupSearchBase = settings.LdapLookupSearchBase;
        var ldapLookupLocationAttribute = settings.LdapLookupLocationAttribute;
        int ldapLookupPort = settings.LdapLookupPort;

        List<DataObjects.ActiveDirectoryUserInfo>? output = null;

        // Make sure the required values are set.
        if (String.IsNullOrWhiteSpace(SearchText) || String.IsNullOrWhiteSpace(ldapLookupRoot) || String.IsNullOrWhiteSpace(ldapLookupSearchBase) ||
            String.IsNullOrWhiteSpace(ldapLookupUsername) || String.IsNullOrWhiteSpace(ldapLookupPassword)) {
            return output;
        }

        if (String.IsNullOrWhiteSpace(ldapLookupLocationAttribute)) {
            ldapLookupLocationAttribute = "physicalDeliveryOfficeName";
        }

        int ldapPort = ldapLookupPort > 0 ? ldapLookupPort : Novell.Directory.Ldap.LdapConnection.DefaultPort;
        if (ldapPort < 1) {
            ldapPort = Novell.Directory.Ldap.LdapConnection.DefaultPort;
        }

        var ldapConn = new Novell.Directory.Ldap.LdapConnection();
        ldapConn.Connect(ldapLookupRoot, ldapPort);

        try {
            ldapConn.Bind(ldapLookupUsername, ldapLookupPassword);
        } catch {
            return output;
        }

        string[] attributes = new string[] {
            "sAMAccountName", "employeeid", "givenName", "sn", "mail",
            "title", "telephoneNumber", "department",
            "name", ldapLookupLocationAttribute, "objectGUID"
        };

        string searchFilter = "(&(objectClass=user)(objectCategory=person)(anr=" + SearchText + "))";

        try {
            var searchResults = ldapConn.Search(ldapLookupSearchBase, 2, searchFilter, attributes, false);

            if (searchResults != null) {
                foreach (var result in searchResults) {
                    var user = new DataObjects.ActiveDirectoryUserInfo();

                    // First, get the GUID
                    Guid objectGUID = Guid.Empty;
                    try {
                        var bytes = result.GetAttribute("objectGUID").ByteValue;
                        if (bytes != null) {
                            objectGUID = new Guid(bytes);
                        }
                    } catch { }

                    if (objectGUID != Guid.Empty) {
                        user.UserId = objectGUID;
                    }

                    // Try to add each remaining property. If properties are missing they throw
                    // an error, so try/catch each attribute.
                    try { user.Department = result.GetAttribute("department").StringValue; } catch { }
                    try { user.Username = result.GetAttribute("sAMAccountName").StringValue; } catch { }
                    try { user.FirstName = result.GetAttribute("givenName").StringValue; } catch { }
                    try { user.LastName = result.GetAttribute("sn").StringValue; } catch { }
                    try { user.Email = result.GetAttribute("mail").StringValue; } catch { }
                    try { user.Phone = result.GetAttribute("telephoneNumber").StringValue; } catch { }
                    try { user.EmployeeId = result.GetAttribute("employeeid").StringValue; } catch { }
                    try { user.Title = result.GetAttribute("title").StringValue; } catch { }
                    try { user.Location = result.GetAttribute(ldapLookupLocationAttribute).StringValue; } catch { }

                    if (output == null) {
                        output = new List<DataObjects.ActiveDirectoryUserInfo>();
                    }
                    output.Add(user);
                }
            }
        } catch { }

        return output;
    }
}