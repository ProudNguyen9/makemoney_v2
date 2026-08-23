/*
  Phase 3d - Soft delete for posts.
  Adds DeletedAt to dbo.Posts so admin article deletions are recoverable.
  Idempotent.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

USE ScrapWebsiteLocal;
GO

IF COL_LENGTH(N'dbo.Posts', N'DeletedAt') IS NULL
BEGIN
    ALTER TABLE dbo.Posts ADD DeletedAt DATETIME2(0) NULL;
END
GO

IF EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'UX_Posts_Slug'
      AND object_id = OBJECT_ID(N'dbo.Posts')
      AND filter_definition IS NULL
)
BEGIN
    DROP INDEX UX_Posts_Slug ON dbo.Posts;
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'UX_Posts_Slug'
      AND object_id = OBJECT_ID(N'dbo.Posts')
)
BEGIN
    CREATE UNIQUE INDEX UX_Posts_Slug ON dbo.Posts(Slug) WHERE DeletedAt IS NULL;
END
GO
