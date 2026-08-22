using ScrapWebsite.ViewModels;

namespace ScrapWebsite.Services.Interfaces;

public interface IPublicHomeQueryService
{
    Task<HomeViewModel> GetHomeAsync(CancellationToken cancellationToken);
}
