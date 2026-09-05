/*
  Phase 4c - SEO keywords cho loại phế liệu, dịch vụ, dự án.
  - Thêm cột SeoKeywords vào dbo.ScrapItems, dbo.Services, dbo.Projects
  - Tương tự dbo.Posts: từ khóa SEO nhập trong form admin,
    hiển thị <meta name="keywords"> ở trang chi tiết công khai.
  Idempotent.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

USE ScrapWebsiteLocal;
GO

IF COL_LENGTH(N'dbo.ScrapItems', N'SeoKeywords') IS NULL
BEGIN
    ALTER TABLE dbo.ScrapItems ADD SeoKeywords NVARCHAR(255) NULL;
END
GO

IF COL_LENGTH(N'dbo.Services', N'SeoKeywords') IS NULL
BEGIN
    ALTER TABLE dbo.Services ADD SeoKeywords NVARCHAR(255) NULL;
END
GO

IF COL_LENGTH(N'dbo.Projects', N'SeoKeywords') IS NULL
BEGIN
    ALTER TABLE dbo.Projects ADD SeoKeywords NVARCHAR(255) NULL;
END
GO

IF COL_LENGTH(N'dbo.ScrapCategories', N'SeoKeywords') IS NULL
BEGIN
    ALTER TABLE dbo.ScrapCategories ADD SeoKeywords NVARCHAR(255) NULL;
END
GO

PRINT N'Phase 4c (seo_keywords_scrap_services_projects) hoan tat.';
GO
