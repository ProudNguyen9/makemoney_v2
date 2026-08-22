USE [ScrapWebsiteLocal];
GO

SET NOCOUNT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET CONCAT_NULL_YIELDS_NULL ON;
GO

MERGE dbo.MediaFiles AS target
USING (VALUES
    (N'Logo chính', N'/assets/images/imported/brand/logo.png', N'brand', N'image/png', N'Logo chính'),
    (N'Logo footer', N'/assets/images/imported/brand/logo-footer.png', N'brand', N'image/png', N'Logo footer'),
    (N'Favicon', N'/assets/images/imported/brand/favicon.png', N'brand', N'image/png', N'Favicon'),
    (N'Apple touch icon', N'/assets/images/imported/brand/apple-touch-icon.png', N'brand', N'image/png', N'Apple touch icon'),
    (N'Avatar thương hiệu', N'/assets/images/imported/brand/avatar.png', N'brand', N'image/png', N'Avatar thương hiệu'),
    (N'Ảnh SEO mặc định', N'/assets/images/imported/brand/seo-og-image.png', N'brand', N'image/png', N'Ảnh SEO mặc định'),
    (N'Banner thương hiệu 1', N'/assets/images/imported/brand/banner-1.jpg', N'brand', N'image/jpeg', N'Banner thương hiệu 1'),
    (N'Banner thương hiệu 2', N'/assets/images/imported/brand/banner-2.jpg', N'brand', N'image/jpeg', N'Banner thương hiệu 2'),
    (N'Banner thương hiệu 3', N'/assets/images/imported/brand/banner-3.jpg', N'brand', N'image/jpeg', N'Banner thương hiệu 3')
) AS source(FileName, Url, Folder, MimeType, AltText)
ON target.Url = source.Url
WHEN MATCHED THEN
    UPDATE SET
        FileName = source.FileName,
        Folder = source.Folder,
        MimeType = source.MimeType,
        AltText = source.AltText,
        Status = N'active'
WHEN NOT MATCHED THEN
    INSERT (FileName, Url, Folder, MimeType, AltText, Status)
    VALUES (source.FileName, source.Url, source.Folder, source.MimeType, source.AltText, N'active');

MERGE dbo.SiteSettings AS target
USING (VALUES
    (N'brand.logo', N'/assets/images/imported/brand/logo.png', N'brand', N'Logo chính'),
    (N'brand.logo_footer', N'/assets/images/imported/brand/logo-footer.png', N'brand', N'Logo footer'),
    (N'brand.favicon', N'/assets/images/imported/brand/favicon.png', N'brand', N'Favicon'),
    (N'brand.apple_touch_icon', N'/assets/images/imported/brand/apple-touch-icon.png', N'brand', N'Apple touch icon'),
    (N'brand.avatar', N'/assets/images/imported/brand/avatar.png', N'brand', N'Avatar thương hiệu'),
    (N'seo.og_image', N'/assets/images/imported/brand/seo-og-image.png', N'seo', N'Ảnh SEO mặc định'),
    (N'brand.banner_1', N'/assets/images/imported/brand/banner-1.jpg', N'brand', N'Banner thương hiệu 1'),
    (N'brand.banner_2', N'/assets/images/imported/brand/banner-2.jpg', N'brand', N'Banner thương hiệu 2'),
    (N'brand.banner_3', N'/assets/images/imported/brand/banner-3.jpg', N'brand', N'Banner thương hiệu 3')
) AS source(SettingKey, SettingValue, SettingGroup, Description)
ON target.SettingKey = source.SettingKey
WHEN MATCHED THEN
    UPDATE SET
        SettingValue = source.SettingValue,
        SettingGroup = source.SettingGroup,
        Description = source.Description,
        UpdatedAt = SYSUTCDATETIME()
WHEN NOT MATCHED THEN
    INSERT (SettingKey, SettingValue, SettingGroup, Description, UpdatedAt)
    VALUES (source.SettingKey, source.SettingValue, source.SettingGroup, source.Description, SYSUTCDATETIME());

UPDATE dbo.Banners
SET ImageUrl = N'/assets/images/imported/brand/banner-1.jpg'
WHERE Id = (
    SELECT TOP (1) Id
    FROM dbo.Banners
    WHERE Status = N'active'
    ORDER BY SortOrder ASC, Id ASC
);

UPDATE dbo.SeoMetadata
SET OgImage = N'/assets/images/imported/brand/seo-og-image.png'
WHERE EntityType = N'Page'
  AND (RoutePath = N'/' OR OgImage IS NULL OR OgImage LIKE N'%bannersandseos%');
GO
