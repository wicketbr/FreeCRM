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
                "TenantId" TEXT NOT NULL,
                "Added" datetime NOT NULL,
                "AddedBy" TEXT NULL,
                "LastModified" datetime NOT NULL,
                "LastModifiedBy" TEXT NULL,
                "Deleted" INTEGER NOT NULL,
                "DeletedAt" datetime NULL
            )
            """);

        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS "Departments" (
                "DepartmentId" TEXT NOT NULL CONSTRAINT "PK_Departments" PRIMARY KEY,
                "DepartmentName" TEXT NOT NULL,
                "ActiveDirectoryNames" TEXT NULL,
                "Enabled" INTEGER NOT NULL,
                "DepartmentGroupId" TEXT NULL,
                "TenantId" TEXT NOT NULL,
                "Added" datetime NOT NULL,
                "AddedBy" TEXT NULL,
                "LastModified" datetime NOT NULL,
                "LastModifiedBy" TEXT NULL,
                "Deleted" INTEGER NOT NULL,
                "DeletedAt" datetime NULL
            )
            """);

        // {{ModuleItemStart:EmailTemplates}}
        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS "EmailTemplates" (
                "EmailTemplateId" TEXT NOT NULL CONSTRAINT "PK_EmailTemplates" PRIMARY KEY,
                "TenantId" TEXT NOT NULL,
                "Name" TEXT NOT NULL,
                "Template" TEXT NULL,
                "Enabled" INTEGER NOT NULL,
                "Added" datetime NOT NULL,
                "AddedBy" TEXT NULL,
                "LastModified" datetime NOT NULL,
                "LastModifiedBy" TEXT NULL,
                "Deleted" INTEGER NOT NULL,
                "DeletedAt" datetime NULL
            );
            """);
        // {{ModuleItemEnd:EmailTemplates}}

        // {{ModuleItemStart:Locations}}
        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS "Locations" (
                "LocationId" TEXT NOT NULL CONSTRAINT "PK_Locations" PRIMARY KEY,
                "TenantId" TEXT NOT NULL,
                "Name" TEXT NOT NULL,
                "Address" TEXT NULL,
                "City" TEXT NULL,
                "State" TEXT NULL,
                "PostalCode" TEXT NULL,
                "CalendarBackgroundColor" TEXT NULL,
                "CalendarForegroundColor" TEXT NULL,
                "Enabled" INTEGER NOT NULL,
                "DefaultLocation" INTEGER NOT NULL,
                "Added" datetime NOT NULL,
                "AddedBy" TEXT NULL,
                "LastModified" datetime NOT NULL,
                "LastModifiedBy" TEXT NULL,
                "Deleted" INTEGER NOT NULL,
                "DeletedAt" datetime NULL
            );
            """);
        // {{ModuleItemEnd:Locations}}

        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS "PluginCache" (
                "RecordId" TEXT NOT NULL CONSTRAINT "PK_PluginCache" PRIMARY KEY,
                "Id" TEXT NOT NULL,
                "Author" TEXT NULL,
                "Name" TEXT NULL,
                "Type" TEXT NULL,
                "Version" TEXT NULL,
                "Properties" TEXT NULL,
                "Namespace" TEXT NULL,
                "ClassName" TEXT NULL,
                "Code" TEXT NULL,
                "AdditionalAssemblies" TEXT NULL,
                "StillExists" INTEGER NOT NULL
            );
            """);

        // {{ModuleItemStart:Services}}
        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS "Services" (
                "ServiceId" TEXT NOT NULL CONSTRAINT "PK_Services" PRIMARY KEY,
                "TenantId" TEXT NOT NULL,
                "Code" TEXT NULL,
                "DefaultService" INTEGER NOT NULL,
                "Description" TEXT NOT NULL,
                "Rate" decimal(19, 4) NOT NULL,
                "DefaultAppointmentDuration" INTEGER NOT NULL,
                "Enabled" INTEGER NOT NULL,
                "Added" datetime NOT NULL,
                "AddedBy" TEXT NULL,
                "LastModified" datetime NOT NULL,
                "LastModifiedBy" TEXT NULL,
                "Deleted" INTEGER NOT NULL,
                "DeletedAt" datetime NULL
            );
            """);
        // {{ModuleItemEnd:Services}}

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
                "LastModified" datetime NOT NULL,
                "LastModifiedBy" TEXT NULL
            )
            """);

        // {{ModuleItemStart:Tags}}
        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS "Tags" (
                "TagId" TEXT NOT NULL CONSTRAINT "PK_Tags" PRIMARY KEY,
                "TenantId" TEXT NOT NULL,
                "Name" TEXT NOT NULL,
                "Style" TEXT NULL,
                "Enabled" INTEGER NOT NULL,
                -- {{ModuleItemStart:Appointments}}
                "UseInAppointments" INTEGER NOT NULL,
                -- {{ModuleItemEnd:Appointments}}
                -- {{ModuleItemStart:EmailTemplates}}
                "UseInEmailTemplates" INTEGER NOT NULL,
                -- {{ModuleItemEnd:EmailTemplates}}
                -- {{ModuleItemStart:Services}}
                "UseInServices" INTEGER NOT NULL,
                -- {{ModuleItemEnd:Services}}
                "Added" datetime NOT NULL,
                "AddedBy" TEXT NULL,
                "LastModified" datetime NOT NULL,
                "LastModifiedBy" TEXT NULL,
                "Deleted" INTEGER NOT NULL,
                "DeletedAt" datetime NULL
            );
            """);
        // {{ModuleItemEnd:Tags}}

        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS "Tenants" (
                "TenantId" TEXT NOT NULL CONSTRAINT "PK_Tenants" PRIMARY KEY,
                "Name" TEXT NOT NULL,
                "TenantCode" TEXT NOT NULL,
                "Enabled" INTEGER NOT NULL,
                "Added" datetime NOT NULL,
                "AddedBy" TEXT NULL,
                "LastModified" datetime NOT NULL,
                "LastModifiedBy" TEXT NULL
            )
            """);

        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS "UDFLabels" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_UDFLabels" PRIMARY KEY,
                "Module" TEXT NOT NULL,
                "UDF" TEXT NOT NULL,
                "Label" TEXT NULL,
                "ShowColumn" INTEGER NULL,
                "ShowInFilter" INTEGER NULL,
                "IncludeInSearch" INTEGER NULL,
                "TenantId" TEXT NOT NULL,
                "LastModified" datetime NOT NULL,
                "LastModifiedBy" TEXT NULL
            )
            """);

        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS "UserGroups" (
                "GroupId" TEXT NOT NULL CONSTRAINT "PK_UserGroups" PRIMARY KEY,
                "TenantId" TEXT NOT NULL,
                "Name" TEXT NOT NULL,
                "Enabled" INTEGER NOT NULL,
                "Settings" TEXT NULL,
                "Added" datetime NOT NULL,
                "AddedBy" TEXT NULL,
                "LastModified" datetime NOT NULL,
                "LastModifiedBy" TEXT NULL,
                "Deleted" INTEGER NOT NULL,
                "DeletedAt" datetime NULL
            );
            """);

        // {{ModuleItemStart:Appointments}}
        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS "Appointments" (
                "AppointmentId" TEXT NOT NULL CONSTRAINT "PK_Appointments" PRIMARY KEY,
                "TenantId" TEXT NOT NULL,
                "Title" TEXT NOT NULL,
                "Start" datetime NOT NULL,
                "End" datetime NOT NULL,
                "AllDay" INTEGER NOT NULL,
                "Meeting" INTEGER NOT NULL,
                -- {{ModuleItemStart:Locations}}
                "LocationId" TEXT NULL,
                -- {{ModuleItemEnd:Locations}}
                "Added" datetime NOT NULL,
                "AddedBy" TEXT NULL,
                "LastModified" datetime NOT NULL,
                "LastModifiedBy" TEXT NULL,
                "Deleted" INTEGER NOT NULL,
                "DeletedAt" datetime NULL,
                "Note" TEXT NULL,
                "ForegroundColor" TEXT NULL,
                "BackgroundColor" TEXT NULL
                -- {{ModuleItemStart:Locations}}
                , CONSTRAINT "FK_Appointments_Locations" FOREIGN KEY ("LocationId") REFERENCES "Locations" ("LocationId")
                -- {{ModuleItemEnd:Locations}}
            );
            """);
        // {{ModuleItemEnd:Appointments}}

        // {{ModuleItemStart:Tags}}
        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS "TagItems" (
                "TagItemId" TEXT NOT NULL CONSTRAINT "PK_TagItems" PRIMARY KEY,
                "TagId" TEXT NOT NULL,
                "TenantId" TEXT NOT NULL,
                "ItemId" TEXT NOT NULL,
                CONSTRAINT "FK_TagItems_Tags" FOREIGN KEY ("TagId") REFERENCES "Tags" ("TagId")
            );
            """);
        // {{ModuleItemEnd:Tags}}

        var usersTable =
            """
            CREATE TABLE IF NOT EXISTS "Users" (
                "UserId" TEXT NOT NULL CONSTRAINT "PK_Users" PRIMARY KEY,
                "TenantId" TEXT NOT NULL,
                "FirstName" TEXT NULL,
                "LastName" TEXT NULL,
                "Email" TEXT NOT NULL,
                "Phone" TEXT NULL,
                "Username" TEXT NOT NULL,
                "EmployeeId" TEXT NULL,
                "DepartmentId" TEXT NULL,
                "Title" TEXT NULL,
                "Location" TEXT NULL,
                "Enabled" INTEGER NOT NULL,
                "LastLogin" datetime NULL,
                "LastLoginSource" TEXT NULL,
                "Admin" INTEGER NOT NULL,
            
            """;

        // {{ModuleItemStart:Appointments}}
        usersTable +=
            """
                "CanBeScheduled" INTEGER NOT NULL,
                "ManageAppointments" INTEGER NOT NULL,
            
            """;
        // {{ModuleItemEnd:Appointments}}

        usersTable +=
            """
                "ManageFiles" INTEGER NOT NULL,
                "Password" TEXT NULL,
                "PreventPasswordChange" INTEGER NOT NULL,
                "FailedLoginAttempts" INTEGER NULL,
                "LastLockoutDate" datetime NULL,
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
                "Added" datetime NOT NULL,
                "AddedBy" TEXT NULL,
                "LastModified" datetime NOT NULL,
                "LastModifiedBy" TEXT NULL,
                "Deleted" INTEGER NOT NULL,
                "DeletedAt" datetime NULL,
                "Preferences" TEXT NULL,
                CONSTRAINT "IX_Users_DepartmentId" FOREIGN KEY ("DepartmentId") REFERENCES "Departments" ("DepartmentId"),
                CONSTRAINT "IX_Users_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("TenantId")
            )
            """;

        m1.Add(usersTable);

        // {{ModuleItemStart:Appointments}}
        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS "AppointmentNotes" (
                "AppointmentNoteId" TEXT NOT NULL CONSTRAINT "PK_AppointmentNotes" PRIMARY KEY,
                "AppointmentId" TEXT NOT NULL,
                "TenantId" TEXT NOT NULL,
                "Added" datetime NOT NULL,
                "AddedBy" TEXT NULL,
                "LastModified" datetime NOT NULL,
                "LastModifiedBy" TEXT NULL,
                "Note" TEXT NULL,
                "Deleted" INTEGER NOT NULL,
                "DeletedAt" datetime NULL,
                CONSTRAINT "FK_AppointmentNotes_Appointments" FOREIGN KEY ("AppointmentId") REFERENCES "Appointments" ("AppointmentId")
            );
            """);

        // {{ModuleItemStart:Services}}
        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS "AppointmentServices" (
                "AppointmentServiceId" TEXT NOT NULL CONSTRAINT "PK_AppointmentServices" PRIMARY KEY,
                "AppointmentId" TEXT NOT NULL,
                "TenantId" TEXT NOT NULL,
                "ServiceId" TEXT NOT NULL,
                "Fee" decimal(19, 4) NULL,
                "Deleted" INTEGER NOT NULL,
                "DeletedAt" datetime NULL,
                "LastModified" datetime NOT NULL,
                "LastModifiedBy" TEXT NULL,
                CONSTRAINT "FK_AppointmentServices_Appointments" FOREIGN KEY ("AppointmentId") REFERENCES "Appointments" ("AppointmentId"),
                CONSTRAINT "FK_AppointmentServices_Services" FOREIGN KEY ("ServiceId") REFERENCES "Services" ("ServiceId")
            );
            """);
        // {{ModuleItemEnd:Services}}

        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS "AppointmentUsers" (
                "AppointmentUserId" TEXT NOT NULL CONSTRAINT "PK_AppointmentUsers" PRIMARY KEY,
                "AppointmentId" TEXT NOT NULL,
                "TenantId" TEXT NOT NULL,
                "UserId" TEXT NOT NULL,
                "AttendanceCode" TEXT NULL,
                "Fees" decimal(19, 4) NULL,
                CONSTRAINT "FK_AppointmentUsers_Appointments" FOREIGN KEY ("AppointmentId") REFERENCES "Appointments" ("AppointmentId"),
                CONSTRAINT "FK_AppointmentUsers_Users" FOREIGN KEY ("UserId") REFERENCES "Users" ("UserId")
            );
            """);
        // {{ModuleItemEnd:Appointments}}

        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS "FileStorage" (
                "FileId" TEXT NOT NULL CONSTRAINT "PK_FileStorage" PRIMARY KEY,
                "ItemId" TEXT NULL,
                "FileName" TEXT NOT NULL,
                "Extension" TEXT NOT NULL,
                "Bytes" INTEGER NULL,
                "Value" BLOB NULL,
                "UploadDate" datetime NOT NULL,
                "UploadedBy" TEXT NULL,
                "UserId" TEXT NULL,
                "SourceFileId" TEXT NULL,
                "TenantId" TEXT NOT NULL,
                "LastModified" datetime NOT NULL,
                "LastModifiedBy" TEXT NULL,
                "Deleted" INTEGER NOT NULL,
                "DeletedAt" datetime NULL,
                CONSTRAINT "IX_FileStorage_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("UserId")
            )
            """);

        // {{ModuleItemStart:Invoices}}
        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS "Invoices" (
                "InvoiceId" TEXT NOT NULL CONSTRAINT "PK_Invoices" PRIMARY KEY,
                "TenantId" TEXT NOT NULL,
                "InvoiceNumber" TEXT NULL,
                "PONumber" TEXT NULL,
                -- {{ModuleItemStart:Appointments}}
                "AppointmentId" TEXT NULL,
                -- {{ModuleItemEnd:Appointments}}
                "UserId" TEXT NULL,
                "Title" TEXT NOT NULL,
                "Items" TEXT NOT NULL,
                "Notes" TEXT NULL,
                "InvoiceCreated" datetime NULL,
                "InvoiceDueDate" datetime NULL,
                "InvoiceSendDate" datetime NULL,
                "InvoiceSent" datetime NULL,
                "InvoiceClosed" datetime NULL,
                "Total" decimal(19, 4) NOT NULL,
                "Added" datetime NOT NULL,
                "AddedBy" TEXT NULL,
                "LastModified" datetime NOT NULL,
                "LastModifiedBy" TEXT NULL,
                "Deleted" INTEGER NOT NULL,
                "DeletedAt" datetime NULL,
                -- {{ModuleItemStart:Appointments}}
                CONSTRAINT "FK_Invoices_Appointments" FOREIGN KEY ("AppointmentId") REFERENCES "Appointments" ("AppointmentId"),
                -- {{ModuleItemEnd:Appointments}}
                CONSTRAINT "FK_Invoices_Tenants" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("TenantId"),
                CONSTRAINT "FK_Invoices_Users" FOREIGN KEY ("UserId") REFERENCES "Users" ("UserId")
            );
            """);
        // {{ModuleItemEnd:Invoices}}

        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS "UserInGroups" (
                "UserInGroupId" TEXT NOT NULL CONSTRAINT "PK_UserInGroups" PRIMARY KEY,
                "UserId" TEXT NOT NULL,
                "TenantId" TEXT NOT NULL,
                "GroupId" TEXT NOT NULL,
                CONSTRAINT "FK_UserInGroups_UserGroups" FOREIGN KEY ("GroupId") REFERENCES "UserGroups" ("GroupId"),
                CONSTRAINT "FK_UserInGroups_Users" FOREIGN KEY ("UserId") REFERENCES "Users" ("UserId")
            );
            """);

        // {{ModuleItemStart:Payments}}
        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS "Payments" (
                "PaymentId" TEXT NOT NULL CONSTRAINT "PK_Payments" PRIMARY KEY,
                "TenantId" TEXT NOT NULL,
                "InvoiceId" TEXT NOT NULL,
                "UserId" TEXT NULL,
                "Notes" TEXT NULL,
                "PaymentDate" datetime NOT NULL,
                "Amount" decimal(19, 4) NOT NULL,
                "Refunded" decimal(19, 4) NULL,
                "RefundedBy" TEXT NULL,
                "RefundDate" datetime NULL,
                "Added" datetime NOT NULL,
                "AddedBy" TEXT NULL,
                "LastModified" datetime NOT NULL,
                "LastModifiedBy" TEXT NULL,
                "Deleted" INTEGER NOT NULL,
                "DeletedAt" datetime NULL,
                CONSTRAINT "FK_Payments_Invoices" FOREIGN KEY ("InvoiceId") REFERENCES "Invoices" ("InvoiceId"),
                CONSTRAINT "FK_Payments_Tenants" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("TenantId"),
                CONSTRAINT "FK_Payments_Users" FOREIGN KEY ("UserId") REFERENCES "Users" ("UserId")
            );
            """);
        // {{ModuleItemEnd:Payments}}

        // {{ModuleItemStart:Appointments}}
        m1.Add(
            """
            CREATE INDEX "IX_AppointmentNotes_AppointmentId" ON "AppointmentNotes" ("AppointmentId");
            """);

        // {{ModuleItemStart:Locations}}
        m1.Add(
            """
            CREATE INDEX "IX_Appointments_LocationId" ON "Appointments" ("LocationId");
            """);
        // {{ModuleItemEnd:Locations}}
        // {{ModuleItemEnd:Appointments}}

        // {{ModuleItemStart:Appointments}}
        // {{ModuleItemStart:Services}}
        m1.Add(
            """
            CREATE INDEX "IX_AppointmentServices_AppointmentId" ON "AppointmentServices" ("AppointmentId");
            """);

        m1.Add(
            """
            CREATE INDEX "IX_AppointmentServices_ServiceId" ON "AppointmentServices" ("ServiceId");
            """);
        // {{ModuleItemEnd:Services}}

        m1.Add(
            """
            CREATE INDEX "IX_AppointmentUsers_AppointmentId" ON "AppointmentUsers" ("AppointmentId");
            """);

        m1.Add(
            """
            CREATE INDEX "IX_AppointmentUsers_UserId" ON "AppointmentUsers" ("UserId");
            """);

        m1.Add(
            """
            CREATE INDEX "IX_Invoices_AppointmentId" ON "Invoices" ("AppointmentId");
            """);
        // {{ModuleItemEnd:Appointments}}

        m1.Add(
            """
            CREATE INDEX "IX_FileStorage_UserId" ON "FileStorage" ("UserId");
            """);

        // {{ModuleItemStart:Invoices}}
        m1.Add(
            """
            CREATE INDEX "IX_Invoices_TenantId" ON "Invoices" ("TenantId");
            """);

        m1.Add(
            """
            CREATE INDEX "IX_Invoices_UserId" ON "Invoices" ("UserId");
            """);

        m1.Add(
            """
            CREATE INDEX "IX_Payments_InvoiceId" ON "Payments" ("InvoiceId");
            """);
        // {{ModuleItemEnd:Invoices}}

        // {{ModuleItemStart:Payments}}
        m1.Add(
            """
            CREATE INDEX "IX_Payments_TenantId" ON "Payments" ("TenantId");
            """);

        m1.Add(
            """
            CREATE INDEX "IX_Payments_UserId" ON "Payments" ("UserId");
            """);
        // {{ModuleItemEnd:Payments}}

        // {{ModuleItemStart:Tags}}
        m1.Add(
            """
            CREATE INDEX "IX_TagItems_TagId" ON "TagItems" ("TagId");
            """);
        // {{ModuleItemEnd:Tags}}

        m1.Add(
            """
            CREATE INDEX "IX_UserInGroups_GroupId" ON "UserInGroups" ("GroupId");
            """);

        m1.Add(
            """
            CREATE INDEX "IX_UserInGroups_UserId" ON "UserInGroups" ("UserId");
            """);

        m1.Add(
            """
            CREATE INDEX "IX_Users_DepartmentId" ON "Users" ("DepartmentId");
            """);

        m1.Add(
            """
            CREATE INDEX "IX_Users_TenantId" ON "Users" ("TenantId");
            """);

        m1.Add(
            """
            INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
            VALUES ('001', '1.0.0')
            EXCEPT
            SELECT * FROM __EFMigrationsHistory WHERE MigrationId='001'
            """);

        output.Add(new DataObjects.DataMigration {
            MigrationId = "001",
            Migration = m1
        });

        return output;
    }
}