USE [ScrapWebsiteLocal];
GO

IF OBJECT_ID(N'dbo.ContactRequests', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ContactRequests (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ContactRequests PRIMARY KEY,
        [Name] NVARCHAR(120) NULL,
        Phone NVARCHAR(30) NOT NULL,
        Email NVARCHAR(255) NULL,
        Zalo NVARCHAR(80) NULL,
        ScrapType NVARCHAR(180) NULL,
        QuantityText NVARCHAR(160) NULL,
        Area NVARCHAR(160) NULL,
        [Message] NVARCHAR(MAX) NULL,
        SourceForm NVARCHAR(80) NOT NULL CONSTRAINT DF_ContactRequests_SourceForm DEFAULT N'quick_quote',
        SourceUrl NVARCHAR(500) NULL,
        [Status] NVARCHAR(30) NOT NULL CONSTRAINT DF_ContactRequests_Status DEFAULT N'new',
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_ContactRequests_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2(0) NULL,
        DeletedAt DATETIME2(0) NULL
    );
END;
GO

IF COL_LENGTH('dbo.ContactRequests', 'Zalo') IS NULL ALTER TABLE dbo.ContactRequests ADD Zalo NVARCHAR(80) NULL;
IF COL_LENGTH('dbo.ContactRequests', 'ScrapType') IS NULL ALTER TABLE dbo.ContactRequests ADD ScrapType NVARCHAR(180) NULL;
IF COL_LENGTH('dbo.ContactRequests', 'QuantityText') IS NULL ALTER TABLE dbo.ContactRequests ADD QuantityText NVARCHAR(160) NULL;
IF COL_LENGTH('dbo.ContactRequests', 'SourceForm') IS NULL ALTER TABLE dbo.ContactRequests ADD SourceForm NVARCHAR(80) NOT NULL CONSTRAINT DF_ContactRequests_SourceForm DEFAULT N'quick_quote';
IF COL_LENGTH('dbo.ContactRequests', 'SourceUrl') IS NULL ALTER TABLE dbo.ContactRequests ADD SourceUrl NVARCHAR(500) NULL;
IF COL_LENGTH('dbo.ContactRequests', 'Status') IS NULL ALTER TABLE dbo.ContactRequests ADD [Status] NVARCHAR(30) NOT NULL CONSTRAINT DF_ContactRequests_Status DEFAULT N'new';
IF COL_LENGTH('dbo.ContactRequests', 'UpdatedAt') IS NULL ALTER TABLE dbo.ContactRequests ADD UpdatedAt DATETIME2(0) NULL;
IF COL_LENGTH('dbo.ContactRequests', 'DeletedAt') IS NULL ALTER TABLE dbo.ContactRequests ADD DeletedAt DATETIME2(0) NULL;
GO

ALTER TABLE dbo.ContactRequests ALTER COLUMN [Name] NVARCHAR(120) NULL;
ALTER TABLE dbo.ContactRequests ALTER COLUMN Phone NVARCHAR(30) NOT NULL;
ALTER TABLE dbo.ContactRequests ALTER COLUMN Email NVARCHAR(255) NULL;
ALTER TABLE dbo.ContactRequests ALTER COLUMN Area NVARCHAR(160) NULL;
GO

UPDATE dbo.ContactRequests
SET [Status] = N'new'
WHERE [Status] IS NULL OR LTRIM(RTRIM([Status])) = N'';
GO

IF OBJECT_ID(N'dbo.ContactRequestFiles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ContactRequestFiles (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ContactRequestFiles PRIMARY KEY,
        ContactRequestId INT NOT NULL,
        MediaFileId INT NULL,
        FileUrl NVARCHAR(500) NOT NULL,
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_ContactRequestFiles_CreatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_ContactRequestFiles_ContactRequests FOREIGN KEY (ContactRequestId) REFERENCES dbo.ContactRequests(Id) ON DELETE CASCADE,
        CONSTRAINT FK_ContactRequestFiles_MediaFiles FOREIGN KEY (MediaFileId) REFERENCES dbo.MediaFiles(Id)
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ContactRequests_StatusCreated' AND object_id = OBJECT_ID(N'dbo.ContactRequests'))
    CREATE INDEX IX_ContactRequests_StatusCreated ON dbo.ContactRequests([Status], CreatedAt DESC);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ContactRequests_Phone' AND object_id = OBJECT_ID(N'dbo.ContactRequests'))
    CREATE INDEX IX_ContactRequests_Phone ON dbo.ContactRequests(Phone);
GO
