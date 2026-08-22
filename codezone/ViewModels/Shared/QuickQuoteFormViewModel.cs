namespace codezone.ViewModels.Shared;

public sealed record QuickQuoteFormViewModel(
    string IdSuffix = "shared",
    bool ShowTitle = false,
    string Title = "Gửi hình — nhận báo giá nhanh",
    string Subtitle = "Điền thông tin, chúng tôi gọi lại chốt giá trong [30 phút].",
    string Hotline = "0974640626",
    string HotlineHref = "tel:0974640626");
