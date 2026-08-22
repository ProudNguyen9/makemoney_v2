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
    (N'site.name', N'Thành Trung', N'general', N'Tên công ty hiển thị ở header/footer'),
    (N'contact.working_hours', N'7:00 - 20:00', N'contact', N'Giờ làm việc hiển thị public'),
    (N'contact.warehouse_address', N'Hóc Môn, TP. Hồ Chí Minh', N'contact', N'Địa chỉ kho hiển thị ở footer'),
    (N'company.tax_code', N'Đang cập nhật', N'company', N'Mã số thuế hiển thị ở footer')
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
