namespace CRM;

public partial class DataAccess
{
    private byte[] DefaultAdminIcon {
        get {
            string byteString = "89504E470D0A1A0A0000000D494844520000003C0000003C08060000003AFCD972000000097048597300000EC400000EC401952B0E1B000008DD49444154789CED9B598C1C4719C7FFD5C79C3BB3BB337B78D747568E8D6D0E250EC1441011422404128930204511F0C01302F28021209022403C40100F2142E24C2424120C8ACC252B126744128814C08E1383CF756CBCB6F79C9DBBE7E883EFAB9E59CF6C7667A6677AD6929DBF5C9E3DBAABEBD75FD57754F76AC53FBDEF41000F514B50B371FD49A1364FEDDBD49ED0E8BFEFE3FA578CDAE3D4F21A6E2C3D78A3010FDF68C0F68D068C3780AF77BD01ECAF1CFA67D59A73F5C7827201A1B99F1BAC3E01139C5D9550223A0525B60322344E394F0030F3B00B17E0E4CEC2292FD031AA0BBF41F2FF4A6C4D923A7E37B46D1F8132F8661774F5619514ACF917605E781A4EFE353A818F11E8B7FC0526AB8A6012FAEECF13F05D2D0F158104B42DF741DB740FAA677E02F3E26FE8C6F070FA0BED1FB063D2B41D4360EF23720A773E8228F43D0780E008CCB33FED3BB43FC0EC909420026F7BD81B6C83F4ED9F846DCCC19EF92D4DEF20FA257F80ED0AB4A9FBA10CDFDADB60767E1AB9B97F2168CDBACEAC0FEA1DD8B1692A8F9083FA68CF5DA98118C4E6FD289CFC1E0606224D91CC2FF9005C8592B89D9CD508FC5074CB5D9839FE0484514634ACF90EED03302DDF446F53B9515A7413F4F814D28BC72044049190E22B74EFC0E45595F024FC943E300163E128D2790A73424738E81F748FC08E9B1EFAEC558516A6C0E412D6A143017FA07B041632FEC234E0A7EC6A01F558CC90CBB92A12311D411FA07B9FD2B605BB380325793BFC9143E9F6A5A6C2C296D02612711D015DF404DD3BB010B0534780AD1F861FAAE62EA192BD40DD5E8DC36C6B8BA85364E92441EB5AF7D03E006BB008D831AE4084273A3ECDA111731374C3B8D595FFDF9F298FC950DE116ABE0C1D6259049DAD4A4B770BED03304DBD4A1AE6F98394137FA1A35318D4B66D288A421016545595D096B188ECD943E4F9F5B52F45D0A655B334AD69AD0B687F524B2AEDCC4B87691DEF833A76E79A873060E3D775C8460B2F1E7914666196ACBBBED797D0A60BCD96D6546FD03E554B42D6C195FF7C0741AA7E94C4DEA6DF32607DFAB2EAB08E2C3ADCB5CAB0F90B7F6C09BB7235EAA66ABAD39BD7B4EA01DAA76AC992E1C929CFA374E44B50767C0EC19BF6E3EAD214F26B9EC2AB476E9145178F3E86DC6BCFB8A5A155A155D27EFB6705BA666955D423776BF5065CDBAF1291CD5429ED85127F1396A7FF80CCF35F47F8F4610CEE7E00E1F17750E53848831132BC287C13C81C150A3D99E9C3284C1FA230BE88D8CDF72130B81DE5A5FFA2B4F80AACD21231075A823374A5EA6039EB862CEEBB1D74F7C05412CA5D8B9B3F056DE2FDD45344FE7864F243089CFB1D32A77E89D9E7BF0C359490206A789C8E8991A351699D5E41297D0E95C202C2C9DD187DE7C3B268A8CB2CCE217BE6D7C89CFE156CB3E45ABC0574B96ABB968EB587EE0E986095F82E2AF8BF0611DDD63C0055C7E0CE8F21BEFD5E94168EA178E5C59AD55E8512204B87E2043F8AE15DF793F56F831ADB2ED774D3A022E348DCF2194426EEC0DC8BDF9037A02D74C59619D930418B161B26DE816D5342F2564EAB92909D4F78D33ED95866C5A0352CE4C01B076F59CD0EAD51A1B1BD9878EFA3B8FC97CFD23DCEB69DDE2582E6DC9BA1D7934760D7AB06761FF054FF3290A28768BAB98EC534AD152BAC4E3C56AB347F84EE718132D8325D3AD0722784BB31CA1CFED687F6066C57E5F66B577933C70D4E4365A2A1AC09E9F05E363941BB924739751269F2038599BF21BAF94E44B7DE8DF48927659E2DB3B0756E521D5A08134303AFC7F3064C7757DD72AFA753EA59157F7256C5A1A99E56BA7D2AB0CB59A45E7E8CD6FC5182B6646A699533D068AD8FDC7600437B3E41970E2032F96E2CBFF263142E3D276F8A5B5009D98768C8CE18BA587267D160B419B17360DEBB22AFCCA1C78B18B231CBE2EF1BC5204BC77F861C85A8E8267262C121B9EB114CBE15E131FA3E34BC72AC3EB01963EFFA261517E761CCBE84CAF219594A9A14C2CA4BC7D1B8BDCBB005C3923F61E8BAE7F660611AB41EA701463C016B5AFB4BD8D953888EDF8AC97B7ED8519F81F8946C7599C602669EF938C1E79B1C1B43E70DD7D2F1880BEDDD69F541ECB52D59F4772787B233C7B1D6FC1DC3E68A96F419B188DA4D1CF61F9AE3AE3B3D8BE4CDBDCD2096652C51666BB47464B9A2494BCBF10A2CFAF2883330B413D9E9DFD3D49C47409FF27C3EAF69F6F0ED0A8F0239320FC05C8117E15002208209F8A960628F2C3ECAA9134D6BB35395165EEEE838B67FE7C014929CD23C8C7F1E803DB91FC1F13BA09337F543C1E11DE4992765CC8D4D7DD0D3B99C811973FFAE3D846B2FCF715818E7619DF82EAE1CA3BA77E0268412BBE494D463DB682D8E420DC4DD6D564547C7A26343C9B7A070F905547317A9AFAD1D9F9ABFF82C1523973BAAA359DE9D96D0100C6918562B482DBF8AF4FC51E914189041157D808A8498FB490E48A1AC48ACA4843CA92809A17C5C7A56AA843894586425ABB40CA76AC86C6A74DF573B1A0AC7E0CCC95F787AF0D655B5C44992AEAB480E456485C235A9BB455D8645DED22CF2BB9C4EEDBD8E565E5DD43C2B1715347BB42072E70ECB8C2ABAE53D6DC7913AF6035432D3AFDBF06BA5AEEB6166E1FD242EBC79CF98CB33373F565B96676DFB85858597BE4519D72042A3B7AC7B5CE6D44164CE1CEA782AD7D5D38E0743F3D60A17DECB54969564D28E9EC4FBD156398DD9E71E42F2ED5F2427F681E66BD232481D7F1CE9934FD1B11C22BD5DB0E73D2D07EEAC64E8B4A852D2EE0334795C5EDBF354FC172EFE1543BB1EA0AC761B8A97FF41A00751499FAE59D6FB857C7DA9656840976559C1B07A869611413804FC2C81FE5D3A41766CB232F2B06657CBF7D796B8325156F2D75E7B13354B528959C9B9DEBE47F5E58DB03841B303E3FCD51F09F870F7A4FA02CCCE8C2B13B674A6E017B43FEADB3B7F0C1D0DAB72E38E37D66C6723DEB36BAFBEBEE4C8D0FCBA8222DC58CD8F3CC535A6EEFB5B9D0CCD4FEEDD04A52A9FFE5D4BE80D798D95A1F9C97D72505F4945AF11B4B221C0ACB553516CA86808990D7D23BE3115CD90232B9637165A11E2470CFC156AFCE89EF743FBFEA778F55474889F0C881A747F2FC909F7225DF39162C9FCF9FF012E2B82361C62351D0000000049454E44AE426082";
            var output = ConvertByteArrayStringToByteArray(CompressedByteArrayStringToFullString(byteString));
            return output;
        }
    }

