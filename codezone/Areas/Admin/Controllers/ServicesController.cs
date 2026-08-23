using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScrapWebsite.Areas.Admin.ViewModels.Forms;
using ScrapWebsite.Services.Admin;

namespace ScrapWebsite.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(AuthenticationSchemes = AdminAuthDefaults.AuthenticationScheme)]
public class ServicesController : Controller
{
    private readonly IAdminServiceQueryService _queryService;
    private readonly IAdminServiceCommandService _commandService;

    public ServicesController(IAdminServiceQueryService queryService, IAdminServiceCommandService commandService)
    {
        _queryService = queryService;
        _commandService = commandService;
    }

    public async Task<IActionResult> Index(string? status, string? q, int page = 1, CancellationToken cancellationToken = default)
    {
        var model = await _queryService.GetServiceListAsync(status, q, Math.Max(1, page), cancellationToken);
        return View(model);
    }

    public async Task<IActionResult> Form(int? id, CancellationToken cancellationToken = default)
    {
        var model = await _queryService.GetServiceFormAsync(id, cancellationToken);
        if (model is null)
        {
            TempData["Error"] = $"Không tìm thấy dịch vụ #{id}.";
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(ServiceFormViewModel form, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Chưa lưu được: vui lòng kiểm tra các trường bị lỗi.";
            return View(nameof(Form), form);
        }

        int id;
        try
        {
            id = await _commandService.SaveServiceAsync(form, cancellationToken);
        }
        catch (InvalidOperationException exception)
        {
            TempData["Error"] = exception.Message;
            return View(nameof(Form), form);
        }

        TempData["Success"] = form.Id == 0
            ? $"Đã thêm dịch vụ \"{form.Title}\"."
            : $"Đã cập nhật dịch vụ \"{form.Title}\".";
        return RedirectToAction(nameof(Form), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id, string? status, string? q, int page = 1, CancellationToken cancellationToken = default)
    {
        await _commandService.ToggleStatusAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index), new { status, q, page });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleFeatured(int id, string? status, string? q, int page = 1, CancellationToken cancellationToken = default)
    {
        await _commandService.ToggleFeaturedAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index), new { status, q, page });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateSort(int id, int sortOrder, string? status, string? q, int page = 1, CancellationToken cancellationToken = default)
    {
        await _commandService.UpdateSortAsync(id, sortOrder, cancellationToken);
        return RedirectToAction(nameof(Index), new { status, q, page });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, string? status, string? q, int page = 1, CancellationToken cancellationToken = default)
    {
        var deleted = await _commandService.DeleteServiceAsync(id, cancellationToken);
        TempData[deleted ? "Success" : "Error"] = deleted ? "Đã xóa dịch vụ." : "Không tìm thấy dịch vụ cần xóa.";
        return RedirectToAction(nameof(Index), new { status, q, page });
    }
}
