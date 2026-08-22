namespace codezone.ViewModels.Shared;

public sealed record PriceTableStaticViewModel(
    string Title,
    string UpdatedText,
    IReadOnlyList<(string Material, string Unit, string Price, string Note)> Rows);
