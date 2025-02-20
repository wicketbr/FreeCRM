namespace CRM;
public partial class DataMigrations
{
    public List<DataObjects.DataMigration> GetMigrationsSQLite()
    {
        List<DataObjects.DataMigration> output = new List<DataObjects.DataMigration>();

        List<string> m1 = new List<string>();
        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
                "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
                "ProductVersion" TEXT NOT NULL
            )
            """);

        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS "DepartmentGroups" (
                "DepartmentGroupId" TEXT NOT NULL CONSTRAINT "PK_DepartmentGroups" PRIMARY KEY,
                "DepartmentGroupName" TEXT NULL,
                "TenantId" TEXT NULL,
                "LastModified" TEXT NULL
            )
            """);

        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS "Departments" (
                "DepartmentId" TEXT NOT NULL CONSTRAINT "PK_Departments" PRIMARY KEY,
                "DepartmentName" TEXT NULL,
                "ActiveDirectoryNames" TEXT NULL,
                "Enabled" INTEGER NULL,
                "DepartmentGroupId" TEXT NULL,
                "TenantId" TEXT NULL,
            "LastModified" TEXT NULL
            )
            """);

        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS "Settings" (
                "SettingId" INTEGER NOT NULL CONSTRAINT "PK_Settings_1" PRIMARY KEY AUTOINCREMENT,
                "SettingName" TEXT NOT NULL,
                "SettingType" TEXT NULL,
                "SettingNotes" TEXT NULL,
                "SettingText" TEXT NULL,
                "TenantId" TEXT NULL,
                "UserId" TEXT NULL,
                "LastModified" TEXT NULL
            )
            """);

        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS "Tenants" (
                "TenantId" TEXT NOT NULL CONSTRAINT "PK_Tenants" PRIMARY KEY,
                "Name" TEXT NOT NULL,
                "TenantCode" TEXT NOT NULL,
                "Enabled" INTEGER NOT NULL,
                "LastModified" TEXT NULL
            )
            """);

        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS "UDFLabels" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_UDFLabels" PRIMARY KEY,
                "Module" TEXT NULL,
                "UDF" TEXT NULL,
                "Label" TEXT NULL,
                "ShowColumn" INTEGER NULL,
                "ShowInFilter" INTEGER NULL,
                "IncludeInSearch" INTEGER NULL,
                "TenantId" TEXT NULL,
                "LastModified" TEXT NULL
            )
            """);

        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS "Users" (
                "UserId" TEXT NOT NULL CONSTRAINT "PK_Users" PRIMARY KEY,
                "TenantId" TEXT NOT NULL,
                "FirstName" TEXT NULL,
                "LastName" TEXT NULL,
                "Email" TEXT NULL,
                "Phone" TEXT NULL,
                "Username" TEXT NOT NULL,
                "EmployeeId" TEXT NULL,
                "DepartmentId" TEXT NULL,
                "Title" TEXT NULL,
                "Location" TEXT NULL,
                "Enabled" INTEGER NULL,
                "LastLogin" datetime NULL,
                "Admin" INTEGER NULL,
                "Password" TEXT NULL,
                "PreventPasswordChange" INTEGER NULL,
                "FailedLoginAttempts" INTEGER NULL,
                "LastLockoutDate" datetime NULL,
                "Source" TEXT NULL,
                "LastModified" TEXT NULL,
                "UDF01" TEXT NULL,
                "UDF02" TEXT NULL,
                "UDF03" TEXT NULL,
                "UDF04" TEXT NULL,
                "UDF05" TEXT NULL,
                "UDF06" TEXT NULL,
                "UDF07" TEXT NULL,
                "UDF08" TEXT NULL,
                "UDF09" TEXT NULL,
                "UDF10" TEXT NULL,
                CONSTRAINT "FK_Users_Departments" FOREIGN KEY ("DepartmentId") REFERENCES "Departments" ("DepartmentId"),
                CONSTRAINT "FK_Users_Tenants" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("TenantId")
            )
            """);

        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS "FileStorage" (
                "FileId" TEXT NOT NULL CONSTRAINT "PK_FileStorage" PRIMARY KEY,
                "ItemId" TEXT NULL,
                "FileName" TEXT NULL,
                "Extension" TEXT NULL,
                "Bytes" INTEGER NULL,
                "Value" BLOB NULL,
                "UploadDate" datetime NULL,
                "UserId" TEXT NULL,
                "SourceFileId" TEXT NULL,
                "TenantId" TEXT NULL,
                "LastModified" TEXT NULL,
                CONSTRAINT "FK_FileStorage_Users" FOREIGN KEY ("UserId") REFERENCES "Users" ("UserId")
            )
            """);

        m1.Add(
            """
            CREATE INDEX "IX_FileStorage_UserId" ON "FileStorage" ("UserId")
            """);

        m1.Add(
            """
            CREATE INDEX "IX_Users_DepartmentId" ON "Users" ("DepartmentId")
            """);

        m1.Add(
            """
            CREATE INDEX "IX_Users_TenantId" ON "Users" ("TenantId")
            """);

        m1.Add(
            """
            INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
            VALUES ('20230204160527_001', '7.0.2')
            EXCEPT
            SELECT * FROM __EFMigrationsHistory WHERE MigrationId='20230204160527_001'
            """);

        output.Add(new DataObjects.DataMigration {
            MigrationId = "20230204160527_001",
            Migration = m1
        });










        List<string> m2 = new List<string>();

        // Make sure the Source field exists in the Users table.
        m2.Add(
            """
            ALTER TABLE Users ADD Source TEXT NULL 
            """);

        m2.Add(
            """
            CREATE TABLE IF NOT EXISTS "UserGroups" (
                "GroupId" TEXT NOT NULL CONSTRAINT "PK_UserGroups" PRIMARY KEY,
                "TenantId" TEXT NOT NULL,
                "Name" TEXT NULL,
                "Enabled" INTEGER NULL,
                "Settings" TEXT NULL
            )
            """);

        m2.Add(
            """
            CREATE TABLE IF NOT EXISTS "UserInGroups" (
                "UserInGroupId" TEXT NOT NULL CONSTRAINT "PK_UserInGroups" PRIMARY KEY,
                "UserId" TEXT NOT NULL,
                "TenantId" TEXT NOT NULL,
                "GroupId" TEXT NOT NULL,
                CONSTRAINT "FK_UserInGroups_UserGroups" FOREIGN KEY ("GroupId") REFERENCES "UserGroups" ("GroupId"),
                CONSTRAINT "FK_UserInGroups_Users" FOREIGN KEY ("UserId") REFERENCES "Users" ("UserId")
            )
            """);

        m2.Add(
            """
            INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
            VALUES ('20230227163529_002', '7.0.2')
            EXCEPT
            SELECT * FROM __EFMigrationsHistory WHERE MigrationId='20230227163529_002'
            """);

        output.Add(new DataObjects.DataMigration {
            MigrationId = "20230227163529_002",
            Migration = m2
        });

        return output;
    }
}