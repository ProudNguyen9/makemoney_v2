using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScrapWebsite.Services.Admin;

namespace ScrapWebsite.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(AuthenticationSchemes = AdminAuthDefaults.AuthenticationScheme)]
public class LeadsController : Controller
{
    private readonly IAdminLeadQueryService _leadQueryService;
    private readonly IAdminLeadCommandService _leadCommandService;

    public LeadsController(IAdminLeadQueryService leadQueryService, IAdminLeadCommandService leadCommandService)
    {
        _leadQueryService = leadQueryService;
        _leadCommandService = leadCommandService;
    }

    public async Task<IActionResult> Index(string? status, string? scrap, [FromQuery] string? leadArea, string? query, int page = 1, CancellationToken cancellationToken = default)
    {
        var model = await _leadQueryService.GetLeadListAsync(Clean(status), Clean(scrap), Clean(leadArea), Clean(query), Math.Max(1, page), cancellationToken);
        return View(model);
    }

    /// <summary>LEAD-004: trang chi tiết một lead (/admin/leads/detail/{id}).</summary>
    public async Task<IActionResult> Detail(int id, CancellationToken cancellationToken = default)
    {
        var lead = await _leadQueryService.GetLeadDetailAsync(id, cancellationToken);
        if (lead is null)
        {
            TempData["Error"] = $"Không tìm thấy yêu cầu báo giá #{id}.";
            return RedirectToAction(nameof(Index));
        }

        return View(lead);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkContacted(int id, string? status, string? scrap, [FromForm] string? leadArea, string? query, int page = 1, CancellationToken cancellationToken = default)
    {
        var updated = await _leadCommandService.MarkContactedAsync(id, cancellationToken);
        if (Request.Headers.XRequestedWith == "XMLHttpRequest")
        {
            return Json(new { ok = updated });
        }

        TempData[updated ? "Success" : "Error"] = updated ? "Đã đánh dấu yêu cầu là đã liên hệ." : "Không tìm thấy yêu cầu.";
        return RedirectToAction(nameof(Index), new { area = "Admin", status, scrap, leadArea, query, page });
    }

    private static string? Clean(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
