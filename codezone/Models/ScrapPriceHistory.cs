namespace ScrapWebsite.Models;

public class ScrapPriceHistory
{
    public int Id { get; set; }

    public int ScrapItemId { get; set; }

    public decimal? PriceValue { get; set; }

    public string? PriceUnit { get; set; }

    public string PriceType { get; set; } = "current";

    public string? Note { get; set; }

    public DateOnly EffectiveDate { get; set; }

    public DateTime RecordedAt { get; set; }

    public ScrapItem? ScrapItem { get; set; }
}
