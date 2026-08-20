using ScrapWebsite.Models;

namespace ScrapWebsite.Data;

public static class SeedData
{
    public static IReadOnlyList<ScrapItem> FeaturedScrapItems { get; } =
    [
        new ScrapItem
        {
            Id = 1,
            Name = "Sat thep phe lieu",
            Slug = "sat-thep-phe-lieu",
            ShortDescription = "Thu mua sat thep cong trinh, nha xuong va dan dung.",
            PrimaryImage = "/images/shared/placeholder.svg",
            IsFeatured = true
        },
        new ScrapItem
        {
            Id = 2,
            Name = "Dong nhom inox",
            Slug = "dong-nhom-inox",
            ShortDescription = "Bao gia nhanh cho kim loai mau va hang cong nghiep.",
            PrimaryImage = "/images/shared/placeholder.svg",
            IsFeatured = true
        }
    ];

    public static IReadOnlyList<Post> LatestPosts { get; } =
    [
        new Post
        {
            Id = 1,
            Title = "Kinh nghiem ban phe lieu duoc gia",
            Slug = "kinh-nghiem-ban-phe-lieu-duoc-gia",
            Excerpt = "Chuan bi hinh anh, so luong va vi tri giup bao gia nhanh hon.",
            CoverImage = "/images/shared/placeholder.svg",
            PublishedAt = DateTime.UtcNow
        }
    ];
}
