namespace CRM;
public partial class DataMigrations
{
    public List<DataObjects.DataMigration> GetMigrationsPostgreSQL()
    {
        List<DataObjects.DataMigration> output = new List<DataObjects.DataMigration>();

        List<string> m1 = new List<string>();
        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
                "MigrationId" character varying(150) NOT NULL,
                "ProductVersion" character varying(32) NOT NULL,
                CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
            );
            """);

        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS "DepartmentGroups" (
                "DepartmentGroupId" TEXT NOT NULL,
                "DepartmentGroupName" TEXT NULL,
                "TenantId" TEXT NULL,
                CONSTRAINT "PK_DepartmentGroups" PRIMARY KEY ("DepartmentGroupId")
            );
            """);

        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS "Departments" (
                "DepartmentId" TEXT NOT NULL,
                "DepartmentName" TEXT NULL,
                "ActiveDirectoryNames" TEXT NULL,
                "Enabled" BOOLEAN NULL,
                "DepartmentGroupId" TEXT NULL,
                "TenantId" TEXT NULL,
                CONSTRAINT "PK_Departments" PRIMARY KEY ("DepartmentId")
            );
            """);

        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS "Settings" (
                "SettingId" INTEGER NOT NULL GENERATED ALWAYS AS IDENTITY ( INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 2147483647 CACHE 1 ),
                "SettingName" TEXT NOT NULL,
                "SettingType" TEXT NULL,
                "SettingNotes" TEXT NULL,
                "SettingText" TEXT NULL,
                "TenantId" TEXT NULL,
                "UserId" TEXT NULL,
                CONSTRAINT "PK_Settings_1" PRIMARY KEY ("SettingId")
            );
            """);

        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS "Tenants" (
                "TenantId" TEXT NOT NULL,
                "Name" TEXT NOT NULL,
                "TenantCode" TEXT NOT NULL,
                "Enabled" BOOLEAN NOT NULL,
                CONSTRAINT "PK_Tenants" PRIMARY KEY ("TenantId")
            );
            """);

        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS "UDFLabels" (
                "Id" TEXT NOT NULL,
                "Module" TEXT NULL,
                "UDF" TEXT NULL,
                "Label" TEXT NULL,
                "ShowColumn" BOOLEAN NULL,
                "ShowInFilter" BOOLEAN NULL,
                "IncludeInSearch" BOOLEAN NULL,
                "TenantId" TEXT NULL,
                CONSTRAINT "PK_UDFLabels" PRIMARY KEY ("Id")
            );
            """);

        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS "Users" (
                "UserId" TEXT NOT NULL,
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
                "Enabled" BOOLEAN NULL,
                "LastLogin" TIMESTAMP NULL,
                "Admin" BOOLEAN NULL,
                "Password" TEXT NULL,
                "PreventPasswordChange" BOOLEAN NULL,
                "FailedLoginAttempts" INTEGER NULL,
                "LastLockoutDate" TIMESTAMP NULL,
                "Source" TEXT NULL,
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
                CONSTRAINT "PK_Users" PRIMARY KEY ("UserId"),
                CONSTRAINT "FK_Users_Departments" FOREIGN KEY ("DepartmentId") REFERENCES "Departments" ("DepartmentId"),
                CONSTRAINT "FK_Users_Tenants" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("TenantId")
            );
            """);

        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS "FileStorage" (
                "FileId" TEXT NOT NULL,
                "ItemId" TEXT NULL,
                "FileName" TEXT NULL,
                "Extension" TEXT NULL,
                "Bytes" INTEGER NULL,
                "Value" BYTEA NULL,
                "UploadDate" TIMESTAMP NULL,
                "UserId" TEXT NULL,
                "SourceFileId" TEXT NULL,
                "TenantId" TEXT NULL,
                CONSTRAINT "PK_FileStorage" PRIMARY KEY ("FileId"),
                CONSTRAINT "FK_FileStorage_Users" FOREIGN KEY ("UserId") REFERENCES "Users" ("UserId")
            );
            """);

        m1.Add(
            """
            CREATE INDEX IF NOT EXISTS "IX_FileStorage_UserId" ON "FileStorage" ("UserId");
            """);

        m1.Add(
            """
            CREATE INDEX IF NOT EXISTS "IX_Users_DepartmentId" ON "Users" ("DepartmentId");
            """);

        m1.Add(
            """
            CREATE INDEX IF NOT EXISTS "IX_Users_TenantId" ON "Users" ("TenantId");
            """);

        m1.Add(
            """
            INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
            VALUES ('20230204160527_001', '7.0.2')
            EXCEPT
            SELECT * FROM "__EFMigrationsHistory" WHERE "MigrationId"='20230204160527_001';
            """);










        List<string> m2 = new List<string>();

        // Make sure the Source field exists in the Users table.
        m2.Add(
            """
            ALTER TABLE "Users" ADD COLUMN IF NOT EXISTS "Source" TEXT NULL 
            """);

        m2.Add(
            """
            CREATE TABLE IF NOT EXISTS "UserGroups" (
                "GroupId" TEXT NOT NULL,
                "TenantId" TEXT NOT NULL,
                "Name" TEXT NULL,
                "Enabled" BOOLEAN NULL,
                "Settings" TEXT NULL,
                CONSTRAINT "PK_UserGroups" PRIMARY KEY ("GroupId")
            );
            """);

        m2.Add(
            """
            CREATE TABLE IF NOT EXISTS "UserInGroups" (
                "UserInGroupId" TEXT NOT NULL,
                "UserId" TEXT NOT NULL,
                "TenantId" TEXT NOT NULL,
                "GroupId" TEXT NOT NULL,
                CONSTRAINT "PK_UserInGroups" PRIMARY KEY ("UserInGroupId"),
                CONSTRAINT "FK_UserInGroups_UserGroups" FOREIGN KEY ("GroupId") REFERENCES "UserGroups" ("GroupId"),
                CONSTRAINT "FK_UserInGroups_Users" FOREIGN KEY ("UserId") REFERENCES "Users" ("UserId")
            );
            """);

        m2.Add(
            """
            INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
            VALUES ('20230227163529_002', '7.0.2')
            EXCEPT
            SELECT * FROM "__EFMigrationsHistory" WHERE "MigrationId"='20230227163529_002';
            """);

        output.Add(new DataObjects.DataMigration {
            MigrationId = "20230227163529_002",
            Migration = m2
        });

        return output;
    }
}