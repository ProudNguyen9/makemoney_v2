using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScrapWebsite.Areas.Admin.ViewModels.Forms;
using ScrapWebsite.Services.Admin;

namespace ScrapWebsite.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(AuthenticationSchemes = AdminAuthDefaults.AuthenticationScheme)]
public class SettingsController : Controller
{
    private readonly IAdminSettingsQueryService _settingsQueryService;
    private readonly IAdminSettingsCommandService _settingsCommandService;

    public SettingsController(IAdminSettingsQueryService settingsQueryService, IAdminSettingsCommandService settingsCommandService)
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
    public async Task<IActionResult> SaveCompany(CompanySettingsFormViewModel form, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Thông tin cài đặt chưa hợp lệ.";
            return RedirectToAction(nameof(Index));
        }

        await _settingsCommandService.SaveCompanySettingsAsync(form, cancellationToken);
        TempData["Success"] = "Đã cập nhật thông tin công ty.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveFavicon(FaviconFormViewModel form, CancellationToken cancellationToken)
    {
        try
        {
            await _settingsCommandService.SaveFaviconAsync(form, cancellationToken);
            TempData["Success"] = "Đã cập nhật favicon.";
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
