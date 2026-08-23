using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ScrapWebsite.Areas.Admin.ViewModels.Forms;
using ScrapWebsite.Services.Admin;

namespace ScrapWebsite.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(AuthenticationSchemes = AdminAuthDefaults.AuthenticationScheme)]
public class ArticlesController : Controller
{
    private readonly IAdminArticleQueryService _articleQueryService;
    private readonly IAdminArticleCommandService _articleCommandService;

    public ArticlesController(IAdminArticleQueryService articleQueryService, IAdminArticleCommandService articleCommandService)
    {
        _articleQueryService = articleQueryService;
        _articleCommandService = articleCommandService;
    }

    public async Task<IActionResult> Index(string? category, string? status, string? q, CancellationToken cancellationToken)
    {
        var model = await _articleQueryService.GetArticleListAsync(category, status, q, cancellationToken);
        return View(model);
    }

    public async Task<IActionResult> Form(int? id, CancellationToken cancellationToken = default)
    {
        var model = await _articleQueryService.GetArticleFormAsync(id, cancellationToken);
        if (model is null)
        {
            TempData["Error"] = $"Không tìm thấy bài viết #{id}.";
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(PostFormViewModel form, CancellationToken cancellationToken = default)
    {
        form.AuthorName = string.IsNullOrWhiteSpace(form.AuthorName)
            ? User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name
            : form.AuthorName;

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Chưa lưu được: vui lòng kiểm tra các trường bị lỗi.";
            var reload = (await _articleQueryService.GetArticleFormAsync(form.Id == 0 ? null : form.Id, cancellationToken))!;
            form.Categories = reload.Categories;
            form.ProductOptions = reload.ProductOptions;
            return View(nameof(Form), form);
        }

        int id;
        try
        {
            id = await _articleCommandService.SaveArticleAsync(form, cancellationToken);
        }
        catch (InvalidOperationException exception)
        {
            TempData["Error"] = exception.Message;
            var reload = (await _articleQueryService.GetArticleFormAsync(form.Id == 0 ? null : form.Id, cancellationToken))!;
            form.Categories = reload.Categories;
            form.ProductOptions = reload.ProductOptions;
            return View(nameof(Form), form);
        }

        TempData["Success"] = form.Id == 0
            ? $"Đã tạo bài viết \"{form.Title}\"."
            : $"Đã cập nhật bài viết \"{form.Title}\".";
        return RedirectToAction(nameof(Form), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id, string? category, string? status, string? q, CancellationToken cancellationToken = default)
    {
        await _articleCommandService.ToggleStatusAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index), new { category, status, q });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleFeatured(int id, string? category, string? status, string? q, CancellationToken cancellationToken = default)
    {
        await _articleCommandService.ToggleFeaturedAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index), new { category, status, q });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, string? category, string? status, string? q, CancellationToken cancellationToken = default)
    {
        var deleted = await _articleCommandService.DeleteArticleAsync(id, cancellationToken);
        TempData[deleted ? "Success" : "Error"] = deleted ? "Đã xóa mềm bài viết." : "Không tìm thấy bài viết cần xóa.";
        return RedirectToAction(nameof(Index), new { category, status, q });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(int id, string? category, string? status, string? q, CancellationToken cancellationToken = default)
    {
        var restored = await _articleCommandService.RestoreArticleAsync(id, cancellationToken);
        TempData[restored ? "Success" : "Error"] = restored ? "Đã khôi phục bài viết." : "Không tìm thấy bài viết cần khôi phục.";
        return RedirectToAction(nameof(Index), new { category, status, q });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PermanentDelete(int id, string? category, string? status, string? q, CancellationToken cancellationToken = default)
    {
        var deleted = await _articleCommandService.PermanentDeleteArticleAsync(id, cancellationToken);
        TempData[deleted ? "Success" : "Error"] = deleted ? "Đã xóa hẳn bài viết." : "Không tìm thấy bài viết đã xóa.";
        return RedirectToAction(nameof(Index), new { category, status, q });
    }
}
