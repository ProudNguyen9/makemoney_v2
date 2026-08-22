/* ============================================================
   GỘP 9 DANH MỤC PHẾ LIỆU LẺ TEẾ THÀNH 6 NHÓM HỢP LÝ
   - Sắt thép    (3): sắt đặc, sắt rỉ sét, sắt vụn công trình (gộp từ "Phế liệu công trình")
   - Đồng        (5): đồng đỏ, đồng vàng, đồng cáp, đồng cháy, mạt đồng
   - Nhôm        (4): định hình, loại 1, lon, vụn
   - Inox        (4): 201, 304, 316, 430
   - Giấy & Nhựa (2): giấy carton, nhựa PET (gộp 2 nhóm 1-item)
   - Máy móc & Điện tử (2): máy móc cũ thanh lý, chì phế liệu (gộp 2 nhóm 1-item)
   ============================================================ */
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

-- 1) Chuyển items về nhóm đích
UPDATE dbo.ScrapItems SET ScrapCategoryId = 1 WHERE ScrapCategoryId = 8; -- công trình -> sắt thép
UPDATE dbo.ScrapItems SET ScrapCategoryId = 5 WHERE ScrapCategoryId = 6; -- nhựa -> giấy & nhựa
UPDATE dbo.ScrapItems SET ScrapCategoryId = 7 WHERE ScrapCategoryId = 9; -- điện tử -> máy móc & điện tử

-- 2) Đổi tên nhóm gọn (slug giữ nguyên, không ảnh hưởng URL public)
UPDATE dbo.ScrapCategories SET Name = N'Đồng',                SortOrder = 1 WHERE Id = 2;
UPDATE dbo.ScrapCategories SET Name = N'Sắt thép',            SortOrder = 2 WHERE Id = 1;
UPDATE dbo.ScrapCategories SET Name = N'Nhôm',                SortOrder = 3 WHERE Id = 3;
UPDATE dbo.ScrapCategories SET Name = N'Inox',                SortOrder = 4 WHERE Id = 4;
UPDATE dbo.ScrapCategories SET Name = N'Giấy & Nhựa',         SortOrder = 5 WHERE Id = 5;
UPDATE dbo.ScrapCategories SET Name = N'Máy móc & Điện tử',   SortOrder = 6 WHERE Id = 7;

-- 3) Xóa các nhóm đã rỗng
DELETE FROM dbo.ScrapCategories WHERE Id IN (6, 8, 9);

COMMIT TRANSACTION;

-- Kiểm tra kết quả
SELECT c.Id, c.Name, c.SortOrder, COUNT(i.Id) AS SoLoai
FROM dbo.ScrapCategories c
LEFT JOIN dbo.ScrapItems i ON i.ScrapCategoryId = c.Id
GROUP BY c.Id, c.Name, c.SortOrder
ORDER BY c.SortOrder;
