using ScrapWebsite.Areas.Admin.ViewModels.Data;
using ScrapWebsite.Areas.Admin.ViewModels.Forms;

namespace ScrapWebsite.Services.Admin;

public interface IAdminDashboardQueryService
{
    Task<AdminDashboardViewModel> GetDashboardAsync(CancellationToken cancellationToken);
}

public interface IAdminScrapQueryService
{
    Task<AdminScrapListViewModel> GetScrapListAsync(string? group, string? status, string? query, int page, CancellationToken cancellationToken);

    Task<IReadOnlyList<AdminCategoryOptionDto>> GetCategoryOptionsAsync(CancellationToken cancellationToken);

    Task<ScrapItemFormViewModel?> GetScrapFormAsync(int? id, CancellationToken cancellationToken);
}

public interface IAdminArticleQueryService
{
    Task<AdminArticleListViewModel> GetArticleListAsync(string? category, string? status, string? query, CancellationToken cancellationToken);

    Task<PostFormViewModel?> GetArticleFormAsync(int? id, CancellationToken cancellationToken);
}

public interface IAdminPriceQueryService
{
    Task<AdminPriceListViewModel> GetPriceListAsync(string? group, string? status, string? query, int page, CancellationToken cancellationToken);
}

public interface IAdminLeadQueryService
{
    Task<AdminLeadListViewModel> GetLeadListAsync(string? status, string? scrap, string? area, string? query, int page, CancellationToken cancellationToken);
}

public interface IAdminSeoQueryService
{
    Task<AdminSeoListViewModel> GetSeoListAsync(string? entityType, string? status, string? indexState, string? query, CancellationToken cancellationToken);
}

public interface IAdminSettingsQueryService
{
    Task<AdminSettingsViewModel> GetSettingsAsync(CancellationToken cancellationToken);
}

public interface IAdminMediaQueryService
{
    Task<AdminMediaListViewModel> GetMediaListAsync(string? group, string? query, CancellationToken cancellationToken);
}

public interface IAdminServiceQueryService
{
    Task<AdminServiceListViewModel> GetServiceListAsync(string? status, string? query, int page, CancellationToken cancellationToken);

    Task<ServiceFormViewModel?> GetServiceFormAsync(int? id, CancellationToken cancellationToken);
}

public interface IAdminLocationQueryService
{
    Task<AdminLocationListViewModel> GetLocationListAsync(string? province, string? status, string? query, int page, CancellationToken cancellationToken);

    Task<LocationFormViewModel?> GetLocationFormAsync(int? id, CancellationToken cancellationToken);
}

public interface IAdminProjectQueryService
{
    Task<AdminProjectListViewModel> GetProjectListAsync(string? projectType, string? status, string? query, int page, CancellationToken cancellationToken);

    Task<ProjectFormViewModel?> GetProjectFormAsync(int? id, CancellationToken cancellationToken);
}

public interface IAdminFaqQueryService
{
    Task<AdminFaqListViewModel> GetFaqListAsync(string? entityType, string? query, int page, CancellationToken cancellationToken);

    Task<FaqFormViewModel?> GetFaqFormAsync(int? id, CancellationToken cancellationToken);
}
