using Microsoft.EntityFrameworkCore;
using ScrapWebsite.Data;
using ScrapWebsite.Services.Interfaces;
using ScrapWebsite.ViewModels.Public;

namespace ScrapWebsite.Services.Public;

public class PublicSeoQueryService : IPublicSeoQueryService
{
    private readonly AppDbContext _dbContext;

    public PublicSeoQueryService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SeoDto> GetByRouteAsync(string routePath, SeoDto fallback, CancellationToken cancellationToken)
    {
        var seo = await _dbContext.SeoMetadata
            .AsNoTracking()
            .Where(item => item.Status == PublicConstants.Active && item.RoutePath == routePath)
            .Select(item => new SeoDto(
                item.SeoTitle,
                item.MetaDescription ?? fallback.Description,
                item.Keywords,
                item.CanonicalUrl ?? item.RoutePath,
                item.OgTitle,
                item.OgDescription,
                item.OgImage,
                item.OgType,
                item.RobotsIndex,
                item.RobotsFollow))
            .FirstOrDefaultAsync(cancellationToken);

        return seo ?? fallback;
    }

    public async Task<SeoDto> GetByEntityAsync(string entityType, int entityId, string routePath, SeoDto fallback, CancellationToken cancellationToken)
    {
        var seo = await _dbContext.SeoMetadata
            .AsNoTracking()
            .Where(item => item.Status == PublicConstants.Active &&
                ((item.EntityType == entityType && item.EntityId == entityId) || item.RoutePath == routePath))
            .OrderByDescending(item => item.EntityType == entityType && item.EntityId == entityId)
            .Select(item => new SeoDto(
                item.SeoTitle,
                item.MetaDescription ?? fallback.Description,
                item.Keywords ?? fallback.Keywords,
                item.CanonicalUrl ?? item.RoutePath,
                item.OgTitle,
                item.OgDescription,
                item.OgImage,
                item.OgType,
                item.RobotsIndex,
                item.RobotsFollow))
            .FirstOrDefaultAsync(cancellationToken);

        return seo ?? fallback;
    }
}
