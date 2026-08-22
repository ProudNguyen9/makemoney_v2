/*
  Scrap CMS database for public website + admin + SEO
  Target: SQL Server

  This script is intentionally self-contained for a clean development database.
  It drops and recreates the CMS tables, then inserts template seed data.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET ARITHABORT ON;
SET NUMERIC_ROUNDABORT OFF;

BEGIN TRANSACTION;

DROP TABLE IF EXISTS dbo.SeoAuditNotes;
DROP TABLE IF EXISTS dbo.SeoSitemapEntries;
DROP TABLE IF EXISTS dbo.SeoRedirects;
DROP TABLE IF EXISTS dbo.SeoMetadata;
DROP TABLE IF EXISTS dbo.ContactRequestFiles;
DROP TABLE IF EXISTS dbo.ContactRequests;
DROP TABLE IF EXISTS dbo.FaqItems;
DROP TABLE IF EXISTS dbo.Posts;
DROP TABLE IF EXISTS dbo.PostCategories;
DROP TABLE IF EXISTS dbo.ProjectImages;
DROP TABLE IF EXISTS dbo.Projects;
DROP TABLE IF EXISTS dbo.Locations;
DROP TABLE IF EXISTS dbo.Services;
DROP TABLE IF EXISTS dbo.ScrapPriceHistory;
DROP TABLE IF EXISTS dbo.ScrapPrices;
DROP TABLE IF EXISTS dbo.ScrapItemImages;
DROP TABLE IF EXISTS dbo.ScrapItems;
DROP TABLE IF EXISTS dbo.ScrapCategories;
DROP TABLE IF EXISTS dbo.HomepageSections;
DROP TABLE IF EXISTS dbo.Pages;
DROP TABLE IF EXISTS dbo.MenuItems;
DROP TABLE IF EXISTS dbo.AdminUserRoles;
DROP TABLE IF EXISTS dbo.AdminRoles;
DROP TABLE IF EXISTS dbo.AdminUsers;
DROP TABLE IF EXISTS dbo.MediaFiles;
DROP TABLE IF EXISTS dbo.SiteSettings;

CREATE TABLE dbo.SiteSettings (
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SiteSettings PRIMARY KEY,
    [Key] NVARCHAR(120) NOT NULL,
    [Value] NVARCHAR(MAX) NULL,
    [Group] NVARCHAR(80) NOT NULL CONSTRAINT DF_SiteSettings_Group DEFAULT N'general',
    Label NVARCHAR(200) NULL,
    InputType NVARCHAR(40) NOT NULL CONSTRAINT DF_SiteSettings_InputType DEFAULT N'text',
    SortOrder INT NOT NULL CONSTRAINT DF_SiteSettings_SortOrder DEFAULT 0,
    IsSystem BIT NOT NULL CONSTRAINT DF_SiteSettings_IsSystem DEFAULT 0,
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_SiteSettings_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2(0) NULL
);

CREATE UNIQUE INDEX UX_SiteSettings_Key ON dbo.SiteSettings([Key]);

CREATE TABLE dbo.MediaFiles (
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_MediaFiles PRIMARY KEY,
    FileName NVARCHAR(255) NOT NULL,
    OriginalFileName NVARCHAR(255) NULL,
    Url NVARCHAR(500) NOT NULL,
    Folder NVARCHAR(160) NULL,
    MimeType NVARCHAR(120) NULL,
    AltText NVARCHAR(255) NULL,
    Caption NVARCHAR(500) NULL,
    Width INT NULL,
    Height INT NULL,
    FileSizeBytes BIGINT NULL,
    Status NVARCHAR(30) NOT NULL CONSTRAINT DF_MediaFiles_Status DEFAULT N'active',
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_MediaFiles_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2(0) NULL,
    DeletedAt DATETIME2(0) NULL
);

CREATE UNIQUE INDEX UX_MediaFiles_Url ON dbo.MediaFiles(Url) WHERE DeletedAt IS NULL;

CREATE TABLE dbo.AdminUsers (
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AdminUsers PRIMARY KEY,
    UserName NVARCHAR(80) NOT NULL,
    Email NVARCHAR(255) NOT NULL,
    DisplayName NVARCHAR(160) NOT NULL,
    PasswordHash NVARCHAR(500) NOT NULL,
    Status NVARCHAR(30) NOT NULL CONSTRAINT DF_AdminUsers_Status DEFAULT N'active',
    LastLoginAt DATETIME2(0) NULL,
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_AdminUsers_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2(0) NULL,
    DeletedAt DATETIME2(0) NULL
);

CREATE UNIQUE INDEX UX_AdminUsers_UserName ON dbo.AdminUsers(UserName) WHERE DeletedAt IS NULL;
CREATE UNIQUE INDEX UX_AdminUsers_Email ON dbo.AdminUsers(Email) WHERE DeletedAt IS NULL;

CREATE TABLE dbo.AdminRoles (
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AdminRoles PRIMARY KEY,
    Name NVARCHAR(80) NOT NULL,
    Description NVARCHAR(300) NULL,
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_AdminRoles_CreatedAt DEFAULT SYSUTCDATETIME()
);

CREATE UNIQUE INDEX UX_AdminRoles_Name ON dbo.AdminRoles(Name);

CREATE TABLE dbo.AdminUserRoles (
    AdminUserId INT NOT NULL,
    AdminRoleId INT NOT NULL,
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_AdminUserRoles_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_AdminUserRoles PRIMARY KEY (AdminUserId, AdminRoleId),
    CONSTRAINT FK_AdminUserRoles_AdminUsers FOREIGN KEY (AdminUserId) REFERENCES dbo.AdminUsers(Id) ON DELETE CASCADE,
    CONSTRAINT FK_AdminUserRoles_AdminRoles FOREIGN KEY (AdminRoleId) REFERENCES dbo.AdminRoles(Id) ON DELETE CASCADE
);

CREATE TABLE dbo.MenuItems (
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_MenuItems PRIMARY KEY,
    ParentId INT NULL,
    Position NVARCHAR(40) NOT NULL CONSTRAINT DF_MenuItems_Position DEFAULT N'header',
    Label NVARCHAR(160) NOT NULL,
    Url NVARCHAR(500) NOT NULL,
    IconCss NVARCHAR(120) NULL,
    CssClass NVARCHAR(160) NULL,
    Target NVARCHAR(30) NULL,
    SortOrder INT NOT NULL CONSTRAINT DF_MenuItems_SortOrder DEFAULT 0,
    Status NVARCHAR(30) NOT NULL CONSTRAINT DF_MenuItems_Status DEFAULT N'published',
    IsFeatured BIT NOT NULL CONSTRAINT DF_MenuItems_IsFeatured DEFAULT 0,
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_MenuItems_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2(0) NULL,
    PublishedAt DATETIME2(0) NULL,
    DeletedAt DATETIME2(0) NULL,
    CONSTRAINT FK_MenuItems_Parent FOREIGN KEY (ParentId) REFERENCES dbo.MenuItems(Id)
);

CREATE INDEX IX_MenuItems_Position ON dbo.MenuItems(Position, Status, SortOrder);

CREATE TABLE dbo.Pages (
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Pages PRIMARY KEY,
    PageType NVARCHAR(60) NOT NULL CONSTRAINT DF_Pages_PageType DEFAULT N'page',
    Title NVARCHAR(255) NOT NULL,
    Slug NVARCHAR(180) NOT NULL,
    RoutePath NVARCHAR(300) NOT NULL,
    Label NVARCHAR(120) NULL,
    HeroTitle NVARCHAR(255) NULL,
    HeroDescription NVARCHAR(800) NULL,
    HeroImageUrl NVARCHAR(500) NULL,
    BodyHtml NVARCHAR(MAX) NULL,
    Status NVARCHAR(30) NOT NULL CONSTRAINT DF_Pages_Status DEFAULT N'published',
    SortOrder INT NOT NULL CONSTRAINT DF_Pages_SortOrder DEFAULT 0,
    IsFeatured BIT NOT NULL CONSTRAINT DF_Pages_IsFeatured DEFAULT 0,
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Pages_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2(0) NULL,
    PublishedAt DATETIME2(0) NULL,
    DeletedAt DATETIME2(0) NULL
);

CREATE UNIQUE INDEX UX_Pages_Slug ON dbo.Pages(Slug) WHERE DeletedAt IS NULL;
CREATE UNIQUE INDEX UX_Pages_RoutePath ON dbo.Pages(RoutePath) WHERE DeletedAt IS NULL;

CREATE TABLE dbo.HomepageSections (
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_HomepageSections PRIMARY KEY,
    SectionKey NVARCHAR(80) NOT NULL,
    Title NVARCHAR(255) NULL,
    Subtitle NVARCHAR(255) NULL,
    Teaser NVARCHAR(MAX) NULL,
    ImageUrl NVARCHAR(500) NULL,
    LinkText NVARCHAR(160) NULL,
    LinkUrl NVARCHAR(500) NULL,
    JsonData NVARCHAR(MAX) NULL,
    Note NVARCHAR(500) NULL,
    Status NVARCHAR(30) NOT NULL CONSTRAINT DF_HomepageSections_Status DEFAULT N'published',
    SortOrder INT NOT NULL CONSTRAINT DF_HomepageSections_SortOrder DEFAULT 0,
    IsFeatured BIT NOT NULL CONSTRAINT DF_HomepageSections_IsFeatured DEFAULT 0,
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_HomepageSections_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2(0) NULL,
    PublishedAt DATETIME2(0) NULL,
    DeletedAt DATETIME2(0) NULL
);

CREATE UNIQUE INDEX UX_HomepageSections_SectionKey ON dbo.HomepageSections(SectionKey) WHERE DeletedAt IS NULL;

CREATE TABLE dbo.ScrapCategories (
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ScrapCategories PRIMARY KEY,
    Name NVARCHAR(160) NOT NULL,
    Slug NVARCHAR(180) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    HeroTitle NVARCHAR(255) NULL,
    HeroDescription NVARCHAR(800) NULL,
    ImageUrl NVARCHAR(500) NULL,
    IconCss NVARCHAR(120) NULL,
    Status NVARCHAR(30) NOT NULL CONSTRAINT DF_ScrapCategories_Status DEFAULT N'published',
    SortOrder INT NOT NULL CONSTRAINT DF_ScrapCategories_SortOrder DEFAULT 0,
    IsFeatured BIT NOT NULL CONSTRAINT DF_ScrapCategories_IsFeatured DEFAULT 0,
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_ScrapCategories_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2(0) NULL,
    PublishedAt DATETIME2(0) NULL,
    DeletedAt DATETIME2(0) NULL
);

CREATE UNIQUE INDEX UX_ScrapCategories_Slug ON dbo.ScrapCategories(Slug) WHERE DeletedAt IS NULL;

CREATE TABLE dbo.ScrapItems (
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ScrapItems PRIMARY KEY,
    ScrapCategoryId INT NULL,
    Name NVARCHAR(180) NOT NULL,
    Slug NVARCHAR(180) NOT NULL,
    ShortDescription NVARCHAR(500) NULL,
    Description NVARCHAR(MAX) NULL,
    PrimaryImage NVARCHAR(500) NULL,
    Unit NVARCHAR(40) NOT NULL CONSTRAINT DF_ScrapItems_Unit DEFAULT N'kg',
    PriceFrom DECIMAL(18,2) NULL,
    PriceTo DECIMAL(18,2) NULL,
    PriceNote NVARCHAR(500) NULL,
    Status NVARCHAR(30) NOT NULL CONSTRAINT DF_ScrapItems_Status DEFAULT N'published',
    SortOrder INT NOT NULL CONSTRAINT DF_ScrapItems_SortOrder DEFAULT 0,
    IsFeatured BIT NOT NULL CONSTRAINT DF_ScrapItems_IsFeatured DEFAULT 0,
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_ScrapItems_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2(0) NULL,
    PublishedAt DATETIME2(0) NULL,
    DeletedAt DATETIME2(0) NULL,
    CONSTRAINT FK_ScrapItems_ScrapCategories FOREIGN KEY (ScrapCategoryId) REFERENCES dbo.ScrapCategories(Id)
);

CREATE UNIQUE INDEX UX_ScrapItems_Slug ON dbo.ScrapItems(Slug) WHERE DeletedAt IS NULL;
CREATE INDEX IX_ScrapItems_Category ON dbo.ScrapItems(ScrapCategoryId, Status, SortOrder);

CREATE TABLE dbo.ScrapItemImages (
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ScrapItemImages PRIMARY KEY,
    ScrapItemId INT NOT NULL,
    ImageUrl NVARCHAR(500) NOT NULL,
    AltText NVARCHAR(255) NULL,
    Caption NVARCHAR(500) NULL,
    SortOrder INT NOT NULL CONSTRAINT DF_ScrapItemImages_SortOrder DEFAULT 0,
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_ScrapItemImages_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_ScrapItemImages_ScrapItems FOREIGN KEY (ScrapItemId) REFERENCES dbo.ScrapItems(Id) ON DELETE CASCADE
);

CREATE TABLE dbo.ScrapPrices (
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ScrapPrices PRIMARY KEY,
    ScrapItemId INT NOT NULL,
    PriceFrom DECIMAL(18,2) NULL,
    PriceTo DECIMAL(18,2) NULL,
    PriceLabel NVARCHAR(160) NULL,
    Unit NVARCHAR(40) NOT NULL CONSTRAINT DF_ScrapPrices_Unit DEFAULT N'kg',
    Trend NVARCHAR(20) NOT NULL CONSTRAINT DF_ScrapPrices_Trend DEFAULT N'flat',
    TrendPercent DECIMAL(9,2) NULL,
    EffectiveDate DATE NOT NULL CONSTRAINT DF_ScrapPrices_EffectiveDate DEFAULT CONVERT(DATE, GETDATE()),
    Status NVARCHAR(30) NOT NULL CONSTRAINT DF_ScrapPrices_Status DEFAULT N'published',
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_ScrapPrices_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2(0) NULL,
    CONSTRAINT FK_ScrapPrices_ScrapItems FOREIGN KEY (ScrapItemId) REFERENCES dbo.ScrapItems(Id) ON DELETE CASCADE
);

CREATE INDEX IX_ScrapPrices_ItemDate ON dbo.ScrapPrices(ScrapItemId, EffectiveDate DESC);

CREATE TABLE dbo.ScrapPriceHistory (
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ScrapPriceHistory PRIMARY KEY,
    ScrapPriceId INT NOT NULL,
    PriceFrom DECIMAL(18,2) NULL,
    PriceTo DECIMAL(18,2) NULL,
    PriceLabel NVARCHAR(160) NULL,
    Unit NVARCHAR(40) NOT NULL,
    ChangedAt DATETIME2(0) NOT NULL CONSTRAINT DF_ScrapPriceHistory_ChangedAt DEFAULT SYSUTCDATETIME(),
    ChangedByAdminUserId INT NULL,
    Note NVARCHAR(500) NULL,
    CONSTRAINT FK_ScrapPriceHistory_ScrapPrices FOREIGN KEY (ScrapPriceId) REFERENCES dbo.ScrapPrices(Id) ON DELETE CASCADE,
    CONSTRAINT FK_ScrapPriceHistory_AdminUsers FOREIGN KEY (ChangedByAdminUserId) REFERENCES dbo.AdminUsers(Id)
);

CREATE TABLE dbo.Services (
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Services PRIMARY KEY,
    Title NVARCHAR(220) NOT NULL,
    Slug NVARCHAR(180) NOT NULL,
    Excerpt NVARCHAR(600) NULL,
    ContentHtml NVARCHAR(MAX) NULL,
    CoverImage NVARCHAR(500) NULL,
    IconCss NVARCHAR(120) NULL,
    Status NVARCHAR(30) NOT NULL CONSTRAINT DF_Services_Status DEFAULT N'published',
    SortOrder INT NOT NULL CONSTRAINT DF_Services_SortOrder DEFAULT 0,
    IsFeatured BIT NOT NULL CONSTRAINT DF_Services_IsFeatured DEFAULT 0,
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Services_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2(0) NULL,
    PublishedAt DATETIME2(0) NULL,
    DeletedAt DATETIME2(0) NULL
);

CREATE UNIQUE INDEX UX_Services_Slug ON dbo.Services(Slug) WHERE DeletedAt IS NULL;

CREATE TABLE dbo.Locations (
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Locations PRIMARY KEY,
    Province NVARCHAR(120) NOT NULL,
    District NVARCHAR(160) NULL,
    Name NVARCHAR(180) NOT NULL,
    Slug NVARCHAR(180) NOT NULL,
    Excerpt NVARCHAR(600) NULL,
    ContentHtml NVARCHAR(MAX) NULL,
    CoverImage NVARCHAR(500) NULL,
    Latitude DECIMAL(10,7) NULL,
    Longitude DECIMAL(10,7) NULL,
    Status NVARCHAR(30) NOT NULL CONSTRAINT DF_Locations_Status DEFAULT N'published',
    SortOrder INT NOT NULL CONSTRAINT DF_Locations_SortOrder DEFAULT 0,
    IsFeatured BIT NOT NULL CONSTRAINT DF_Locations_IsFeatured DEFAULT 0,
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Locations_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2(0) NULL,
    PublishedAt DATETIME2(0) NULL,
    DeletedAt DATETIME2(0) NULL
);

CREATE UNIQUE INDEX UX_Locations_Slug ON dbo.Locations(Slug) WHERE DeletedAt IS NULL;
CREATE INDEX IX_Locations_Province ON dbo.Locations(Province, Status, SortOrder);

CREATE TABLE dbo.Projects (
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Projects PRIMARY KEY,
    Title NVARCHAR(255) NOT NULL,
    Slug NVARCHAR(180) NOT NULL,
    ProjectType NVARCHAR(120) NULL,
    LocationText NVARCHAR(255) NULL,
    Excerpt NVARCHAR(700) NULL,
    ContentHtml NVARCHAR(MAX) NULL,
    CoverImage NVARCHAR(500) NULL,
    CompletedAt DATE NULL,
    QuantityText NVARCHAR(120) NULL,
    DurationText NVARCHAR(120) NULL,
    Status NVARCHAR(30) NOT NULL CONSTRAINT DF_Projects_Status DEFAULT N'published',
    SortOrder INT NOT NULL CONSTRAINT DF_Projects_SortOrder DEFAULT 0,
    IsFeatured BIT NOT NULL CONSTRAINT DF_Projects_IsFeatured DEFAULT 0,
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Projects_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2(0) NULL,
    PublishedAt DATETIME2(0) NULL,
    DeletedAt DATETIME2(0) NULL
);

CREATE UNIQUE INDEX UX_Projects_Slug ON dbo.Projects(Slug) WHERE DeletedAt IS NULL;

CREATE TABLE dbo.ProjectImages (
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ProjectImages PRIMARY KEY,
    ProjectId INT NOT NULL,
    ImageUrl NVARCHAR(500) NOT NULL,
    AltText NVARCHAR(255) NULL,
    Caption NVARCHAR(500) NULL,
    SortOrder INT NOT NULL CONSTRAINT DF_ProjectImages_SortOrder DEFAULT 0,
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_ProjectImages_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_ProjectImages_Projects FOREIGN KEY (ProjectId) REFERENCES dbo.Projects(Id) ON DELETE CASCADE
);

CREATE TABLE dbo.PostCategories (
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PostCategories PRIMARY KEY,
    Name NVARCHAR(160) NOT NULL,
    Slug NVARCHAR(180) NOT NULL,
    Description NVARCHAR(500) NULL,
    Status NVARCHAR(30) NOT NULL CONSTRAINT DF_PostCategories_Status DEFAULT N'published',
    SortOrder INT NOT NULL CONSTRAINT DF_PostCategories_SortOrder DEFAULT 0,
    IsFeatured BIT NOT NULL CONSTRAINT DF_PostCategories_IsFeatured DEFAULT 0,
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_PostCategories_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2(0) NULL,
    PublishedAt DATETIME2(0) NULL,
    DeletedAt DATETIME2(0) NULL
);

CREATE UNIQUE INDEX UX_PostCategories_Slug ON dbo.PostCategories(Slug) WHERE DeletedAt IS NULL;

CREATE TABLE dbo.Posts (
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Posts PRIMARY KEY,
    PostCategoryId INT NULL,
    Title NVARCHAR(255) NOT NULL,
    Slug NVARCHAR(180) NOT NULL,
    Excerpt NVARCHAR(700) NULL,
    ContentHtml NVARCHAR(MAX) NULL,
    CoverImage NVARCHAR(500) NULL,
    AuthorName NVARCHAR(160) NULL,
    Status NVARCHAR(30) NOT NULL CONSTRAINT DF_Posts_Status DEFAULT N'published',
    SortOrder INT NOT NULL CONSTRAINT DF_Posts_SortOrder DEFAULT 0,
    IsFeatured BIT NOT NULL CONSTRAINT DF_Posts_IsFeatured DEFAULT 0,
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Posts_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2(0) NULL,
    PublishedAt DATETIME2(0) NULL,
    DeletedAt DATETIME2(0) NULL,
    CONSTRAINT FK_Posts_PostCategories FOREIGN KEY (PostCategoryId) REFERENCES dbo.PostCategories(Id)
);

CREATE UNIQUE INDEX UX_Posts_Slug ON dbo.Posts(Slug) WHERE DeletedAt IS NULL;

CREATE TABLE dbo.FaqItems (
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_FaqItems PRIMARY KEY,
    EntityType NVARCHAR(60) NOT NULL,
    EntityId INT NULL,
    RoutePath NVARCHAR(300) NULL,
    Question NVARCHAR(500) NOT NULL,
    Answer NVARCHAR(MAX) NOT NULL,
    Status NVARCHAR(30) NOT NULL CONSTRAINT DF_FaqItems_Status DEFAULT N'published',
    SortOrder INT NOT NULL CONSTRAINT DF_FaqItems_SortOrder DEFAULT 0,
    IsFeatured BIT NOT NULL CONSTRAINT DF_FaqItems_IsFeatured DEFAULT 0,
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_FaqItems_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2(0) NULL,
    PublishedAt DATETIME2(0) NULL,
    DeletedAt DATETIME2(0) NULL
);

CREATE INDEX IX_FaqItems_Entity ON dbo.FaqItems(EntityType, EntityId, RoutePath, Status, SortOrder);

CREATE TABLE dbo.ContactRequests (
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ContactRequests PRIMARY KEY,
    Name NVARCHAR(120) NULL,
    Phone NVARCHAR(30) NOT NULL,
    Email NVARCHAR(255) NULL,
    Zalo NVARCHAR(80) NULL,
    ScrapType NVARCHAR(180) NULL,
    QuantityText NVARCHAR(160) NULL,
    Area NVARCHAR(160) NULL,
    Message NVARCHAR(MAX) NULL,
    SourceForm NVARCHAR(80) NOT NULL CONSTRAINT DF_ContactRequests_SourceForm DEFAULT N'quick_quote',
    SourceUrl NVARCHAR(500) NULL,
    Status NVARCHAR(30) NOT NULL CONSTRAINT DF_ContactRequests_Status DEFAULT N'new',
    AssignedToAdminUserId INT NULL,
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_ContactRequests_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2(0) NULL,
    PublishedAt DATETIME2(0) NULL,
    DeletedAt DATETIME2(0) NULL,
    CONSTRAINT FK_ContactRequests_AdminUsers FOREIGN KEY (AssignedToAdminUserId) REFERENCES dbo.AdminUsers(Id)
);

CREATE INDEX IX_ContactRequests_StatusCreated ON dbo.ContactRequests(Status, CreatedAt DESC);
CREATE INDEX IX_ContactRequests_Phone ON dbo.ContactRequests(Phone);

CREATE TABLE dbo.ContactRequestFiles (
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ContactRequestFiles PRIMARY KEY,
    ContactRequestId INT NOT NULL,
    MediaFileId INT NULL,
    FileUrl NVARCHAR(500) NOT NULL,
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_ContactRequestFiles_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_ContactRequestFiles_ContactRequests FOREIGN KEY (ContactRequestId) REFERENCES dbo.ContactRequests(Id) ON DELETE CASCADE,
    CONSTRAINT FK_ContactRequestFiles_MediaFiles FOREIGN KEY (MediaFileId) REFERENCES dbo.MediaFiles(Id)
);

CREATE TABLE dbo.SeoMetadata (
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SeoMetadata PRIMARY KEY,
    EntityType NVARCHAR(60) NOT NULL,
    EntityId INT NULL,
    RoutePath NVARCHAR(300) NULL,
    SeoTitle NVARCHAR(255) NOT NULL,
    MetaDescription NVARCHAR(500) NULL,
    Keywords NVARCHAR(500) NULL,
    CanonicalUrl NVARCHAR(500) NULL,
    OgTitle NVARCHAR(255) NULL,
    OgDescription NVARCHAR(500) NULL,
    OgImage NVARCHAR(500) NULL,
    OgType NVARCHAR(60) NOT NULL CONSTRAINT DF_SeoMetadata_OgType DEFAULT N'website',
    RobotsIndex BIT NOT NULL CONSTRAINT DF_SeoMetadata_RobotsIndex DEFAULT 1,
    RobotsFollow BIT NOT NULL CONSTRAINT DF_SeoMetadata_RobotsFollow DEFAULT 1,
    SchemaType NVARCHAR(120) NULL,
    SchemaJsonOverride NVARCHAR(MAX) NULL,
    Status NVARCHAR(30) NOT NULL CONSTRAINT DF_SeoMetadata_Status DEFAULT N'active',
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_SeoMetadata_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2(0) NULL
);

CREATE UNIQUE INDEX UX_SeoMetadata_Entity ON dbo.SeoMetadata(EntityType, EntityId) WHERE EntityId IS NOT NULL;
CREATE UNIQUE INDEX UX_SeoMetadata_RoutePath ON dbo.SeoMetadata(RoutePath) WHERE RoutePath IS NOT NULL;

CREATE TABLE dbo.SeoRedirects (
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SeoRedirects PRIMARY KEY,
    SourcePath NVARCHAR(300) NOT NULL,
    TargetPath NVARCHAR(500) NOT NULL,
    StatusCode INT NOT NULL CONSTRAINT DF_SeoRedirects_StatusCode DEFAULT 301,
    IsActive BIT NOT NULL CONSTRAINT DF_SeoRedirects_IsActive DEFAULT 1,
    HitCount INT NOT NULL CONSTRAINT DF_SeoRedirects_HitCount DEFAULT 0,
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_SeoRedirects_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2(0) NULL
);

CREATE UNIQUE INDEX UX_SeoRedirects_SourcePath ON dbo.SeoRedirects(SourcePath);

CREATE TABLE dbo.SeoSitemapEntries (
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SeoSitemapEntries PRIMARY KEY,
    EntityType NVARCHAR(60) NOT NULL,
    EntityId INT NULL,
    RoutePath NVARCHAR(300) NOT NULL,
    Priority DECIMAL(3,2) NOT NULL CONSTRAINT DF_SeoSitemapEntries_Priority DEFAULT 0.50,
    ChangeFrequency NVARCHAR(30) NOT NULL CONSTRAINT DF_SeoSitemapEntries_ChangeFrequency DEFAULT N'weekly',
    IncludeInSitemap BIT NOT NULL CONSTRAINT DF_SeoSitemapEntries_Include DEFAULT 1,
    LastModifiedAt DATETIME2(0) NULL,
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_SeoSitemapEntries_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2(0) NULL
);

CREATE UNIQUE INDEX UX_SeoSitemapEntries_RoutePath ON dbo.SeoSitemapEntries(RoutePath);

CREATE TABLE dbo.SeoAuditNotes (
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SeoAuditNotes PRIMARY KEY,
    SeoMetadataId INT NULL,
    EntityType NVARCHAR(60) NOT NULL,
    EntityId INT NULL,
    RoutePath NVARCHAR(300) NULL,
    Severity NVARCHAR(30) NOT NULL CONSTRAINT DF_SeoAuditNotes_Severity DEFAULT N'info',
    Message NVARCHAR(800) NOT NULL,
    IsResolved BIT NOT NULL CONSTRAINT DF_SeoAuditNotes_IsResolved DEFAULT 0,
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_SeoAuditNotes_CreatedAt DEFAULT SYSUTCDATETIME(),
    ResolvedAt DATETIME2(0) NULL,
    CONSTRAINT FK_SeoAuditNotes_SeoMetadata FOREIGN KEY (SeoMetadataId) REFERENCES dbo.SeoMetadata(Id) ON DELETE SET NULL
);

CREATE INDEX IX_SeoAuditNotes_Open ON dbo.SeoAuditNotes(IsResolved, Severity, CreatedAt DESC);

INSERT INTO dbo.SiteSettings ([Key], [Value], [Group], Label, InputType, SortOrder, IsSystem)
VALUES
(N'company.name', N'[TÊN CÔNG TY]', N'company', N'Tên công ty', N'text', 1, 1),
(N'company.tax_code', N'[MÃ SỐ THUẾ]', N'company', N'Mã số thuế', N'text', 2, 0),
(N'company.hotline', N'[HOTLINE]', N'contact', N'Hotline', N'tel', 3, 1),
(N'company.zalo', N'[ZALO]', N'contact', N'Zalo', N'text', 4, 1),
(N'company.email', N'[EMAIL]', N'contact', N'Email', N'email', 5, 1),
(N'company.address', N'[ĐỊA CHỈ]', N'contact', N'Địa chỉ', N'text', 6, 1),
(N'company.warehouse_address', N'[ĐỊA CHỈ KHO]', N'contact', N'Địa chỉ kho', N'text', 7, 0),
(N'company.business_hours', N'[GIỜ LÀM VIỆC]', N'contact', N'Giờ làm việc', N'text', 8, 0),
(N'seo.site_title', N'Thu mua phế liệu [TÊN CÔNG TY] - Giá cao tận nơi', N'seo', N'Site title', N'text', 20, 1),
(N'seo.default_description', N'Thu mua phế liệu tận nơi giá cao: sắt, đồng, nhôm, inox, giấy, nhựa. Khảo sát miễn phí, cân tại chỗ, thanh toán ngay.', N'seo', N'Meta description mặc định', N'textarea', 21, 1),
(N'seo.google_verification', NULL, N'seo', N'Google Search Verification', N'text', 22, 0),
(N'seo.bing_verification', NULL, N'seo', N'Bing Verification', N'text', 23, 0),
(N'tracking.ga4', NULL, N'tracking', N'Google Analytics 4', N'text', 30, 0),
(N'tracking.gtm', NULL, N'tracking', N'Google Tag Manager', N'text', 31, 0);

INSERT INTO dbo.MediaFiles (FileName, Url, Folder, MimeType, AltText, Width, Height, Status)
VALUES
(N'logo.svg', N'/assets/images/logo/logo.svg', N'logo', N'image/svg+xml', N'Logo [TÊN CÔNG TY]', 186, 46, N'active'),
(N'logo-footer.svg', N'/assets/images/logo/logo-footer.svg', N'logo', N'image/svg+xml', N'Logo footer [TÊN CÔNG TY]', 186, 44, N'active'),
(N'hero-main.svg', N'/assets/images/hero/hero-main.svg', N'hero', N'image/svg+xml', N'Sân kho phân loại phế liệu', 1600, 1000, N'active'),
(N'hero-01.svg', N'/assets/images/hero/hero-01.svg', N'hero', N'image/svg+xml', N'Đội xe thu gom phế liệu', 1600, 1000, N'active'),
(N'hero-02.svg', N'/assets/images/hero/hero-02.svg', N'hero', N'image/svg+xml', N'Công nhân xử lý phế liệu', 1600, 1000, N'active'),
(N'company-yard.svg', N'/assets/images/company/company-yard.svg', N'company', N'image/svg+xml', N'Sân kho phế liệu', 1200, 800, N'active'),
(N'company-scale.svg', N'/assets/images/company/company-scale.svg', N'company', N'image/svg+xml', N'Cân điện tử minh bạch', 1200, 800, N'active'),
(N'scrap-copper.svg', N'/assets/images/scrap/scrap-copper.svg', N'scrap', N'image/svg+xml', N'Phế liệu đồng', 1200, 900, N'active'),
(N'project-01-cover.svg', N'/assets/images/projects/project-01-cover.svg', N'projects', N'image/svg+xml', N'Dự án thanh lý nhà xưởng', 1280, 800, N'active'),
(N'news-01.svg', N'/assets/images/news/news-01.svg', N'news', N'image/svg+xml', N'Bảng giá phế liệu hôm nay', 1200, 675, N'active');

INSERT INTO dbo.AdminRoles (Name, Description)
VALUES (N'SuperAdmin', N'Quản trị toàn bộ hệ thống'), (N'Editor', N'Quản trị nội dung và SEO'), (N'Sales', N'Xử lý lead báo giá');

INSERT INTO dbo.AdminUsers (UserName, Email, DisplayName, PasswordHash, Status)
VALUES (N'admin', N'admin@example.com', N'[QUẢN TRỊ VIÊN]', N'CHANGE_ME_HASH_BEFORE_PRODUCTION', N'active');

INSERT INTO dbo.AdminUserRoles (AdminUserId, AdminRoleId)
SELECT u.Id, r.Id FROM dbo.AdminUsers u CROSS JOIN dbo.AdminRoles r WHERE u.UserName = N'admin' AND r.Name = N'SuperAdmin';

INSERT INTO dbo.MenuItems (Position, Label, Url, SortOrder, Status, PublishedAt)
VALUES
(N'header', N'Trang chủ', N'/', 1, N'published', SYSUTCDATETIME()),
(N'header', N'Giới thiệu', N'/gioi-thieu', 2, N'published', SYSUTCDATETIME()),
(N'header', N'Phế liệu thu mua', N'/phe-lieu', 3, N'published', SYSUTCDATETIME()),
(N'header', N'Dịch vụ', N'/dich-vu', 4, N'published', SYSUTCDATETIME()),
(N'header', N'Bảng giá', N'/bang-gia', 5, N'published', SYSUTCDATETIME()),
(N'header', N'Khu vực', N'/khu-vuc', 6, N'published', SYSUTCDATETIME()),
(N'header', N'Tin tức', N'/tin-tuc', 7, N'published', SYSUTCDATETIME()),
(N'header', N'Liên hệ', N'/lien-he', 8, N'published', SYSUTCDATETIME()),
(N'footer_scrap', N'Phế liệu đồng', N'/phe-lieu/dong', 1, N'published', SYSUTCDATETIME()),
(N'footer_services', N'Thu mua tận nơi', N'/dich-vu/thu-mua-tan-noi', 1, N'published', SYSUTCDATETIME()),
(N'footer_locations', N'Đồng Nai', N'/khu-vuc/dong-nai', 1, N'published', SYSUTCDATETIME());

INSERT INTO dbo.Pages (PageType, Title, Slug, RoutePath, Label, HeroTitle, HeroDescription, HeroImageUrl, Status, SortOrder, IsFeatured, PublishedAt)
VALUES
(N'home', N'Trang chủ', N'trang-chu', N'/', N'Trang chủ', N'Thu mua phế liệu giá tốt - tận nơi', N'Khảo sát nhanh, cân minh bạch, thanh toán ngay tại kho khách hàng.', N'/assets/images/hero/hero-main.svg', N'published', 1, 1, SYSUTCDATETIME()),
(N'page', N'Giới thiệu', N'gioi-thieu', N'/gioi-thieu', N'Giới thiệu', N'Về [TÊN CÔNG TY]', N'[10+] năm thu mua phế liệu tại Đồng Nai, TP.HCM, Bình Dương.', N'/assets/images/company/company-yard.svg', N'published', 2, 0, SYSUTCDATETIME()),
(N'page', N'Năng lực', N'nang-luc', N'/nang-luc', N'Năng lực', N'Năng lực thu mua & tháo dỡ', N'Đội xe, máy móc và nhân sự đủ sức xử lý lô hàng lớn.', N'/assets/images/company/company-truck.svg', N'published', 3, 0, SYSUTCDATETIME()),
(N'page', N'Phế liệu', N'phe-lieu', N'/phe-lieu', N'Phế liệu thu mua', N'Danh sách phế liệu thu mua', N'Đồng, sắt thép, nhôm, inox, điện tử, máy móc và phế liệu hỗn hợp.', N'/assets/images/scrap/scrap-copper.svg', N'published', 4, 1, SYSUTCDATETIME()),
(N'page', N'Bảng giá', N'bang-gia', N'/bang-gia', N'Bảng giá thu mua', N'Bảng giá phế liệu hôm nay', N'Giá thu mua tham khảo cập nhật [DD/MM/YYYY] cho phế liệu đồng, sắt, nhôm, inox.', N'/assets/images/company/company-scale.svg', N'published', 5, 1, SYSUTCDATETIME()),
(N'page', N'Dịch vụ', N'dich-vu', N'/dich-vu', N'Dịch vụ', N'Dịch vụ thu mua phế liệu', N'Tận nơi, nhà xưởng, công trình, máy móc và thu gom định kỳ.', N'/assets/images/hero/hero-01.svg', N'published', 6, 1, SYSUTCDATETIME()),
(N'page', N'Khu vực', N'khu-vuc', N'/khu-vuc', N'Khu vực', N'Khu vực thu mua phế liệu', N'Phục vụ Đồng Nai, TP.HCM, Bình Dương và các khu vực lân cận.', N'/assets/images/locations/location-map.svg', N'published', 7, 0, SYSUTCDATETIME()),
(N'page', N'Dự án', N'du-an', N'/du-an', N'Dự án', N'Dự án thu mua phế liệu tiêu biểu', N'Nhà xưởng, công trình, máy móc và thu gom định kỳ.', N'/assets/images/projects/project-03-cover.svg', N'published', 8, 0, SYSUTCDATETIME()),
(N'page', N'Tin tức', N'tin-tuc', N'/tin-tuc', N'Tin tức - kiến thức', N'Cập nhật giá & kinh nghiệm bán phế liệu', N'Tin giá, kiến thức phân loại và kinh nghiệm thanh lý.', N'/assets/images/news/news-01.svg', N'published', 9, 0, SYSUTCDATETIME()),
(N'page', N'Chính sách hoa hồng', N'hoa-hong', N'/hoa-hong', N'Chính sách hoa hồng', N'Giới thiệu nguồn phế liệu - nhận hoa hồng cao', N'Kết nối nguồn hàng nhà xưởng, công trình, kho bãi và nhận hoa hồng minh bạch.', N'/assets/images/hero/hero-01.svg', N'published', 10, 0, SYSUTCDATETIME()),
(N'page', N'Liên hệ', N'lien-he', N'/lien-he', N'Liên hệ', N'Liên hệ nhận báo giá thu mua', N'Gọi hotline, nhắn Zalo hoặc gửi hình phế liệu để được báo giá nhanh.', N'/assets/images/hero/hero-02.svg', N'published', 11, 0, SYSUTCDATETIME()),
(N'search', N'Tìm kiếm', N'tim-kiem', N'/tim-kiem', N'Tìm kiếm', N'Tìm kiếm', N'Tìm loại phế liệu, bảng giá, dịch vụ và tin tức.', N'/assets/images/hero/hero-02.svg', N'published', 12, 0, SYSUTCDATETIME()),
(N'error', N'404', N'404', N'/404', N'Không tìm thấy', N'Không tìm thấy trang', N'Trang bạn yêu cầu không tồn tại hoặc đã đổi đường dẫn.', N'/assets/images/hero/hero-02.svg', N'published', 99, 0, SYSUTCDATETIME());

INSERT INTO dbo.HomepageSections (SectionKey, Title, Subtitle, Teaser, ImageUrl, LinkText, LinkUrl, JsonData, Note, Status, SortOrder, IsFeatured, PublishedAt)
VALUES
(N'hero', N'THU MUA PHẾ LIỆU GIÁ TỐT - TẬN NƠI', N'Doanh nghiệp & cá nhân - thanh lý toàn bộ', N'Khảo sát nhanh, cân minh bạch, thanh toán ngay tại kho khách hàng.', N'/assets/images/hero/hero-main.svg', N'Gửi hình báo giá', N'#quoteModal', NULL, N'Hero chính trang chủ', N'published', 1, 1, SYSUTCDATETIME()),
(N'trust', N'Cam kết', NULL, N'Báo giá nhanh, thu gom tận nơi, cân minh bạch, thanh toán ngay.', NULL, NULL, NULL, NULL, NULL, N'published', 2, 0, SYSUTCDATETIME()),
(N'featured_scrap', N'Các loại phế liệu chúng tôi thu mua', N'Phế liệu thu mua', NULL, NULL, N'Xem tất cả loại phế liệu', N'/phe-lieu', NULL, NULL, N'published', 3, 1, SYSUTCDATETIME()),
(N'price_board', N'Bảng giá phế liệu hôm nay', N'Bảng giá tham khảo', NULL, NULL, N'Xem bảng giá đầy đủ', N'/bang-gia', NULL, N'Nguồn: admin Bảng giá', N'published', 4, 1, SYSUTCDATETIME()),
(N'story', N'Chúng tôi không chỉ thu mua phế liệu.', N'Về chúng tôi', N'Chúng tôi xử lý cả những lô hàng khó: nhà xưởng, dây chuyền cũ, công trình.', N'/assets/images/company/company-yard.svg', N'Tìm hiểu về công ty', N'/gioi-thieu', NULL, NULL, N'published', 5, 0, SYSUTCDATETIME()),
(N'stats', N'Năng lực qua số liệu', NULL, N'[10+] năm kinh nghiệm, [500+] dự án, [15] phương tiện.', NULL, NULL, NULL, NULL, NULL, N'published', 6, 0, SYSUTCDATETIME()),
(N'projects', N'Dự án thu mua gần đây', N'Dự án tiêu biểu', NULL, NULL, N'Xem tất cả dự án', N'/du-an', NULL, NULL, N'published', 7, 0, SYSUTCDATETIME()),
(N'process', N'Thu mua phế liệu 6 bước', N'Quy trình', N'Tiếp nhận - báo giá - khảo sát - chốt giá - thu gom - thanh toán.', NULL, NULL, NULL, NULL, NULL, N'published', 8, 0, SYSUTCDATETIME()),
(N'locations', N'Chúng tôi thu mua tại các khu vực sau', N'Khu vực hoạt động', NULL, NULL, N'Xem tất cả khu vực', N'/khu-vuc', NULL, NULL, N'published', 9, 0, SYSUTCDATETIME()),
(N'referral', N'Giới thiệu nguồn phế liệu - nhận hoa hồng cao', N'Chính sách hoa hồng', NULL, N'/assets/images/company/company-truck.svg', N'Xem mức hoa hồng', N'/hoa-hong', NULL, NULL, N'published', 10, 0, SYSUTCDATETIME()),
(N'news', N'Cập nhật giá & kinh nghiệm bán phế liệu', N'Tin tức - kiến thức', NULL, NULL, N'Xem tất cả bài viết', N'/tin-tuc', NULL, NULL, N'published', 11, 0, SYSUTCDATETIME()),
(N'faq', N'Khách hàng hay hỏi chúng tôi', N'Câu hỏi thường gặp', NULL, NULL, NULL, NULL, NULL, NULL, N'published', 12, 0, SYSUTCDATETIME()),
(N'final_cta', N'Bạn đang có phế liệu cần thanh lý?', N'Bán phế liệu ngay hôm nay', N'Gửi hình cho chúng tôi để nhận báo giá nhanh.', N'/assets/images/hero/hero-02.svg', N'Gửi hình nhận giá', N'#quoteModal', NULL, NULL, N'published', 13, 1, SYSUTCDATETIME());

INSERT INTO dbo.ScrapCategories (Name, Slug, Description, HeroTitle, HeroDescription, ImageUrl, SortOrder, IsFeatured, Status, PublishedAt)
VALUES
(N'Đồng', N'dong', N'Đồng đỏ, đồng vàng, dây điện và cáp đồng.', N'Thu mua phế liệu đồng giá cao', N'Đồng đỏ, đồng vàng, dây điện, đồng cáp - cân minh bạch, thanh toán ngay.', N'/assets/images/scrap/scrap-copper.svg', 1, 1, N'published', SYSUTCDATETIME()),
(N'Sắt thép', N'sat-thep', N'Sắt vụn, sắt công trình, thép hình, tôn.', N'Thu mua sắt thép phế liệu', N'Thu gom sắt thép công trình, nhà xưởng và dân dụng.', N'/assets/images/scrap/scrap-iron.svg', 2, 1, N'published', SYSUTCDATETIME()),
(N'Nhôm', N'nhom', N'Nhôm thanh, nhôm vụn, nhôm máy.', N'Thu mua phế liệu nhôm', N'Báo giá nhanh cho nhôm thanh, nhôm vụn, nhôm hợp kim.', N'/assets/images/scrap/scrap-aluminum.svg', 3, 1, N'published', SYSUTCDATETIME()),
(N'Inox', N'inox', N'Inox 304, inox 201 và inox hỗn hợp.', N'Thu mua phế liệu inox', N'Phân loại inox rõ ràng, giá theo mác và độ sạch.', N'/assets/images/scrap/scrap-stainless.svg', 4, 1, N'published', SYSUTCDATETIME()),
(N'Điện tử', N'dien-tu', N'Bo mạch, linh kiện, thiết bị điện tử.', N'Thu mua phế liệu điện tử', N'Nhận bo mạch, linh kiện và thiết bị điện tử lỗi.', N'/assets/images/scrap/scrap-board.svg', 5, 0, N'published', SYSUTCDATETIME()),
(N'Máy móc', N'may-moc', N'Motor, máy cũ, thiết bị công nghiệp.', N'Thu mua máy móc cũ', N'Mua motor, máy móc, dây chuyền cũ và thiết bị công nghiệp.', N'/assets/images/scrap/scrap-motor.svg', 6, 0, N'published', SYSUTCDATETIME()),
(N'Khác', N'khac', N'Phế liệu hỗn hợp, vật tư công trình.', N'Thu mua phế liệu hỗn hợp', N'Nhận nhà xưởng, công trình và các loại phế liệu khác.', N'/assets/images/scrap/scrap-misc.svg', 7, 0, N'published', SYSUTCDATETIME());

INSERT INTO dbo.ScrapItems (ScrapCategoryId, Name, Slug, ShortDescription, Description, PrimaryImage, Unit, PriceFrom, PriceTo, PriceNote, Status, SortOrder, IsFeatured, PublishedAt)
SELECT Id, N'Đồng đỏ sạch', N'dong-do', N'Đồng đỏ, đồng tím sạch, ít lẫn tạp chất.', N'Thu mua đồng đỏ sạch theo độ sạch thực tế, cân điện tử tại chỗ.', N'/assets/images/scrap/scrap-copper.svg', N'kg', 75000, 95000, N'Giá theo độ sạch và số lượng', N'published', 1, 1, SYSUTCDATETIME() FROM dbo.ScrapCategories WHERE Slug = N'dong'
UNION ALL SELECT Id, N'Đồng vàng / hợp kim', N'dong-vang-hop-kim', N'Đồng vàng, đồng thau, hợp kim đồng.', N'Báo giá theo tỷ lệ đồng và tình trạng lô hàng.', N'/assets/images/scrap/scrap-copper.svg', N'kg', 55000, 65000, N'Giá tham khảo', N'published', 2, 1, SYSUTCDATETIME() FROM dbo.ScrapCategories WHERE Slug = N'dong'
UNION ALL SELECT Id, N'Sắt vụn / sắt công trình', N'sat-vun-sat-cong-trinh', N'Sắt thép công trình, sắt vụn dân dụng.', N'Có xe và nhân công thu gom tận nơi.', N'/assets/images/scrap/scrap-iron.svg', N'kg', 6000, 9000, N'Giá theo loại sắt', N'published', 3, 1, SYSUTCDATETIME() FROM dbo.ScrapCategories WHERE Slug = N'sat-thep'
UNION ALL SELECT Id, N'Nhôm thanh / nhôm vụn', N'nhom-thanh-nhom-vun', N'Nhôm thanh, nhôm vụn, nhôm hợp kim.', N'Phân loại nhôm để báo giá chính xác.', N'/assets/images/scrap/scrap-aluminum.svg', N'kg', 22000, 35000, N'Giá theo độ sạch', N'published', 4, 1, SYSUTCDATETIME() FROM dbo.ScrapCategories WHERE Slug = N'nhom'
UNION ALL SELECT Id, N'Inox 304', N'inox-304', N'Inox 304 giá tốt, nhận tận nơi.', N'Giá theo mác inox và độ lẫn tạp chất.', N'/assets/images/scrap/scrap-stainless.svg', N'kg', 13000, 18000, N'Giá tham khảo', N'published', 5, 1, SYSUTCDATETIME() FROM dbo.ScrapCategories WHERE Slug = N'inox'
UNION ALL SELECT Id, N'Bo mạch điện tử', N'bo-mach-dien-tu', N'Bo mạch, linh kiện và thiết bị điện tử lỗi.', N'Nhận thu mua số lượng nhỏ và lô doanh nghiệp.', N'/assets/images/scrap/scrap-board.svg', N'kg', NULL, NULL, N'Gửi hình để báo giá', N'published', 6, 0, SYSUTCDATETIME() FROM dbo.ScrapCategories WHERE Slug = N'dien-tu'
UNION ALL SELECT Id, N'Motor / máy móc cũ', N'motor-may-moc-cu', N'Motor điện, máy cũ, thiết bị công nghiệp.', N'Có đội tháo dỡ và vận chuyển khi cần.', N'/assets/images/scrap/scrap-motor.svg', N'kg', NULL, NULL, N'Khảo sát thực tế', N'published', 7, 0, SYSUTCDATETIME() FROM dbo.ScrapCategories WHERE Slug = N'may-moc';

INSERT INTO dbo.ScrapItemImages (ScrapItemId, ImageUrl, AltText, Caption, SortOrder)
SELECT Id, PrimaryImage, Name, ShortDescription, 1 FROM dbo.ScrapItems;

INSERT INTO dbo.ScrapPrices (ScrapItemId, PriceFrom, PriceTo, PriceLabel, Unit, Trend, TrendPercent, EffectiveDate, Status)
SELECT Id, PriceFrom, PriceTo,
       CASE WHEN PriceFrom IS NULL THEN N'Liên hệ' ELSE FORMAT(PriceFrom, N'N0', N'vi-VN') + N' - ' + FORMAT(PriceTo, N'N0', N'vi-VN') END,
       Unit,
       CASE Slug WHEN N'dong-do' THEN N'up' WHEN N'sat-vun-sat-cong-trinh' THEN N'down' ELSE N'flat' END,
       CASE Slug WHEN N'dong-do' THEN 2 WHEN N'sat-vun-sat-cong-trinh' THEN -1 WHEN N'nhom-thanh-nhom-vun' THEN 1 ELSE 0 END,
       CONVERT(DATE, GETDATE()),
       N'published'
FROM dbo.ScrapItems;

INSERT INTO dbo.ScrapPriceHistory (ScrapPriceId, PriceFrom, PriceTo, PriceLabel, Unit, Note)
SELECT Id, PriceFrom, PriceTo, PriceLabel, Unit, N'Seed giá ban đầu theo template' FROM dbo.ScrapPrices;

INSERT INTO dbo.Services (Title, Slug, Excerpt, ContentHtml, CoverImage, SortOrder, IsFeatured, Status, PublishedAt)
VALUES
(N'Thu mua phế liệu tận nơi', N'thu-mua-tan-noi', N'Đội xe đến tận nơi cân và thanh toán ngay.', N'<p>Thu mua phế liệu tận nơi cho cá nhân, kho xưởng và công trình.</p>', N'/assets/images/company/company-warehouse.svg', 1, 1, N'published', SYSUTCDATETIME()),
(N'Thu mua phế liệu doanh nghiệp', N'thu-mua-phe-lieu-doanh-nghiep', N'Hợp đồng định kỳ, chứng từ rõ ràng.', N'<p>Phục vụ doanh nghiệp cần quy trình và lịch thu gom ổn định.</p>', N'/assets/images/hero/hero-01.svg', 2, 1, N'published', SYSUTCDATETIME()),
(N'Thu mua & thanh lý nhà xưởng', N'thanh-ly-nha-xuong', N'Một đầu mối: định giá, tháo dỡ, vận chuyển, bàn giao.', N'<p>Thanh lý nhà xưởng trọn gói, có đội tháo dỡ và xe tải.</p>', N'/assets/images/projects/project-01-cover.svg', 3, 1, N'published', SYSUTCDATETIME()),
(N'Thu mua phế liệu công trình', N'thu-mua-phe-lieu-cong-trinh', N'Nhận sắt thép, tôn, dây điện tại công trình.', N'<p>Điều phối nhân công và xe theo tiến độ công trình.</p>', N'/assets/images/projects/project-02-cover.svg', 4, 0, N'published', SYSUTCDATETIME()),
(N'Thu mua máy móc cũ', N'thu-mua-may-moc-cu', N'Motor, dây chuyền, thiết bị công nghiệp.', N'<p>Khảo sát máy móc cũ và báo giá theo tình trạng thực tế.</p>', N'/assets/images/scrap/scrap-motor.svg', 5, 0, N'published', SYSUTCDATETIME()),
(N'Thu gom định kỳ', N'thu-gom-dinh-ky', N'Lịch thu gom định kỳ cho nhà máy và khu công nghiệp.', N'<p>Thiết lập lịch thu gom, cân đối soát và thanh toán định kỳ.</p>', N'/assets/images/company/company-yard.svg', 6, 0, N'published', SYSUTCDATETIME());

INSERT INTO dbo.Locations (Province, District, Name, Slug, Excerpt, ContentHtml, CoverImage, SortOrder, IsFeatured, Status, PublishedAt)
VALUES
(N'Đồng Nai', N'Biên Hòa', N'Thu mua phế liệu tại Biên Hòa', N'dong-nai', N'Phủ toàn tỉnh Đồng Nai, ưu tiên Biên Hòa, Long Thành, Nhơn Trạch.', N'<p>Thu mua phế liệu tận nơi tại Đồng Nai.</p>', N'/assets/images/locations/location-dongnai.svg', 1, 1, N'published', SYSUTCDATETIME()),
(N'TP. Hồ Chí Minh', N'Thủ Đức', N'Thu mua phế liệu tại TP.HCM', N'tp-ho-chi-minh', N'Phục vụ Thủ Đức, Quận 7, Quận 12, Bình Tân, Hóc Môn, Củ Chi.', N'<p>Thu mua phế liệu tận nơi tại TP.HCM.</p>', N'/assets/images/locations/location-hcm.svg', 2, 1, N'published', SYSUTCDATETIME()),
(N'Bình Dương', N'Dĩ An', N'Thu mua phế liệu tại Bình Dương', N'binh-duong', N'Phục vụ Dĩ An, Thuận An, Bến Cát, Tân Uyên.', N'<p>Thu mua phế liệu tận nơi tại Bình Dương.</p>', N'/assets/images/locations/location-binhduong.svg', 3, 1, N'published', SYSUTCDATETIME());

INSERT INTO dbo.Projects (Title, Slug, ProjectType, LocationText, Excerpt, ContentHtml, CoverImage, CompletedAt, QuantityText, DurationText, SortOrder, IsFeatured, Status, PublishedAt)
VALUES
(N'Tháo dỡ - thanh lý nhà xưởng 2.000m²', N'thanh-ly-nha-xuong-bien-hoa', N'Nhà xưởng', N'Biên Hòa, Đồng Nai', N'45 tấn sắt thép, tôn và dây điện hoàn tất trong 5 ngày.', N'<p>Dự án tháo dỡ và thanh lý nhà xưởng tại Biên Hòa.</p>', N'/assets/images/projects/project-01-cover.svg', '2026-08-16', N'45 tấn', N'5 ngày', 1, 1, N'published', SYSUTCDATETIME()),
(N'Thu gom 40 tấn sắt công trình', N'thu-gom-sat-cong-trinh-thu-duc', N'Công trình', N'Thủ Đức, TP.HCM', N'Thu gom sắt thép công trình, cân và thanh toán trong ngày.', N'<p>Thu gom phế liệu công trình cao ốc.</p>', N'/assets/images/projects/project-02-cover.svg', '2026-07-20', N'40 tấn', N'2 ngày', 2, 1, N'published', SYSUTCDATETIME()),
(N'Mua dây chuyền sản xuất đã qua sử dụng', N'thanh-ly-day-chuyen-san-xuat-di-an', N'Máy móc', N'Dĩ An, Bình Dương', N'Mua lại dây chuyền máy móc cũ và vận chuyển khỏi xưởng.', N'<p>Thanh lý dây chuyền máy móc sản xuất.</p>', N'/assets/images/projects/project-03-cover.svg', '2026-06-18', N'1 dây chuyền', N'3 ngày', 3, 1, N'published', SYSUTCDATETIME());

INSERT INTO dbo.ProjectImages (ProjectId, ImageUrl, AltText, Caption, SortOrder)
SELECT Id, CoverImage, Title, Excerpt, 1 FROM dbo.Projects;

INSERT INTO dbo.PostCategories (Name, Slug, Description, SortOrder, IsFeatured, Status, PublishedAt)
VALUES
(N'Bảng giá', N'bang-gia', N'Cập nhật bảng giá phế liệu.', 1, 1, N'published', SYSUTCDATETIME()),
(N'Kiến thức', N'kien-thuc', N'Kiến thức phân loại và bán phế liệu.', 2, 1, N'published', SYSUTCDATETIME()),
(N'Kinh nghiệm', N'kinh-nghiem', N'Kinh nghiệm thanh lý và bán phế liệu.', 3, 0, N'published', SYSUTCDATETIME());

INSERT INTO dbo.Posts (PostCategoryId, Title, Slug, Excerpt, ContentHtml, CoverImage, AuthorName, SortOrder, IsFeatured, Status, PublishedAt)
SELECT Id, N'Bảng giá phế liệu hôm nay: đồng tăng nhẹ, sắt đi ngang', N'gia-phe-lieu-hom-nay', N'Tổng hợp biến động giá thu mua phế liệu trong tuần.', N'<p>Đồng đỏ tiếp tục tăng nhẹ, sắt thép giữ ổn định.</p>', N'/assets/images/news/news-01.svg', N'[QUẢN TRỊ VIÊN]', 1, 1, N'published', SYSUTCDATETIME() FROM dbo.PostCategories WHERE Slug = N'bang-gia'
UNION ALL SELECT Id, N'Cách phân loại dây điện đồng để bán được giá cao', N'cach-phan-loai-day-dien-dong', N'Chuẩn bị dây điện và phân loại lõi đồng trước khi bán.', N'<p>Phân loại dây điện giúp báo giá chính xác hơn.</p>', N'/assets/images/news/news-02.svg', N'[QUẢN TRỊ VIÊN]', 2, 1, N'published', SYSUTCDATETIME() FROM dbo.PostCategories WHERE Slug = N'kien-thuc'
UNION ALL SELECT Id, N'Thanh lý nhà xưởng: cần chuẩn bị gì để không bị ép giá?', N'thanh-ly-nha-xuong-can-chuan-bi-gi', N'Checklist trước khi thanh lý nhà xưởng và máy móc cũ.', N'<p>Chuẩn bị danh mục tài sản, hình ảnh và thời gian bàn giao.</p>', N'/assets/images/news/news-03.svg', N'[QUẢN TRỊ VIÊN]', 3, 0, N'published', SYSUTCDATETIME() FROM dbo.PostCategories WHERE Slug = N'kinh-nghiem';

INSERT INTO dbo.FaqItems (EntityType, EntityId, RoutePath, Question, Answer, SortOrder, IsFeatured, Status, PublishedAt)
VALUES
(N'Page', NULL, N'/', N'Chỉ có ít phế liệu dưới 100kg có bán được không?', N'Được. Với số lượng nhỏ bạn có thể mang tới kho hoặc liên hệ để được tư vấn tuyến xe phù hợp.', 1, 1, N'published', SYSUTCDATETIME()),
(N'Page', NULL, N'/', N'Giá thu mua thay đổi như thế nào?', N'Giá biến động theo thị trường và được chốt theo hình ảnh hoặc khảo sát thực tế.', 2, 1, N'published', SYSUTCDATETIME()),
(N'Page', NULL, N'/', N'Thanh toán bằng hình thức nào?', N'Thanh toán tiền mặt hoặc chuyển khoản ngay sau khi cân xong.', 3, 1, N'published', SYSUTCDATETIME()),
(N'Page', NULL, N'/bang-gia', N'Bảng giá có phải giá chốt cuối cùng không?', N'Bảng giá là giá tham khảo. Giá chốt phụ thuộc độ sạch, số lượng và vị trí thu gom.', 1, 1, N'published', SYSUTCDATETIME()),
(N'Page', NULL, N'/dich-vu', N'Có nhận tháo dỡ nhà xưởng không?', N'Có. Chúng tôi có đội tháo dỡ, xe tải và thiết bị hỗ trợ.', 1, 1, N'published', SYSUTCDATETIME()),
(N'Page', NULL, N'/khu-vuc/dong-nai', N'Bao lâu có mặt tại Đồng Nai?', N'Tùy khu vực và lô hàng, chúng tôi ưu tiên khảo sát nhanh trong ngày.', 1, 1, N'published', SYSUTCDATETIME()),
(N'Post', NULL, N'/tin-tuc/gia-phe-lieu-hom-nay', N'Nên bán phế liệu khi giá tăng không?', N'Nếu cần dòng tiền hoặc mặt bằng, bạn nên chốt giá theo lô sau khi gửi hình thực tế.', 1, 0, N'published', SYSUTCDATETIME()),
(N'ScrapItem', NULL, N'/phe-lieu/dong-do', N'Đồng đỏ sạch là gì?', N'Đồng đỏ sạch là đồng ít lẫn tạp chất, thường có giá thu mua cao hơn đồng hỗn hợp.', 1, 1, N'published', SYSUTCDATETIME());

INSERT INTO dbo.ContactRequests (Name, Phone, Email, Zalo, ScrapType, QuantityText, Area, Message, SourceForm, SourceUrl, Status)
VALUES
(N'Nguyễn Văn A', N'09xx xxx xxx', NULL, N'[ZALO]', N'Đồng / dây điện', N'Khoảng 2 tấn', N'Đồng Nai', N'Lead mẫu từ form báo giá nhanh.', N'quick_quote', N'/', N'new');

INSERT INTO dbo.SeoMetadata (EntityType, EntityId, RoutePath, SeoTitle, MetaDescription, Keywords, CanonicalUrl, OgTitle, OgDescription, OgImage, OgType, RobotsIndex, RobotsFollow, SchemaType, Status)
VALUES
(N'Page', NULL, N'/', N'Thu Mua Phế Liệu Giá Tốt - Tận Nơi Tại Đồng Nai, TP.HCM, Bình Dương | [TÊN CÔNG TY]', N'Thu mua phế liệu đồng, sắt, nhôm, inox, dây điện, máy móc cũ giá cao. Cân minh bạch, thu gom tận nơi, thanh toán ngay.', N'thu mua phế liệu, bảng giá phế liệu, phế liệu đồng', N'https://example.com/', N'Thu Mua Phế Liệu Giá Tốt - Tận Nơi | [TÊN CÔNG TY]', N'Thu mua phế liệu đồng, sắt, nhôm, inox giá cao - cân minh bạch, thanh toán ngay.', N'/assets/images/hero/hero-main.svg', N'website', 1, 1, N'Organization,LocalBusiness,FAQPage,BreadcrumbList', N'active'),
(N'Page', NULL, N'/gioi-thieu', N'Về [TÊN CÔNG TY] - [10+] Năm Thu Mua Phế Liệu Tận Tâm', N'Tìm hiểu [TÊN CÔNG TY] - [10+] năm thu mua phế liệu tại Đồng Nai, TP.HCM, Bình Dương.', NULL, N'https://example.com/gioi-thieu', N'Về [TÊN CÔNG TY]', N'Hành trình từ một xe tải nhỏ đến đội xe [15] phương tiện.', N'/assets/images/company/company-yard.svg', N'website', 1, 1, N'Organization,LocalBusiness,FAQPage,BreadcrumbList', N'active'),
(N'Page', NULL, N'/nang-luc', N'Năng Lực Thu Mua & Tháo Dỡ - Đội Xe, Máy Móc, Nhân Sự | [TÊN CÔNG TY]', N'Năng lực thu mua phế liệu: đội xe, xe cẩu, máy cắt và nhân sự kỹ thuật cho lô hàng lớn.', NULL, N'https://example.com/nang-luc', N'Năng Lực Thu Mua & Tháo Dỡ', N'Đủ sức nhận nhà xưởng, dây chuyền sản xuất, kết cấu công trình.', N'/assets/images/company/company-truck.svg', N'website', 1, 1, N'Organization,LocalBusiness,FAQPage,BreadcrumbList', N'active'),
(N'Page', NULL, N'/phe-lieu', N'Danh Sách Phế Liệu Thu Mua - Đồng, Sắt, Nhôm, Inox Giá Cao | [TÊN CÔNG TY]', N'Danh mục [20+] loại phế liệu chúng tôi thu mua: đồng đỏ, đồng vàng, dây điện, sắt thép, nhôm, inox, motor, bo mạch.', NULL, N'https://example.com/phe-lieu', N'Danh Sách Phế Liệu Thu Mua', N'Toàn bộ loại phế liệu đang thu mua, giá tham khảo theo kg.', N'/assets/images/scrap/scrap-copper.svg', N'website', 1, 1, N'Organization,LocalBusiness,FAQPage,BreadcrumbList', N'active'),
(N'Page', NULL, N'/bang-gia', N'Bảng Giá Phế Liệu Hôm Nay - Đồng, Sắt, Nhôm, Inox | [TÊN CÔNG TY]', N'Giá thu mua phế liệu tham khảo cập nhật hàng ngày: đồng, sắt thép, nhôm, inox, dây điện và vật liệu khác.', NULL, N'https://example.com/bang-gia', N'Bảng Giá Phế Liệu Hôm Nay', N'Giá thu mua phế liệu tham khảo cập nhật hàng ngày.', N'/assets/images/company/company-scale.svg', N'website', 1, 1, N'Organization,LocalBusiness,FAQPage,BreadcrumbList', N'active'),
(N'Page', NULL, N'/dich-vu', N'Dịch Vụ Thu Mua Phế Liệu - Tận Nơi, Nhà Xưởng, Công Trình, Máy Móc | [TÊN CÔNG TY]', N'Sáu nhóm dịch vụ thu mua phế liệu: tận nơi, hợp đồng doanh nghiệp, thanh lý nhà xưởng, công trình, máy móc, định kỳ.', NULL, N'https://example.com/dich-vu', N'Dịch Vụ Thu Mua Phế Liệu', N'Thu mua tận nơi, thanh lý nhà xưởng, công trình, máy móc.', N'/assets/images/hero/hero-01.svg', N'website', 1, 1, N'Organization,LocalBusiness,Service,FAQPage,BreadcrumbList', N'active'),
(N'Page', NULL, N'/khu-vuc', N'Khu Vực Thu Mua Phế Liệu - Đồng Nai, TP.HCM, Bình Dương | [TÊN CÔNG TY]', N'Thu mua phế liệu tận nơi tại Đồng Nai, TP.HCM, Bình Dương và khu vực lân cận.', NULL, N'https://example.com/khu-vuc', N'Khu Vực Thu Mua Phế Liệu', N'Phủ các khu vực trọng điểm miền Nam.', N'/assets/images/locations/location-map.svg', N'website', 1, 1, N'Organization,LocalBusiness,BreadcrumbList', N'active'),
(N'Page', NULL, N'/du-an', N'Dự Án Thu Mua Phế Liệu Tiêu Biểu - Nhà Xưởng, Công Trình, Máy Móc | [TÊN CÔNG TY]', N'Danh sách dự án thu mua phế liệu đã thực hiện: nhà xưởng, công trình, máy móc, thu gom định kỳ.', NULL, N'https://example.com/du-an', N'Dự Án Thu Mua Phế Liệu Tiêu Biểu', N'Một số hợp đồng tiêu biểu đã thực hiện.', N'/assets/images/projects/project-03-cover.svg', N'website', 1, 1, N'Organization,LocalBusiness,ItemList,BreadcrumbList', N'active'),
(N'Page', NULL, N'/tin-tuc', N'Tin tức - kiến thức phế liệu | [TÊN CÔNG TY]', N'Cập nhật giá phế liệu, kiến thức phân loại và kinh nghiệm bán phế liệu.', NULL, N'https://example.com/tin-tuc', N'Tin tức - kiến thức phế liệu', N'Cập nhật giá và kinh nghiệm bán phế liệu.', N'/assets/images/news/news-01.svg', N'website', 1, 1, N'Organization,LocalBusiness,ItemList,BreadcrumbList', N'active'),
(N'Page', NULL, N'/hoa-hong', N'Chính Sách Hoa Hồng Giới Thiệu Phế Liệu | [TÊN CÔNG TY]', N'Giới thiệu nguồn phế liệu cho chúng tôi và nhận hoa hồng theo giá trị lô hàng thực tế.', NULL, N'https://example.com/hoa-hong', N'Chính Sách Hoa Hồng Giới Thiệu Phế Liệu', N'Có nguồn nhà xưởng, công trình cần thanh lý phế liệu? Giới thiệu để nhận hoa hồng.', N'/assets/images/hero/hero-01.svg', N'website', 1, 1, N'Organization,FAQPage,BreadcrumbList', N'active'),
(N'Page', NULL, N'/lien-he', N'Liên hệ thu mua phế liệu - [TÊN CÔNG TY]', N'Gọi hotline, nhắn Zalo hoặc gửi hình phế liệu để nhận báo giá nhanh.', NULL, N'https://example.com/lien-he', N'Liên hệ thu mua phế liệu', N'Gửi hình phế liệu để được báo giá nhanh.', N'/assets/images/hero/hero-02.svg', N'website', 1, 1, N'Organization,LocalBusiness,BreadcrumbList', N'active'),
(N'Page', NULL, N'/tim-kiem', N'Tìm Kiếm - [TÊN CÔNG TY]', N'Tìm loại phế liệu, bảng giá thu mua, dịch vụ tận nơi và tin tức kiến thức.', NULL, N'https://example.com/tim-kiem', N'Tìm Kiếm - [TÊN CÔNG TY]', N'Tìm loại phế liệu, bảng giá, dịch vụ và tin tức.', N'/assets/images/hero/hero-02.svg', N'website', 0, 1, N'Organization,LocalBusiness,SearchAction', N'active'),
(N'Page', NULL, N'/404', N'404 - Không Tìm Thấy Trang | [TÊN CÔNG TY]', N'Trang không tồn tại hoặc đã đổi đường dẫn - quay về trang chủ để tiếp tục.', NULL, N'https://example.com/404', N'404 - Không Tìm Thấy Trang', N'Trang không tồn tại hoặc đã đổi đường dẫn.', N'/assets/images/hero/hero-02.svg', N'website', 0, 1, N'BreadcrumbList', N'active'),
(N'ScrapItem', (SELECT Id FROM dbo.ScrapItems WHERE Slug = N'dong-do'), N'/phe-lieu/dong-do', N'Thu Mua Đồng Đỏ Giá 75-95K/kg Tận Nơi - Đồng Tím Sạch | [TÊN CÔNG TY]', N'Thu mua đồng đỏ giá 75.000 - 95.000đ/kg tùy độ sạch. Cân minh bạch, thanh toán ngay.', NULL, N'https://example.com/phe-lieu/dong-do', N'Thu Mua Đồng Đỏ Giá 75-95K/kg Tận Nơi', N'Đồng đỏ sạch giá theo độ sạch thực tế.', N'/assets/images/scrap/scrap-copper.svg', N'website', 1, 1, N'Organization,LocalBusiness,Product,FAQPage,BreadcrumbList', N'active'),
(N'Service', (SELECT Id FROM dbo.Services WHERE Slug = N'thanh-ly-nha-xuong'), N'/dich-vu/thanh-ly-nha-xuong', N'Thu Mua & Thanh Lý Nhà Xưởng Trọn Gói - Tháo Dỡ, Vận Chuyển, Thanh Toán | [TÊN CÔNG TY]', N'Thanh lý nhà xưởng trọn gói: định giá, tháo dỡ, thu mua máy móc, vận chuyển và bàn giao mặt bằng.', NULL, N'https://example.com/dich-vu/thanh-ly-nha-xuong', N'Thu Mua & Thanh Lý Nhà Xưởng Trọn Gói', N'Một đầu mối cho định giá, tháo dỡ, vận chuyển, thanh toán.', N'/assets/images/projects/project-01-cover.svg', N'website', 1, 1, N'Organization,LocalBusiness,Service,FAQPage,BreadcrumbList', N'active'),
(N'Post', (SELECT Id FROM dbo.Posts WHERE Slug = N'gia-phe-lieu-hom-nay'), N'/tin-tuc/gia-phe-lieu-hom-nay', N'Bảng Giá Phế Liệu Tuần Này: Đồng Giữ Đà Tăng, Sắt Đi Ngang', N'Bảng giá phế liệu tuần này: đồng đỏ 75-95 nghìn/kg giữ đà tăng, sắt vụn 6-9 nghìn/kg đi ngang.', NULL, N'https://example.com/tin-tuc/gia-phe-lieu-hom-nay', N'Bảng Giá Phế Liệu Tuần Này', N'Giá đồng tiếp tục tăng, sắt thép giao dịch quanh mốc cũ.', N'/assets/images/news/news-01.svg', N'article', 1, 1, N'Organization,LocalBusiness,NewsArticle,FAQPage,BreadcrumbList', N'active'),
(N'Project', (SELECT Id FROM dbo.Projects WHERE Slug = N'thanh-ly-nha-xuong-bien-hoa'), N'/du-an/thanh-ly-nha-xuong-bien-hoa', N'Dự Án: Tháo Dỡ & Thanh Lý Nhà Xưởng 2.000m² Tại Biên Hòa | [TÊN CÔNG TY]', N'Dự án tháo dỡ và thanh lý nhà xưởng 2.000m² tại Biên Hòa: 45 tấn sắt thép hoàn tất trong 5 ngày.', NULL, N'https://example.com/du-an/thanh-ly-nha-xuong-bien-hoa', N'Dự Án: Tháo Dỡ & Thanh Lý Nhà Xưởng 2.000m²', N'45 tấn sắt thép, tôn và dây điện - bàn giao mặt bằng sạch.', N'/assets/images/projects/project-01-cover.svg', N'website', 1, 1, N'Organization,LocalBusiness,Article,ImageGallery,BreadcrumbList', N'active'),
(N'Location', (SELECT Id FROM dbo.Locations WHERE Slug = N'dong-nai'), N'/khu-vuc/dong-nai', N'Thu Mua Phế Liệu Tại Đồng Nai - Biên Hòa, Long Thành, Nhơn Trạch | [TÊN CÔNG TY]', N'Thu mua phế liệu tận nơi tại Đồng Nai: Biên Hòa, Long Thành, Nhơn Trạch, Trảng Bom, Vĩnh Cửu.', NULL, N'https://example.com/khu-vuc/dong-nai', N'Thu Mua Phế Liệu Tại Đồng Nai', N'Phủ toàn tỉnh Đồng Nai, khảo sát nhanh trong ngày.', N'/assets/images/locations/location-dongnai.svg', N'website', 1, 1, N'Organization,LocalBusiness,FAQPage,BreadcrumbList', N'active');

INSERT INTO dbo.SeoSitemapEntries (EntityType, EntityId, RoutePath, Priority, ChangeFrequency, IncludeInSitemap, LastModifiedAt)
SELECT EntityType, EntityId, RoutePath,
       CASE WHEN RoutePath = N'/' THEN 1.00 WHEN EntityType IN (N'ScrapItem', N'Service') THEN 0.80 ELSE 0.60 END,
       CASE WHEN RoutePath IN (N'/bang-gia', N'/tin-tuc') THEN N'daily' WHEN EntityType = N'Post' THEN N'weekly' ELSE N'monthly' END,
       CASE WHEN RobotsIndex = 1 THEN 1 ELSE 0 END,
       SYSUTCDATETIME()
FROM dbo.SeoMetadata
WHERE RoutePath IS NOT NULL;

INSERT INTO dbo.SeoRedirects (SourcePath, TargetPath, StatusCode, IsActive)
VALUES
(N'/about.html', N'/gioi-thieu', 301, 1),
(N'/prices.html', N'/bang-gia', 301, 1),
(N'/scrap.html', N'/phe-lieu', 301, 1),
(N'/services.html', N'/dich-vu', 301, 1),
(N'/contact.html', N'/lien-he', 301, 1);

INSERT INTO dbo.SeoAuditNotes (SeoMetadataId, EntityType, EntityId, RoutePath, Severity, Message, IsResolved)
SELECT Id, EntityType, EntityId, RoutePath, N'info', N'Seed SEO theo template. Cần thay placeholder [TÊN CÔNG TY], [HOTLINE], domain canonical trước production.', 0
FROM dbo.SeoMetadata
WHERE RoutePath IN (N'/', N'/bang-gia', N'/lien-he');

COMMIT TRANSACTION;

SELECT
    (SELECT COUNT(*) FROM dbo.SiteSettings) AS SiteSettingsCount,
    (SELECT COUNT(*) FROM dbo.Pages) AS PagesCount,
    (SELECT COUNT(*) FROM dbo.ScrapCategories) AS ScrapCategoriesCount,
    (SELECT COUNT(*) FROM dbo.ScrapItems) AS ScrapItemsCount,
    (SELECT COUNT(*) FROM dbo.ScrapPrices) AS ScrapPricesCount,
    (SELECT COUNT(*) FROM dbo.SeoMetadata) AS SeoMetadataCount,
    (SELECT COUNT(*) FROM dbo.FaqItems) AS FaqItemsCount;
