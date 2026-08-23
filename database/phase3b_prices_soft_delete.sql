/*
  Phase 3b — Soft delete for price rows.
  Adds DeletedAt to dbo.ScrapPrices so admin deletions are recoverable.
  Idempotent.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

USE ScrapWebsiteLocal;
GO

IF COL_LENGTH(N'dbo.ScrapPrices', N'DeletedAt') IS NULL
BEGIN
    ALTER TABLE dbo.ScrapPrices ADD DeletedAt DATETIME2(0) NULL;
END
GO
