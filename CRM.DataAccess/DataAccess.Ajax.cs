namespace CRM;

public partial interface IDataAccess
{
    Task<DataObjects.AjaxLookup> AjaxUserSearch(DataObjects.AjaxLookup Lookup, bool LocalOnly = false);
}

public partial class DataAccess
{
    public async Task<DataObjects.AjaxLookup> AjaxUserSearch(DataObjects.AjaxLookup Lookup, bool LocalOnly = false)
    {
        // A combination of results from the local users table and from Active Directory
        List<DataObjects.AjaxResults> results = new List<DataObjects.AjaxResults>();

        // First, find local accounts (limit to 25)
        int Local = 0;
        var recs = data.Users.Where(x => x.TenantId == Lookup.TenantId && x.Enabled == true && x.Deleted != true);

        string search = StringValue(Lookup.Search).ToLower();

        string LastName = String.Empty;
        string FirstName = String.Empty;
        string[] Names;

        if (search.Contains(",")) {
            // Check "Last, First"
            Names = search.Split(',');
            try {
                LastName += Names[0];
                FirstName += Names[1];
            } catch { }
            LastName = LastName.Trim().ToLower();
            FirstName = FirstName.Trim().ToLower();

            if (!String.IsNullOrWhiteSpace(LastName) && !String.IsNullOrWhiteSpace(FirstName)) {
                recs = recs.Where(x => x.LastName != null && x.LastName.ToLower().StartsWith(LastName) && x.FirstName != null && x.FirstName.ToLower().StartsWith(FirstName));
            } else {
                recs = recs.Where(x => x.LastName != null && x.LastName.ToLower().StartsWith(LastName));
            }
        } else if (search.Contains(" ")) {
            // Check "First Last"
            Names = search.Split(' ');
            try {
                FirstName += Names[0];
                LastName += Names[1];
            } catch { }
            LastName = LastName.Trim().ToLower();
            FirstName = FirstName.Trim().ToLower();

            if (!String.IsNullOrWhiteSpace(LastName) && !String.IsNullOrWhiteSpace(FirstName)) {
                recs = recs.Where(x => x.LastName != null && x.LastName.ToLower().StartsWith(LastName) && x.FirstName != null && x.FirstName.ToLower().StartsWith(FirstName));
            } else {
                recs = recs.Where(x => x.FirstName != null && x.FirstName.ToLower().StartsWith(FirstName));
            }
        } else {
            recs = recs.Where(
                x => (x.FirstName != null && x.FirstName.ToLower().Contains(search)) ||
                (x.LastName != null && x.LastName.ToLower().Contains(search)) ||
                (x.Email != null && x.Email.ToLower().Contains(search))
                );
        }

        recs = recs.OrderBy(x => x.LastName).ThenBy(x => x.FirstName);

        if (recs != null && recs.Count() > 0) {
            //results.Add(new DataObjects.AjaxResults {
            //	value = Guid.Empty.ToString(),
            //	label = "--- Local ---"
            //});
            foreach (var rec in recs) {
                if(StringValue(rec.Username).ToLower() != "admin") {
                    Local += 1;
                    if (Local < 26) {
                        string deptName = rec.Department != null && !String.IsNullOrEmpty(rec.Department.DepartmentName) ? rec.Department.DepartmentName : String.Empty;
                        string location = rec.Location != null ? rec.Location : String.Empty;

                        results.Add(new DataObjects.AjaxResults {
                            value = rec.UserId.ToString(),
                            label = DisplayNameFromLastAndFirst(rec.LastName, rec.FirstName, rec.Email, deptName, location) +
                                (!String.IsNullOrWhiteSpace(rec.Email) ? " (" + rec.Email + ")" : ""),
                            email = !String.IsNullOrWhiteSpace(rec.Email) ? rec.Email : "",
                            username = rec.Username,
                            extra1 = rec.Phone,
                            extra2 = rec.Location,
                            extra3 = rec.DepartmentId.ToString()
                        });
                    }
                }
            }
        }

        // Next, see if there are any ad results to add
        int ldapResults = 0;

        if (!LocalOnly) {
            // See if there are any LDAP results. This will return null if this tenant isn't configured for LDAP.
            List<string> excludeEmails = new List<string>();
            if (results.Count() > 0) {
                foreach (var item in results.Where(x => !String.IsNullOrWhiteSpace(x.email))) {
                    if (!String.IsNullOrEmpty(item.email)) {
                        excludeEmails.Add(item.email.ToLower());
                    }
                }
            }

            var ldapLookupResults = await GetActiveDirectorySearchResults(Lookup.TenantId, search, 25, excludeEmails);
            if (ldapLookupResults != null && ldapLookupResults.Any()) {
                results.Add(new DataObjects.AjaxResults {
                    value = Guid.Empty.ToString(),
                    label = "--- LDAP Results ---"
                });

                foreach (var ldapUser in ldapLookupResults) {
                    // Only add if we don't already have this user
                    var match = results.FirstOrDefault(x => x.email == ldapUser.Email);
                    if (match == null) {
                        ldapResults += 1;
                        if (ldapResults < 26) {
                            results.Add(new DataObjects.AjaxResults {
                                value = ldapUser.UserId.HasValue ? ((Guid)ldapUser.UserId).ToString() : Guid.Empty.ToString(),
                                label = DisplayNameFromLastAndFirst(ldapUser.LastName, ldapUser.FirstName, ldapUser.Email, ldapUser.Department, ldapUser.Location) + " [LDAP]" +
                                    (!String.IsNullOrWhiteSpace(ldapUser.Email) ? " (" + ldapUser.Email + ")" : ""),
                                email = ldapUser.Email
                            });
                        }
                    }
                }
            }
        }


        if (Local > 25 || ldapResults > 25) {
            // We limited the results
            results.Insert(0, new DataObjects.AjaxResults {
                value = Guid.Empty.ToString(),
                label = "--- Too many results found, please narrow your search ---"
            });
        }

        Lookup.Results = results;
        return Lookup;
    }
}