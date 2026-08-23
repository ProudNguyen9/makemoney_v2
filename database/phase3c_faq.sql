/*
  Phase 3c — FAQ management.
  Creates dbo.FaqItems and seeds FAQs harvested from the previous static public pages.
  Idempotent.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET CONCAT_NULL_YIELDS_NULL ON;
GO

USE ScrapWebsiteLocal;
GO

IF OBJECT_ID(N'dbo.FaqItems', N'U') IS NULL
BEGIN
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
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_FaqItems_EntityType' AND object_id = OBJECT_ID(N'dbo.FaqItems'))
BEGIN
    CREATE INDEX IX_FaqItems_EntityType ON dbo.FaqItems(EntityType, Status, SortOrder);
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.FaqItems)
BEGIN
    INSERT INTO dbo.FaqItems (EntityType, Question, Answer, Status, SortOrder, PublishedAt) VALUES
    (N'home', N'Chỉ có ít phế liệu (dưới 100kg) có bán được không?',
     N'<p>Được. Với số lượng nhỏ bạn có thể mang tới kho của chúng tôi để cân bán ngay. Nếu ở gần khu vực trục chính, chúng tôi vẫn có thể ghé lấy khi có xe đi cùng tuyến.</p>',
     N'published', 1, SYSUTCDATETIME()),
    (N'home', N'Giá thu mua thay đổi như thế nào, khi nào cập nhật?',
     N'<p>Giá phế liệu biến động theo thị trường thế giới và tỷ giá. Bảng giá trên website được cập nhật [hàng ngày], giá chính xác được chốt khi chúng tôi xem hình hoặc khảo sát thực tế lô hàng của bạn.</p>',
     N'published', 2, SYSUTCDATETIME()),
    (N'home', N'Thanh toán bằng hình thức nào, có hóa đơn VAT không?',
     N'<p>Chúng tôi thanh toán tiền mặt ngay sau khi cân, hoặc chuyển khoản trong ngày. Với doanh nghiệp cần hóa đơn, chúng tôi xuất hóa đơn mua phế liệu đầy đủ theo quy định.</p>',
     N'published', 3, SYSUTCDATETIME()),
    (N'home', N'Có nhận tháo dỡ nhà xưởng, công trình luôn không?',
     N'<p>Có. Chúng tôi có đội tháo dỡ và xe cẩu, nhận giải phóng mặt bằng nhà xưởng, công trình — vừa tháo vừa thu mua phế liệu phát sinh. Xem <a href="/dich-vu">dịch vụ tháo dỡ</a> để biết thêm.</p>',
     N'published', 4, SYSUTCDATETIME()),

    (N'prices', N'Bảng giá này được cập nhật khi nào?',
     N'<p>Chúng tôi đối chiếu giá kim loại thế giới và giá các kho đầu mối mỗi sáng, bảng giá trên trang này cập nhật [hàng ngày]. Trong ngày nếu thị trường biến động mạnh, mức báo giá qua điện thoại có thể khác chút so với bảng — nhân viên luôn báo rõ trước khi hẹn khảo sát.</p>',
     N'published', 1, SYSUTCDATETIME()),
    (N'prices', N'Vì sao giá thực nhận khác giá trong bảng?',
     N'<p>Bảng giá là khung tham khảo cho hàng sạch, số lượng lớn. Lô của bạn có thể lẫn tạp chất, ướt nước hoặc nằm ở khu vực xa — sau khi xem hình hoặc đến khảo sát, nhân viên sẽ báo giá cụ thể kèm lý do chênh lệch, bạn hoàn toàn quyết định bán hay không.</p>',
     N'published', 2, SYSUTCDATETIME()),
    (N'prices', N'Lô hàng lớn có được giá tốt hơn không?',
     N'<p>Có. Với lô đồng từ 1 tấn, sắt thép từ 5 tấn hoặc thanh lý toàn bộ nhà xưởng, chúng tôi tính giá theo bậc khối lượng và cử xe chuyên dụng — mức cộng thêm có thể từ vài phần trăm tới hàng trăm nghìn đồng mỗi tấn so với giá lẻ trong bảng.</p>',
     N'published', 3, SYSUTCDATETIME()),
    (N'prices', N'Có xuất hóa đơn VAT khi thu mua không?',
     N'<p>Có. Doanh nghiệp cần hồ sơ đầy đủ sẽ được xuất hóa đơn mua phế liệu theo quy định, kèm phiếu cân và biên bản giao nhận. Bạn chỉ cần cung cấp mã số thuế và thông tin xuất hóa đơn trước khi thu gom.</p>',
     N'published', 4, SYSUTCDATETIME()),
    (N'prices', N'Khu vực nào được nhận thu gom tận nơi?',
     N'<p>Đội xe phủ Đồng Nai, TP. Hồ Chí Minh, Bình Dương, Bà Rịa – Vũng Tàu và Long An. Với lô lớn, chúng tôi vẫn nhận tại các tỉnh lân cận — xem chi tiết từng khu vực và bảng giá áp dụng tại trang <a href="/khu-vuc">khu vực thu mua</a>.</p>',
     N'published', 5, SYSUTCDATETIME()),

    (N'services', N'Có thu gom định kỳ cho văn phòng không, hay chỉ nhận nhà xưởng?',
     N'<p>Có. Văn phòng, tòa nhà chung cư vẫn ký được lịch thu gom định kỳ — chủ yếu là giấy, thùng carton, lon nhôm và dây điện cũ sau sửa chữa. Chúng tôi đặt thùng chứa tại khu vực bạn chỉ định và đến đúng lịch, không cần gọi từng lần.</p>',
     N'published', 1, SYSUTCDATETIME()),
    (N'services', N'Chi phí tháo dỡ tính như thế nào?',
     N'<p>Tùy khối lượng và độ phức tạp của công việc. Với nhà xưởng có phế liệu thu hồi, thông thường chi phí tháo dỡ được trừ trực tiếp vào giá trị phế liệu — khách hàng nhận phần chênh lệch, không phải trả tiền riêng hai lần.</p>',
     N'published', 2, SYSUTCDATETIME()),
    (N'services', N'Sau khi thu gom, có nhận luôn rác thải không phải phế liệu không?',
     N'<p>Phế liệu có giá trị thu mua thì chúng tôi nhận và trả tiền. Rác thải sinh hoạt như bọc nilon, thạch cao, rác xây không thể thu mua sẽ được thông báo rõ từ buổi khảo sát — nếu khối lượng lớn, chúng tôi hỗ trợ liên hệ đơn vị vận chuyển đúng quy định.</p>',
     N'published', 3, SYSUTCDATETIME()),
    (N'services', N'Muốn ký hợp đồng thu mua định kỳ thì cần chuẩn bị gì?',
     N'<p>Chỉ cần thông tin pháp nhân của doanh nghiệp (mã số thuế, người đại diện), danh mục phế liệu dự kiến phát sinh và tần suất thu gom mong muốn. Chúng tôi soạn hợp đồng mẫu, thống nhất biểu giá tham chiếu và lịch thu gom — ký một lần, mọi đợt sau chỉ cần cân và đối chiếu bảng kê.</p>',
     N'published', 4, SYSUTCDATETIME()),

    (N'referral', N'Khi nào tôi nhận được hoa hồng?',
     N'<p>Sau khi lô hàng hoàn tất thu gom và chúng tôi đã thanh toán cho bên bán, hoa hồng được chuyển khoản cho bạn trong [3] ngày làm việc. Với hợp đồng lớn có nhiều đợt thu gom, hoa hồng chi trả theo từng đợt tương ứng.</p>',
     N'published', 1, SYSUTCDATETIME()),
    (N'referral', N'Hoa hồng nhận bằng hình thức nào?',
     N'<p>Mặc định chuyển khoản về số tài khoản bạn cung cấp. Nếu bạn ở gần kho, có thể nhận tiền mặt trực tiếp tại văn phòng — báo trước qua Zalo để chúng tôi chuẩn bị.</p>',
     N'published', 2, SYSUTCDATETIME()),
    (N'referral', N'Tôi có phải đi cùng khi khảo sát không?',
     N'<p>Không bắt buộc. Nhưng nếu đi cùng, bạn nghe trực tiếp báo giá và chứng kiến cả quá trình cân — số tiền hoa hồng rõ ràng ngay từ đầu, không phải chờ đối chiếu lại sau.</p>',
     N'published', 3, SYSUTCDATETIME()),
    (N'referral', N'Giới thiệu nhiều lần cùng một nguồn tính sao?',
     N'<p>Tính cho lần giao dịch đầu tiên. Từ lần sau, nguồn hàng đó đã nằm trong hệ thống khách hàng của công ty — bạn hãy giữ liên hệ, chúng tôi ưu tiên chia sẻ nguồn mới cho người giới thiệu lâu năm.</p>',
     N'published', 4, SYSUTCDATETIME());
END
GO
