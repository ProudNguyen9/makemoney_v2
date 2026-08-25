namespace ScrapWebsite.Models;

/// <summary>
/// Bản nháp tự lưu khi admin đang soạn/sửa bài viết.
/// PostKey: "post-{PostId}" (bài có sẵn) hoặc "new-{guid}" (bài chưa lưu lần nào).
/// </summary>
public class PostAutosave
{
    public string PostKey { get; set; } = string.Empty;

    public string DataJson { get; set; } = string.Empty;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
