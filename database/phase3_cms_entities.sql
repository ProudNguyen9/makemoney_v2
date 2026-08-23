/*
  Phase 3 — Admin CRUD entities: Services, Locations, Projects, ProjectImages.
  Target database: ScrapWebsiteLocal (schema copied from database/scrap_cms.sql).
  Idempotent: only creates objects when missing; seeds sample rows only when the table is empty.
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

IF OBJECT_ID(N'dbo.Services', N'U') IS NULL
BEGIN
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
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_Services_Slug' AND object_id = OBJECT_ID(N'dbo.Services'))
BEGIN
    CREATE UNIQUE INDEX UX_Services_Slug ON dbo.Services(Slug) WHERE DeletedAt IS NULL;
END
GO

IF OBJECT_ID(N'dbo.Locations', N'U') IS NULL
BEGIN
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
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_Locations_Slug' AND object_id = OBJECT_ID(N'dbo.Locations'))
BEGIN
    CREATE UNIQUE INDEX UX_Locations_Slug ON dbo.Locations(Slug) WHERE DeletedAt IS NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Locations_Province' AND object_id = OBJECT_ID(N'dbo.Locations'))
BEGIN
    CREATE INDEX IX_Locations_Province ON dbo.Locations(Province, Status, SortOrder);
END
GO

IF OBJECT_ID(N'dbo.Projects', N'U') IS NULL
BEGIN
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
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_Projects_Slug' AND object_id = OBJECT_ID(N'dbo.Projects'))
BEGIN
    CREATE UNIQUE INDEX UX_Projects_Slug ON dbo.Projects(Slug) WHERE DeletedAt IS NULL;
END
GO

IF OBJECT_ID(N'dbo.ProjectImages', N'U') IS NULL
BEGIN
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
END
GO

/* ------------------------------------------------------------------
   Seed — ported from the static admin list views so lists are not empty.
   ------------------------------------------------------------------ */

