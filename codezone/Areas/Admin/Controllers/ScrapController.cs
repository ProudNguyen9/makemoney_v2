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
            ? "Đã xóa loại phế liệu (xóa mềm — có thể khôi phục trong DB)."
            : "Không tìm thấy loại phế liệu cần xóa.";
        return RedirectToAction(nameof(Index), new { group, status, q, page });
    }

    // ------------------------------------------------------------------
    // Nhóm phế liệu (CAT-001..003)
    // ------------------------------------------------------------------

    public async Task<IActionResult> Categories(CancellationToken cancellationToken = default)
    {
        var model = await _scrapQueryService.GetScrapCategoryListAsync(cancellationToken);
        return View(model);
    }

    public async Task<IActionResult> CategoryForm(int? id, CancellationToken cancellationToken = default)
    {
        var model = await _scrapQueryService.GetScrapCategoryFormAsync(id, cancellationToken);
        if (model is null)
        {
            TempData["Error"] = $"Không tìm thấy nhóm phế liệu #{id}.";
            return RedirectToAction(nameof(Categories));
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveCategory(ScrapCategoryFormViewModel form, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Chưa lưu được nhóm phế liệu: vui lòng kiểm tra các trường bị lỗi.";
            return View(nameof(CategoryForm), form);
        }

        try
        {
            await _scrapCommandService.SaveScrapCategoryAsync(form, cancellationToken);
        }
        catch (InvalidOperationException exception)
        {
            TempData["Error"] = exception.Message;
            return View(nameof(CategoryForm), form);
        }

        TempData["Success"] = form.Id == 0
            ? $"Đã thêm nhóm phế liệu \"{form.Name}\"."
            : $"Đã cập nhật nhóm phế liệu \"{form.Name}\".";
        return RedirectToAction(nameof(Categories));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleCategoryStatus(int id, CancellationToken cancellationToken = default)
    {
        var toggled = await _scrapCommandService.ToggleCategoryStatusAsync(id, cancellationToken);
        TempData[toggled ? "Success" : "Error"] = toggled
            ? "Đã chuyển trạng thái ẩn / hiển thị của nhóm phế liệu."
            : "Không tìm thấy nhóm phế liệu.";
        return RedirectToAction(nameof(Categories));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var deleted = await _scrapCommandService.DeleteScrapCategoryAsync(id, cancellationToken);
            TempData[deleted ? "Success" : "Error"] = deleted
                ? "Đã xóa nhóm phế liệu rỗng."
                : "Không tìm thấy nhóm phế liệu cần xóa.";
        }
        catch (InvalidOperationException exception)
        {
            TempData["Error"] = exception.Message;
        }

        return RedirectToAction(nameof(Categories));
    }
}