    private void SeedTestData()
    {
        DateTime now = DateTime.UtcNow;

        // Make sure the initial admin and default tenants are created.
        var adminTenant = data.Tenants.FirstOrDefault(x => x.TenantId == _guid1);
        if (adminTenant == null) {
            data.Tenants.Add(new Tenant {
                Added = now,
                AddedBy = "Seeded Test Data",
                TenantId = _guid1,
                TenantCode = "admin",
                Enabled = true,
                Name = "Admin",
                LastModified = now,
            });
            data.SaveChanges();
            SeedTestData_CreateDefaultTenantData(_guid1);
        }

        var defaultTenant = data.Tenants.FirstOrDefault(x => x.TenantId == _guid2);
        if (defaultTenant == null) {
            data.Tenants.Add(new Tenant {
                Added = now,
                AddedBy = "Seeded Test Data",
                TenantId = _guid2,
                TenantCode = "tenant1",
                Enabled = true,
                Name = "Tenant 1",
                LastModified = now,
            });
            data.SaveChanges();
            SeedTestData_CreateDefaultTenantData(_guid2);
        }

        bool newRecord = false;
        string adminPassword = String.Empty;

        // Make sure there is an Admin record in the admin tenant
        var adminUser = data.Users.FirstOrDefault(x => x.TenantId == _guid1 && x.UserId == _guid1);
        if (adminUser == null) {
            adminUser = new User {
                UserId = _guid1,
                TenantId = _guid1,
                Username = "admin",
            };
            newRecord = true;
        }
        adminUser.Added = now;
        adminUser.AddedBy = "Seeded Test Data";

        if (String.IsNullOrWhiteSpace(adminUser.Email)) {
            adminUser.Email = "admin@local";
        }

        if (String.IsNullOrWhiteSpace(adminUser.FirstName)) {
            adminUser.FirstName = "Admin";
        }

        if (String.IsNullOrWhiteSpace(adminUser.LastName)) {
            adminUser.LastName = "User";
        }

        if (String.IsNullOrWhiteSpace(adminUser.EmployeeId)) {
            adminUser.EmployeeId = "app.admin";
        }

        adminUser.Enabled = true;
        adminUser.LastModified = now;
        adminUser.PreventPasswordChange = false;
        adminUser.Admin = true;
        adminUser.CanBeScheduled = false;
        adminUser.ManageFiles = true;
        adminUser.ManageAppointments = true;
        adminUser.Deleted = false;
        adminUser.DeletedAt = null;
        if (String.IsNullOrEmpty(adminUser.Password)) {
            adminPassword = "admin";
            adminUser.Password = HashPassword(adminPassword);
        }
        if (newRecord) {
            data.Users.Add(adminUser);
        }
        data.SaveChanges();

        // If there is no user icon for the Admin user add it.
        var icon = DefaultAdminIcon;

        var adminIcon = data.FileStorages.FirstOrDefault(x => x.ItemId == null && x.UserId == _guid1);
        if (adminIcon == null) {
            data.FileStorages.Add(new FileStorage {
                Bytes = icon.Length,
                Extension = ".png",
                FileId = Guid.NewGuid(),
                FileName = "Admin.png",
                ItemId = (Guid?)null,
                LastModified = now,
                TenantId = Guid.Empty,
                UploadDate = now,
                UploadedBy = "Seeded Test Data",
                UserId = _guid1,
                Value = icon
            });
            data.SaveChanges();
        }

        if (String.IsNullOrEmpty(adminPassword)) {
            adminPassword = "admin";
        }

        // Next, make sure that an admin user exists in each tenant account using the same password that is set on the admin tenant account.
        List<Guid> tenantIds = new List<Guid>();
        var tenants = data.Tenants.Where(x => x.TenantId != _guid1).ToList();
        if (tenants != null && tenants.Any()) {
            foreach (var tenant in tenants) {
                tenantIds.Add(tenant.TenantId);
            }
        }

        if (tenantIds.Any()) {
            foreach (var tenantId in tenantIds) {
                newRecord = false;
                var tenantAdmin = data.Users.FirstOrDefault(x => x.TenantId == tenantId && x.UserId == tenantId);
                if (tenantAdmin == null) {
                    tenantAdmin = new EFModels.EFModels.User {
                        TenantId = tenantId,
                        LastModified = now,
                        UserId = tenantId, // Set the admin user id in each tenant to the tenant id
                        Username = "admin",
                    };
                    newRecord = true;
                }
                tenantAdmin.Added = now;
                tenantAdmin.AddedBy = "Seeded Test Data";
                tenantAdmin.LastModified = now;
                tenantAdmin.LastModifiedBy = "Seeded Test Data";

                if (String.IsNullOrWhiteSpace(tenantAdmin.FirstName)) {
                    tenantAdmin.FirstName = "Admin";
                }

                if (String.IsNullOrWhiteSpace(tenantAdmin.LastName)) {
                    tenantAdmin.LastName = "User";
                }

                if (String.IsNullOrWhiteSpace(tenantAdmin.Email)) {
                    tenantAdmin.Email = "admin@local";
                }

                if (String.IsNullOrWhiteSpace(tenantAdmin.EmployeeId)) {
                    tenantAdmin.EmployeeId = "app.admin";
                }

                tenantAdmin.Enabled = true;
                tenantAdmin.Admin = true;
                tenantAdmin.Deleted = false;
                tenantAdmin.DeletedAt = null;

                if (String.IsNullOrWhiteSpace(tenantAdmin.Password)) {
                    tenantAdmin.Password = HashPassword(adminPassword);
                }

                tenantAdmin.PreventPasswordChange = false;
                tenantAdmin.CanBeScheduled = false;
                tenantAdmin.ManageFiles = true;
                tenantAdmin.ManageAppointments = true;

                if (newRecord) {
                    data.Users.Add(tenantAdmin);
                }
                data.SaveChanges();

                // Make sure there is an icon for this user.
                var adminUserIcon = data.FileStorages.FirstOrDefault(x => x.ItemId == null && x.UserId == tenantId);
                if (adminUserIcon == null) {
                    data.FileStorages.Add(new FileStorage {
                        Bytes = icon.Length,
                        Deleted = false,
                        Extension = ".png",
                        FileId = Guid.NewGuid(),
                        FileName = "Admin.png",
                        ItemId = (Guid?)null,
                        LastModified = now,
                        LastModifiedBy = "Seeded Test Data",
                        TenantId = Guid.Empty,
                        UploadDate = now,
                        UploadedBy = "Seeded Test Data",
                        UserId = tenantId,
                        Value = icon,
                    });
                    data.SaveChanges();
                }
            }
        }

        // If any passwords are in the old encrypted format, then decrypt them and use the new hash format.
        var oldPasswords = data.Users.Where(x => x.Password != null && x.Password.Contains(",0x"));
        if (oldPasswords != null && oldPasswords.Any()) {
            foreach(var oldPassword in oldPasswords) {
                var password = Decrypt(oldPassword.Password);
                if (!String.IsNullOrWhiteSpace(password)) {
                    oldPassword.Password = HashPassword(password);
                }
            }
        }

// {{ModuleItemStart:Appointments}}
        // If this is the local database and we are in debug mode, then make sure
        // that the test events are in the current month.
#if DEBUG
        if (_connectionString == "Data Source=localhost;Initial Catalog=CRM;TrustServerCertificate=True;User ID=sa;Password=saPassword;MultipleActiveResultSets=True;") {
            var events = data.Appointments.Where(x => x.TenantId == _guid2).ToList();
            if(events != null && events.Any()) {
                foreach(var item in events) {
                    if (item.Start.Month != now.Month || item.Start.Year != now.Year) {
                        item.Start = SeedTestData_AdjustDate(item.Start);
                        item.End = SeedTestData_AdjustDate(item.End);

                        if (item.End < item.Start) {
                            item.End = item.Start;
                        }
                    }
                }
            }
        }
#endif
// {{ModuleItemEnd:Appointments}}
        data.SaveChanges();
    }

