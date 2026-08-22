USE [ScrapWebsiteLocal];
GO

SET NOCOUNT ON;

DECLARE @Settings TABLE
(
    SettingKey nvarchar(120) NOT NULL PRIMARY KEY,
    SettingValue nvarchar(max) NOT NULL,
    SettingGroup nvarchar(50) NOT NULL,
    Description nvarchar(255) NULL
);

INSERT INTO @Settings (SettingKey, SettingValue, SettingGroup, Description)
VALUES
    (N'brand.default_hero_image', N'/assets/images/imported/brand/banner-1.jpg', N'brand', N'Fallback hero image for public template pages'),
    (N'brand.default_cta_image', N'/assets/images/imported/brand/banner-3.jpg', N'brand', N'Fallback final CTA image for public template pages'),
    (N'public.contact.map_image', N'/assets/images/imported/brand/banner-1.jpg', N'public', N'Fallback contact/location map visual'),
    (N'public.image.truck', N'/assets/images/imported/brand/banner-2.jpg', N'public', N'Fallback truck image'),
    (N'public.image.yard', N'/assets/images/imported/brand/banner-1.jpg', N'public', N'Fallback yard/warehouse image'),
    (N'public.image.team', N'/assets/images/imported/brand/banner-3.jpg', N'public', N'Fallback team image'),
    (N'public.image.scale', N'/assets/images/imported/brand/banner-3.jpg', N'public', N'Fallback scale/price image'),
    (N'public.image.scrap', N'/assets/images/imported/products/thumuasatvuncongtrinh8.jpg', N'public', N'Fallback scrap material image'),
    (N'public.image.project', N'/assets/images/imported/products/thumuamaymoccuthanhly1.jpg', N'public', N'Fallback project image'),
    (N'public.image.news', N'/assets/images/imported/brand/seo-og-image.png', N'public', N'Fallback news image'),
    (N'home.response_time_text', N'30 phút', N'home', N'Public response time text'),
    (N'home.price_updated_text', CONVERT(nvarchar(10), GETDATE(), 103), N'home', N'Public price updated text'),
    (N'public.about.hero_title', N'Giới thiệu Phế Liệu Thành Trung', N'public', N'About page hero title'),
    (N'public.capability.hero_title', N'Năng lực thu mua phế liệu', N'public', N'Capability page hero title'),
    (N'public.services.hero_title', N'Dịch vụ thu mua phế liệu', N'public', N'Services page hero title'),
    (N'public.prices.hero_title', N'Bảng giá phế liệu hôm nay', N'public', N'Prices page hero title'),
    (N'public.locations.hero_title', N'Khu vực thu mua phế liệu tận nơi', N'public', N'Locations page hero title'),
    (N'public.projects.hero_title', N'Dự án thu mua gần đây', N'public', N'Projects page hero title'),
    (N'public.referral.hero_title', N'Hoa hồng giới thiệu siêu hấp dẫn', N'public', N'Referral page hero title'),
    (N'public.search.hero_title', N'Tìm kiếm phế liệu và tin tức', N'public', N'Search page hero title');
INSERT INTO @Settings (SettingKey, SettingValue, SettingGroup, Description)
VALUES
    (N'public.metric.generic', N'20', N'public', N'Fallback numeric value for old template metric placeholders'),
    (N'public.search.result_count', N'12', N'public', N'Fallback search result count for old template placeholder');

