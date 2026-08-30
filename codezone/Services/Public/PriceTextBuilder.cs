namespace ScrapWebsite.Services.Public;

public static class PriceTextBuilder
{
    public static string Build(decimal? priceValue, string? priceLabel, string? unit)
    {
        return priceValue.HasValue
            ? $"{priceValue.Value:N0}đ/{(string.IsNullOrWhiteSpace(unit) ? "kg" : unit)}"
            : !string.IsNullOrWhiteSpace(priceLabel)
                ? priceLabel
                : "Liên hệ báo giá";
    }
}
