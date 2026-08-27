namespace ScrapWebsite.ViewModels;

/// <summary>Bảng giá public /bang-gia — dữ liệu đọc trực tiếp từ DB (PRI-006).</summary>
public class PriceBoardViewModel
{
    public IReadOnlyList<PriceBoardGroup> Groups { get; set; } = [];

    public DateOnly? LastUpdatedAt =>
        Groups.SelectMany(group => group.Rows)
            .Select(row => row.EffectiveDate)
            .DefaultIfEmpty()
            .Max();
}

public class PriceBoardGroup
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>Id anchor cho nav: gia-{slug}.</summary>
    public string AnchorId => $"gia-{Slug}";

    public IReadOnlyList<PriceBoardRow> Rows { get; set; } = [];

    public DateOnly? UpdatedAt =>
        Rows.Select(row => row.EffectiveDate)
            .DefaultIfEmpty()
            .Max();
}

public class PriceBoardRow
{
    public int ItemId { get; set; }

    public string ItemName { get; set; } = string.Empty;

    public string? ShortDescription { get; set; }

    public string? Label { get; set; }

    public decimal? Value { get; set; }

    public string Unit { get; set; } = "kg";

    public DateOnly EffectiveDate { get; set; }

    public bool IsFirstOfItem { get; set; }

    public int RowSpan { get; set; } = 1;

    public string PriceText =>
        !string.IsNullOrWhiteSpace(Label)
            ? Label
            : Value.HasValue
                ? Value.Value.ToString("N0")
                : "Liên hệ";
}
