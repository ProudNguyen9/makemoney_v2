namespace ScrapWebsite.Services.Interfaces;

public interface IPublicPageContentService
{
    Task<IReadOnlyDictionary<string, string>> GetSettingsAsync(CancellationToken cancellationToken);
}
