namespace ScrapWebsite.Models;

public class ScrapPrice
{
    public int Id { get; set; }

    public int ScrapItemId { get; set; }

    public decimal? PriceValue { get; set; }

    public string? PriceLabel { get; set; }

    public string Unit { get; set; } = "kg";

    public DateOnly EffectiveDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public ScrapItem? ScrapItem { get; set; }
}
