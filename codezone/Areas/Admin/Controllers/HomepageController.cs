using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScrapWebsite.Areas.Admin.ViewModels.Forms;
using ScrapWebsite.Services.Admin;

namespace ScrapWebsite.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(AuthenticationSchemes = AdminAuthDefaults.AuthenticationScheme)]
public class HomepageController : Controller
{
    private readonly IAdminSettingsQueryService _settingsQueryService;
    private readonly IAdminSettingsCommandService _settingsCommandService;

    public HomepageController(IAdminSettingsQueryService settingsQueryService, IAdminSettingsCommandService settingsCommandService)
    {
        _settingsQueryService = settingsQueryService;
        _settingsCommandService = settingsCommandService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = await _settingsQueryService.GetSettingsAsync(cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveHomepageSettings(HomepageSettingsFormViewModel form, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Thông tin trang chủ chưa hợp lệ.";
            return RedirectToAction(nameof(Index));
        }

        await _settingsCommandService.SaveHomepageSettingsAsync(form, cancellationToken);
        TempData["Success"] = "Đã cập nhật cấu hình trang chủ.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveBrandAssets(BrandAssetsFormViewModel form, CancellationToken cancellationToken)
    {
        try
        {
            await _settingsCommandService.SaveBrandAssetsAsync(form, cancellationToken);
            TempData["Success"] = "Đã cập nhật ảnh thương hiệu.";
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
