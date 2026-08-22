USE ScrapWebsiteLocal;
GO

SET NOCOUNT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET CONCAT_NULL_YIELDS_NULL ON;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ScrapItems_PublicCursor' AND object_id = OBJECT_ID(N'dbo.ScrapItems'))
BEGIN
    CREATE INDEX IX_ScrapItems_PublicCursor
    ON dbo.ScrapItems(Status, IsFeatured DESC, SortOrder ASC, PublishedAt DESC, Id DESC)
    INCLUDE (Name, Slug, ShortDescription, PrimaryImage, Unit, PriceFrom, PriceLabel, ScrapCategoryId);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ScrapItems_PublicSlug' AND object_id = OBJECT_ID(N'dbo.ScrapItems'))
BEGIN
    CREATE INDEX IX_ScrapItems_PublicSlug
    ON dbo.ScrapItems(Status, Slug)
    INCLUDE (Id, Name, ShortDescription, Description, PrimaryImage, Unit, PriceFrom, PriceLabel, ScrapCategoryId);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Posts_PublicCursor' AND object_id = OBJECT_ID(N'dbo.Posts'))
BEGIN
    CREATE INDEX IX_Posts_PublicCursor
    ON dbo.Posts(Status, IsFeatured DESC, SortOrder ASC, PublishedAt DESC, Id DESC)
    INCLUDE (Title, Slug, Excerpt, CoverImage, PostCategoryId, AuthorName);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Posts_PublicSlug' AND object_id = OBJECT_ID(N'dbo.Posts'))
BEGIN
    CREATE INDEX IX_Posts_PublicSlug
    ON dbo.Posts(Status, Slug)
    INCLUDE (Id, Title, Excerpt, ContentHtml, CoverImage, PostCategoryId, AuthorName, PublishedAt);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ScrapItemImages_PublicGallery' AND object_id = OBJECT_ID(N'dbo.ScrapItemImages'))
BEGIN
    CREATE INDEX IX_ScrapItemImages_PublicGallery
    ON dbo.ScrapItemImages(ScrapItemId, OrderIndex, Id)
    INCLUDE (ImageUrl, Caption);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PostImages_PublicGallery' AND object_id = OBJECT_ID(N'dbo.PostImages'))
BEGIN
    CREATE INDEX IX_PostImages_PublicGallery
    ON dbo.PostImages(PostId, OrderIndex, Id)
    INCLUDE (ImageUrl, Caption);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ScrapPrices_PublicLatest' AND object_id = OBJECT_ID(N'dbo.ScrapPrices'))
BEGIN
    CREATE INDEX IX_ScrapPrices_PublicLatest
    ON dbo.ScrapPrices(ScrapItemId, EffectiveDate DESC, Id DESC)
    INCLUDE (PriceValue, PriceLabel, Unit);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SeoMetadata_PublicRoute' AND object_id = OBJECT_ID(N'dbo.SeoMetadata'))
BEGIN
    CREATE INDEX IX_SeoMetadata_PublicRoute
    ON dbo.SeoMetadata(Status, RoutePath)
    INCLUDE (SeoTitle, MetaDescription, Keywords, CanonicalUrl, OgTitle, OgDescription, OgImage, OgType, RobotsIndex, RobotsFollow);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SeoMetadata_PublicEntity' AND object_id = OBJECT_ID(N'dbo.SeoMetadata'))
BEGIN
    CREATE INDEX IX_SeoMetadata_PublicEntity
    ON dbo.SeoMetadata(Status, EntityType, EntityId)
    INCLUDE (RoutePath, SeoTitle, MetaDescription, Keywords, CanonicalUrl, OgTitle, OgDescription, OgImage, OgType, RobotsIndex, RobotsFollow);
END
GO
