namespace CRM;
public partial class DataMigrations
{
    public List<DataObjects.DataMigration> GetMigrationsSqlServer()
    {
        List<DataObjects.DataMigration> output = new List<DataObjects.DataMigration>();

        List<string> m1 = new List<string>();
        m1.Add(
            """
            IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
            BEGIN
                CREATE TABLE [__EFMigrationsHistory] (
                    [MigrationId] nvarchar(150) NOT NULL,
                    [ProductVersion] nvarchar(32) NOT NULL,
                    CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
                )
            END
            """);

        m1.Add(
            """
            IF OBJECT_ID(N'[DepartmentGroups]') IS NULL
            BEGIN
                CREATE TABLE [DepartmentGroups] (
                    [DepartmentGroupId] uniqueidentifier NOT NULL,
                    [DepartmentGroupName] nvarchar(200) NULL,
                    [TenantId] uniqueidentifier NULL,
                    CONSTRAINT [PK_DepartmentGroups] PRIMARY KEY ([DepartmentGroupId])
                )
            END
            """);

        m1.Add(
            """
            IF OBJECT_ID(N'[Departments]') IS NULL
            BEGIN
                CREATE TABLE [Departments] (
                    [DepartmentId] uniqueidentifier NOT NULL,
                    [DepartmentName] nvarchar(100) NULL,
                    [ActiveDirectoryNames] nvarchar(100) NULL,
                    [Enabled] bit NULL,
                    [DepartmentGroupId] uniqueidentifier NULL,
                    [TenantId] uniqueidentifier NULL,
                    CONSTRAINT [PK_Departments] PRIMARY KEY ([DepartmentId])
                )
            END
            """);

        m1.Add(
            """
            IF OBJECT_ID(N'[Settings]') IS NULL
            BEGIN
                CREATE TABLE [Settings] (
                    [SettingId] int NOT NULL IDENTITY,
                    [SettingName] nvarchar(100) NOT NULL,
                    [SettingType] nvarchar(100) NULL,
                    [SettingNotes] nvarchar(max) NULL,
                    [SettingText] nvarchar(max) NULL,
                    [TenantId] uniqueidentifier NULL,
                    [UserId] uniqueidentifier NULL,
                    CONSTRAINT [PK_Settings_1] PRIMARY KEY ([SettingId])
                )
            END
            """);

        m1.Add(
            """
            IF OBJECT_ID(N'[Tenants]') IS NULL
            BEGIN
                CREATE TABLE [Tenants] (
                    [TenantId] uniqueidentifier NOT NULL,
                    [Name] nvarchar(200) NOT NULL,
                    [TenantCode] nvarchar(50) NOT NULL,
                    [Enabled] bit NOT NULL,
                    CONSTRAINT [PK_Tenants] PRIMARY KEY ([TenantId])
                )
            END
            """);

        m1.Add(
            """
            IF OBJECT_ID(N'[UDFLabels]') IS NULL
            BEGIN
                CREATE TABLE [UDFLabels] (
                    [Id] uniqueidentifier NOT NULL,
                    [Module] nvarchar(20) NULL,
                    [UDF] nvarchar(10) NULL,
                    [Label] nvarchar(max) NULL,
                    [ShowColumn] bit NULL,
                    [ShowInFilter] bit NULL,
                    [IncludeInSearch] bit NULL,
                    [TenantId] uniqueidentifier NULL,
                    CONSTRAINT [PK_UDFLabels] PRIMARY KEY ([Id])
                )
            END
            """);

        m1.Add(
            """
            IF OBJECT_ID(N'[Users]') IS NULL
            BEGIN
                CREATE TABLE [Users] (
                    [UserId] uniqueidentifier NOT NULL,
                    [TenantId] uniqueidentifier NOT NULL,
                    [FirstName] nvarchar(100) NULL,
                    [LastName] nvarchar(100) NULL,
                    [Email] nvarchar(100) NULL,
                    [Phone] nvarchar(20) NULL,
                    [Username] nvarchar(100) NOT NULL,
                    [EmployeeId] nvarchar(50) NULL,
                    [DepartmentId] uniqueidentifier NULL,
                    [Title] nvarchar(255) NULL,
                    [Location] nvarchar(255) NULL,
                    [Enabled] bit NULL,
                    [LastLogin] datetime NULL,
                    [Admin] bit NULL,
                    [Password] nvarchar(max) NULL,
                    [PreventPasswordChange] bit NULL,
                    [FailedLoginAttempts] int NULL,
                    [LastLockoutDate] datetime NULL,
                    [Source] nvarchar(100) NULL,
                    [UDF01] nvarchar(500) NULL,
                    [UDF02] nvarchar(500) NULL,
                    [UDF03] nvarchar(500) NULL,
                    [UDF04] nvarchar(500) NULL,
                    [UDF05] nvarchar(500) NULL,
                    [UDF06] nvarchar(500) NULL,
                    [UDF07] nvarchar(500) NULL,
                    [UDF08] nvarchar(500) NULL,
                    [UDF09] nvarchar(500) NULL,
                    [UDF10] nvarchar(500) NULL,
                    CONSTRAINT [PK_Users] PRIMARY KEY ([UserId]),
                    CONSTRAINT [FK_Users_Departments] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([DepartmentId]),
                    CONSTRAINT [FK_Users_Tenants] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([TenantId])
                )
            END
            """);

        m1.Add(
            """
            IF OBJECT_ID(N'[FileStorage]') IS NULL
            BEGIN
                CREATE TABLE [FileStorage] (
                    [FileId] uniqueidentifier NOT NULL,
                    [ItemId] uniqueidentifier NULL,
                    [FileName] nvarchar(255) NULL,
                    [Extension] nvarchar(15) NULL,
                    [Bytes] bigint NULL,
                    [Value] varbinary(max) NULL,
                    [UploadDate] datetime NULL,
                    [UserId] uniqueidentifier NULL,
                    [SourceFileId] nvarchar(100) NULL,
                    [TenantId] uniqueidentifier NULL,
                    CONSTRAINT [PK_FileStorage] PRIMARY KEY ([FileId]),
                    CONSTRAINT [FK_FileStorage_Users] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId])
                )
            END
            """);

        m1.Add(
            """
            IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_NAME='FK_FileStorage_Users')
            BEGIN
            	ALTER TABLE [FileStorage] DROP CONSTRAINT FK_FileStorage_Users
            END

            IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_NAME='IX_FileStorage_UserId')
            BEGIN
            	ALTER TABLE[dbo].[FileStorage]  WITH CHECK ADD CONSTRAINT[IX_FileStorage_UserId] FOREIGN KEY([UserId])
                REFERENCES[dbo].[Users]([UserId])
                ALTER TABLE[dbo].[FileStorage] CHECK CONSTRAINT[IX_FileStorage_UserId]
            END
            """);

        m1.Add(
            """
            IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_NAME='FK_Users_Departments')
            BEGIN
            	ALTER TABLE [Users] DROP CONSTRAINT FK_Users_Departments
            END

            IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_NAME='IX_Users_DepartmentId')
            BEGIN
                ALTER TABLE[dbo].[Users] WITH CHECK ADD CONSTRAINT[IX_Users_DepartmentId] FOREIGN KEY([DepartmentId])
                REFERENCES[dbo].[Departments]([DepartmentId])
                ALTER TABLE[dbo].[Users] CHECK CONSTRAINT[IX_Users_DepartmentId]
            END
            """);

        m1.Add(
            """
            IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_NAME='FK_Users_Tenants')
            BEGIN
            	ALTER TABLE [Users] DROP CONSTRAINT FK_Users_Tenants
            END

            IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_NAME='IX_Users_TenantId')
            BEGIN
                ALTER TABLE [dbo].[Users]  WITH CHECK ADD  CONSTRAINT [IX_Users_TenantId] FOREIGN KEY([TenantId])
                REFERENCES [dbo].[Tenants] ([TenantId])
                ALTER TABLE [dbo].[Users] CHECK CONSTRAINT [IX_Users_TenantId]
            END
            """);

        m1.Add(
            """
            IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NOT NULL
            BEGIN
            	IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId]=N'20230204160527_001')
            	BEGIN
            		INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
            		VALUES (N'20230204160527_001', N'7.0.2')
            	END
            END
            """);

        output.Add(new DataObjects.DataMigration {
            MigrationId = "20230204160527_001",
            Migration = m1
        });










        List<string> m2 = new List<string>();

        // Make sure the Source field exists in the Users table.
        m2.Add(
            """
            IF NOT EXISTS (SELECT TOP 1 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE [TABLE_NAME] = 'Users' AND [COLUMN_NAME] = 'Source') BEGIN
            	ALTER TABLE [Users] ADD [Source] nvarchar(100) NULL
            END
            """);

        m2.Add(
            """
            IF OBJECT_ID(N'[UserGroups]') IS NULL
            BEGIN
                CREATE TABLE [UserGroups] (
                    [GroupId] uniqueidentifier NOT NULL,
                    [TenantId] uniqueidentifier NOT NULL,
                    [Name] nvarchar(100) NULL,
                    [Enabled] bit NOT NULL,
                    [Settings] nvarchar(max) NULL,
                    CONSTRAINT [PK_UserGroups] PRIMARY KEY ([GroupId])
                )
            END
            """);

        m2.Add(
            """
            IF OBJECT_ID(N'[UserInGroups]') IS NULL
            BEGIN
                CREATE TABLE [UserInGroups] (
                    [UserInGroupId] uniqueidentifier NOT NULL,
                    [UserId] uniqueidentifier NOT NULL,
                    [TenantId] uniqueidentifier NOT NULL,
                    [GroupId] uniqueidentifier NOT NULL,
                    CONSTRAINT [PK_UserInGroups] PRIMARY KEY ([UserInGroupId]),
                    CONSTRAINT [FK_UserInGroups_UserGroups] FOREIGN KEY ([GroupId]) REFERENCES [UserGroups] ([GroupId]),
                    CONSTRAINT [FK_UserInGroups_Users] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId])
                )
            END
            """);

        m2.Add(
            """
            IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NOT NULL
            BEGIN
            	IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId]=N'20230227163529_002')
            	BEGIN
            		INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
            		VALUES (N'20230227163529_002', N'7.0.2')
            	END
            END
            """);

        output.Add(new DataObjects.DataMigration {
            MigrationId = "20230227163529_002",
            Migration = m2
        });

        return output;
    }
}