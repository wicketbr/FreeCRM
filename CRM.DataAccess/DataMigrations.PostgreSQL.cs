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
                "DepartmentGroupId" uuid NOT NULL,
                "DepartmentGroupName" character varying(200),
                "TenantId" uuid NOT NULL,
                "Added" TIMESTAMP NOT NULL,
                "AddedBy" character varying(100),
                "LastModified" TIMESTAMP NOT NULL,
                "LastModifiedBy" character varying(100),
                "Deleted" boolean NOT NULL,
                "DeletedAt" TIMESTAMP,
                CONSTRAINT "PK_DepartmentGroups" PRIMARY KEY ("DepartmentGroupId")
            );
            """);

        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS "Departments" (
                "DepartmentId" uuid NOT NULL,
                "DepartmentName" character varying(100) NOT NULL,
                "ActiveDirectoryNames" character varying(100),
                "Enabled" boolean NOT NULL,
                "DepartmentGroupId" uuid,
                "TenantId" uuid NOT NULL,
                "Added" TIMESTAMP NOT NULL,
                "AddedBy" character varying(100),
                "LastModified" TIMESTAMP NOT NULL,
                "LastModifiedBy" character varying(100),
                "Deleted" boolean NOT NULL,
                "DeletedAt" TIMESTAMP,
                CONSTRAINT "PK_Departments" PRIMARY KEY ("DepartmentId")
            );
            """);

        // {{ModuleItemStart:EmailTemplates}}
        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS "EmailTemplates" (
                "EmailTemplateId" uuid NOT NULL,
                "TenantId" uuid NOT NULL,
                "Name" character varying(300) NOT NULL,
                "Template" text,
                "Enabled" boolean NOT NULL,
                "Added" TIMESTAMP NOT NULL,
                "AddedBy" character varying(100),
                "LastModified" TIMESTAMP NOT NULL,
                "LastModifiedBy" character varying(100),
                "Deleted" boolean NOT NULL,
                "DeletedAt" TIMESTAMP,
                CONSTRAINT "PK_EmailTemplates" PRIMARY KEY ("EmailTemplateId")
            );
            """);
        // {{ModuleItemEnd:EmailTemplates}}

        // {{ModuleItemStart:Locations}}
        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS "Locations" (
                "LocationId" uuid NOT NULL,
                "TenantId" uuid NOT NULL,
                "Name" character varying(200) NOT NULL,
                "Address" character varying(200),
                "City" character varying(100),
                "State" character varying(50),
                "PostalCode" character varying(50),
                "CalendarBackgroundColor" character varying(100),
                "CalendarForegroundColor" character varying(100),
                "Enabled" boolean NOT NULL,
                "DefaultLocation" boolean NOT NULL,
                "Added" TIMESTAMP NOT NULL,
                "AddedBy" character varying(100),
                "LastModified" TIMESTAMP NOT NULL,
                "LastModifiedBy" character varying(100),
                "Deleted" boolean NOT NULL,
                "DeletedAt" TIMESTAMP,
                CONSTRAINT "PK_Locations" PRIMARY KEY ("LocationId")
            );
            """);
        // {{ModuleItemEnd:Locations}}

        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS "PluginCache" (
                "RecordId" uuid NOT NULL,
                "Id" uuid NOT NULL,
                "Author" character varying(100),
                "Name" character varying(100),
                "Type" character varying(100),
                "Version" character varying(100),
                "Properties" text,
                "Namespace" character varying(100),
                "ClassName" character varying(100),
                "Code" text,
                "AdditionalAssemblies" text,
                "StillExists" boolean NOT NULL,
                CONSTRAINT "PK_PluginCache" PRIMARY KEY ("RecordId")
            );
            """);

        // {{ModuleItemStart:Services}}
        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS "Services" (
                "ServiceId" uuid NOT NULL,
                "TenantId" uuid NOT NULL,
                "Code" character varying(50),
                "DefaultService" boolean NOT NULL,
                "Description" character varying(200) NOT NULL,
                "Rate" numeric(19,4) NOT NULL,
                "DefaultAppointmentDuration" integer NOT NULL,
                "Enabled" boolean NOT NULL,
                "Added" TIMESTAMP NOT NULL,
                "AddedBy" character varying(100),
                "LastModified" TIMESTAMP NOT NULL,
                "LastModifiedBy" character varying(100),
                "Deleted" boolean NOT NULL,
                "DeletedAt" TIMESTAMP,
                CONSTRAINT "PK_Services" PRIMARY KEY ("ServiceId")
            );
            """);
        // {{ModuleItemEnd:Services}}

        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS "Settings" (
                "SettingId" integer GENERATED BY DEFAULT AS IDENTITY,
                "SettingName" character varying(100) NOT NULL,
                "SettingType" character varying(100),
                "SettingNotes" text,
                "SettingText" text,
                "TenantId" uuid,
                "UserId" uuid,
                "LastModified" TIMESTAMP NOT NULL,
                "LastModifiedBy" character varying(100),
                CONSTRAINT "PK_Settings_1" PRIMARY KEY ("SettingId")
            );
            """);

        // {{ModuleItemStart:Tags}}
        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS "Tags" (
                "TagId" uuid NOT NULL,
                "TenantId" uuid NOT NULL,
                "Name" character varying(200) NOT NULL,
                "Style" text,
                "Enabled" boolean NOT NULL,
                -- {{ModuleItemStart:Appointments}}
                "UseInAppointments" boolean NOT NULL,
                -- {{ModuleItemEnd:Appointments}}
                -- {{ModuleItemStart:EmailTemplates}}
                "UseInEmailTemplates" boolean NOT NULL,
                -- {{ModuleItemEnd:EmailTemplates}}
                -- {{ModuleItemStart:Services}}
                "UseInServices" boolean NOT NULL,
                -- {{ModuleItemEnd:Services}}
                "Added" TIMESTAMP NOT NULL,
                "AddedBy" character varying(100),
                "LastModified" TIMESTAMP NOT NULL,
                "LastModifiedBy" character varying(100),
                "Deleted" boolean NOT NULL,
                "DeletedAt" TIMESTAMP,
                CONSTRAINT "PK_Tags" PRIMARY KEY ("TagId")
            );
            """);
        // {{ModuleItemEnd:Tags}}

        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS "Tenants" (
                "TenantId" uuid NOT NULL,
                "Name" character varying(200) NOT NULL,
                "TenantCode" character varying(50) NOT NULL,
                "Enabled" boolean NOT NULL,
                "Added" TIMESTAMP NOT NULL,
                "AddedBy" character varying(100),
                "LastModified" TIMESTAMP NOT NULL,
                "LastModifiedBy" character varying(100),
                CONSTRAINT "PK_Tenants" PRIMARY KEY ("TenantId")
            );
            """);

        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS "UDFLabels" (
                "Id" uuid NOT NULL,
                "Module" character varying(20) NOT NULL,
                "UDF" character varying(10) NOT NULL,
                "Label" text,
                "ShowColumn" boolean,
                "ShowInFilter" boolean,
                "IncludeInSearch" boolean,
                "TenantId" uuid NOT NULL,
                "LastModified" TIMESTAMP NOT NULL,
                "LastModifiedBy" character varying(100),
                CONSTRAINT "PK_UDFLabels" PRIMARY KEY ("Id")
            );
            """);

        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS "UserGroups" (
                "GroupId" uuid NOT NULL,
                "TenantId" uuid NOT NULL,
                "Name" character varying(100) NOT NULL,
                "Enabled" boolean NOT NULL,
                "Settings" text,
                "Added" TIMESTAMP NOT NULL,
                "AddedBy" character varying(100),
                "LastModified" TIMESTAMP NOT NULL,
                "LastModifiedBy" character varying(100),
                "Deleted" boolean NOT NULL,
                "DeletedAt" TIMESTAMP,
                CONSTRAINT "PK_UserGroups" PRIMARY KEY ("GroupId")
            );
            """);

        // {{ModuleItemStart:Appointments}}
        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS "Appointments" (
                "AppointmentId" uuid NOT NULL,
                "TenantId" uuid NOT NULL,
                "Title" character varying(200) NOT NULL,
                "Start" TIMESTAMP NOT NULL,
                "End" TIMESTAMP NOT NULL,
                "AllDay" boolean NOT NULL,
                "Meeting" boolean NOT NULL,
                -- {{ModuleItemStart:Locations}}
                "LocationId" uuid,
                -- {{ModuleItemEnd:Locations}}
                "Added" TIMESTAMP NOT NULL,
                "AddedBy" character varying(100),
                "LastModified" TIMESTAMP NOT NULL,
                "LastModifiedBy" character varying(100),
                "Deleted" boolean NOT NULL,
                "DeletedAt" TIMESTAMP,
                "Note" text,
                "ForegroundColor" character varying(100),
                "BackgroundColor" character varying(100)
                -- {{ModuleItemStart:Locations}}
                ,CONSTRAINT "PK_Appointments" PRIMARY KEY ("AppointmentId"),
                CONSTRAINT "FK_Appointments_Locations" FOREIGN KEY ("LocationId") REFERENCES "Locations" ("LocationId")
                -- {{ModuleItemEnd:Locations}}
            );
            """);
        // {{ModuleItemEnd:Appointments}}

        // {{ModuleItemStart:Tags}}
        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS "TagItems" (
                "TagItemId" uuid NOT NULL,
                "TagId" uuid NOT NULL,
                "TenantId" uuid NOT NULL,
                "ItemId" uuid NOT NULL,
                CONSTRAINT "PK_TagItems" PRIMARY KEY ("TagItemId"),
                CONSTRAINT "FK_TagItems_Tags" FOREIGN KEY ("TagId") REFERENCES "Tags" ("TagId")
            );
            """);
        // {{ModuleItemEnd:Tags}}

        var usersTable =
            """
            CREATE TABLE IF NOT EXISTS "Users" (
                "UserId" uuid NOT NULL,
                "TenantId" uuid NOT NULL,
                "FirstName" character varying(100),
                "LastName" character varying(100),
                "Email" character varying(100) NOT NULL,
                "Phone" character varying(20),
                "Username" character varying(100) NOT NULL,
                "EmployeeId" character varying(50),
                "DepartmentId" uuid,
                "Title" character varying(255),
                "Location" character varying(255),
                "Enabled" boolean NOT NULL,
                "LastLogin" TIMESTAMP,
                "LastLoginSource" character varying(50),
                "Admin" boolean NOT NULL,
            
            """;

        // {{ModuleItemStart:Appointments}}
        usersTable +=
            """
                "CanBeScheduled" boolean NOT NULL,
                "ManageAppointments" boolean NOT NULL,
            
            """;
        // {{ModuleItemEnd:Appointments}}

        usersTable +=
            """
                "ManageFiles" boolean NOT NULL,
                "Password" text,
                "PreventPasswordChange" boolean NOT NULL,
                "FailedLoginAttempts" integer,
                "LastLockoutDate" TIMESTAMP,
                "Source" character varying(100),
                "UDF01" character varying(500),
                "UDF02" character varying(500),
                "UDF03" character varying(500),
                "UDF04" character varying(500),
                "UDF05" character varying(500),
                "UDF06" character varying(500),
                "UDF07" character varying(500),
                "UDF08" character varying(500),
                "UDF09" character varying(500),
                "UDF10" character varying(500),
                "Added" TIMESTAMP NOT NULL,
                "AddedBy" character varying(100),
                "LastModified" TIMESTAMP NOT NULL,
                "LastModifiedBy" character varying(100),
                "Deleted" boolean NOT NULL,
                "DeletedAt" TIMESTAMP,
                "Preferences" text,
                CONSTRAINT "PK_Users" PRIMARY KEY ("UserId"),
                CONSTRAINT "IX_Users_DepartmentId" FOREIGN KEY ("DepartmentId") REFERENCES "Departments" ("DepartmentId"),
                CONSTRAINT "IX_Users_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("TenantId")
            );
            """;

        m1.Add(usersTable);

        // {{ModuleItemStart:Appointments}}
        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS "AppointmentNotes" (
                "AppointmentNoteId" uuid NOT NULL,
                "AppointmentId" uuid NOT NULL,
                "TenantId" uuid NOT NULL,
                "Added" TIMESTAMP NOT NULL,
                "AddedBy" character varying(100),
                "LastModified" TIMESTAMP NOT NULL,
                "LastModifiedBy" character varying(100),
                "Note" text,
                "Deleted" boolean NOT NULL,
                "DeletedAt" TIMESTAMP,
                CONSTRAINT "PK_AppointmentNotes" PRIMARY KEY ("AppointmentNoteId"),
                CONSTRAINT "FK_AppointmentNotes_Appointments" FOREIGN KEY ("AppointmentId") REFERENCES "Appointments" ("AppointmentId")
            );
            """);

        // {{ModuleItemStart:Services}}
        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS "AppointmentServices" (
                "AppointmentServiceId" uuid NOT NULL,
                "AppointmentId" uuid NOT NULL,
                "TenantId" uuid NOT NULL,
                "ServiceId" uuid NOT NULL,
                "Fee" numeric(19,4),
                "Deleted" boolean NOT NULL,
                "DeletedAt" TIMESTAMP,
                "LastModified" TIMESTAMP NOT NULL,
                "LastModifiedBy" character varying(100),
                CONSTRAINT "PK_AppointmentServices" PRIMARY KEY ("AppointmentServiceId"),
                CONSTRAINT "FK_AppointmentServices_Appointments" FOREIGN KEY ("AppointmentId") REFERENCES "Appointments" ("AppointmentId"),
                CONSTRAINT "FK_AppointmentServices_Services" FOREIGN KEY ("ServiceId") REFERENCES "Services" ("ServiceId")
            );
            """);
        // {{ModuleItemEnd:Services}}

        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS "AppointmentUsers" (
                "AppointmentUserId" uuid NOT NULL,
                "AppointmentId" uuid NOT NULL,
                "TenantId" uuid NOT NULL,
                "UserId" uuid NOT NULL,
                "AttendanceCode" character varying(50),
                "Fees" numeric(19,4),
                CONSTRAINT "PK_AppointmentUsers" PRIMARY KEY ("AppointmentUserId"),
                CONSTRAINT "FK_AppointmentUsers_Appointments" FOREIGN KEY ("AppointmentId") REFERENCES "Appointments" ("AppointmentId"),
                CONSTRAINT "FK_AppointmentUsers_Users" FOREIGN KEY ("UserId") REFERENCES "Users" ("UserId")
            );
            """);
        // {{ModuleItemEnd:Appointments}}

        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS "FileStorage" (
                "FileId" uuid NOT NULL,
                "ItemId" uuid,
                "FileName" character varying(255) NOT NULL,
                "Extension" character varying(15) NOT NULL,
                "Bytes" bigint,
                "Value" bytea,
                "UploadDate" TIMESTAMP NOT NULL,
                "UploadedBy" character varying(100),
                "UserId" uuid,
                "SourceFileId" character varying(100),
                "TenantId" uuid NOT NULL,
                "LastModified" TIMESTAMP NOT NULL,
                "LastModifiedBy" character varying(100),
                "Deleted" boolean NOT NULL,
                "DeletedAt" TIMESTAMP,
                CONSTRAINT "PK_FileStorage" PRIMARY KEY ("FileId"),
                CONSTRAINT "IX_FileStorage_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("UserId")
            );
            """);

        // {{ModuleItemStart:Invoices}}
        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS "Invoices" (
                "InvoiceId" uuid NOT NULL,
                "TenantId" uuid NOT NULL,
                "InvoiceNumber" character varying(100),
                "PONumber" character varying(100),
                -- {{ModuleItemStart:Appointments}}
                "AppointmentId" uuid,
                -- {{ModuleItemEnd:Appointments}}
                "UserId" uuid,
                "Title" character varying(255) NOT NULL,
                "Items" text NOT NULL,
                "Notes" text,
                "InvoiceCreated" TIMESTAMP,
                "InvoiceDueDate" TIMESTAMP,
                "InvoiceSendDate" TIMESTAMP,
                "InvoiceSent" TIMESTAMP,
                "InvoiceClosed" TIMESTAMP,
                "Total" numeric(19,4) NOT NULL,
                "Added" TIMESTAMP NOT NULL,
                "AddedBy" character varying(100),
                "LastModified" TIMESTAMP NOT NULL,
                "LastModifiedBy" character varying(100),
                "Deleted" boolean NOT NULL,
                "DeletedAt" TIMESTAMP,
                CONSTRAINT "PK_Invoices" PRIMARY KEY ("InvoiceId"),
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
                "UserInGroupId" uuid NOT NULL,
                "UserId" uuid NOT NULL,
                "TenantId" uuid NOT NULL,
                "GroupId" uuid NOT NULL,
                CONSTRAINT "PK_UserInGroups" PRIMARY KEY ("UserInGroupId"),
                CONSTRAINT "FK_UserInGroups_UserGroups" FOREIGN KEY ("GroupId") REFERENCES "UserGroups" ("GroupId"),
                CONSTRAINT "FK_UserInGroups_Users" FOREIGN KEY ("UserId") REFERENCES "Users" ("UserId")
            );
            """);

        // {{ModuleItemStart:Payments}}
        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS "Payments" (
                "PaymentId" uuid NOT NULL,
                "TenantId" uuid NOT NULL,
                "InvoiceId" uuid NOT NULL,
                "UserId" uuid,
                "Notes" text,
                "PaymentDate" TIMESTAMP NOT NULL,
                "Amount" numeric(19,4) NOT NULL,
                "Refunded" numeric(19,4),
                "RefundedBy" character varying(100),
                "RefundDate" TIMESTAMP,
                "Added" TIMESTAMP NOT NULL,
                "AddedBy" character varying(100),
                "LastModified" TIMESTAMP NOT NULL,
                "LastModifiedBy" character varying(100),
                "Deleted" boolean NOT NULL,
                "DeletedAt" TIMESTAMP,
                CONSTRAINT "PK_Payments" PRIMARY KEY ("PaymentId"),
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
        // {{ModuleItemEnd:Appointments}}

        m1.Add(
            """
            CREATE INDEX "IX_FileStorage_UserId" ON "FileStorage" ("UserId");
            """);

        // {{ModuleItemStart:Invoices}}
        m1.Add(
            """
            CREATE INDEX "IX_Invoices_AppointmentId" ON "Invoices" ("AppointmentId");
            """);

        m1.Add(
            """
            CREATE INDEX "IX_Invoices_TenantId" ON "Invoices" ("TenantId");
            """);

        m1.Add(
            """
            CREATE INDEX "IX_Invoices_UserId" ON "Invoices" ("UserId");
            """);
        // {{ModuleItemEnd:Invoices}}

        // {{ModuleItemStart:Payments}}
        m1.Add(
            """
            CREATE INDEX "IX_Payments_InvoiceId" ON "Payments" ("InvoiceId");
            """);


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
            SELECT * FROM "__EFMigrationsHistory" WHERE "MigrationId"='001';
            """);

        output.Add(new DataObjects.DataMigration {
            MigrationId = "001",
            Migration = m1
        });

        return output;
    }
}