IF NOT EXISTS (SELECT 1 FROM dbo.Services)
BEGIN
    INSERT INTO dbo.Services (Title, Slug, Excerpt, CoverImage, IconCss, Status, SortOrder, IsFeatured, PublishedAt) VALUES
    (N'Thu mua phế liệu tận nơi', N'thu-mua-phe-lieu-tan-noi', N'Dành cho cá nhân và hộ kinh doanh có phế liệu tồn tại nhà, tại cửa hàng — bạn chụp hình gửi qua Zalo, chúng tôi điều xe và nhân công đến cân tại chỗ.', '/assets/images/company/company-warehouse.svg', N'bi-truck', N'published', 1, 1, SYSUTCDATETIME()),
    (N'Thu mua phế liệu doanh nghiệp', N'thu-mua-phe-lieu-doanh-nghiep', N'Hợp đồng định kỳ cho nhà máy, xưởng cơ khí, doanh nghiệp sản xuất — đầy đủ hóa đơn, đối chiếu số liệu theo từng đợt thu gom.', '/assets/images/hero/hero-01.svg', N'bi-building', N'published', 2, 0, SYSUTCDATETIME()),
    (N'Thu mua & thanh lý nhà xưởng', N'thu-mua-thanh-ly-nha-xuong', N'Giải phóng mặt bằng trọn gói: định giá tổng thể, tháo dỡ kết cấu, thu mua phế liệu phát sinh và bàn giao mặt bằng sạch trong một hợp đồng duy nhất.', '/assets/images/projects/project-01-cover.svg', N'bi-house-gear', N'published', 3, 0, SYSUTCDATETIME()),
    (N'Thu mua phế liệu công trình', N'thu-mua-phe-lieu-cong-trinh', N'Nhận kết cấu thép, tôn lợp, giàn giáo, cây số và phế liệu phát sinh từ công trình xây dựng — sửa chữa — cải tạo.', '/assets/images/projects/project-02-cover.svg', N'bi-cone-striped', N'published', 4, 0, SYSUTCDATETIME()),
    (N'Thu mua máy móc cũ', N'thu-mua-may-moc-cu', N'Mua lại motor, máy sản xuất, dây chuyền đã qua sử dụng — định giá theo tình trạng máy, có kỹ thuật viên đến kiểm tra và tháo lắp.', '/assets/images/scrap/scrap-motor.svg', N'bi-gear-wide-connected', N'published', 5, 0, SYSUTCDATETIME()),
    (N'Thu gom phế liệu định kỳ', N'thu-gom-dinh-ky', N'Lịch thu gom cố định cho nhà máy, văn phòng, chung cư — đặt thùng chứa tại chỗ, nhân viên đến đúng hẹn theo tuần hoặc theo tháng.', '/assets/images/company/company-yard.svg', N'bi-calendar-check', N'published', 6, 0, SYSUTCDATETIME());
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Locations)
BEGIN
    INSERT INTO dbo.Locations (Province, District, Name, Slug, Excerpt, Status, SortOrder, IsFeatured, PublishedAt) VALUES
    (N'Đồng Nai', N'Biên Hòa', N'Thu mua phế liệu Biên Hòa', N'thu-mua-phe-lieu-bien-hoa', N'Thu mua phế liệu giá cao tại Biên Hòa — điều xe tận nơi trong ngày, cân minh bạch, thanh toán ngay.', N'published', 1, 1, SYSUTCDATETIME()),
    (N'Đồng Nai', N'Long Thành', N'Thu mua phế liệu Long Thành', N'thu-mua-phe-lieu-long-thanh', N'Nhận thu mua phế liệu tận nơi tại Long Thành — phục vụ cả khu công nghiệp Long Thành và khu dân cư.', N'published', 2, 0, SYSUTCDATETIME()),
    (N'Đồng Nai', N'Nhơn Trạch', N'Thu mua phế liệu Nhơn Trạch', N'thu-mua-phe-lieu-nhon-trach', N'Thu mua phế liệu tận nơi tại Nhơn Trạch — ký hợp đồng định kỳ cho nhà máy, xưởng sản xuất.', N'published', 3, 0, SYSUTCDATETIME()),
    (N'Đồng Nai', N'Trảng Bom', N'Thu mua phế liệu Trảng Bom', N'thu-mua-phe-lieu-trang-bom', N'Thu mua sắt thép, đồng, nhôm, giấy tại Trảng Bom — giá theo bảng giá cập nhật hàng tuần.', N'published', 4, 0, SYSUTCDATETIME()),
    (N'TP.HCM', N'Thủ Đức', N'Thu mua phế liệu Thủ Đức', N'thu-mua-phe-lieu-thu-duc', N'Thu mua phế liệu giá cao tại Thủ Đức — điều xe và nhân công đến cân tận nơi, thanh toán ngay trong buổi.', N'published', 5, 1, SYSUTCDATETIME()),
    (N'TP.HCM', N'Quận 12', N'Thu mua phế liệu Quận 12', N'thu-mua-phe-lieu-quan-12', N'Nhận thu mua phế liệu tận nơi tại Quận 12 — từ lô hộ kinh doanh nhỏ đến thanh lý nhà xưởng.', N'published', 6, 0, SYSUTCDATETIME()),
    (N'Bình Dương', N'Dĩ An', N'Thu mua phế liệu Dĩ An', N'thu-mua-phe-lieu-di-an', N'Thu mua phế liệu tận nơi tại Dĩ An — phục vụ khu công nghiệp và hộ kinh doanh, đủ chứng từ hóa đơn.', N'published', 7, 0, SYSUTCDATETIME()),
    (N'Bình Dương', N'Thuận An', N'Thu mua phế liệu Thuận An', N'thu-mua-phe-lieu-thuan-an', N'Thu mua phế liệu giá cao tại Thuận An — lịch thu gom định kỳ cho nhà máy, xưởng cơ khí.', N'published', 8, 0, SYSUTCDATETIME()),
    (N'Đồng Nai', N'Định Quán', N'Thu mua phế liệu Định Quán', N'thu-mua-phe-lieu-dinh-quan', N'Trang khu vực Định Quán đang hoàn thiện nội dung.', N'draft', 9, 0, NULL);
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Projects)
BEGIN
    INSERT INTO dbo.Projects (Title, Slug, ProjectType, LocationText, Excerpt, CoverImage, CompletedAt, QuantityText, DurationText, Status, SortOrder, IsFeatured, PublishedAt) VALUES
    (N'Tháo dỡ — thanh lý nhà xưởng 2.000m²', N'thao-do-thanh-ly-nha-xuong-2000m2', N'Nhà xưởng', N'Biên Hòa', N'Tháo dỡ kết cấu khung thép và thu mua toàn bộ phế liệu phát sinh — bàn giao mặt bằng sạch đúng tiến độ.', '/assets/images/projects/project-01-cover.svg', DATEADD(MONTH, -2, CONVERT(date, SYSUTCDATETIME())), N'45 tấn sắt thép', N'7 ngày', N'published', 1, 1, SYSUTCDATETIME()),
    (N'Thu mua phế liệu công trình cao ốc Thủ Đức', N'thu-mua-phe-lieu-cong-trinh-cao-oc-thu-duc', N'Công trình', N'Thủ Đức', N'Thu mua kết cấu thép, tôn lợp và giàn giáo phát sinh từ công trình cao ốc — thu gom theo nhịp công trường.', '/assets/images/projects/project-02-cover.svg', DATEADD(MONTH, -3, CONVERT(date, SYSUTCDATETIME())), N'28 tấn thép', N'10 ngày', N'published', 2, 0, SYSUTCDATETIME()),
    (N'Thanh lý 3,2 tấn dây cáp đồng nhà máy dệt', N'thanh-ly-3-2-tan-day-cap-dong-nha-may-det', N'Đồng', N'Dĩ An', N'Thanh lý toàn bộ dây cáp đồng cũ của nhà máy dệt — bóc tách, cân và thanh toán ngay tại xưởng.', '/assets/images/projects/project-03-cover.svg', DATEADD(MONTH, -4, CONVERT(date, SYSUTCDATETIME())), N'3,2 tấn đồng', N'1 buổi', N'published', 3, 0, SYSUTCDATETIME()),
    (N'Thu mua 18 tấn sắt thép công trình cầu', N'thu-mua-18-tan-sat-thep-cong-trinh-cau', N'Sắt', N'Long Thành', N'Thu mua 18 tấn sắt thép thanh dư và kết cấu tạm từ công trình cầu — cắt chia theo yêu cầu trước khi vận chuyển.', '/assets/images/projects/project-04-cover.svg', DATEADD(MONTH, -5, CONVERT(date, SYSUTCDATETIME())), N'18 tấn sắt thép', N'3 ngày', N'published', 4, 0, SYSUTCDATETIME()),
    (N'Thanh lý dây chuyền máy móc xưởng gỗ', N'thanh-ly-day-chuyen-may-moc-xuong-go', N'Máy móc', N'Bến Cát', N'Thanh lý dây chuyền máy móc sản xuất của xưởng gỗ — kỹ thuật viên kiểm tra, định giá và tháo lắp an toàn.', '/assets/images/projects/project-05-cover.svg', DATEADD(MONTH, -6, CONVERT(date, SYSUTCDATETIME())), N'1 dây chuyền', N'2 ngày', N'published', 5, 1, SYSUTCDATETIME()),
    (N'Thu gom phế liệu định kỳ khu chế xuất', N'thu-gom-phe-lieu-dinh-ky-khu-che-xuat', N'Khác', N'Nhơn Trạch', N'Ký lịch thu gom định kỳ theo tuần cho khu chế xuất — đặt thùng chứa tại chỗ, đối chiếu bảng kê từng đợt.', '/assets/images/projects/project-06-cover.svg', DATEADD(MONTH, -1, CONVERT(date, SYSUTCDATETIME())), N'Định kỳ hàng tuần', N'Thu gom theo tuần', N'published', 6, 0, SYSUTCDATETIME());
END
GO
