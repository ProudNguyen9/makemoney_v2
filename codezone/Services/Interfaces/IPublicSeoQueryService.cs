using ScrapWebsite.ViewModels.Public;

namespace ScrapWebsite.Services.Interfaces;

public interface IPublicSeoQueryService
{
    Task<SeoDto> GetByRouteAsync(string routePath, SeoDto fallback, CancellationToken cancellationToken);

    Task<SeoDto> GetByEntityAsync(string entityType, int entityId, string routePath, SeoDto fallback, CancellationToken cancellationToken);
}
