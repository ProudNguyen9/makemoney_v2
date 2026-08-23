SET NOCOUNT ON;
GO

IF OBJECT_ID(N'dbo.PostProductLinks', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PostProductLinks (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PostProductLinks PRIMARY KEY,
        PostId INT NOT NULL,
        ScrapItemId INT NOT NULL,
        SortOrder INT NOT NULL CONSTRAINT DF_PostProductLinks_SortOrder DEFAULT 0,
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_PostProductLinks_CreatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_PostProductLinks_Posts FOREIGN KEY (PostId) REFERENCES dbo.Posts(Id) ON DELETE CASCADE,
        CONSTRAINT FK_PostProductLinks_ScrapItems FOREIGN KEY (ScrapItemId) REFERENCES dbo.ScrapItems(Id)
    );
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'UX_PostProductLinks_Post_ScrapItem'
      AND object_id = OBJECT_ID(N'dbo.PostProductLinks')
)
BEGIN
    CREATE UNIQUE INDEX UX_PostProductLinks_Post_ScrapItem
    ON dbo.PostProductLinks(PostId, ScrapItemId);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_PostProductLinks_PostId_SortOrder'
      AND object_id = OBJECT_ID(N'dbo.PostProductLinks')
)
BEGIN
    CREATE INDEX IX_PostProductLinks_PostId_SortOrder
    ON dbo.PostProductLinks(PostId, SortOrder, Id);
END;
GO
