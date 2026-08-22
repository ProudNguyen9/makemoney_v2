using ScrapWebsite.Areas.Admin.ViewModels.Data;

namespace ScrapWebsite.Services.Admin;

public interface IAdminDashboardQueryService
{
    Task<AdminDashboardViewModel> GetDashboardAsync(CancellationToken cancellationToken);
}

public interface IAdminScrapQueryService
{
    Task<AdminScrapListViewModel> GetScrapListAsync(string? group, string? status, string? query, CancellationToken cancellationToken);
}

public interface IAdminArticleQueryService
{
    Task<AdminArticleListViewModel> GetArticleListAsync(string? category, string? status, string? query, CancellationToken cancellationToken);
}

public interface IAdminPriceQueryService
{
    Task<AdminPriceListViewModel> GetPriceListAsync(string? group, string? query, CancellationToken cancellationToken);
}

public interface IAdminSeoQueryService
{
    Task<AdminSeoListViewModel> GetSeoListAsync(CancellationToken cancellationToken);
}

public interface IAdminSettingsQueryService
{
    Task<AdminSettingsViewModel> GetSettingsAsync(CancellationToken cancellationToken);
}
