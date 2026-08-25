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
        return await SaveCore(form, cancellationToken, saveAsDraft: false);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveDraft(PostFormViewModel form, CancellationToken cancellationToken = default)
    {
        return await SaveCore(form, cancellationToken, saveAsDraft: true);
    }

    /// <summary>
    /// Tự lưu khi đang soạn (AJAX). Bài mới / bản nháp: lưu thành bản nháp thật.
    /// Bài đã xuất bản: chỉ ghi bản nháp tạm, không thay đổi bài live.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AutoSave([FromForm] PostFormViewModel form, string? autosaveKey, CancellationToken cancellationToken = default)
    {
        // Tự lưu chấp nhận nội dung dang dở nên bỏ qua validate bắt buộc.
        ModelState.Clear();

        if (string.IsNullOrWhiteSpace(form.Title))
        {
            return Json(new { ok = false, reason = "Cần có tiêu đề để tự lưu." });
        }

        try
        {
            var isPublishedPost = false;
            if (form.Id > 0)
            {
                var status = await _articleQueryService.GetArticleStatusAsync(form.Id, cancellationToken);
                isPublishedPost = string.Equals(status, "published", StringComparison.OrdinalIgnoreCase);
            }

            if (isPublishedPost)
            {
                var key = $"post-{form.Id}";
                await _articleCommandService.AutoSaveArticleDraftAsync(key, form, cancellationToken);
                return Json(new { ok = true, mode = "temp" });
            }

            form.Status = "draft";
            form.AuthorName = string.IsNullOrWhiteSpace(form.AuthorName)
                ? User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name
                : form.AuthorName;

            foreach (var key in new[] { nameof(PostFormViewModel.PostCategoryId), nameof(PostFormViewModel.Content) })
            {
                ModelState[key]?.Errors.Clear();
                ModelState[key]?.ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid;
            }

            var savedId = await _articleCommandService.SaveArticleAsync(form, cancellationToken);
            return Json(new
            {
                ok = true,
                mode = "post",
                id = savedId,
                url = Url.Action(nameof(Form), new { id = savedId })
            });
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            return Json(new { ok = false, reason = ex.Message });
        }
    }

    private async Task<IActionResult> SaveCore(PostFormViewModel form, CancellationToken cancellationToken, bool saveAsDraft)
    {
        form.AuthorName = string.IsNullOrWhiteSpace(form.AuthorName)
            ? User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name
            : form.AuthorName;

        if (saveAsDraft)
        {
            form.Status = "draft";
            // Bản nháp cho phép chưa chọn chuyên mục / chưa có nội dung đầy đủ.
            foreach (var key in new[] { nameof(PostFormViewModel.PostCategoryId), nameof(PostFormViewModel.Content) })
            {
                ModelState[key]?.Errors.Clear();
                ModelState[key]?.ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid;
            }
        }

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

        TempData["Success"] = saveAsDraft
            ? $"Đã lưu bản nháp \"{form.Title}\". Bài viết chưa hiển thị trên website."
            : form.Id == 0
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
