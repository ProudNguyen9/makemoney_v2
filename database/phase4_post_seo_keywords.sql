/*
  Phase 4 - SEO keywords cho bài viết + hỗ trợ bản nháp.
  - Thêm cột SeoKeywords vào dbo.Posts: từ khóa SEO nhập trong form admin,
    hiển thị <meta name="keywords"> ở trang chi tiết bài viết.
  - Bản nháp dùng sẵn cột Status = N'draft' nên không cần thay đổi cấu trúc.
  Idempotent.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

USE ScrapWebsiteLocal;
GO

IF COL_LENGTH(N'dbo.Posts', N'SeoKeywords') IS NULL
BEGIN
    ALTER TABLE dbo.Posts ADD SeoKeywords NVARCHAR(255) NULL;
END
GO

PRINT N'Phase 4 (post_seo_keywords) hoan tat.';
GO
