USE [ScrapWebsiteLocal];
GO

SET NOCOUNT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;

IF OBJECT_ID(N'dbo.AdminUsers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AdminUsers (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AdminUsers PRIMARY KEY,
        UserName NVARCHAR(80) NOT NULL,
        Email NVARCHAR(255) NOT NULL,
        DisplayName NVARCHAR(160) NOT NULL,
        [Role] NVARCHAR(50) NOT NULL CONSTRAINT DF_AdminUsers_Role DEFAULT N'Admin',
        PasswordHash NVARCHAR(500) NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_AdminUsers_IsActive DEFAULT 1,
        [Status] NVARCHAR(30) NOT NULL CONSTRAINT DF_AdminUsers_Status DEFAULT N'active',
        LastLoginAt DATETIME2(0) NULL,
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_AdminUsers_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2(0) NULL,
        DeletedAt DATETIME2(0) NULL
    );

    CREATE UNIQUE INDEX UX_AdminUsers_Email ON dbo.AdminUsers(Email);
    CREATE UNIQUE INDEX UX_AdminUsers_UserName ON dbo.AdminUsers(UserName);
END;
GO

IF COL_LENGTH(N'dbo.AdminUsers', N'UserName') IS NULL
    ALTER TABLE dbo.AdminUsers ADD UserName NVARCHAR(80) NULL;

IF COL_LENGTH(N'dbo.AdminUsers', N'Role') IS NULL
    ALTER TABLE dbo.AdminUsers ADD [Role] NVARCHAR(50) NOT NULL CONSTRAINT DF_AdminUsers_Role DEFAULT N'Admin';

IF COL_LENGTH(N'dbo.AdminUsers', N'IsActive') IS NULL
    ALTER TABLE dbo.AdminUsers ADD IsActive BIT NOT NULL CONSTRAINT DF_AdminUsers_IsActive DEFAULT 1;

IF COL_LENGTH(N'dbo.AdminUsers', N'Status') IS NULL
    ALTER TABLE dbo.AdminUsers ADD [Status] NVARCHAR(30) NOT NULL CONSTRAINT DF_AdminUsers_Status DEFAULT N'active';

IF COL_LENGTH(N'dbo.AdminUsers', N'UpdatedAt') IS NULL
    ALTER TABLE dbo.AdminUsers ADD UpdatedAt DATETIME2(0) NULL;

IF COL_LENGTH(N'dbo.AdminUsers', N'DeletedAt') IS NULL
    ALTER TABLE dbo.AdminUsers ADD DeletedAt DATETIME2(0) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_AdminUsers_Email' AND object_id = OBJECT_ID(N'dbo.AdminUsers'))
    CREATE UNIQUE INDEX UX_AdminUsers_Email ON dbo.AdminUsers(Email);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_AdminUsers_UserName' AND object_id = OBJECT_ID(N'dbo.AdminUsers'))
    EXEC(N'CREATE UNIQUE INDEX UX_AdminUsers_UserName ON dbo.AdminUsers(UserName) WHERE UserName IS NOT NULL');
GO

DECLARE @Admins TABLE (
    UserName NVARCHAR(80) NOT NULL,
    Email NVARCHAR(256) NOT NULL,
    DisplayName NVARCHAR(160) NOT NULL,
    [Role] NVARCHAR(50) NOT NULL,
    PasswordHash NVARCHAR(500) NOT NULL
);

INSERT INTO @Admins (UserName, Email, DisplayName, [Role], PasswordHash)
VALUES
(N'admin', N'admin@phelieuthanhtrung.vn', N'Quản trị hệ thống', N'Admin', N'v1:100000:cNdSZb5WpBYHl1lxUtWvYg==:aStQOQvyuiArFpusnVWlq0jTInJgFUA6oLPYEQqjIPs='),
(N'editor', N'editor@phelieuthanhtrung.vn', N'Biên tập nội dung', N'Editor', N'v1:100000:UNx7LpOYZVqd+3G9B8eosA==:TP7494bnZ8EORvr2r+8z7y2cIuGgPJdSLCUNbYE5IwE='),
(N'sale', N'sale@phelieuthanhtrung.vn', N'Nhân viên báo giá', N'Sales', N'v1:100000:UxD+Eu3bKOF8qPsZDZDxsA==:K3P88S3lGna502B8fr4afBzzSVaCRnrpRLDzqnFl5oM=');

MERGE dbo.AdminUsers AS target
USING @Admins AS source
ON target.Email = source.Email
WHEN MATCHED THEN
    UPDATE SET
        UserName = source.UserName,
        DisplayName = source.DisplayName,
        [Role] = source.[Role],
        PasswordHash = source.PasswordHash,
        IsActive = 1,
        [Status] = N'active',
        DeletedAt = NULL,
        UpdatedAt = SYSUTCDATETIME()
WHEN NOT MATCHED THEN
    INSERT (UserName, Email, DisplayName, [Role], PasswordHash, IsActive, [Status], CreatedAt)
    VALUES (source.UserName, source.Email, source.DisplayName, source.[Role], source.PasswordHash, 1, N'active', SYSUTCDATETIME());
GO
