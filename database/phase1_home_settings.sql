USE [ScrapWebsiteLocal];
GO

SET NOCOUNT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET CONCAT_NULL_YIELDS_NULL ON;
GO

MERGE dbo.SiteSettings AS target
USING (VALUES
    (N'home.price_updated_text', CONVERT(nvarchar(10), GETDATE(), 103), N'home', N'Ngày cập nhật bảng giá trang chủ'),
    (N'home.response_time_text', N'30 phút', N'home', N'Thời gian phản hồi báo giá'),
    (N'home.about_image_main', N'/assets/images/imported/brand/banner-1.jpg', N'home', N'Ảnh chính phần về chúng tôi'),
    (N'home.about_image_truck', N'/assets/images/imported/brand/banner-2.jpg', N'home', N'Ảnh phụ đội xe phần về chúng tôi'),
    (N'home.about_image_scale', N'/assets/images/imported/brand/banner-3.jpg', N'home', N'Ảnh phụ cân/kho phần về chúng tôi'),
    (N'home.project_image_1', N'/assets/images/imported/products/thumuasatvuncongtrinh8.jpg', N'home', N'Ảnh dự án nổi bật 1'),
    (N'home.project_image_2', N'/assets/images/imported/products/thumuamaymoccuthanhly1.jpg', N'home', N'Ảnh dự án nổi bật 2'),
    (N'home.project_image_3', N'/assets/images/imported/products/thumuadongcap1.jpg', N'home', N'Ảnh dự án nổi bật 3'),
    (N'home.referral_image', N'/assets/images/imported/brand/banner-2.jpg', N'home', N'Ảnh nền section giới thiệu nhận hoa hồng'),
    (N'home.final_cta_image', N'/assets/images/imported/brand/banner-3.jpg', N'home', N'Ảnh nền CTA cuối trang chủ')
) AS source(SettingKey, SettingValue, SettingGroup, Description)
ON target.SettingKey = source.SettingKey
WHEN MATCHED THEN
    UPDATE SET
        SettingValue = CASE
            WHEN target.SettingValue IS NULL OR LTRIM(RTRIM(target.SettingValue)) = N'' THEN source.SettingValue
            ELSE target.SettingValue
        END,
        SettingGroup = source.SettingGroup,
        Description = source.Description,
        UpdatedAt = SYSUTCDATETIME()
WHEN NOT MATCHED THEN
    INSERT (SettingKey, SettingValue, SettingGroup, Description, UpdatedAt)
    VALUES (source.SettingKey, source.SettingValue, source.SettingGroup, source.Description, SYSUTCDATETIME());
GO
