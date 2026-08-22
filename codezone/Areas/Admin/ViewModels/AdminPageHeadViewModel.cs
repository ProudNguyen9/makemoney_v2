namespace codezone.Areas.Admin.ViewModels;

public sealed record AdminPageHeadViewModel(
    string Title,
    string? Subtitle = null,
    string? ActionUrl = null,
    string? ActionText = null,
    string ActionIcon = "bi bi-plus-lg");
