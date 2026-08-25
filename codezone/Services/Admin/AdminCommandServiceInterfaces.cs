using ScrapWebsite.Areas.Admin.ViewModels.Forms;

namespace ScrapWebsite.Services.Admin;

public interface IAdminPriceCommandService
{
    /// <summary>Saves the price rows ticked in the bulk table. Returns the number of changed rows.</summary>
    Task<int> SavePriceBulkAsync(IReadOnlyList<PriceBulkRowInput> rows, CancellationToken cancellationToken);

    Task<bool> DeletePriceAsync(int priceId, CancellationToken cancellationToken);

    /// <summary>Soft-deletes every ticked row. Returns the number of removed rows.</summary>
    Task<int> DeletePriceBulkAsync(IReadOnlyList<PriceBulkRowInput> rows, CancellationToken cancellationToken);
}

public interface IAdminLeadCommandService
{
    Task<bool> MarkContactedAsync(int id, CancellationToken cancellationToken);
}

public interface IAdminScrapCommandService
{
    Task<int> SaveScrapItemAsync(ScrapItemFormViewModel form, CancellationToken cancellationToken);

    Task<bool> ToggleStatusAsync(int id, CancellationToken cancellationToken);

    Task<bool> ToggleFeaturedAsync(int id, CancellationToken cancellationToken);

    Task<bool> UpdateSortAsync(int id, int sortOrder, CancellationToken cancellationToken);

    Task<bool> DeleteScrapItemAsync(int id, CancellationToken cancellationToken);
}

public interface IAdminServiceCommandService
{
    Task<int> SaveServiceAsync(ServiceFormViewModel form, CancellationToken cancellationToken);

    Task<bool> ToggleStatusAsync(int id, CancellationToken cancellationToken);

    Task<bool> ToggleFeaturedAsync(int id, CancellationToken cancellationToken);

    Task<bool> UpdateSortAsync(int id, int sortOrder, CancellationToken cancellationToken);

    Task<bool> DeleteServiceAsync(int id, CancellationToken cancellationToken);
}

public interface IAdminLocationCommandService
{
    Task<int> SaveLocationAsync(LocationFormViewModel form, CancellationToken cancellationToken);

    Task<bool> ToggleStatusAsync(int id, CancellationToken cancellationToken);

    Task<bool> ToggleFeaturedAsync(int id, CancellationToken cancellationToken);

    Task<bool> UpdateSortAsync(int id, int sortOrder, CancellationToken cancellationToken);

    Task<bool> DeleteLocationAsync(int id, CancellationToken cancellationToken);
}

public interface IAdminProjectCommandService
{
    Task<int> SaveProjectAsync(ProjectFormViewModel form, CancellationToken cancellationToken);

    Task<bool> ToggleStatusAsync(int id, CancellationToken cancellationToken);

    Task<bool> ToggleFeaturedAsync(int id, CancellationToken cancellationToken);

    Task<bool> UpdateSortAsync(int id, int sortOrder, CancellationToken cancellationToken);

    Task<bool> DeleteProjectAsync(int id, CancellationToken cancellationToken);
}

public interface IAdminArticleCommandService
{
    Task<int> SaveArticleAsync(PostFormViewModel form, CancellationToken cancellationToken);

    /// <summary>Tự lưu nội dung đang soạn vào bảng PostAutosaves (không đụng bài đã xuất bản).</summary>
    Task AutoSaveArticleDraftAsync(string postKey, PostFormViewModel form, CancellationToken cancellationToken);

    Task<bool> ToggleStatusAsync(int id, CancellationToken cancellationToken);

    Task<bool> ToggleFeaturedAsync(int id, CancellationToken cancellationToken);

    Task<bool> DeleteArticleAsync(int id, CancellationToken cancellationToken);

    Task<bool> RestoreArticleAsync(int id, CancellationToken cancellationToken);

    Task<bool> PermanentDeleteArticleAsync(int id, CancellationToken cancellationToken);
}

public interface IAdminFaqCommandService
{
    Task<int> SaveFaqAsync(FaqFormViewModel form, CancellationToken cancellationToken);

    Task<bool> ToggleStatusAsync(int id, CancellationToken cancellationToken);

    Task<bool> UpdateSortAsync(int id, int sortOrder, CancellationToken cancellationToken);

    Task<bool> DeleteFaqAsync(int id, CancellationToken cancellationToken);
}

public interface IAdminSettingsCommandService
{
    Task SaveCompanySettingsAsync(CompanySettingsFormViewModel form, CancellationToken cancellationToken);

    Task SaveHomepageSettingsAsync(HomepageSettingsFormViewModel form, CancellationToken cancellationToken);

    Task SaveBrandAssetsAsync(BrandAssetsFormViewModel form, CancellationToken cancellationToken);

    Task SaveFaviconAsync(FaviconFormViewModel form, CancellationToken cancellationToken);

    Task SaveSmtpSettingsAsync(SmtpSettingsFormViewModel form, CancellationToken cancellationToken);
}

public interface IAdminMediaCommandService
{
    Task SaveMediaSettingImageAsync(MediaSettingImageFormViewModel form, CancellationToken cancellationToken);
}

public interface IAdminSeoCommandService
{
    Task SaveSeoMetadataAsync(SeoMetadataFormViewModel form, CancellationToken cancellationToken);

    Task SaveSeoSiteSettingsAsync(SeoSiteSettingsFormViewModel form, CancellationToken cancellationToken);
}
