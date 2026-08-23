using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScrapWebsite.Areas.Admin.ViewModels.Forms;
using ScrapWebsite.Services.Admin;

namespace ScrapWebsite.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(AuthenticationSchemes = AdminAuthDefaults.AuthenticationScheme)]
public class FaqController : Controller
{
    private readonly IAdminFaqQueryService _queryService;
    private readonly IAdminFaqCommandService _commandService;

    public FaqController(IAdminFaqQueryService queryService, IAdminFaqCommandService commandService)
    {
        _queryService = queryService;
        _commandService = commandService;
    }

    public async Task<IActionResult> Index(string? assign, string? q, int page = 1, CancellationToken cancellationToken = default)
    {
        var model = await _queryService.GetFaqListAsync(assign, q, Math.Max(1, page), cancellationToken);
        return View(model);
    }

    public async Task<IActionResult> Form(int? id, CancellationToken cancellationToken = default)
    {
        var model = await _queryService.GetFaqFormAsync(id, cancellationToken);
        if (model is null)
        {
            TempData["Error"] = $"Không tìm thấy câu hỏi #{id}.";
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(FaqFormViewModel form, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Chưa lưu được: vui lòng kiểm tra các trường bị lỗi.";
            return View(nameof(Form), form);
        }

        int id;
        try
        {
            id = await _commandService.SaveFaqAsync(form, cancellationToken);
        }
        catch (InvalidOperationException exception)
        {
            TempData["Error"] = exception.Message;
            return View(nameof(Form), form);
        }

        TempData["Success"] = form.Id == 0
            ? $"Đã thêm FAQ \"{form.Question}\"."
            : $"Đã cập nhật FAQ \"{form.Question}\".";
        return RedirectToAction(nameof(Form), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id, string? assign, string? q, int page = 1, CancellationToken cancellationToken = default)
    {
        await _commandService.ToggleStatusAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index), new { assign, q, page });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateSort(int id, int sortOrder, string? assign, string? q, int page = 1, CancellationToken cancellationToken = default)
    {
        await _commandService.UpdateSortAsync(id, sortOrder, cancellationToken);
        return RedirectToAction(nameof(Index), new { assign, q, page });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, string? assign, string? q, int page = 1, CancellationToken cancellationToken = default)
    {
        var deleted = await _commandService.DeleteFaqAsync(id, cancellationToken);
        TempData[deleted ? "Success" : "Error"] = deleted ? "Đã xóa FAQ." : "Không tìm thấy FAQ cần xóa.";
        return RedirectToAction(nameof(Index), new { assign, q, page });
    }
}
