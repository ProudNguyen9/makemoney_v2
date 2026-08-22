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

    public DbSet<ScrapPriceHistory> ScrapPriceHistory => Set<ScrapPriceHistory>();

    public DbSet<Post> Posts => Set<Post>();

    public DbSet<PostCategory> PostCategories => Set<PostCategory>();

    public DbSet<PostImage> PostImages => Set<PostImage>();

    public DbSet<Banner> Banners => Set<Banner>();

    public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();

    public DbSet<ContactRequest> ContactRequests => Set<ContactRequest>();

    public DbSet<MediaFile> MediaFiles => Set<MediaFile>();

    public DbSet<SeoMetadata> SeoMetadata => Set<SeoMetadata>();

    public DbSet<SeoSitemapEntry> SeoSitemapEntries => Set<SeoSitemapEntry>();

    public DbSet<SeoRedirect> SeoRedirects => Set<SeoRedirect>();

    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ScrapItem>()
            .Property(item => item.PriceFrom)
            .HasPrecision(18, 2);

        modelBuilder.Entity<ScrapPrice>()
            .Property(price => price.PriceValue)
            .HasPrecision(18, 2);

        modelBuilder.Entity<ScrapPriceHistory>()
            .Property(price => price.PriceValue)
            .HasPrecision(18, 2);

        modelBuilder.Entity<SeoSitemapEntry>()
            .Property(entry => entry.Priority)
            .HasPrecision(3, 2);

        modelBuilder.Entity<ScrapItem>()
            .HasOne(item => item.Category)
            .WithMany(category => category.ScrapItems)
            .HasForeignKey(item => item.ScrapCategoryId);

        modelBuilder.Entity<ScrapImage>()
            .ToTable("ScrapItemImages")
            .HasOne(image => image.ScrapItem)
            .WithMany(item => item.Images)
            .HasForeignKey(image => image.ScrapItemId);

        modelBuilder.Entity<ScrapPrice>()
            .HasOne(price => price.ScrapItem)
            .WithMany(item => item.Prices)
            .HasForeignKey(price => price.ScrapItemId);

        modelBuilder.Entity<ScrapPriceHistory>()
            .HasOne(price => price.ScrapItem)
            .WithMany()
            .HasForeignKey(price => price.ScrapItemId);

        modelBuilder.Entity<Post>()
            .Property(post => post.Content)
            .HasColumnName("ContentHtml");

        modelBuilder.Entity<Post>()
            .HasOne(post => post.Category)
            .WithMany(category => category.Posts)
            .HasForeignKey(post => post.PostCategoryId);

        modelBuilder.Entity<PostImage>()
            .HasOne(image => image.Post)
            .WithMany(post => post.Images)
            .HasForeignKey(image => image.PostId);

        modelBuilder.Entity<SiteSetting>()
            .Property(setting => setting.Key)
            .HasColumnName("SettingKey");

        modelBuilder.Entity<SiteSetting>()
            .Property(setting => setting.Value)
            .HasColumnName("SettingValue");

        modelBuilder.Entity<SiteSetting>()
            .Property(setting => setting.Group)
            .HasColumnName("SettingGroup");

        modelBuilder.Entity<AdminUser>()
            .HasIndex(user => user.Email)
            .IsUnique();

        modelBuilder.Entity<AdminUser>()
            .HasIndex(user => user.UserName)
            .IsUnique();

        modelBuilder.Entity<AdminUser>()
            .Property(user => user.Email)
            .HasMaxLength(255);

        modelBuilder.Entity<AdminUser>()
            .Property(user => user.UserName)
            .HasMaxLength(80);

        modelBuilder.Entity<AdminUser>()
            .Property(user => user.DisplayName)
            .HasMaxLength(160);

        modelBuilder.Entity<AdminUser>()
            .Property(user => user.Role)
            .HasMaxLength(50);

        modelBuilder.Entity<AdminUser>()
            .Property(user => user.PasswordHash)
            .HasMaxLength(500);

        modelBuilder.Entity<AdminUser>()
            .Property(user => user.Status)
            .HasMaxLength(30);
    }
}
