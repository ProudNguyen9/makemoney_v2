using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScrapWebsite.Areas.Admin.ViewModels.Forms;
using ScrapWebsite.Services.Admin;

namespace ScrapWebsite.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(AuthenticationSchemes = AdminAuthDefaults.AuthenticationScheme)]
public class ScrapController : Controller
{
    private readonly IAdminScrapQueryService _scrapQueryService;
    private readonly IAdminScrapCommandService _scrapCommandService;

    public ScrapController(IAdminScrapQueryService scrapQueryService, IAdminScrapCommandService scrapCommandService)
    {
        _scrapQueryService = scrapQueryService;
        _scrapCommandService = scrapCommandService;
    }

    public async Task<IActionResult> Index(string? group, string? status, string? q, int page = 1, CancellationToken cancellationToken = default)
    {
        var model = await _scrapQueryService.GetScrapListAsync(group, status, q, Math.Max(1, page), cancellationToken);
        return View(model);
    }

    public async Task<IActionResult> Form(int? id, CancellationToken cancellationToken = default)
    {
        if (id is > 0)
        {
            var existing = await _scrapQueryService.GetScrapFormAsync(id, cancellationToken);
            if (existing is null)
            {
                TempData["Error"] = $"Không tìm thấy loại phế liệu #{id}.";
                return RedirectToAction(nameof(Index));
            }

            return View(existing);
        }

        var model = await _scrapQueryService.GetScrapFormAsync(null, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(ScrapItemFormViewModel form, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            form.Categories = await _scrapQueryService.GetCategoryOptionsAsync(cancellationToken);
            TempData["Error"] = "Chưa lưu được: vui lòng kiểm tra các trường bị lỗi.";
            return View(nameof(Form), form);
        }

        int id;
        try
        {
            id = await _scrapCommandService.SaveScrapItemAsync(form, cancellationToken);
        }
        catch (InvalidOperationException exception)
        {
            form.Categories = await _scrapQueryService.GetCategoryOptionsAsync(cancellationToken);
            TempData["Error"] = exception.Message;
            return View(nameof(Form), form);
        }

        TempData["Success"] = form.Id == 0
            ? $"Đã thêm loại phế liệu \"{form.Name}\" (ảnh tải lên được chuyển sang WebP để tiết kiệm dung lượng)."
            : $"Đã cập nhật loại phế liệu \"{form.Name}\".";
        return RedirectToAction(nameof(Form), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id, string? group, string? status, string? q, int page = 1, CancellationToken cancellationToken = default)
    {
        await _scrapCommandService.ToggleStatusAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index), new { group, status, q, page });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleFeatured(int id, string? group, string? status, string? q, int page = 1, CancellationToken cancellationToken = default)
    {
        await _scrapCommandService.ToggleFeaturedAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index), new { group, status, q, page });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateSort(int id, int sortOrder, string? group, string? status, string? q, int page = 1, CancellationToken cancellationToken = default)
    {
        await _scrapCommandService.UpdateSortAsync(id, sortOrder, cancellationToken);
        return RedirectToAction(nameof(Index), new { group, status, q, page });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, string? group, string? status, string? q, int page = 1, CancellationToken cancellationToken = default)
    {
        var deleted = await _scrapCommandService.DeleteScrapItemAsync(id, cancellationToken);
        TempData[deleted ? "Success" : "Error"] = deleted
            ? "Đã xóa loại phế liệu (kèm dòng giá và ảnh)."
            : "Không tìm thấy loại phế liệu cần xóa.";
        return RedirectToAction(nameof(Index), new { group, status, q, page });
    }
}
