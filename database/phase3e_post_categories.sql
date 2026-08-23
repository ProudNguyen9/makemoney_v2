/*
  Phase 3e — Add article categories.
  Idempotent: existing slugs are preserved and never inserted twice.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

USE ScrapWebsiteLocal;

IF NOT EXISTS (SELECT 1 FROM dbo.PostCategories WHERE Slug = N'bang-gia')
BEGIN
    INSERT INTO dbo.PostCategories (Id, Name, Slug, Description, SortOrder, Status)
    SELECT ISNULL(MAX(Id), 0) + 1, N'Bảng giá', N'bang-gia', N'Cập nhật bảng giá thu mua phế liệu mới nhất.', 3, N'published'
    FROM dbo.PostCategories;
END;

IF NOT EXISTS (SELECT 1 FROM dbo.PostCategories WHERE Slug = N'kien-thuc')
BEGIN
    INSERT INTO dbo.PostCategories (Id, Name, Slug, Description, SortOrder, Status)
    SELECT ISNULL(MAX(Id), 0) + 1, N'Kiến thức', N'kien-thuc', N'Kiến thức phân loại và nhận biết các loại phế liệu.', 4, N'published'
    FROM dbo.PostCategories;
END;

IF NOT EXISTS (SELECT 1 FROM dbo.PostCategories WHERE Slug = N'kinh-nghiem')
BEGIN
    INSERT INTO dbo.PostCategories (Id, Name, Slug, Description, SortOrder, Status)
    SELECT ISNULL(MAX(Id), 0) + 1, N'Kinh nghi' + NCHAR(7879) + N'm', N'kinh-nghiem', N'Kinh nghiem thanh ly, ban phe lieu va toi uu gia tri.', 5, N'published'
    FROM dbo.PostCategories;
END;

-- Repair names if this script was first executed through a non-UTF8 sqlcmd console.
UPDATE dbo.PostCategories
SET Name = N'B' + NCHAR(7843) + N'ng gi' + NCHAR(225)
WHERE Slug = N'bang-gia';

UPDATE dbo.PostCategories
SET Name = N'Ki' + NCHAR(7871) + N'n th' + NCHAR(7913) + N'c'
WHERE Slug = N'kien-thuc';

UPDATE dbo.PostCategories
SET Name = N'Kinh nghi' + NCHAR(7879) + N'm'
WHERE Slug = N'kinh-nghiem';
