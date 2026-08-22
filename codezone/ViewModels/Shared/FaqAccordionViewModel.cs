namespace codezone.ViewModels.Shared;

public sealed record FaqAccordionViewModel(string Id, IReadOnlyList<FaqItemViewModel> Items);
