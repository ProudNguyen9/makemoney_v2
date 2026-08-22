USE [ScrapWebsiteLocal];
GO

SET NOCOUNT ON;

-- Ensure every published post has at least one public image row for detail rendering.
INSERT INTO dbo.PostImages (PostId, ImageUrl, Caption, OrderIndex)
SELECT
    post.Id,
    COALESCE(NULLIF(LTRIM(RTRIM(post.CoverImage)), N''), N'/assets/images/imported/brand/seo-og-image.png'),
    post.Title,
    0
FROM dbo.Posts AS post
WHERE post.Status = N'published'
  AND NOT EXISTS (
      SELECT 1
      FROM dbo.PostImages AS image
      WHERE image.PostId = post.Id
  );

-- Fix empty or local-looking PostImages URLs with the post cover/fallback.
UPDATE image
SET image.ImageUrl = COALESCE(NULLIF(LTRIM(RTRIM(post.CoverImage)), N''), N'/assets/images/imported/brand/seo-og-image.png')
FROM dbo.PostImages AS image
INNER JOIN dbo.Posts AS post ON post.Id = image.PostId
WHERE image.ImageUrl IS NULL
   OR LTRIM(RTRIM(image.ImageUrl)) = N''
   OR image.ImageUrl LIKE N'../%'
   OR image.ImageUrl LIKE N'~/%'
   OR image.ImageUrl LIKE N'C:%'
   OR image.ImageUrl LIKE N'D:%';

-- Replace broken template inline image src values inside imported HTML.
DECLARE @PostId int;
DECLARE @Html nvarchar(max);
DECLARE @Replacement nvarchar(1024);
DECLARE @Needle nvarchar(128) = N'src="../assets/images/blogs/inline/';
DECLARE @Start int;
DECLARE @End int;

DECLARE post_cursor CURSOR LOCAL FAST_FORWARD FOR
SELECT
    post.Id,
    post.ContentHtml,
    COALESCE(
        (
            SELECT TOP (1) image.ImageUrl
            FROM dbo.PostImages AS image
            WHERE image.PostId = post.Id
              AND image.ImageUrl IS NOT NULL
              AND LTRIM(RTRIM(image.ImageUrl)) <> N''
            ORDER BY image.OrderIndex ASC, image.Id ASC
        ),
        NULLIF(LTRIM(RTRIM(post.CoverImage)), N''),
        N'/assets/images/imported/brand/seo-og-image.png'
    ) AS Replacement
FROM dbo.Posts AS post
WHERE post.ContentHtml LIKE N'%../assets/images/blogs/inline/%';

OPEN post_cursor;
FETCH NEXT FROM post_cursor INTO @PostId, @Html, @Replacement;

WHILE @@FETCH_STATUS = 0
BEGIN
    WHILE CHARINDEX(@Needle, @Html) > 0
    BEGIN
        SET @Start = CHARINDEX(@Needle, @Html);
        SET @End = CHARINDEX(N'"', @Html, @Start + LEN(@Needle));

        IF @End = 0
        BEGIN
            BREAK;
        END;

        SET @Html = STUFF(@Html, @Start, @End - @Start + 1, N'src="' + @Replacement + N'"');
    END;

    UPDATE dbo.Posts
    SET ContentHtml = @Html,
        UpdatedAt = SYSUTCDATETIME()
    WHERE Id = @PostId;

    FETCH NEXT FROM post_cursor INTO @PostId, @Html, @Replacement;
END;

CLOSE post_cursor;
DEALLOCATE post_cursor;

SELECT COUNT(*) AS BrokenInlineImages
FROM dbo.Posts
WHERE ContentHtml LIKE N'%../assets/images/blogs/inline/%';

SELECT COUNT(*) AS BadPostImageRows
FROM dbo.PostImages
WHERE ImageUrl IS NULL
   OR LTRIM(RTRIM(ImageUrl)) = N''
   OR ImageUrl LIKE N'../%'
   OR ImageUrl LIKE N'~/%'
   OR ImageUrl LIKE N'C:%'
   OR ImageUrl LIKE N'D:%';
GO
