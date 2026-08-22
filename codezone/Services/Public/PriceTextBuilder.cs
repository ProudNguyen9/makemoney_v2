namespace ScrapWebsite.Services.Public;

public static class PriceTextBuilder
{
    public static string Build(decimal? priceValue, string? priceLabel, string? unit)
    {
        if (!string.IsNullOrWhiteSpace(priceLabel))
        {
            return priceLabel;
        }

        return priceValue.HasValue
            ? $"{priceValue.Value:N0}đ/{(string.IsNullOrWhiteSpace(unit) ? "kg" : unit)}"
            : "Liên hệ báo giá";
    }
}
