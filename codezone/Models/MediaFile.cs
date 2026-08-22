namespace ScrapWebsite.Models;

public class MediaFile
{
    public int Id { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public string? Folder { get; set; }

    public string? MimeType { get; set; }

    public string? AltText { get; set; }

    public string Status { get; set; } = "active";
}
