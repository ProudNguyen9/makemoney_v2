using System.ComponentModel.DataAnnotations;
using ScrapWebsite.Areas.Admin.ViewModels.Data;

namespace ScrapWebsite.Areas.Admin.ViewModels.Forms;

public sealed class ScrapPriceRowInput
{
    public string? Label { get; set; }

    [Range(0, 9_999_999_999, ErrorMessage = "Giá không hợp lệ.")]
    public decimal? PriceValue { get; set; }

    [MaxLength(50)]
    public string Unit { get; set; } = "kg";
}

public sealed class ScrapItemFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên loại phế liệu.")]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Slug { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn nhóm phế liệu.")]
    public int? CategoryId { get; set; }

    [MaxLength(500)]
    public string? ShortDescription { get; set; }

    public string? Description { get; set; }

    [MaxLength(255, ErrorMessage = "Giá tham khảo tối đa 255 ký tự.")]
    public string? PriceLabel { get; set; }

    [MaxLength(50)]
    public string? Unit { get; set; } = "kg";

    [MaxLength(30)]
    public string Status { get; set; } = "published";

    [Range(0, 9999)]
    public int SortOrder { get; set; }

    public bool IsFeatured { get; set; }

    public IFormFile? ThumbFile { get; set; }

    public IFormFile? BannerFile { get; set; }

    public string? CurrentThumbUrl { get; set; }

    public string? CurrentBannerUrl { get; set; }

    public bool RemoveThumb { get; set; }

    public bool RemoveBanner { get; set; }

    public List<ScrapPriceRowInput> PriceRows { get; set; } = new();

    public IReadOnlyList<AdminCategoryOptionDto> Categories { get; set; } = Array.Empty<AdminCategoryOptionDto>();
}

public sealed class ScrapCategoryFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên nhóm phế liệu.")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Slug { get; set; }

    public string? Description { get; set; }

    [Range(0, 9999)]
    public int SortOrder { get; set; }

    [MaxLength(30)]
    public string Status { get; set; } = "published";
}

public sealed class PriceBulkRowInput
{
    public int PriceId { get; set; }

    public int ScrapItemId { get; set; }

    public bool Selected { get; set; }

    [Range(0, 9_999_999_999, ErrorMessage = "Giá không hợp lệ.")]
    public decimal? PriceValue { get; set; }

    [MaxLength(50)]
    public string? Unit { get; set; } = "kg";
}

public sealed class ServiceFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tiêu đề dịch vụ.")]
    [MaxLength(220)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(180)]
    public string? Slug { get; set; }

    [MaxLength(120)]
    public string? IconCss { get; set; }

    [MaxLength(600)]
    public string? Excerpt { get; set; }

    public string? ContentHtml { get; set; }

    public IFormFile? CoverFile { get; set; }

    public string? CurrentCoverUrl { get; set; }

    public bool RemoveCover { get; set; }

    [MaxLength(30)]
    public string Status { get; set; } = "published";

    [Range(0, 9999)]
    public int SortOrder { get; set; }

    public bool IsFeatured { get; set; }
}

public sealed class LocationFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tỉnh / thành phố.")]
    [MaxLength(120)]
    public string Province { get; set; } = string.Empty;

    [MaxLength(160)]
    public string? District { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên khu vực.")]
    [MaxLength(180)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(180)]
    public string? Slug { get; set; }

    [MaxLength(600)]
    public string? Excerpt { get; set; }

    public string? ContentHtml { get; set; }

    public IFormFile? CoverFile { get; set; }

    public string? CurrentCoverUrl { get; set; }

    public bool RemoveCover { get; set; }

    [Range(-90, 90, ErrorMessage = "Vĩ độ từ -90 đến 90.")]
    public decimal? Latitude { get; set; }

    [Range(-180, 180, ErrorMessage = "Kinh độ từ -180 đến 180.")]
    public decimal? Longitude { get; set; }

    [MaxLength(30)]
    public string Status { get; set; } = "published";

    [Range(0, 9999)]
    public int SortOrder { get; set; }

    public bool IsFeatured { get; set; }
}

public sealed class PostFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tiêu đề bài viết.")]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Slug { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn chuyên mục.")]
    public int? PostCategoryId { get; set; }

    [MaxLength(700)]
    public string? Excerpt { get; set; }

    public string? Content { get; set; }

    public IFormFile? CoverFile { get; set; }

    public string? CurrentCoverUrl { get; set; }

    public bool RemoveCover { get; set; }

    [MaxLength(30)]
    public string Status { get; set; } = "published";

    public DateTime? PublishedAt { get; set; }

    [Range(0, 9999)]
    public int SortOrder { get; set; }

    public bool IsFeatured { get; set; }

    [MaxLength(120)]
    public string? AuthorName { get; set; }

    [MaxLength(255)]
    public string? SeoKeywords { get; set; }

    /// <summary>Khóa bản nháp tự lưu: "post-{id}" hoặc "new-{guid}".</summary>
    public string? AutosaveKey { get; set; }

    public DateTime? AutosavedAtUtc { get; set; }

    public bool RestoredFromAutosave { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }

    public List<int> LinkedProductIds { get; set; } = new();

    public IReadOnlyList<AdminLinkedProductDto> ProductOptions { get; set; } = Array.Empty<AdminLinkedProductDto>();

    public IReadOnlyList<AdminCategoryOptionDto> Categories { get; set; } = Array.Empty<AdminCategoryOptionDto>();
}

