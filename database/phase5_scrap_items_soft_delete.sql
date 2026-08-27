/*
  Phase 5 - Soft delete for scrap items.
  Adds DeletedAt to dbo.ScrapItems so admin scrap deletions are recoverable
  (SCR-007: trước đây xóa phế liệu là HARD DELETE).
  Idempotent — chạy lại không gây lỗi.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

USE ScrapWebsiteLocal;
GO

IF COL_LENGTH(N'dbo.ScrapItems', N'DeletedAt') IS NULL
BEGIN
    ALTER TABLE dbo.ScrapItems ADD DeletedAt DATETIME2(0) NULL;
END
GO

PRINT N'Phase 5 hoàn tất: dbo.ScrapItems.DeletedAt sẵn sàng (xóa mềm phế liệu).';
GO
