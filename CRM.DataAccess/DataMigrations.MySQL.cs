namespace CRM;
public partial class DataMigrations
{
    public List<DataObjects.DataMigration> GetMigrationsMySQL()
    {
        List<DataObjects.DataMigration> output = new List<DataObjects.DataMigration>();

        List<string> m1 = new List<string>();
        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
                `MigrationId` varchar(150) NOT NULL,
                `ProductVersion` varchar(32) NOT NULL,
                PRIMARY KEY (`MigrationId`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
            """);

        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS `DepartmentGroups` (
                `DepartmentGroupId` char(36) NOT NULL,
                `DepartmentGroupName` varchar(200) NULL,
                `TenantId` char(36) NOT NULL,
                `Added` datetime NOT NULL,
                `AddedBy` varchar(100) NULL,
                `LastModified` datetime NOT NULL,
                `LastModifiedBy` varchar(100) NULL,
                `Deleted` tinyint(1) NOT NULL,
                `DeletedAt` datetime NULL,
                PRIMARY KEY (`DepartmentGroupId`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
            """);

        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS `Departments` (
                `DepartmentId` char(36) NOT NULL,
                `DepartmentName` varchar(100) NOT NULL,
                `ActiveDirectoryNames` varchar(100) NULL,
                `Enabled` tinyint(1) NOT NULL,
                `DepartmentGroupId` char(36) NULL,
                `TenantId` char(36) NOT NULL,
                `Added` datetime NOT NULL,
                `AddedBy` varchar(100) NULL,
                `LastModified` datetime NOT NULL,
                `LastModifiedBy` varchar(100) NULL,
                `Deleted` tinyint(1) NOT NULL,
                `DeletedAt` datetime NULL,
                PRIMARY KEY (`DepartmentId`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
            """);

        // {{ModuleItemStart:EmailTemplates}}
        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS `EmailTemplates` (
                `EmailTemplateId` char(36) NOT NULL,
                `TenantId` char(36) NOT NULL,
                `Name` varchar(300) NOT NULL,
                `Template` longtext NULL,
                `Enabled` tinyint(1) NOT NULL,
                `Added` datetime NOT NULL,
                `AddedBy` varchar(100) NULL,
                `LastModified` datetime NOT NULL,
                `LastModifiedBy` varchar(100) NULL,
                `Deleted` tinyint(1) NOT NULL,
                `DeletedAt` datetime NULL,
                PRIMARY KEY (`EmailTemplateId`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
            """);
        // {{ModuleItemEnd:EmailTemplates}}

        // {{ModuleItemStart:Locations}}
        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS `Locations` (
                `LocationId` char(36) NOT NULL,
                `TenantId` char(36) NOT NULL,
                `Name` varchar(200) NOT NULL,
                `Address` varchar(200) NULL,
                `City` varchar(100) NULL,
                `State` varchar(50) NULL,
                `PostalCode` varchar(50) NULL,
                `CalendarBackgroundColor` varchar(100) NULL,
                `CalendarForegroundColor` varchar(100) NULL,
                `Enabled` tinyint(1) NOT NULL,
                `DefaultLocation` tinyint(1) NOT NULL,
                `Added` datetime NOT NULL,
                `AddedBy` varchar(100) NULL,
                `LastModified` datetime NOT NULL,
                `LastModifiedBy` varchar(100) NULL,
                `Deleted` tinyint(1) NOT NULL,
                `DeletedAt` datetime NULL,
                PRIMARY KEY (`LocationId`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
            """);
        // {{ModuleItemEnd:Locations}}

        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS `PluginCache` (
                `RecordId` char(36) NOT NULL,
                `Id` char(36) NOT NULL,
                `Author` varchar(100) NULL,
                `Name` varchar(100) NULL,
                `Type` varchar(100) NULL,
                `Version` varchar(100) NULL,
                `Properties` longtext NULL,
                `Namespace` varchar(100) NULL,
                `ClassName` varchar(100) NULL,
                `Code` longtext NULL,
                `AdditionalAssemblies` longtext NULL,
                `StillExists` tinyint(1) NOT NULL,
                PRIMARY KEY (`RecordId`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
            """);

        // {{ModuleItemStart:Services}}
        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS `Services` (
                `ServiceId` char(36) NOT NULL,
                `TenantId` char(36) NOT NULL,
                `Code` varchar(50) NULL,
                `DefaultService` tinyint(1) NOT NULL,
                `Description` varchar(200) NOT NULL,
                `Rate` decimal(19,4) NOT NULL,
                `DefaultAppointmentDuration` int NOT NULL,
                `Enabled` tinyint(1) NOT NULL,
                `Added` datetime NOT NULL,
                `AddedBy` varchar(100) NULL,
                `LastModified` datetime NOT NULL,
                `LastModifiedBy` varchar(100) NULL,
                `Deleted` tinyint(1) NOT NULL,
                `DeletedAt` datetime NULL,
                PRIMARY KEY (`ServiceId`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
            """);
        // {{ModuleItemEnd:Services}}

        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS `Settings` (
                `SettingId` int NOT NULL AUTO_INCREMENT,
                `SettingName` varchar(100) NOT NULL,
                `SettingType` varchar(100) NULL,
                `SettingNotes` longtext NULL,
                `SettingText` longtext NULL,
                `TenantId` char(36) NULL,
                `UserId` char(36) NULL,
                `LastModified` datetime NOT NULL,
                `LastModifiedBy` varchar(100) NULL,
                PRIMARY KEY (`SettingId`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
            """);

        // {{ModuleItemStart:Tags}}
        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS `Tags` (
                `TagId` char(36) NOT NULL,
                `TenantId` char(36) NOT NULL,
                `Name` varchar(200) NOT NULL,
                `Style` longtext NULL,
                `Enabled` tinyint(1) NOT NULL,
                -- {{ModuleItemStart:Appointments}}
                `UseInAppointments` tinyint(1) NOT NULL,
                -- {{ModuleItemEnd:Appointments}}
                -- {{ModuleItemStart:EmailTemplates}}
                `UseInEmailTemplates` tinyint(1) NOT NULL,
                -- {{ModuleItemEnd:EmailTemplates}}
                -- {{ModuleItemStart:Services}}
                `UseInServices` tinyint(1) NOT NULL,
                -- {{ModuleItemEnd:Services}}
                `Added` datetime NOT NULL,
                `AddedBy` varchar(100) NULL,
                `LastModified` datetime NOT NULL,
                `LastModifiedBy` varchar(100) NULL,
                `Deleted` tinyint(1) NOT NULL,
                `DeletedAt` datetime NULL,
                PRIMARY KEY (`TagId`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
            """);
        // {{ModuleItemEnd:Tags}}

        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS `Tenants` (
                `TenantId` char(36) NOT NULL,
                `Name` varchar(200) NOT NULL,
                `TenantCode` varchar(50) NOT NULL,
                `Enabled` tinyint(1) NOT NULL,
                `Added` datetime NOT NULL,
                `AddedBy` varchar(100) NULL,
                `LastModified` datetime NOT NULL,
                `LastModifiedBy` varchar(100) NULL,
                PRIMARY KEY (`TenantId`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
            """);

        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS `UDFLabels` (
                `Id` char(36) NOT NULL,
                `Module` varchar(20) NOT NULL,
                `UDF` varchar(10) NOT NULL,
                `Label` longtext NULL,
                `ShowColumn` tinyint(1) NULL,
                `ShowInFilter` tinyint(1) NULL,
                `IncludeInSearch` tinyint(1) NULL,
                `TenantId` char(36) NOT NULL,
                `LastModified` datetime NOT NULL,
                `LastModifiedBy` varchar(100) NULL,
                PRIMARY KEY (`Id`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
            """);

        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS `UserGroups` (
                `GroupId` char(36) NOT NULL,
                `TenantId` char(36) NOT NULL,
                `Name` varchar(100) NOT NULL,
                `Enabled` tinyint(1) NOT NULL,
                `Settings` longtext NULL,
                `Added` datetime NOT NULL,
                `AddedBy` varchar(100) NULL,
                `LastModified` datetime NOT NULL,
                `LastModifiedBy` varchar(100) NULL,
                `Deleted` tinyint(1) NOT NULL,
                `DeletedAt` datetime NULL,
                PRIMARY KEY (`GroupId`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
            """);

        // {{ModuleItemStart:Appointments}}
        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS `Appointments` (
                `AppointmentId` char(36) NOT NULL,
                `TenantId` char(36) NOT NULL,
                `Title` varchar(200) NOT NULL,
                `Start` datetime NOT NULL,
                `End` datetime NOT NULL,
                `AllDay` tinyint(1) NOT NULL,
                `Meeting` tinyint(1) NOT NULL,
                -- {{ModuleItemStart:Locations}}
                `LocationId` char(36) NULL,
                -- {{ModuleItemEnd:Locations}}
                `Added` datetime NOT NULL,
                `AddedBy` varchar(100) NULL,
                `LastModified` datetime NOT NULL,
                `LastModifiedBy` varchar(100) NULL,
                `Deleted` tinyint(1) NOT NULL,
                `DeletedAt` datetime NULL,
                `Note` longtext NULL,
                `ForegroundColor` varchar(100) NULL,
                `BackgroundColor` varchar(100) NULL,
                PRIMARY KEY (`AppointmentId`)
                -- {{ModuleItemStart:Locations}}
                ,CONSTRAINT `FK_Appointments_Locations` FOREIGN KEY (`LocationId`) REFERENCES `Locations` (`LocationId`)
                -- {{ModuleItemEnd:Locations}}
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
            """);
        // {{ModuleItemEnd:Appointments}}

        // {{ModuleItemStart:Tags}}
        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS `TagItems` (
                `TagItemId` char(36) NOT NULL,
                `TagId` char(36) NOT NULL,
                `TenantId` char(36) NOT NULL,
                `ItemId` char(36) NOT NULL,
                PRIMARY KEY (`TagItemId`),
                CONSTRAINT `FK_TagItems_Tags` FOREIGN KEY (`TagId`) REFERENCES `Tags` (`TagId`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
            """);
        // {{ModuleItemEnd:Tags}}

        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS `Users` (
                `UserId` char(36) NOT NULL,
                `TenantId` char(36) NOT NULL,
                `FirstName` varchar(100) NULL,
                `LastName` varchar(100) NULL,
                `Email` varchar(100) NOT NULL,
                `Phone` varchar(20) NULL,
                `Username` varchar(100) NOT NULL,
                `EmployeeId` varchar(50) NULL,
                `DepartmentId` char(36) NULL,
                `Title` varchar(255) NULL,
                `Location` varchar(255) NULL,
                `Enabled` tinyint(1) NOT NULL,
                `LastLogin` datetime NULL,
                `LastLoginSource` varchar(50) NULL,
                `Admin` tinyint(1) NOT NULL,
                `CanBeScheduled` tinyint(1) NOT NULL,
                `ManageFiles` tinyint(1) NOT NULL,
                `ManageAppointments` tinyint(1) NOT NULL,
                `Password` longtext NULL,
                `PreventPasswordChange` tinyint(1) NOT NULL,
                `FailedLoginAttempts` int NULL,
                `LastLockoutDate` datetime NULL,
                `Source` varchar(100) NULL,
                `UDF01` varchar(500) NULL,
                `UDF02` varchar(500) NULL,
                `UDF03` varchar(500) NULL,
                `UDF04` varchar(500) NULL,
                `UDF05` varchar(500) NULL,
                `UDF06` varchar(500) NULL,
                `UDF07` varchar(500) NULL,
                `UDF08` varchar(500) NULL,
                `UDF09` varchar(500) NULL,
                `UDF10` varchar(500) NULL,
                `Added` datetime NOT NULL,
                `AddedBy` varchar(100) NULL,
                `LastModified` datetime NOT NULL,
                `LastModifiedBy` varchar(100) NULL,
                `Deleted` tinyint(1) NOT NULL,
                `DeletedAt` datetime NULL,
                `Preferences` longtext NULL,
                PRIMARY KEY (`UserId`),
                CONSTRAINT `IX_Users_DepartmentId` FOREIGN KEY (`DepartmentId`) REFERENCES `Departments` (`DepartmentId`),
                CONSTRAINT `IX_Users_TenantId` FOREIGN KEY (`TenantId`) REFERENCES `Tenants` (`TenantId`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
            """);

        // {{ModuleItemStart:Appointments}}
        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS `AppointmentNotes` (
                `AppointmentNoteId` char(36) NOT NULL,
                `AppointmentId` char(36) NOT NULL,
                `TenantId` char(36) NOT NULL,
                `Added` datetime NOT NULL,
                `AddedBy` varchar(100) NULL,
                `LastModified` datetime NOT NULL,
                `LastModifiedBy` varchar(100) NULL,
                `Note` longtext NULL,
                `Deleted` tinyint(1) NOT NULL,
                `DeletedAt` datetime NULL,
                PRIMARY KEY (`AppointmentNoteId`),
                CONSTRAINT `FK_AppointmentNotes_Appointments` FOREIGN KEY (`AppointmentId`) REFERENCES `Appointments` (`AppointmentId`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
            """);

        // {{ModuleItemStart:Services}}
        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS `AppointmentServices` (
                `AppointmentServiceId` char(36) NOT NULL,
                `AppointmentId` char(36) NOT NULL,
                `TenantId` char(36) NOT NULL,
                `ServiceId` char(36) NOT NULL,
                `Fee` decimal(19,4) NULL,
                `Deleted` tinyint(1) NOT NULL,
                `DeletedAt` datetime NULL,
                `LastModified` datetime NOT NULL,
                `LastModifiedBy` varchar(100) NULL,
                PRIMARY KEY (`AppointmentServiceId`),
                CONSTRAINT `FK_AppointmentServices_Appointments` FOREIGN KEY (`AppointmentId`) REFERENCES `Appointments` (`AppointmentId`),
                CONSTRAINT `FK_AppointmentServices_Services` FOREIGN KEY (`ServiceId`) REFERENCES `Services` (`ServiceId`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
            """);
        // {{ModuleItemEnd:Services}}

        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS `AppointmentUsers` (
                `AppointmentUserId` char(36) NOT NULL,
                `AppointmentId` char(36) NOT NULL,
                `TenantId` char(36) NOT NULL,
                `UserId` char(36) NOT NULL,
                `AttendanceCode` varchar(50) NULL,
                `Fees` decimal(19,4) NULL,
                PRIMARY KEY (`AppointmentUserId`),
                CONSTRAINT `FK_AppointmentUsers_Appointments` FOREIGN KEY (`AppointmentId`) REFERENCES `Appointments` (`AppointmentId`),
                CONSTRAINT `FK_AppointmentUsers_Users` FOREIGN KEY (`UserId`) REFERENCES `Users` (`UserId`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
            """);
        // {{ModuleItemEnd:Appointments}}

        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS `FileStorage` (
                `FileId` char(36) NOT NULL,
                `ItemId` char(36) NULL,
                `FileName` varchar(255) NOT NULL,
                `Extension` varchar(15) NOT NULL,
                `Bytes` bigint NULL,
                `Value` longblob NULL,
                `UploadDate` datetime NOT NULL,
                `UploadedBy` varchar(100) NULL,
                `UserId` char(36) NULL,
                `SourceFileId` varchar(100) NULL,
                `TenantId` char(36) NOT NULL,
                `LastModified` datetime NOT NULL,
                `LastModifiedBy` varchar(100) NULL,
                `Deleted` tinyint(1) NOT NULL,
                `DeletedAt` datetime NULL,
                PRIMARY KEY (`FileId`),
                CONSTRAINT `IX_FileStorage_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`UserId`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
            """);

        // {{ModuleItemStart:Invoices}}
        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS `Invoices` (
                `InvoiceId` char(36) NOT NULL,
                `TenantId` char(36) NOT NULL,
                `InvoiceNumber` varchar(100) NULL,
                `PONumber` varchar(100) NULL,
                -- {{ModuleItemStart:Appointments}}
                `AppointmentId` char(36) NULL,
                -- {{ModuleItemEnd:Appointments}}
                `UserId` char(36) NULL,
                `Title` varchar(255) NOT NULL,
                `Items` longtext NOT NULL,
                `Notes` longtext NULL,
                `InvoiceCreated` datetime NULL,
                `InvoiceDueDate` datetime NULL,
                `InvoiceSendDate` datetime NULL,
                `InvoiceSent` datetime NULL,
                `InvoiceClosed` datetime NULL,
                `Total` decimal(19,4) NOT NULL,
                `Added` datetime NOT NULL,
                `AddedBy` varchar(100) NULL,
                `LastModified` datetime NOT NULL,
                `LastModifiedBy` varchar(100) NULL,
                `Deleted` tinyint(1) NOT NULL,
                `DeletedAt` datetime NULL,
                PRIMARY KEY (`InvoiceId`),
                -- {{ModuleItemStart:Appointments}}
                CONSTRAINT `FK_Invoices_Appointments` FOREIGN KEY (`AppointmentId`) REFERENCES `Appointments` (`AppointmentId`),
                -- {{ModuleItemEnd:Appointments}}
                CONSTRAINT `FK_Invoices_Tenants` FOREIGN KEY (`TenantId`) REFERENCES `Tenants` (`TenantId`),
                CONSTRAINT `FK_Invoices_Users` FOREIGN KEY (`UserId`) REFERENCES `Users` (`UserId`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
            """);
        // {{ModuleItemEnd:Invoices}}

        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS `UserInGroups` (
                `UserInGroupId` char(36) NOT NULL,
                `UserId` char(36) NOT NULL,
                `TenantId` char(36) NOT NULL,
                `GroupId` char(36) NOT NULL,
                PRIMARY KEY (`UserInGroupId`),
                CONSTRAINT `FK_UserInGroups_UserGroups` FOREIGN KEY (`GroupId`) REFERENCES `UserGroups` (`GroupId`),
                CONSTRAINT `FK_UserInGroups_Users` FOREIGN KEY (`UserId`) REFERENCES `Users` (`UserId`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
            """);

        // {{ModuleItemStart:Payments}}
        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS `Payments` (
                `PaymentId` char(36) NOT NULL,
                `TenantId` char(36) NOT NULL,
                `InvoiceId` char(36) NOT NULL,
                `UserId` char(36) NULL,
                `Notes` longtext NULL,
                `PaymentDate` datetime NOT NULL,
                `Amount` decimal(19,4) NOT NULL,
                `Refunded` decimal(19,4) NULL,
                `RefundedBy` varchar(100) NULL,
                `RefundDate` datetime NULL,
                `Added` datetime NOT NULL,
                `AddedBy` varchar(100) NULL,
                `LastModified` datetime NOT NULL,
                `LastModifiedBy` varchar(100) NULL,
                `Deleted` tinyint(1) NOT NULL,
                `DeletedAt` datetime NULL,
                PRIMARY KEY (`PaymentId`),
                CONSTRAINT `FK_Payments_Invoices` FOREIGN KEY (`InvoiceId`) REFERENCES `Invoices` (`InvoiceId`),
                CONSTRAINT `FK_Payments_Tenants` FOREIGN KEY (`TenantId`) REFERENCES `Tenants` (`TenantId`),
                CONSTRAINT `FK_Payments_Users` FOREIGN KEY (`UserId`) REFERENCES `Users` (`UserId`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
            """);
        // {{ModuleItemEnd:Payments}}

        // {{ModuleItemStart:Appointments}}
        m1.Add(
            """
            CREATE INDEX `IX_AppointmentNotes_AppointmentId` ON `AppointmentNotes` (`AppointmentId`);
            """);

        // {{ModuleItemStart:Locations}}
        m1.Add(
            """
            CREATE INDEX `IX_Appointments_LocationId` ON `Appointments` (`LocationId`);
            """);
        // {{ModuleItemEnd:Locations}}
        // {{ModuleItemEnd:Appointments}}

        // {{ModuleItemStart:Appointments}}
        // {{ModuleItemStart:Services}}
        m1.Add(
            """
            CREATE INDEX `IX_AppointmentServices_AppointmentId` ON `AppointmentServices` (`AppointmentId`);
            """);
        // {{ModuleItemEnd:Services}}

        m1.Add(
            """
            CREATE INDEX `IX_AppointmentServices_ServiceId` ON `AppointmentServices` (`ServiceId`);
            """);

        m1.Add(
            """
            CREATE INDEX `IX_AppointmentUsers_AppointmentId` ON `AppointmentUsers` (`AppointmentId`);
            """);

        m1.Add(
            """
            CREATE INDEX `IX_AppointmentUsers_UserId` ON `AppointmentUsers` (`UserId`);
            """);
        // {{ModuleItemEnd:Appointments}}

        m1.Add(
            """
            CREATE INDEX `IX_FileStorage_UserId` ON `FileStorage` (`UserId`);
            """);

        // {{ModuleItemStart:Invoices}}
        m1.Add(
            """
            CREATE INDEX `IX_Invoices_AppointmentId` ON `Invoices` (`AppointmentId`);
            """);

        m1.Add(
            """
            CREATE INDEX `IX_Invoices_TenantId` ON `Invoices` (`TenantId`);
            """);

        m1.Add(
            """
            CREATE INDEX `IX_Invoices_UserId` ON `Invoices` (`UserId`);
            """);

        m1.Add(
            """
            CREATE INDEX `IX_Payments_InvoiceId` ON `Payments` (`InvoiceId`);
            """);
        // {{ModuleItemEnd:Invoices}}

        // {{ModuleItemStart:Payments}}
        m1.Add(
            """
            CREATE INDEX `IX_Payments_TenantId` ON `Payments` (`TenantId`);
            """);

        m1.Add(
            """
            CREATE INDEX `IX_Payments_UserId` ON `Payments` (`UserId`);
            """);
        // {{ModuleItemEnd:Payments}}

        // {{ModuleItemStart:Tags}}
        m1.Add(
            """
            CREATE INDEX `IX_TagItems_TagId` ON `TagItems` (`TagId`);
            """);
        // {{ModuleItemEnd:Tags}}

        m1.Add(
            """
            CREATE INDEX `IX_UserInGroups_GroupId` ON `UserInGroups` (`GroupId`);
            """);

        m1.Add(
            """
            CREATE INDEX `IX_UserInGroups_UserId` ON `UserInGroups` (`UserId`);
            """);

        m1.Add(
            """
            CREATE INDEX `IX_Users_DepartmentId` ON `Users` (`DepartmentId`);
            """);

        m1.Add(
            """
            CREATE INDEX `IX_Users_TenantId` ON `Users` (`TenantId`);
            """);

        m1.Add(
            """
            INSERT IGNORE INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
            VALUES ('001', '1.0.0');
            """);


        output.Add(new DataObjects.DataMigration {
            MigrationId = "001",
            Migration = m1
        });

        return output;
    }
}