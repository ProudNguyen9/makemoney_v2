USE [ScrapWebsiteLocal];
GO

SET NOCOUNT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET CONCAT_NULL_YIELDS_NULL ON;
GO

UPDATE dbo.Banners
SET ImageUrl = N'/assets/images/imported/brand/banner-1.jpg'
WHERE Status = N'active'
  AND (ImageUrl IS NULL OR LTRIM(RTRIM(ImageUrl)) = N'');

UPDATE dbo.ScrapItems
SET PrimaryImage = N'/assets/images/imported/products/thumuadongdo1.jpg'
WHERE Status = N'published'
  AND (PrimaryImage IS NULL OR LTRIM(RTRIM(PrimaryImage)) = N'');

UPDATE dbo.Posts
SET CoverImage = N'/assets/images/imported/blogs/service/thumuamotocutannoiuytin2471.webp'
WHERE Status = N'published'
  AND (CoverImage IS NULL OR LTRIM(RTRIM(CoverImage)) = N'');

UPDATE dbo.SeoMetadata
SET OgImage = N'/assets/images/imported/brand/seo-og-image.png'
WHERE Status = N'active'
  AND (OgImage IS NULL OR LTRIM(RTRIM(OgImage)) = N'');
GO