public sealed class BrandAssetsFormViewModel
{
    public IFormFile? LogoFile { get; set; }

    public IFormFile? FooterLogoFile { get; set; }
}

public sealed class CompanySettingsFormViewModel
{
    [Required]
    [MaxLength(160)]
    public string CompanyName { get; set; } = string.Empty;

    [MaxLength(60)]
    public string? TaxCode { get; set; }

    [MaxLength(300)]
    public string? Address { get; set; }

    [MaxLength(40)]
    public string? Hotline { get; set; }

    [MaxLength(80)]
    public string? Zalo { get; set; }

    [MaxLength(160)]
    public string? Email { get; set; }

    [MaxLength(120)]
    public string? WorkingHours { get; set; }

    [MaxLength(300)]
    public string? PurchaseAreas { get; set; }

    [MaxLength(300)]
    public string? Facebook { get; set; }
}

public sealed class HomepageSettingsFormViewModel
{
    [MaxLength(80)]
    public string? PriceUpdatedText { get; set; }

    [MaxLength(80)]
    public string? ResponseTimeText { get; set; }
}

public sealed class SmtpSettingsFormViewModel
{
    [Required]
    [MaxLength(160)]
    public string Host { get; set; } = string.Empty;

    [Range(1, 65535)]
    public int Port { get; set; } = 587;

    public bool EnableSsl { get; set; } = true;

    [MaxLength(160)]
    public string? UserName { get; set; }

    [MaxLength(200)]
    public string? Password { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(160)]
    public string FromEmail { get; set; } = string.Empty;

    [MaxLength(160)]
    public string? FromName { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string ToEmail { get; set; } = string.Empty;
}

public sealed class FaviconFormViewModel
{
    public IFormFile? FaviconFile { get; set; }
}

public sealed class MediaSettingImageFormViewModel
{
    [Required]
    [MaxLength(120)]
    public string Key { get; set; } = string.Empty;

    public IFormFile? ImageFile { get; set; }
}

public sealed class SeoMetadataFormViewModel
{
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string SeoTitle { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? MetaDescription { get; set; }

    [MaxLength(255)]
    public string? OgTitle { get; set; }

    [MaxLength(500)]
    public string? OgDescription { get; set; }

    [MaxLength(500)]
    public string? OgImage { get; set; }

    public bool RobotsIndex { get; set; }

    public bool RobotsFollow { get; set; }

    [MaxLength(30)]
    public string Status { get; set; } = "active";
}

public sealed class SeoSiteSettingsFormViewModel
{
    [MaxLength(255)]
    public string? SiteTitle { get; set; }

    [MaxLength(500)]
    public string? DefaultDescription { get; set; }

    [MaxLength(255)]
    public string? DefaultOgTitle { get; set; }

    [MaxLength(500)]
    public string? DefaultOgImage { get; set; }

    public IFormFile? DefaultOgImageFile { get; set; }
}

public sealed class FaqFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập câu hỏi.")]
    [MaxLength(500)]
    public string Question { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập câu trả lời.")]
    public string Answer { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn trang gán FAQ.")]
    [MaxLength(60)]
    public string EntityType { get; set; } = "home";

    [MaxLength(30)]
    public string Status { get; set; } = "published";

    [Range(0, 9999)]
    public int SortOrder { get; set; }
}

public sealed class ProjectGalleryRowInput
{
    public int Id { get; set; }

    public string? ImageUrl { get; set; }

    [MaxLength(255)]
    public string? AltText { get; set; }

    [Range(0, 9999)]
    public int SortOrder { get; set; }

    public bool Remove { get; set; }
}

public sealed class ProjectFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tiêu đề dự án.")]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(180)]
    public string? Slug { get; set; }

    [MaxLength(120)]
    public string? ProjectType { get; set; }

    [MaxLength(255)]
    public string? LocationText { get; set; }

    [MaxLength(700)]
    public string? Excerpt { get; set; }

    public string? ContentHtml { get; set; }

    public IFormFile? CoverFile { get; set; }

    public string? CurrentCoverUrl { get; set; }

    public bool RemoveCover { get; set; }

    public DateOnly? CompletedAt { get; set; }

    [MaxLength(120)]
    public string? QuantityText { get; set; }

    [MaxLength(120)]
    public string? DurationText { get; set; }

    [MaxLength(30)]
    public string Status { get; set; } = "published";

    [Range(0, 9999)]
    public int SortOrder { get; set; }

    public bool IsFeatured { get; set; }

    public List<ProjectGalleryRowInput> Gallery { get; set; } = new();

    public List<IFormFile> GalleryFiles { get; set; } = new();
}
