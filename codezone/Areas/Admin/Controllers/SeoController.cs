using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScrapWebsite.Areas.Admin.ViewModels.Forms;
using ScrapWebsite.Services.Admin;

namespace ScrapWebsite.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(AuthenticationSchemes = AdminAuthDefaults.AuthenticationScheme)]
public class SeoController : Controller
{
    private readonly IAdminSeoQueryService _seoQueryService;
    private readonly IAdminSeoCommandService _seoCommandService;

    public SeoController(IAdminSeoQueryService seoQueryService, IAdminSeoCommandService seoCommandService)
    {
        _seoQueryService = seoQueryService;
        _seoCommandService = seoCommandService;
    }

    public async Task<IActionResult> Index(string? entityType, string? status, string? indexState, string? query, CancellationToken cancellationToken)
    {
        var model = await _seoQueryService.GetSeoListAsync(entityType, status, indexState, query, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveMetadata(SeoMetadataFormViewModel form, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Thông tin SEO chưa hợp lệ.";
            return RedirectToAction(nameof(Index));
        }

        await _seoCommandService.SaveSeoMetadataAsync(form, cancellationToken);
        TempData["Success"] = "Đã cập nhật SEO.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveSiteSettings(SeoSiteSettingsFormViewModel form, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Cấu hình SEO toàn site chưa hợp lệ.";
            return RedirectToAction(nameof(Index));
        }

        await _seoCommandService.SaveSeoSiteSettingsAsync(form, cancellationToken);
        TempData["Success"] = "Đã cập nhật SEO toàn site.";
        return RedirectToAction(nameof(Index));
    }
}
