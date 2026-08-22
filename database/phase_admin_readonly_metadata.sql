USE [ScrapWebsiteLocal];
GO

SET NOCOUNT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;

IF OBJECT_ID(N'dbo.ContactRequests', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ContactRequests (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ContactRequests PRIMARY KEY,
        [Name] NVARCHAR(100) NOT NULL,
        Phone NVARCHAR(20) NOT NULL,
        Email NVARCHAR(100) NULL,
        Area NVARCHAR(100) NULL,
        [Message] NVARCHAR(MAX) NULL,
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_ContactRequests_CreatedAt DEFAULT SYSUTCDATETIME()
    );

    CREATE INDEX IX_ContactRequests_CreatedAt ON dbo.ContactRequests(CreatedAt DESC);
END;

DECLARE @Settings TABLE (
    SettingKey NVARCHAR(120) NOT NULL,
    SettingValue NVARCHAR(MAX) NOT NULL,
    SettingGroup NVARCHAR(50) NOT NULL,
    [Description] NVARCHAR(255) NULL
);

INSERT INTO @Settings (SettingKey, SettingValue, SettingGroup, [Description])
VALUES
(N'site.name', N'Phế Liệu Thành Trung', N'site', N'Tên thương hiệu hiển thị trong admin và public'),
(N'company.tax_code', N'Đang cập nhật', N'company', N'Mã số thuế'),
(N'contact.phone', N'0974640626', N'contact', N'Hotline chính'),
(N'contact.zalo', N'0974640626', N'contact', N'Số Zalo'),
(N'contact.email', N'phelieuthanhtrung@gmail.com', N'contact', N'Email liên hệ'),
(N'contact.address', N'TP.HCM, Bình Dương, Đồng Nai', N'contact', N'Địa chỉ/khu vực hiển thị'),
(N'contact.working_hours', N'T2-CN: 7:00 - 20:00', N'contact', N'Giờ làm việc'),
(N'site.logo', N'/assets/images/imported/brand/logo.png', N'site', N'Logo header'),
(N'site.footer_logo', N'/assets/images/imported/brand/logo-footer.png', N'site', N'Logo footer'),
(N'site.favicon', N'/favicon.ico', N'site', N'Favicon'),
(N'site.default_og_image', N'/assets/images/imported/brand/banner-1.jpg', N'seo', N'OG image mặc định'),
(N'seo.site_title', N'Thu mua phế liệu giá cao tận nơi', N'seo', N'Site title mặc định'),
(N'seo.default_description', N'Thu mua phế liệu tận nơi giá cao, cân minh bạch, thanh toán nhanh tại TP.HCM, Bình Dương, Đồng Nai.', N'seo', N'Meta description mặc định'),
(N'seo.default_og_image', N'/assets/images/imported/brand/banner-1.jpg', N'seo', N'OG image mặc định'),
(N'system.cache_minutes', N'5', N'system', N'Thời gian cache public settings');

MERGE dbo.SiteSettings AS target
USING @Settings AS source
ON target.SettingKey = source.SettingKey
WHEN MATCHED AND (target.SettingValue IS NULL OR LTRIM(RTRIM(target.SettingValue)) = N'') THEN
    UPDATE SET
        SettingValue = source.SettingValue,
        SettingGroup = source.SettingGroup,
        [Description] = source.[Description],
        UpdatedAt = SYSUTCDATETIME()
WHEN NOT MATCHED THEN
    INSERT (SettingKey, SettingValue, SettingGroup, [Description], UpdatedAt)
    VALUES (source.SettingKey, source.SettingValue, source.SettingGroup, source.[Description], SYSUTCDATETIME());

DECLARE @SeoPages TABLE (
    RoutePath NVARCHAR(255) NOT NULL,
    SeoTitle NVARCHAR(255) NOT NULL,
    MetaDescription NVARCHAR(500) NOT NULL,
    OgImage NVARCHAR(500) NOT NULL
);

INSERT INTO @SeoPages (RoutePath, SeoTitle, MetaDescription, OgImage)
VALUES
(N'/', N'Thu mua phế liệu giá cao tận nơi', N'Thu mua phế liệu tận nơi, cân minh bạch, thanh toán nhanh.', N'/assets/images/imported/brand/banner-1.jpg'),
(N'/phe-lieu', N'Danh mục phế liệu thu mua giá cao', N'Danh sách các loại phế liệu đang thu mua và giá tham khảo mới nhất.', N'/assets/images/imported/products/thumuasatdac1.jpg'),
(N'/tin-tuc', N'Tin tức và bảng giá phế liệu mới nhất', N'Cập nhật giá phế liệu, kinh nghiệm bán phế liệu và kiến thức thu mua.', N'/assets/images/imported/brand/banner-1.jpg'),
(N'/bang-gia', N'Bảng giá phế liệu hôm nay', N'Bảng giá thu mua phế liệu tham khảo, cập nhật theo dữ liệu website.', N'/assets/images/imported/brand/banner-2.jpg'),
(N'/lien-he', N'Liên hệ thu mua phế liệu', N'Liên hệ hotline, Zalo để gửi hình và nhận báo giá phế liệu nhanh.', N'/assets/images/imported/brand/banner-3.jpg'),
(N'/gioi-thieu', N'Giới thiệu Phế Liệu Thành Trung', N'Tìm hiểu năng lực thu mua, vận chuyển và thanh toán phế liệu tận nơi.', N'/assets/images/imported/brand/banner-1.jpg'),
(N'/nang-luc', N'Năng lực thu mua phế liệu', N'Năng lực xe tải, nhân sự, quy trình khảo sát và thu gom phế liệu.', N'/assets/images/imported/brand/banner-2.jpg'),
(N'/dich-vu', N'Dịch vụ thu mua phế liệu', N'Dịch vụ thu mua, tháo dỡ, vận chuyển và thanh lý phế liệu.', N'/assets/images/imported/brand/banner-3.jpg'),
(N'/khu-vuc', N'Khu vực thu mua phế liệu', N'Các khu vực phục vụ thu mua phế liệu tại TP.HCM và tỉnh lân cận.', N'/assets/images/imported/brand/banner-1.jpg'),
(N'/du-an', N'Dự án thu mua phế liệu', N'Các dự án thu mua, thanh lý nhà xưởng và công trình đã thực hiện.', N'/assets/images/imported/brand/banner-2.jpg'),
(N'/hoa-hong', N'Hoa hồng giới thiệu phế liệu', N'Chính sách hoa hồng giới thiệu nguồn phế liệu hấp dẫn, minh bạch.', N'/assets/images/imported/brand/banner-3.jpg');

MERGE dbo.SeoMetadata AS target
USING @SeoPages AS source
ON target.RoutePath = source.RoutePath
WHEN MATCHED AND (
    target.SeoTitle IS NULL OR LTRIM(RTRIM(target.SeoTitle)) = N''
    OR target.MetaDescription IS NULL OR LTRIM(RTRIM(target.MetaDescription)) = N''
    OR target.OgImage IS NULL OR LTRIM(RTRIM(target.OgImage)) = N''
) THEN
    UPDATE SET
        SeoTitle = COALESCE(NULLIF(LTRIM(RTRIM(target.SeoTitle)), N''), source.SeoTitle),
        MetaDescription = COALESCE(NULLIF(LTRIM(RTRIM(target.MetaDescription)), N''), source.MetaDescription),
        CanonicalUrl = COALESCE(NULLIF(LTRIM(RTRIM(target.CanonicalUrl)), N''), source.RoutePath),
        OgTitle = COALESCE(NULLIF(LTRIM(RTRIM(target.OgTitle)), N''), source.SeoTitle),
        OgDescription = COALESCE(NULLIF(LTRIM(RTRIM(target.OgDescription)), N''), source.MetaDescription),
        OgImage = COALESCE(NULLIF(LTRIM(RTRIM(target.OgImage)), N''), source.OgImage),
        OgType = COALESCE(NULLIF(LTRIM(RTRIM(target.OgType)), N''), N'website'),
        Status = COALESCE(NULLIF(LTRIM(RTRIM(target.Status)), N''), N'active')
WHEN NOT MATCHED THEN
    INSERT (EntityType, EntityId, RoutePath, SeoTitle, MetaDescription, Keywords, CanonicalUrl, OgTitle, OgDescription, OgImage, OgType, RobotsIndex, RobotsFollow, Status)
    VALUES (N'Page', NULL, source.RoutePath, source.SeoTitle, source.MetaDescription, NULL, source.RoutePath, source.SeoTitle, source.MetaDescription, source.OgImage, N'website', 1, 1, N'active');

SELECT
    (SELECT COUNT(*) FROM dbo.SiteSettings) AS SiteSettingsCount,
    (SELECT COUNT(*) FROM dbo.SeoMetadata) AS SeoMetadataCount,
    (SELECT COUNT(*) FROM dbo.ContactRequests) AS ContactRequestsCount;
