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

    public List<int> LinkedProductIds { get; set; } = new();

    public IReadOnlyList<AdminCategoryOptionDto> ProductOptions { get; set; } = Array.Empty<AdminCategoryOptionDto>();

    public IReadOnlyList<AdminCategoryOptionDto> Categories { get; set; } = Array.Empty<AdminCategoryOptionDto>();
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
