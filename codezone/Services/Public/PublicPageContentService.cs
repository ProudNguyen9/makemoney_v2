using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ScrapWebsite.Data;
using ScrapWebsite.Services.Interfaces;

namespace ScrapWebsite.Services.Public;

public sealed class PublicPageContentService : IPublicPageContentService
{
    private const string CacheKey = "public:page-content-settings";
    private readonly AppDbContext _dbContext;
    private readonly IMemoryCache _cache;

    public PublicPageContentService(AppDbContext dbContext, IMemoryCache cache)
    {
        _dbContext = dbContext;
        _cache = cache;
    }

    public async Task<IReadOnlyDictionary<string, string>> GetSettingsAsync(CancellationToken cancellationToken)
    {
        var settings = await _cache.GetOrCreateAsync(CacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            entry.SlidingExpiration = TimeSpan.FromMinutes(1);

            var values = await _dbContext.SiteSettings
                .AsNoTracking()
                .Where(setting => setting.Key.StartsWith("public.") ||
                                  setting.Key.StartsWith("brand.") ||
                                  setting.Key.StartsWith("contact.") ||
                                  setting.Key.StartsWith("company.") ||
                                  setting.Key.StartsWith("home."))
                .Select(setting => new { setting.Key, setting.Value })
                .ToDictionaryAsync(setting => setting.Key, setting => setting.Value ?? string.Empty, cancellationToken);

            return values;
        });

        return settings ?? new Dictionary<string, string>();
    }
}
