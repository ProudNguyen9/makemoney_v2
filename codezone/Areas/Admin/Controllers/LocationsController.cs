using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScrapWebsite.Areas.Admin.ViewModels.Forms;
using ScrapWebsite.Services.Admin;

namespace ScrapWebsite.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(AuthenticationSchemes = AdminAuthDefaults.AuthenticationScheme)]
public class LocationsController : Controller
{
    private readonly IAdminLocationQueryService _queryService;
    private readonly IAdminLocationCommandService _commandService;

    public LocationsController(IAdminLocationQueryService queryService, IAdminLocationCommandService commandService)
    {
        _queryService = queryService;
        _commandService = commandService;
    }

    public async Task<IActionResult> Index(string? province, string? status, string? q, int page = 1, CancellationToken cancellationToken = default)
    {
        var model = await _queryService.GetLocationListAsync(province, status, q, Math.Max(1, page), cancellationToken);
        return View(model);
    }

    public async Task<IActionResult> Form(int? id, CancellationToken cancellationToken = default)
    {
        var model = await _queryService.GetLocationFormAsync(id, cancellationToken);
        if (model is null)
        {
            TempData["Error"] = $"Không tìm thấy khu vực #{id}.";
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(LocationFormViewModel form, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Chưa lưu được: vui lòng kiểm tra các trường bị lỗi.";
            return View(nameof(Form), form);
        }

        int id;
        try
        {
            id = await _commandService.SaveLocationAsync(form, cancellationToken);
        }
        catch (InvalidOperationException exception)
        {
            TempData["Error"] = exception.Message;
            return View(nameof(Form), form);
        }

        TempData["Success"] = form.Id == 0
            ? $"Đã thêm khu vực \"{form.Name}\"."
            : $"Đã cập nhật khu vực \"{form.Name}\".";
        return RedirectToAction(nameof(Form), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id, string? province, string? status, string? q, int page = 1, CancellationToken cancellationToken = default)
    {
        await _commandService.ToggleStatusAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index), new { province, status, q, page });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleFeatured(int id, string? province, string? status, string? q, int page = 1, CancellationToken cancellationToken = default)
    {
        await _commandService.ToggleFeaturedAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index), new { province, status, q, page });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateSort(int id, int sortOrder, string? province, string? status, string? q, int page = 1, CancellationToken cancellationToken = default)
    {
        await _commandService.UpdateSortAsync(id, sortOrder, cancellationToken);
        return RedirectToAction(nameof(Index), new { province, status, q, page });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, string? province, string? status, string? q, int page = 1, CancellationToken cancellationToken = default)
    {
        var deleted = await _commandService.DeleteLocationAsync(id, cancellationToken);
        TempData[deleted ? "Success" : "Error"] = deleted ? "Đã xóa khu vực." : "Không tìm thấy khu vực cần xóa.";
        return RedirectToAction(nameof(Index), new { province, status, q, page });
    }
}
