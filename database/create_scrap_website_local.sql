/*
  Create compact local database for Codezone public website.
  Source database: WebPheLieu
  Target database: ScrapWebsiteLocal
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET CONCAT_NULL_YIELDS_NULL ON;

IF DB_ID(N'ScrapWebsiteLocal') IS NULL
BEGIN
    CREATE DATABASE ScrapWebsiteLocal;
END
GO

USE ScrapWebsiteLocal;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET CONCAT_NULL_YIELDS_NULL ON;

DROP TABLE IF EXISTS dbo.SeoRedirects;
DROP TABLE IF EXISTS dbo.SeoSitemapEntries;
DROP TABLE IF EXISTS dbo.SeoMetadata;
DROP TABLE IF EXISTS dbo.PostImages;
DROP TABLE IF EXISTS dbo.Posts;
DROP TABLE IF EXISTS dbo.PostCategories;
DROP TABLE IF EXISTS dbo.ScrapPriceHistory;
DROP TABLE IF EXISTS dbo.ScrapPrices;
DROP TABLE IF EXISTS dbo.ScrapItemImages;
DROP TABLE IF EXISTS dbo.ScrapItems;
DROP TABLE IF EXISTS dbo.ScrapCategories;
DROP TABLE IF EXISTS dbo.Banners;
DROP TABLE IF EXISTS dbo.MediaFiles;
DROP TABLE IF EXISTS dbo.SiteSettings;
GO

CREATE TABLE dbo.SiteSettings (
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SiteSettings PRIMARY KEY,
    SettingKey NVARCHAR(100) NOT NULL,
    SettingValue NVARCHAR(MAX) NULL,
    SettingGroup NVARCHAR(50) NOT NULL CONSTRAINT DF_SiteSettings_SettingGroup DEFAULT N'general',
    Description NVARCHAR(255) NULL,
    UpdatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_SiteSettings_UpdatedAt DEFAULT SYSUTCDATETIME()
);

CREATE TABLE dbo.MediaFiles (
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_MediaFiles PRIMARY KEY,
    FileName NVARCHAR(255) NOT NULL,
    Url NVARCHAR(500) NOT NULL,
    Folder NVARCHAR(160) NULL,
    MimeType NVARCHAR(120) NULL,
    AltText NVARCHAR(255) NULL,
    Status NVARCHAR(30) NOT NULL CONSTRAINT DF_MediaFiles_Status DEFAULT N'active'
);

CREATE TABLE dbo.Banners (
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Banners PRIMARY KEY,
    Title NVARCHAR(255) NOT NULL,
    Subtitle NVARCHAR(MAX) NULL,
    ImageUrl NVARCHAR(500) NULL,
    PrimaryButtonText NVARCHAR(100) NULL,
    PrimaryButtonUrl NVARCHAR(500) NULL,
    SecondaryButtonText NVARCHAR(100) NULL,
    SecondaryButtonUrl NVARCHAR(500) NULL,
    SortOrder INT NOT NULL CONSTRAINT DF_Banners_SortOrder DEFAULT 0,
    Status NVARCHAR(30) NOT NULL CONSTRAINT DF_Banners_Status DEFAULT N'active'
);

CREATE TABLE dbo.ScrapCategories (
    Id INT NOT NULL CONSTRAINT PK_ScrapCategories PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Slug NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    SortOrder INT NOT NULL CONSTRAINT DF_ScrapCategories_SortOrder DEFAULT 0,
    Status NVARCHAR(30) NOT NULL CONSTRAINT DF_ScrapCategories_Status DEFAULT N'published'
);

CREATE TABLE dbo.ScrapItems (
    Id INT NOT NULL CONSTRAINT PK_ScrapItems PRIMARY KEY,
    ScrapCategoryId INT NULL,
    Name NVARCHAR(255) NOT NULL,
    Slug NVARCHAR(255) NOT NULL,
    ShortDescription NVARCHAR(MAX) NULL,
    Description NVARCHAR(MAX) NULL,
    PrimaryImage NVARCHAR(500) NULL,
    Unit NVARCHAR(50) NULL,
    PriceFrom DECIMAL(18,2) NULL,
    PriceLabel NVARCHAR(255) NULL,
    Status NVARCHAR(30) NOT NULL CONSTRAINT DF_ScrapItems_Status DEFAULT N'published',
    SortOrder INT NOT NULL CONSTRAINT DF_ScrapItems_SortOrder DEFAULT 0,
    IsFeatured BIT NOT NULL CONSTRAINT DF_ScrapItems_IsFeatured DEFAULT 0,
    PublishedAt DATETIME2(0) NULL,
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_ScrapItems_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_ScrapItems_UpdatedAt DEFAULT SYSUTCDATETIME(),
    DeletedAt DATETIME2(0) NULL,
    CONSTRAINT FK_ScrapItems_ScrapCategories FOREIGN KEY (ScrapCategoryId) REFERENCES dbo.ScrapCategories(Id)
);

CREATE TABLE dbo.ScrapItemImages (
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ScrapItemImages PRIMARY KEY,
    ScrapItemId INT NOT NULL,
    ImageUrl NVARCHAR(500) NOT NULL,
    Caption NVARCHAR(255) NULL,
    OrderIndex INT NOT NULL CONSTRAINT DF_ScrapItemImages_OrderIndex DEFAULT 0,
    CONSTRAINT FK_ScrapItemImages_ScrapItems FOREIGN KEY (ScrapItemId) REFERENCES dbo.ScrapItems(Id) ON DELETE CASCADE
);

CREATE TABLE dbo.ScrapPrices (
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ScrapPrices PRIMARY KEY,
    ScrapItemId INT NOT NULL,
    PriceValue DECIMAL(18,2) NULL,
    PriceLabel NVARCHAR(255) NULL,
    Unit NVARCHAR(50) NOT NULL CONSTRAINT DF_ScrapPrices_Unit DEFAULT N'kg',
    EffectiveDate DATE NOT NULL CONSTRAINT DF_ScrapPrices_EffectiveDate DEFAULT CONVERT(date, SYSUTCDATETIME()),
    CONSTRAINT FK_ScrapPrices_ScrapItems FOREIGN KEY (ScrapItemId) REFERENCES dbo.ScrapItems(Id) ON DELETE CASCADE
);

CREATE TABLE dbo.ScrapPriceHistory (
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ScrapPriceHistory PRIMARY KEY,
    ScrapItemId INT NOT NULL,
    PriceValue DECIMAL(18,2) NULL,
    PriceUnit NVARCHAR(50) NULL,
    PriceType NVARCHAR(20) NOT NULL,
    Note NVARCHAR(255) NULL,
    EffectiveDate DATE NOT NULL,
    RecordedAt DATETIME2(0) NOT NULL,
    CONSTRAINT FK_ScrapPriceHistory_ScrapItems FOREIGN KEY (ScrapItemId) REFERENCES dbo.ScrapItems(Id) ON DELETE CASCADE
);

CREATE TABLE dbo.PostCategories (
    Id INT NOT NULL CONSTRAINT PK_PostCategories PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Slug NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    SortOrder INT NOT NULL CONSTRAINT DF_PostCategories_SortOrder DEFAULT 0,
    Status NVARCHAR(30) NOT NULL CONSTRAINT DF_PostCategories_Status DEFAULT N'published'
);

CREATE TABLE dbo.Posts (
    Id INT NOT NULL CONSTRAINT PK_Posts PRIMARY KEY,
    PostCategoryId INT NULL,
    Title NVARCHAR(255) NOT NULL,
    Slug NVARCHAR(255) NOT NULL,
    Excerpt NVARCHAR(MAX) NULL,
    ContentHtml NVARCHAR(MAX) NULL,
    CoverImage NVARCHAR(500) NULL,
    PublishedAt DATETIME2(0) NOT NULL,
    Status NVARCHAR(30) NOT NULL CONSTRAINT DF_Posts_Status DEFAULT N'published',
    SortOrder INT NOT NULL CONSTRAINT DF_Posts_SortOrder DEFAULT 0,
    IsFeatured BIT NOT NULL CONSTRAINT DF_Posts_IsFeatured DEFAULT 0,
    AuthorName NVARCHAR(160) NULL,
    SeoKeywords NVARCHAR(255) NULL,
    CreatedAt DATETIME2(0) NOT NULL,
    UpdatedAt DATETIME2(0) NOT NULL,
    CONSTRAINT FK_Posts_PostCategories FOREIGN KEY (PostCategoryId) REFERENCES dbo.PostCategories(Id)
);

CREATE TABLE dbo.PostImages (
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PostImages PRIMARY KEY,
    PostId INT NOT NULL,
    ImageUrl NVARCHAR(500) NOT NULL,
    Caption NVARCHAR(255) NULL,
    OrderIndex INT NOT NULL CONSTRAINT DF_PostImages_OrderIndex DEFAULT 0,
    CONSTRAINT FK_PostImages_Posts FOREIGN KEY (PostId) REFERENCES dbo.Posts(Id) ON DELETE CASCADE
);

CREATE TABLE dbo.SeoMetadata (
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SeoMetadata PRIMARY KEY,
    EntityType NVARCHAR(60) NOT NULL,
    EntityId INT NULL,
    RoutePath NVARCHAR(500) NULL,
    SeoTitle NVARCHAR(255) NOT NULL,
    MetaDescription NVARCHAR(500) NULL,
    Keywords NVARCHAR(255) NULL,
    CanonicalUrl NVARCHAR(500) NULL,
    OgTitle NVARCHAR(255) NULL,
    OgDescription NVARCHAR(500) NULL,
    OgImage NVARCHAR(500) NULL,
    OgType NVARCHAR(60) NOT NULL CONSTRAINT DF_SeoMetadata_OgType DEFAULT N'website',
    RobotsIndex BIT NOT NULL CONSTRAINT DF_SeoMetadata_RobotsIndex DEFAULT 1,
    RobotsFollow BIT NOT NULL CONSTRAINT DF_SeoMetadata_RobotsFollow DEFAULT 1,
    Status NVARCHAR(30) NOT NULL CONSTRAINT DF_SeoMetadata_Status DEFAULT N'active'
);

CREATE TABLE dbo.SeoSitemapEntries (
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SeoSitemapEntries PRIMARY KEY,
    EntityType NVARCHAR(60) NOT NULL,
    EntityId INT NULL,
    RoutePath NVARCHAR(500) NOT NULL,
    Priority DECIMAL(3,2) NOT NULL CONSTRAINT DF_SeoSitemapEntries_Priority DEFAULT 0.50,
    ChangeFrequency NVARCHAR(30) NOT NULL CONSTRAINT DF_SeoSitemapEntries_ChangeFrequency DEFAULT N'weekly',
    IncludeInSitemap BIT NOT NULL CONSTRAINT DF_SeoSitemapEntries_Include DEFAULT 1,
    LastModifiedAt DATETIME2(0) NULL
);

CREATE TABLE dbo.SeoRedirects (
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SeoRedirects PRIMARY KEY,
    SourcePath NVARCHAR(500) NOT NULL,
    TargetPath NVARCHAR(500) NOT NULL,
    StatusCode INT NOT NULL CONSTRAINT DF_SeoRedirects_StatusCode DEFAULT 301,
    IsActive BIT NOT NULL CONSTRAINT DF_SeoRedirects_IsActive DEFAULT 1
);
GO

CREATE UNIQUE INDEX UX_SiteSettings_Key ON dbo.SiteSettings(SettingKey);
CREATE UNIQUE INDEX UX_MediaFiles_Url ON dbo.MediaFiles(Url);
CREATE UNIQUE INDEX UX_ScrapCategories_Slug ON dbo.ScrapCategories(Slug);
CREATE UNIQUE INDEX UX_ScrapItems_Slug ON dbo.ScrapItems(Slug);
CREATE INDEX IX_ScrapItems_Public ON dbo.ScrapItems(Status, SortOrder, PublishedAt DESC);
CREATE INDEX IX_ScrapItems_CategoryStatus ON dbo.ScrapItems(ScrapCategoryId, Status);
CREATE UNIQUE INDEX UX_PostCategories_Slug ON dbo.PostCategories(Slug);
CREATE UNIQUE INDEX UX_Posts_Slug ON dbo.Posts(Slug);
CREATE INDEX IX_Posts_Public ON dbo.Posts(Status, SortOrder, PublishedAt DESC);
CREATE INDEX IX_Posts_CategoryStatus ON dbo.Posts(PostCategoryId, Status);
CREATE UNIQUE INDEX UX_SeoMetadata_RoutePath ON dbo.SeoMetadata(RoutePath) WHERE RoutePath IS NOT NULL;
CREATE UNIQUE INDEX UX_SeoMetadata_Entity ON dbo.SeoMetadata(EntityType, EntityId) WHERE EntityId IS NOT NULL;
CREATE UNIQUE INDEX UX_SeoSitemapEntries_RoutePath ON dbo.SeoSitemapEntries(RoutePath);
CREATE UNIQUE INDEX UX_SeoRedirects_SourcePath ON dbo.SeoRedirects(SourcePath);
GO

DROP TABLE IF EXISTS #SelectedProducts;
DROP TABLE IF EXISTS #SelectedPosts;

SELECT TOP (20) *
INTO #SelectedProducts
FROM WebPheLieu.dbo.products
WHERE status IN ('active', 'published')
ORDER BY id;

SELECT TOP (20) *
INTO #SelectedPosts
FROM WebPheLieu.dbo.blog_posts
WHERE status = 'published'
ORDER BY sort_published_at DESC, id DESC;

INSERT INTO dbo.SiteSettings (SettingKey, SettingValue, SettingGroup, Description, UpdatedAt)
SELECT setting_key, setting_value, setting_group, description, updated_at
FROM WebPheLieu.dbo.site_settings;

INSERT INTO dbo.ScrapCategories (Id, Name, Slug, Description, SortOrder, Status)
SELECT id, name, slug, description, ROW_NUMBER() OVER (ORDER BY id), N'published'
FROM WebPheLieu.dbo.product_categories
WHERE id IN (SELECT DISTINCT category_id FROM #SelectedProducts);

INSERT INTO dbo.ScrapItems (Id, ScrapCategoryId, Name, Slug, ShortDescription, Description, PrimaryImage, Unit, PriceFrom, PriceLabel, Status, SortOrder, IsFeatured, PublishedAt, CreatedAt, UpdatedAt)
SELECT id, category_id, name, slug, short_description, description,
       REPLACE(REPLACE(primary_image, '~/assets/images/products/', '/assets/images/imported/products/'), '/assets/images/products/', '/assets/images/imported/products/'),
       unit, price_value, price_label, N'published', ROW_NUMBER() OVER (ORDER BY id), is_featured, created_at, created_at, updated_at
FROM #SelectedProducts;

INSERT INTO dbo.ScrapItemImages (ScrapItemId, ImageUrl, Caption, OrderIndex)
SELECT product_id,
       REPLACE(REPLACE(image_url, '~/assets/images/products/', '/assets/images/imported/products/'), '/assets/images/products/', '/assets/images/imported/products/'),
       caption,
       order_index
FROM WebPheLieu.dbo.product_images
WHERE product_id IN (SELECT Id FROM dbo.ScrapItems);

WITH LatestPrices AS (
    SELECT product_id, price_value, price_unit, note, effective_date,
           ROW_NUMBER() OVER (PARTITION BY product_id ORDER BY effective_date DESC, recorded_at DESC, id DESC) AS rn
    FROM WebPheLieu.dbo.price_history
    WHERE product_id IN (SELECT Id FROM dbo.ScrapItems)
)
INSERT INTO dbo.ScrapPrices (ScrapItemId, PriceValue, PriceLabel, Unit, EffectiveDate)
SELECT product_id, price_value, note, COALESCE(price_unit, N'kg'), effective_date
FROM LatestPrices
WHERE rn = 1;

INSERT INTO dbo.ScrapPriceHistory (ScrapItemId, PriceValue, PriceUnit, PriceType, Note, EffectiveDate, RecordedAt)
SELECT product_id, price_value, price_unit, price_type, note, effective_date, recorded_at
FROM WebPheLieu.dbo.price_history
WHERE product_id IN (SELECT Id FROM dbo.ScrapItems);

INSERT INTO dbo.PostCategories (Id, Name, Slug, Description, SortOrder, Status)
SELECT id, name, slug, description, ROW_NUMBER() OVER (ORDER BY id), N'published'
FROM WebPheLieu.dbo.blog_categories
WHERE id IN (
    SELECT DISTINCT bpc.category_id
    FROM WebPheLieu.dbo.blog_post_categories bpc
    WHERE bpc.post_id IN (SELECT id FROM #SelectedPosts)
);

INSERT INTO dbo.Posts (Id, PostCategoryId, Title, Slug, Excerpt, ContentHtml, CoverImage, PublishedAt, Status, SortOrder, IsFeatured, AuthorName, CreatedAt, UpdatedAt)
SELECT post.id,
       (SELECT TOP (1) category_id FROM WebPheLieu.dbo.blog_post_categories WHERE post_id = post.id ORDER BY category_id),
       post.title,
       post.slug,
       post.excerpt,
       post.content,
       REPLACE(REPLACE(REPLACE(post.cover_image, '~/assets/images/blogs/', '/assets/images/imported/blogs/'), '/assets/images/blogs/', '/assets/images/imported/blogs/'), '\', '/'),
       COALESCE(post.published_at, post.created_at),
       N'published',
       ROW_NUMBER() OVER (ORDER BY post.sort_published_at DESC, post.id DESC),
       CASE WHEN ROW_NUMBER() OVER (ORDER BY post.sort_published_at DESC, post.id DESC) <= 4 THEN 1 ELSE 0 END,
       N'Quản trị viên',
       post.created_at,
       post.updated_at
FROM #SelectedPosts post;

INSERT INTO dbo.PostImages (PostId, ImageUrl, Caption, OrderIndex)
SELECT blog_id,
       REPLACE(REPLACE(REPLACE(image_url, '~/assets/images/blogs/', '/assets/images/imported/blogs/'), '/assets/images/blogs/', '/assets/images/imported/blogs/'), '\', '/'),
       caption,
       order_index
FROM WebPheLieu.dbo.blog_images
WHERE blog_id IN (SELECT Id FROM dbo.Posts);

INSERT INTO dbo.Banners (Title, Subtitle, ImageUrl, PrimaryButtonText, PrimaryButtonUrl, SecondaryButtonText, SecondaryButtonUrl, SortOrder, Status)
SELECT TOP (1)
       title,
       subtitle,
       (SELECT TOP (1) REPLACE(REPLACE(image_url, '~/assets/images/bannersandseos/', '/assets/images/imported/banners/'), '/assets/images/bannersandseos/', '/assets/images/imported/banners/') FROM WebPheLieu.dbo.banner_images WHERE banner_id = banners.id ORDER BY order_index),
       button_primary_text,
       button_primary_link,
       button_secondary_text,
       button_secondary_link,
       order_index,
       status
FROM WebPheLieu.dbo.banners
ORDER BY order_index;

INSERT INTO dbo.MediaFiles (FileName, Url, Folder, MimeType, AltText, Status)
SELECT RIGHT(v.Url, CHARINDEX('/', REVERSE(v.Url) + '/') - 1),
       v.Url,
       MIN(v.Folder),
       MIN(v.MimeType),
       MIN(v.AltText),
       N'active'
FROM (
    SELECT PrimaryImage AS Url, N'products' AS Folder, N'image/jpeg' AS MimeType, Name AS AltText FROM dbo.ScrapItems WHERE PrimaryImage IS NOT NULL
    UNION
    SELECT ImageUrl, N'products', N'image/jpeg', Caption FROM dbo.ScrapItemImages
    UNION
    SELECT CoverImage, N'blogs', CASE WHEN CoverImage LIKE N'%.webp' THEN N'image/webp' ELSE N'image/jpeg' END, Title FROM dbo.Posts WHERE CoverImage IS NOT NULL
    UNION
    SELECT ImageUrl, N'blogs', CASE WHEN ImageUrl LIKE N'%.webp' THEN N'image/webp' ELSE N'image/jpeg' END, Caption FROM dbo.PostImages
    UNION
    SELECT ImageUrl, N'banners', N'image/jpeg', Title FROM dbo.Banners WHERE ImageUrl IS NOT NULL
) v
WHERE v.Url IS NOT NULL
GROUP BY v.Url;

INSERT INTO dbo.SeoMetadata (EntityType, EntityId, RoutePath, SeoTitle, MetaDescription, Keywords, CanonicalUrl, OgTitle, OgDescription, OgImage, OgType, RobotsIndex, RobotsFollow, Status)
VALUES
(N'Page', NULL, N'/', N'Thu mua phế liệu giá cao tận nơi', N'Thu mua phế liệu tận nơi, cân minh bạch, thanh toán ngay. Nhận đồng, sắt, nhôm, inox, máy móc cũ và hàng thanh lý.', N'thu mua phế liệu, giá phế liệu', N'/', N'Thu mua phế liệu giá cao tận nơi', N'Báo giá nhanh, thu gom tận nơi, thanh toán ngay.', (SELECT TOP (1) ImageUrl FROM dbo.Banners), N'website', 1, 1, N'active'),
(N'Page', NULL, N'/phe-lieu', N'Danh mục phế liệu thu mua giá cao', N'Danh sách phế liệu đang thu mua: đồng, nhôm, sắt, inox, máy móc cũ, giấy carton, nhựa PET.', N'phế liệu thu mua, bảng giá phế liệu', N'/phe-lieu', N'Danh mục phế liệu thu mua', N'Cập nhật các loại phế liệu và giá tham khảo.', (SELECT TOP (1) PrimaryImage FROM dbo.ScrapItems ORDER BY SortOrder), N'website', 1, 1, N'active'),
(N'Page', NULL, N'/tin-tuc', N'Tin tức và bảng giá phế liệu mới nhất', N'Cập nhật kinh nghiệm bán phế liệu, bảng giá và hướng dẫn phân loại để bán được giá tốt hơn.', N'tin tức phế liệu, bảng giá phế liệu', N'/tin-tuc', N'Tin tức phế liệu', N'Bảng giá, kinh nghiệm và kiến thức phế liệu.', (SELECT TOP (1) CoverImage FROM dbo.Posts ORDER BY PublishedAt DESC), N'website', 1, 1, N'active');

INSERT INTO dbo.SeoMetadata (EntityType, EntityId, RoutePath, SeoTitle, MetaDescription, Keywords, CanonicalUrl, OgTitle, OgDescription, OgImage, OgType, RobotsIndex, RobotsFollow, Status)
SELECT N'ScrapItem', Id, N'/phe-lieu/' + Slug, Name + N' giá cao tận nơi', LEFT(COALESCE(ShortDescription, Description, Name), 500), Name, N'/phe-lieu/' + Slug, Name, LEFT(COALESCE(ShortDescription, Description, Name), 500), PrimaryImage, N'website', 1, 1, N'active'
FROM dbo.ScrapItems;

INSERT INTO dbo.SeoMetadata (EntityType, EntityId, RoutePath, SeoTitle, MetaDescription, Keywords, CanonicalUrl, OgTitle, OgDescription, OgImage, OgType, RobotsIndex, RobotsFollow, Status)
SELECT N'Post', Id, N'/tin-tuc/' + Slug, Title, LEFT(COALESCE(Excerpt, Title), 500), NULL, N'/tin-tuc/' + Slug, Title, LEFT(COALESCE(Excerpt, Title), 500), CoverImage, N'article', 1, 1, N'active'
FROM dbo.Posts;

INSERT INTO dbo.SeoSitemapEntries (EntityType, EntityId, RoutePath, Priority, ChangeFrequency, IncludeInSitemap, LastModifiedAt)
SELECT EntityType, EntityId, RoutePath,
       CASE WHEN RoutePath = N'/' THEN 1.00 WHEN EntityType = N'Page' THEN 0.80 ELSE 0.60 END,
       CASE WHEN EntityType = N'Post' THEN N'weekly' ELSE N'daily' END,
       1,
       SYSUTCDATETIME()
FROM dbo.SeoMetadata
WHERE RoutePath IS NOT NULL;

SELECT
    (SELECT COUNT(*) FROM dbo.ScrapItems) AS ScrapItemsCount,
    (SELECT COUNT(*) FROM dbo.Posts) AS PostsCount,
    (SELECT COUNT(*) FROM dbo.MediaFiles) AS MediaFilesCount,
    (SELECT COUNT(*) FROM dbo.SeoMetadata) AS SeoMetadataCount;
