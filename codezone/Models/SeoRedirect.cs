namespace ScrapWebsite.Models;

public class SeoRedirect
{
    public int Id { get; set; }

    public string SourcePath { get; set; } = string.Empty;

    public string TargetPath { get; set; } = string.Empty;

    public int StatusCode { get; set; } = 301;

    public bool IsActive { get; set; } = true;
}
