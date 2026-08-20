using Microsoft.EntityFrameworkCore;
using ScrapWebsite.Models;

namespace ScrapWebsite.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<ScrapItem> ScrapItems => Set<ScrapItem>();

    public DbSet<ScrapCategory> ScrapCategories => Set<ScrapCategory>();

    public DbSet<ScrapImage> ScrapImages => Set<ScrapImage>();

    public DbSet<ScrapPrice> ScrapPrices => Set<ScrapPrice>();

    public DbSet<Post> Posts => Set<Post>();

    public DbSet<PostCategory> PostCategories => Set<PostCategory>();

    public DbSet<Banner> Banners => Set<Banner>();

    public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();

    public DbSet<ContactRequest> ContactRequests => Set<ContactRequest>();
}
