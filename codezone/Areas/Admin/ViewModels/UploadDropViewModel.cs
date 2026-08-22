namespace codezone.Areas.Admin.ViewModels;

public sealed record UploadDropViewModel(
    string Label = "Kéo thả file vào đây hoặc bấm chọn",
    string Hint = "JPG/PNG/WebP, tối đa 5MB/ảnh",
    string InputName = "files",
    bool Multiple = true,
    bool Compact = false);
