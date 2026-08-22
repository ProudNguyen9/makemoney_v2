/* Đổi slug nhóm phế liệu cho gọn — dùng cho trang nhóm /phe-lieu/nhom/{slug} */
SET NOCOUNT ON;
BEGIN TRANSACTION;
UPDATE dbo.ScrapCategories SET Slug = 'dong'             WHERE Id = 2;
UPDATE dbo.ScrapCategories SET Slug = 'sat-thep'         WHERE Id = 1;
UPDATE dbo.ScrapCategories SET Slug = 'nhom'             WHERE Id = 3;
UPDATE dbo.ScrapCategories SET Slug = 'inox'             WHERE Id = 4;
UPDATE dbo.ScrapCategories SET Slug = 'giay-nhua'        WHERE Id = 5;
UPDATE dbo.ScrapCategories SET Slug = 'may-moc-dien-tu'  WHERE Id = 7;
COMMIT TRANSACTION;
SELECT Id, Name, Slug, SortOrder FROM dbo.ScrapCategories ORDER BY SortOrder;
