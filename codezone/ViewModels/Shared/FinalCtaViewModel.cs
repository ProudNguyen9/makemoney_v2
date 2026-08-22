namespace codezone.ViewModels.Shared;

public sealed record FinalCtaViewModel(
    string Label,
    string Title,
    string Description,
    string? BackgroundImage = null,
    string TitleId = "cta-title");
