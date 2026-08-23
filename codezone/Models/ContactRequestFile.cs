namespace ScrapWebsite.Models;

public class ContactRequestFile
{
    public int Id { get; set; }

    public int ContactRequestId { get; set; }

    public ContactRequest? ContactRequest { get; set; }

    public int? MediaFileId { get; set; }

    public string FileUrl { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
