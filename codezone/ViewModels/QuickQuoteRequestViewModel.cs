using System.ComponentModel.DataAnnotations;

namespace ScrapWebsite.ViewModels;

public sealed class QuickQuoteRequestViewModel
{
    [MaxLength(100)]
    public string? Name { get; set; }

    [Required]
    [Phone]
    [MaxLength(30)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(80)]
    public string? Zalo { get; set; }

    [Required]
    [MaxLength(180)]
    public string Scrap { get; set; } = string.Empty;

    [MaxLength(160)]
    public string? Quantity { get; set; }

    [Required]
    [MaxLength(160)]
    public string Area { get; set; } = string.Empty;

    public string? Note { get; set; }

    [MaxLength(500)]
    public string? SourceUrl { get; set; }

    public List<IFormFile> Images { get; set; } = new();
}
