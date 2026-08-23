namespace ScrapWebsite.Services.Admin;

internal sealed record AdminMediaCatalogItem(
    string Key,
    string GroupKey,
    string GroupName,
    string Label,
    string Description,
    string RecommendedSize,
    string Folder,
    string NameHint,
    int MaxWidth,
    string FallbackUrl);

internal static class AdminMediaCatalog
{
    public static readonly IReadOnlyList<AdminMediaCatalogItem> Items =
    [
        new("brand.banner_1", "home-banner", "Trang chủ - Banner", "Banner 1", "Ảnh banner chính ở đầu trang chủ.", "1920 x 900px", "brand", "home-banner-1", 1920, "/assets/images/imported/brand/banner-1.jpg"),
        new("brand.banner_2", "home-banner", "Trang chủ - Banner", "Banner 2", "Ảnh banner phụ ở đầu trang chủ.", "1920 x 900px", "brand", "home-banner-2", 1920, "/assets/images/imported/brand/banner-2.jpg"),
        new("brand.banner_3", "home-banner", "Trang chủ - Banner", "Banner 3", "Ảnh banner phụ ở đầu trang chủ.", "1920 x 900px", "brand", "home-banner-3", 1920, "/assets/images/imported/brand/banner-3.jpg"),

        new("home.about_image_main", "home-about", "Trang chủ - Giới thiệu", "Ảnh chính giới thiệu", "Ảnh lớn trong section giới thiệu/năng lực.", "1200 x 800px", "content", "about-main", 1600, "/assets/images/imported/brand/banner-1.jpg"),
        new("home.about_image_truck", "home-about", "Trang chủ - Giới thiệu", "Ảnh đội xe", "Ảnh phụ về xe hoặc thu gom.", "1200 x 800px", "content", "about-truck", 1400, "/assets/images/imported/brand/banner-2.jpg"),
        new("home.about_image_scale", "home-about", "Trang chủ - Giới thiệu", "Ảnh cân/kho", "Ảnh phụ về cân, kho hoặc phân loại.", "1200 x 800px", "content", "about-scale", 1400, "/assets/images/imported/brand/banner-3.jpg"),

        new("home.project_image_1", "home-project", "Trang chủ - Dự án & CTA", "Ảnh dự án 1", "Ảnh minh họa dự án nổi bật đầu tiên.", "1200 x 800px", "content", "project-highlight-1", 1400, "/assets/images/imported/products/thumuasatvuncongtrinh8.jpg"),
        new("home.project_image_2", "home-project", "Trang chủ - Dự án & CTA", "Ảnh dự án 2", "Ảnh minh họa dự án nổi bật thứ hai.", "1200 x 800px", "content", "project-highlight-2", 1400, "/assets/images/imported/products/thumuamaymoccuthanhly1.jpg"),
        new("home.project_image_3", "home-project", "Trang chủ - Dự án & CTA", "Ảnh dự án 3", "Ảnh minh họa dự án nổi bật thứ ba.", "1200 x 800px", "content", "project-highlight-3", 1400, "/assets/images/imported/products/thumuadongcap1.jpg"),
        new("home.referral_image", "home-project", "Trang chủ - Dự án & CTA", "Ảnh hoa hồng", "Ảnh nền section giới thiệu nhận hoa hồng.", "1600 x 900px", "content", "referral", 1920, "/assets/images/imported/brand/banner-2.jpg"),
        new("home.final_cta_image", "home-project", "Trang chủ - Dự án & CTA", "Ảnh CTA cuối trang", "Ảnh nền khối kêu gọi liên hệ cuối trang chủ.", "1600 x 900px", "content", "final-cta", 1920, "/assets/images/imported/brand/banner-3.jpg"),

        new("news.hero_image", "news", "Trang tin tức", "Hero tin tức", "Ảnh nền/hero trang danh sách tin tức.", "1600 x 900px", "content", "news-hero", 1920, "/assets/images/imported/brand/seo-og-image.png"),
        new("public.image.news", "news", "Trang tin tức", "Ảnh tin tức mặc định", "Ảnh fallback cho các khối tin tức khi thiếu ảnh riêng.", "1200 x 675px", "content", "news-fallback", 1400, "/assets/images/imported/brand/seo-og-image.png"),

        new("public.contact.map_image", "contact", "Trang liên hệ", "Ảnh liên hệ/bản đồ", "Ảnh minh họa khu vực liên hệ hoặc bản đồ.", "1200 x 800px", "content", "contact-map", 1400, "/assets/images/imported/brand/banner-1.jpg"),

        new("brand.default_hero_image", "fallback", "Ảnh fallback toàn site", "Hero mặc định", "Ảnh hero mặc định cho các trang public khi thiếu ảnh riêng.", "1600 x 900px", "brand", "default-hero", 1920, "/assets/images/imported/brand/banner-1.jpg"),
        new("brand.default_cta_image", "fallback", "Ảnh fallback toàn site", "CTA mặc định", "Ảnh CTA mặc định cho các trang public khi thiếu ảnh riêng.", "1600 x 900px", "brand", "default-cta", 1920, "/assets/images/imported/brand/banner-3.jpg"),

        new("public.image.truck", "shared", "Ảnh public dùng chung", "Ảnh xe thu gom", "Ảnh fallback cho nội dung nói về xe/thu gom.", "1200 x 800px", "content", "public-truck", 1400, "/assets/images/imported/brand/banner-2.jpg"),
        new("public.image.yard", "shared", "Ảnh public dùng chung", "Ảnh kho/bãi", "Ảnh fallback cho sân kho hoặc nhà xưởng.", "1200 x 800px", "content", "public-yard", 1400, "/assets/images/imported/brand/banner-1.jpg"),
        new("public.image.team", "shared", "Ảnh public dùng chung", "Ảnh đội ngũ", "Ảnh fallback cho đội ngũ/nhân sự.", "1200 x 800px", "content", "public-team", 1400, "/assets/images/imported/brand/banner-3.jpg"),
        new("public.image.scale", "shared", "Ảnh public dùng chung", "Ảnh cân/phân loại", "Ảnh fallback cho cân, báo giá hoặc phân loại.", "1200 x 800px", "content", "public-scale", 1400, "/assets/images/imported/brand/banner-3.jpg"),
        new("public.image.scrap", "shared", "Ảnh public dùng chung", "Ảnh phế liệu", "Ảnh fallback cho loại phế liệu.", "1200 x 800px", "content", "public-scrap", 1400, "/assets/images/imported/products/thumuasatvuncongtrinh8.jpg"),
        new("public.image.project", "shared", "Ảnh public dùng chung", "Ảnh dự án", "Ảnh fallback cho dự án/công trình.", "1200 x 800px", "content", "public-project", 1400, "/assets/images/imported/products/thumuamaymoccuthanhly1.jpg"),

        new("brand.avatar", "brand-extra", "Thương hiệu phụ", "Avatar thương hiệu", "Ảnh đại diện thương hiệu dùng ở các vị trí phụ.", "512 x 512px", "brand", "brand-avatar", 800, "/assets/images/imported/brand/avatar.png"),
        new("brand.apple_touch_icon", "brand-extra", "Thương hiệu phụ", "Apple touch icon", "Icon khi lưu website ra màn hình chính iOS.", "512 x 512px", "brand", "apple-touch-icon", 512, "/assets/images/imported/brand/apple-touch-icon.png")
    ];

    public static AdminMediaCatalogItem? Find(string? key)
        => Items.FirstOrDefault(item => string.Equals(item.Key, key?.Trim(), StringComparison.OrdinalIgnoreCase));
}
