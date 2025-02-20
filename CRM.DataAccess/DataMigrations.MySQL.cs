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
                `DepartmentGroupId` CHAR(36) NOT NULL,
                `DepartmentGroupName` TEXT NULL,
                `TenantId` CHAR(36) NULL,
                PRIMARY KEY (`DepartmentGroupId`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
            """);

        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS `Departments` (
                `DepartmentId` CHAR(36) NOT NULL,
                `DepartmentName` TEXT NULL,
                `ActiveDirectoryNames` TEXT NULL,
                `Enabled` INTEGER NULL,
                `DepartmentGroupId` CHAR(36) NULL,
                `TenantId` CHAR(36) NULL,
                PRIMARY KEY (`DepartmentId`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
            """);

        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS `Settings` (
                `SettingId` INTEGER NOT NULL AUTO_INCREMENT,
                `SettingName` TEXT NOT NULL,
                `SettingType` TEXT NULL,
                `SettingNotes` TEXT NULL,
                `SettingText` TEXT NULL,
                `TenantId` CHAR(36) NULL,
                `UserId` CHAR(36) NULL,
                PRIMARY KEY (`SettingId`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
            """);

        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS `Tenants` (
                `TenantId` CHAR(36) NOT NULL,
                `Name` TEXT NOT NULL,
                `TenantCode` TEXT NOT NULL,
                `Enabled` INTEGER NOT NULL,
                PRIMARY KEY (`TenantId`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
            """);

        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS `UDFLabels` (
                `Id` CHAR(36) NOT NULL,
                `Module` TEXT NULL,
                `UDF` TEXT NULL,
                `Label` TEXT NULL,
                `ShowColumn` INTEGER NULL,
                `ShowInFilter` INTEGER NULL,
                `IncludeInSearch` INTEGER NULL,
                `TenantId` CHAR(36) NULL,
                PRIMARY KEY (`Id`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
            """);

        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS `Users` (
                `UserId` CHAR(36) NOT NULL,
                `TenantId` CHAR(36) NOT NULL,
                `FirstName` TEXT NULL,
                `LastName` TEXT NULL,
                `Email` TEXT NULL,
                `Phone` TEXT NULL,
                `Username` TEXT NOT NULL,
                `EmployeeId` TEXT NULL,
                `DepartmentId` CHAR(36) NULL,
                `Title` TEXT NULL,
                `Location` TEXT NULL,
                `Enabled` INTEGER NULL,
                `LastLogin` datetime NULL,
                `Admin` INTEGER NULL,
                `Password` TEXT NULL,
                `PreventPasswordChange` INTEGER NULL,
                `FailedLoginAttempts` INTEGER NULL,
                `LastLockoutDate` datetime NULL,
                `Source` TEXT NULL,
                `UDF01` TEXT NULL,
                `UDF02` TEXT NULL,
                `UDF03` TEXT NULL,
                `UDF04` TEXT NULL,
                `UDF05` TEXT NULL,
                `UDF06` TEXT NULL,
                `UDF07` TEXT NULL,
                `UDF08` TEXT NULL,
                `UDF09` TEXT NULL,
                `UDF10` TEXT NULL,
                PRIMARY KEY (`UserId`),
                CONSTRAINT `FK_Users_Departments` FOREIGN KEY (`DepartmentId`) REFERENCES `Departments` (`DepartmentId`),
                CONSTRAINT `FK_Users_Tenants` FOREIGN KEY (`TenantId`) REFERENCES `Tenants` (`TenantId`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
            """);

        m1.Add(
            """
            CREATE TABLE IF NOT EXISTS `FileStorage` (
                `FileId` CHAR(36) NOT NULL,
                `ItemId` CHAR(36) NULL,
                `FileName` TEXT NULL,
                `Extension` TEXT NULL,
                `Bytes` INTEGER NULL,
                `Value` BLOB NULL,
                `UploadDate` datetime NULL,
                `UserId` CHAR(36) NULL,
                `SourceFileId` TEXT NULL,
                `TenantId` CHAR(36) NULL,
                PRIMARY KEY (`FileId`),
                CONSTRAINT `FK_FileStorage_Users` FOREIGN KEY (`UserId`) REFERENCES `Users` (`UserId`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
            """);

        m1.Add(
            """
            CREATE INDEX `IX_FileStorage_UserId` ON `FileStorage` (`UserId`);
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
            VALUES ('20230204160527_001', '7.0.2');
            """);


        output.Add(new DataObjects.DataMigration {
            MigrationId = "20230204160527_001",
            Migration = m1
        });










        List<string> m2 = new List<string>();

        // Make sure the Source field exists in the Users table.
        m2.Add(
            """
            ALTER TABLE `Users` ADD COLUMN `Source` TEXT NULL 
            """);

        m2.Add(
            """
            CREATE TABLE IF NOT EXISTS `UserGroups` (
                `GroupId` CHAR(36) NOT NULL,
                `TenantId` CHAR(36) NOT NULL,
                `Name` TEXT NULL,
                `Enabled` INTEGER NULL,
                `Settings` TEXT NULL,
                PRIMARY KEY (`GroupId`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
            """);

        m2.Add(
            """
            CREATE TABLE IF NOT EXISTS `UserInGroups` (
                `UserInGroupId` CHAR(36) NOT NULL,
                `UserId` CHAR(36) NOT NULL,
                `TenantId` CHAR(36) NOT NULL,
                `GroupId` CHAR(36) NOT NULL,
                PRIMARY KEY (`UserInGroupId`),
                CONSTRAINT `FK_UserInGroups_UserGroups` FOREIGN KEY (`GroupId`) REFERENCES `UserGroups` (`GroupId`),
                CONSTRAINT `FK_UserInGroups_Users` FOREIGN KEY (`UserId`) REFERENCES `Users` (`UserId`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
            """);

        m2.Add(
            """
            INSERT IGNORE INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
            VALUES ('20230227163529_002', '7.0.2');
            """);

        output.Add(new DataObjects.DataMigration {
            MigrationId = "20230227163529_002",
            Migration = m2
        });

        return output;
    }
}