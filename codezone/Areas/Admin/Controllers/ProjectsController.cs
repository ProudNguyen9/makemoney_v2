using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScrapWebsite.Areas.Admin.ViewModels.Forms;
using ScrapWebsite.Services.Admin;

namespace ScrapWebsite.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(AuthenticationSchemes = AdminAuthDefaults.AuthenticationScheme)]
public class ProjectsController : Controller
{
    private readonly IAdminProjectQueryService _queryService;
    private readonly IAdminProjectCommandService _commandService;

    public ProjectsController(IAdminProjectQueryService queryService, IAdminProjectCommandService commandService)
    {
        _queryService = queryService;
        _commandService = commandService;
    }

    public async Task<IActionResult> Index(string? type, string? status, string? q, int page = 1, CancellationToken cancellationToken = default)
    {
        var model = await _queryService.GetProjectListAsync(type, status, q, Math.Max(1, page), cancellationToken);
        return View(model);
    }

    public async Task<IActionResult> Form(int? id, CancellationToken cancellationToken = default)
    {
        var model = await _queryService.GetProjectFormAsync(id, cancellationToken);
        if (model is null)
        {
            TempData["Error"] = $"Không tìm thấy dự án #{id}.";
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(ProjectFormViewModel form, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Chưa lưu được: vui lòng kiểm tra các trường bị lỗi.";
            return View(nameof(Form), form);
        }

        int id;
        try
        {
            id = await _commandService.SaveProjectAsync(form, cancellationToken);
        }
        catch (InvalidOperationException exception)
        {
            TempData["Error"] = exception.Message;
            return View(nameof(Form), form);
        }

        TempData["Success"] = form.Id == 0
            ? $"Đã thêm dự án \"{form.Title}\"."
            : $"Đã cập nhật dự án \"{form.Title}\".";
        return RedirectToAction(nameof(Form), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id, string? type, string? status, string? q, int page = 1, CancellationToken cancellationToken = default)
    {
        await _commandService.ToggleStatusAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index), new { type, status, q, page });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleFeatured(int id, string? type, string? status, string? q, int page = 1, CancellationToken cancellationToken = default)
    {
        await _commandService.ToggleFeaturedAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index), new { type, status, q, page });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateSort(int id, int sortOrder, string? type, string? status, string? q, int page = 1, CancellationToken cancellationToken = default)
    {
        await _commandService.UpdateSortAsync(id, sortOrder, cancellationToken);
        return RedirectToAction(nameof(Index), new { type, status, q, page });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, string? type, string? status, string? q, int page = 1, CancellationToken cancellationToken = default)
    {
        var deleted = await _commandService.DeleteProjectAsync(id, cancellationToken);
        TempData[deleted ? "Success" : "Error"] = deleted ? "Đã xóa dự án." : "Không tìm thấy dự án cần xóa.";
        return RedirectToAction(nameof(Index), new { type, status, q, page });
    }
}
