using System.ComponentModel.DataAnnotations;

namespace ScrapWebsite.Models;

public class ContactRequest
{
    public int Id { get; set; }

    [StringLength(100)]
    public string? Name { get; set; }

    [Required]
    [Phone]
    [StringLength(30)]
    public string Phone { get; set; } = string.Empty;

    [EmailAddress]
    [StringLength(255)]
    public string? Email { get; set; }

    [StringLength(80)]
    public string? Zalo { get; set; }

    [StringLength(180)]
    public string? ScrapType { get; set; }

    [StringLength(160)]
    public string? QuantityText { get; set; }

    [StringLength(160)]
    public string? Area { get; set; }

    public string? Message { get; set; }

    [StringLength(80)]
    public string SourceForm { get; set; } = "quick_quote";

    [StringLength(500)]
    public string? SourceUrl { get; set; }

    [StringLength(30)]
    public string Status { get; set; } = "new";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public List<ContactRequestFile> Files { get; set; } = new();
}
