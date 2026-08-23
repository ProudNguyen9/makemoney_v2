using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScrapWebsite.Areas.Admin.ViewModels.Forms;
using ScrapWebsite.Services.Admin;

namespace ScrapWebsite.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(AuthenticationSchemes = AdminAuthDefaults.AuthenticationScheme)]
public class PricesController : Controller
{
    private readonly IAdminPriceQueryService _priceQueryService;
    private readonly IAdminPriceCommandService _priceCommandService;
    private readonly IAdminScrapCommandService _scrapCommandService;

    public PricesController(
        IAdminPriceQueryService priceQueryService,
        IAdminPriceCommandService priceCommandService,
        IAdminScrapCommandService scrapCommandService)
    {
        _priceQueryService = priceQueryService;
        _priceCommandService = priceCommandService;
        _scrapCommandService = scrapCommandService;
    }

    public async Task<IActionResult> Index(string? group, string? status, string? q, int page = 1, CancellationToken cancellationToken = default)
    {
        var model = await _priceQueryService.GetPriceListAsync(group, status, q, Math.Max(1, page), cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveBulk(List<PriceBulkRowInput> rows, string? group, string? status, string? q, int page = 1, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Dữ liệu giá không hợp lệ, vui lòng kiểm tra lại.";
            return RedirectToAction(nameof(Index), new { group, status, q, page });
        }

        var changed = await _priceCommandService.SavePriceBulkAsync(rows ?? [], cancellationToken);
        TempData["Success"] = changed > 0
            ? $"Đã cập nhật {changed} dòng giá (đã ghi lịch sử thay đổi)."
            : "Không có dòng giá nào thay đổi.";
        return RedirectToAction(nameof(Index), new { group, status, q, page });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteBulk(List<PriceBulkRowInput> rows, string? group, string? status, string? q, int page = 1, CancellationToken cancellationToken = default)
    {
        var deleted = await _priceCommandService.DeletePriceBulkAsync(rows ?? [], cancellationToken);
        TempData[deleted > 0 ? "Success" : "Error"] = deleted > 0
            ? $"Đã xóa {deleted} dòng giá đã chọn (xóa mềm — dữ liệu vẫn giữ trong hệ thống)."
            : "Chưa chọn dòng giá nào để xóa.";
        return RedirectToAction(nameof(Index), new { group, status, q, page });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, string? group, string? status, string? q, int page = 1, CancellationToken cancellationToken = default)
    {
        var deleted = await _priceCommandService.DeletePriceAsync(id, cancellationToken);
        TempData[deleted ? "Success" : "Error"] = deleted ? "Đã xóa dòng giá (xóa mềm)." : "Không tìm thấy dòng giá cần xóa.";
        return RedirectToAction(nameof(Index), new { group, status, q, page });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleItem(int id, string? group, string? status, string? q, int page = 1, CancellationToken cancellationToken = default)
    {
        var toggled = await _scrapCommandService.ToggleStatusAsync(id, cancellationToken);
        TempData[toggled ? "Success" : "Error"] = toggled
            ? "Đã chuyển trạng thái ẩn / hiển thị của loại phế liệu."
            : "Không tìm thấy loại phế liệu.";
        return RedirectToAction(nameof(Index), new { group, status, q, page });
    }
}