MERGE dbo.SiteSettings AS target
USING @Settings AS source
ON target.SettingKey = source.SettingKey
WHEN MATCHED AND (
       target.SettingValue IS NULL
    OR LTRIM(RTRIM(target.SettingValue)) = N''
    OR target.SettingValue LIKE N'%[[]HOTLINE]%'
    OR target.SettingValue LIKE N'%[[]ZALO]%'
    OR target.SettingValue LIKE N'%[[]EMAIL]%'
    OR target.SettingValue LIKE N'%[[]ĐỊA CHỈ]%'
    OR target.SettingValue LIKE N'%[[]DD/MM/YYYY]%'
    OR target.SettingValue LIKE N'%C:\%'
    OR target.SettingValue LIKE N'%D:\%'
    OR target.SettingValue LIKE N'%../%'
    OR target.SettingValue LIKE N'%~/%'
    OR target.SettingValue LIKE N'%Ã%'
    OR target.SettingValue LIKE N'%á»%'
    OR target.SettingValue LIKE N'%Ä%'
)
THEN UPDATE SET
    SettingValue = source.SettingValue,
    SettingGroup = source.SettingGroup,
    Description = source.Description,
    UpdatedAt = SYSUTCDATETIME()
WHEN NOT MATCHED THEN
    INSERT (SettingKey, SettingValue, SettingGroup, Description, UpdatedAt)
    VALUES (source.SettingKey, source.SettingValue, source.SettingGroup, source.Description, SYSUTCDATETIME());

UPDATE dbo.SiteSettings
SET SettingValue = REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(SettingValue,
    N'[HOTLINE]', COALESCE((SELECT TOP (1) SettingValue FROM dbo.SiteSettings WHERE SettingKey = N'contact.phone'), N'0974640626')),
    N'[ZALO]', COALESCE((SELECT TOP (1) SettingValue FROM dbo.SiteSettings WHERE SettingKey = N'contact.zalo'), N'0974640626')),
    N'[EMAIL]', COALESCE((SELECT TOP (1) SettingValue FROM dbo.SiteSettings WHERE SettingKey = N'contact.email'), N'phelieuthanhtrung@gmail.com')),
    N'[ĐỊA CHỈ]', COALESCE((SELECT TOP (1) SettingValue FROM dbo.SiteSettings WHERE SettingKey = N'contact.warehouse_address'), N'TP.HCM')),
    N'[DD/MM/YYYY]', CONVERT(nvarchar(10), GETDATE(), 103)),
    N'[XX]', N'10'),
    UpdatedAt = SYSUTCDATETIME()
WHERE SettingValue LIKE N'%[[]HOTLINE]%'
   OR SettingValue LIKE N'%[[]ZALO]%'
   OR SettingValue LIKE N'%[[]EMAIL]%'
   OR SettingValue LIKE N'%[[]ĐỊA CHỈ]%'
   OR SettingValue LIKE N'%[[]DD/MM/YYYY]%'
   OR SettingValue LIKE N'%[[]XX]%';

UPDATE dbo.SiteSettings
SET SettingValue = N'30 ph' + NCHAR(250) + N't',
    UpdatedAt = SYSUTCDATETIME()
WHERE SettingKey = N'home.response_time_text'
  AND SettingValue <> N'30 ph' + NCHAR(250) + N't';

SELECT COUNT(*) AS BadPublicSettingPaths
FROM dbo.SiteSettings
WHERE (SettingKey LIKE N'public.%' OR SettingKey LIKE N'brand.%' OR SettingKey LIKE N'home.%')
  AND (
        SettingValue LIKE N'%C:\%'
     OR SettingValue LIKE N'%D:\%'
     OR SettingValue LIKE N'%../%'
     OR SettingValue LIKE N'%~/%'
  );

SELECT COUNT(*) AS PublicSettingPlaceholders
FROM dbo.SiteSettings
WHERE (SettingKey LIKE N'public.%' OR SettingKey LIKE N'brand.%' OR SettingKey LIKE N'home.%')
  AND (
        SettingValue LIKE N'%[[]HOTLINE]%'
     OR SettingValue LIKE N'%[[]ZALO]%'
     OR SettingValue LIKE N'%[[]EMAIL]%'
     OR SettingValue LIKE N'%[[]ĐỊA CHỈ]%'
     OR SettingValue LIKE N'%[[]DD/MM/YYYY]%'
  );
GO
