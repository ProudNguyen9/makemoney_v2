using codezone.ViewModels.Shared;

namespace ScrapWebsite.Services.Interfaces;

public interface ISiteChromeService
{
    Task<SiteChromeViewModel> GetAsync(CancellationToken cancellationToken);
}