    private DateTime SeedTestData_AdjustDate(DateTime date)
    {
        var output = date;

        var now = DateTime.Now;

        if (date.Year != now.Year || date.Month != now.Month) {
            var lastDayOfMonth = DateTime.DaysInMonth(now.Year, now.Month);

            var day = date.Day;

            if (day > lastDayOfMonth) {
                day = lastDayOfMonth;
            }

            output = new DateTime(now.Year, now.Month, day, date.Hour, date.Minute, date.Second);
        }

        return output;
    }

    // Called when a new tenant record is created to add defaults
    private void SeedTestData_CreateDefaultTenantData(Guid TenantId)
    {
        DateTime now = DateTime.UtcNow;

        var tenantSettings = GetTenantSettings(TenantId);
        tenantSettings.LoginOptions = new List<string> { "local" };
        //tenantSettings.JasonWebTokenKey = TenantId.ToString().Replace("-", "");
        tenantSettings.AllowUsersToManageAvatars = true;
        tenantSettings.AllowUsersToManageBasicProfileInfo = true;
        tenantSettings.AllowUsersToManageBasicProfileInfoElements = new List<string> { "name", "email", "phone", "employeeid", "title", "department", "location" };
        tenantSettings.RequirePreExistingAccountToLogIn = TenantId == _guid1 ? true : false;

        SaveTenantSettings(TenantId, tenantSettings);

        // For the main test account add some default data
        if (TenantId == _guid2) {
            // Make sure there is at least one department group
            Guid departmentGroupId = Guid.Empty;
            var deptGroups = data.DepartmentGroups.Where(x => x.TenantId == TenantId);
            if (deptGroups != null && deptGroups.Any()) {
                departmentGroupId = deptGroups.First().DepartmentGroupId;
            } else {
                departmentGroupId = Guid.NewGuid();
                data.DepartmentGroups.Add(new DepartmentGroup {
                    Added = now,
                    AddedBy = "Seeded Test Data",
                    LastModified = now,
                    LastModifiedBy = "Seeded Test Data",
                    DepartmentGroupId = departmentGroupId,
                    DepartmentGroupName = "Main Departments",
                    TenantId = TenantId
                });
                data.SaveChanges();
            }

            // Make sure there is at least one department
            Guid departmentId = Guid.Empty;
            var depts = data.Departments.Where(x => x.TenantId == TenantId);
            if (depts != null && depts.Any()) {
                departmentId = depts.First().DepartmentId;
            } else {
                departmentId = Guid.NewGuid();
                data.Departments.Add(new Department {
                    Added = now,
                    AddedBy = "Seeded Test Data",
                    LastModified = now,
                    LastModifiedBy = "Seeded Test Data",
                    DepartmentId = departmentId,
                    TenantId = TenantId,
                    DepartmentName = "IT",
                    DepartmentGroupId = departmentGroupId,
                    ActiveDirectoryNames = "{IT Active Directory Name}",
                    Enabled = true
                });
                data.SaveChanges();
            }

            var testUser = data.Users.FirstOrDefault(x => x.TenantId == TenantId && x.Username != null && x.Username.ToLower() == "test");
            if (testUser == null) {
                // Add a couple test users
                data.Users.Add(new User {
                    UserId = Guid.NewGuid(),
                    Added = now,
                    AddedBy = "Seeded Test Data",
                    LastModified = now,
                    LastModifiedBy = "Seeded Test Data",
                    TenantId = TenantId,
                    FirstName = "Test",
                    LastName = "User",
                    Email = "testuser@local",
                    Username = "test",
                    Password = HashPassword("test"),
                    Enabled = true,
                    EmployeeId = "000000001",
                    Phone = "509-555-1212",
                    Location = "Works from Home",
                    Title = "A Test Admin User",
                    DepartmentId = departmentId != Guid.Empty ? departmentId : null,
                    Admin = true,
                    LastLogin = DateTime.UtcNow.AddDays(-1)
                });

                data.Users.Add(new User {
                    UserId = Guid.NewGuid(),
                    Added = now,
                    AddedBy = "Seeded Test Data",
                    LastModified = now,
                    LastModifiedBy = "Seeded Test Data",
                    TenantId = TenantId,
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@local",
                    Username = "john.doe",
                    Password = HashPassword("test"),
                    Enabled = true,
                    EmployeeId = "000000002",
                    Phone = "208-555-1212",
                    Location = "Works from Home",
                    Title = "A Regular User That's Enabled",
                    DepartmentId = departmentId != Guid.Empty ? departmentId : null,
                    LastLogin = DateTime.UtcNow.AddDays(-2)
                });

                data.Users.Add(new User {
                    UserId = Guid.NewGuid(),
                    Added = now,
                    AddedBy = "Seeded Test Data",
                    LastModified = now,
                    LastModifiedBy = "Seeded Test Data",
                    TenantId = TenantId,
                    FirstName = "Jane",
                    LastName = "Doe",
                    Email = "jane.doe@local",
                    Username = "jane.doe",
                    Password = HashPassword("test"),
                    Enabled = false,
                    EmployeeId = "000000003",
                    Phone = "916-555-1212",
                    Location = "Works from Home",
                    Title = "A Regular User That's Disabled",
                    DepartmentId = departmentId != Guid.Empty ? departmentId : null,
                    LastLogin = DateTime.UtcNow.AddDays(-3)
                });

                data.SaveChanges();
            }
        }
    }
}