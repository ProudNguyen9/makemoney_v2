USE [ScrapWebsiteLocal];
GO

SET NOCOUNT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET CONCAT_NULL_YIELDS_NULL ON;
GO

MERGE dbo.SiteSettings AS target
USING (VALUES
    (N'news.hero_image', N'/assets/images/imported/brand/seo-og-image.png', N'news', N'Ảnh hero trang tin tức'),
    (N'news.hero_title', N'Tin tức & kiến thức phế liệu', N'news', N'Tiêu đề hero trang tin tức'),
    (N'news.hero_description', N'Cập nhật giá phế liệu, kinh nghiệm thanh lý và thông tin thu mua mới nhất.', N'news', N'Mô tả hero trang tin tức')
) AS source(SettingKey, SettingValue, SettingGroup, Description)
ON target.SettingKey = source.SettingKey
WHEN MATCHED THEN
    UPDATE SET
        SettingValue = CASE
            WHEN target.SettingValue IS NULL OR LTRIM(RTRIM(target.SettingValue)) = N'' THEN source.SettingValue
            ELSE target.SettingValue
        END,
        SettingGroup = source.SettingGroup,
        Description = source.Description,
        UpdatedAt = SYSUTCDATETIME()
WHEN NOT MATCHED THEN
    INSERT (SettingKey, SettingValue, SettingGroup, Description, UpdatedAt)
    VALUES (source.SettingKey, source.SettingValue, source.SettingGroup, source.Description, SYSUTCDATETIME());

UPDATE dbo.Posts
SET CoverImage = N'/assets/images/imported/brand/seo-og-image.png'
WHERE Status = N'published'
  AND (CoverImage IS NULL OR LTRIM(RTRIM(CoverImage)) = N'');
GO
