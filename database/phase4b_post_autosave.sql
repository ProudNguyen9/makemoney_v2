/*
  Phase 4b - Tự động lưu bản nháp khi đang soạn/sửa bài viết (auto-save).
  - Bảng dbo.PostAutosaves giữ nội dung đang soạn dạng JSON:
      + Bài MỚI hoặc bài NHÁP: tự động lưu thành bản nháp thật trong dbo.Posts,
        bảng này chỉ làm nơi dọn dẹp.
      + Bài ĐÃ XUẤT BẢN: nội dung đang sửa được lưu tạm tại đây (không đổi bài live),
        mở lại form sẽ thấy bản nháp tạm và bấm "Lưu" để áp dụng.
  - PostKey: "post-{PostId}" với bài có sẵn, "new-{guid}" với bài chưa lưu.
  Idempotent.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

USE ScrapWebsiteLocal;
GO

IF OBJECT_ID(N'dbo.PostAutosaves', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PostAutosaves (
        PostKey NVARCHAR(64) NOT NULL CONSTRAINT PK_PostAutosaves PRIMARY KEY,
        DataJson NVARCHAR(MAX) NOT NULL,
        UpdatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_PostAutosaves_UpdatedAt DEFAULT SYSUTCDATETIME()
    );
END
GO

PRINT N'Phase 4b (post_autosave) hoan tat.';
GO
