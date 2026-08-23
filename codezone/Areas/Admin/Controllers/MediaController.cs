using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScrapWebsite.Areas.Admin.ViewModels.Forms;
using ScrapWebsite.Services.Admin;
using ScrapWebsite.Services.Media;

namespace ScrapWebsite.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(AuthenticationSchemes = AdminAuthDefaults.AuthenticationScheme)]
public class MediaController : Controller
{
    private readonly IAdminMediaQueryService _mediaQueryService;
    private readonly IAdminMediaCommandService _mediaCommandService;
    private readonly IImageUploadService _imageUploadService;

    public MediaController(
        IAdminMediaQueryService mediaQueryService,
        IAdminMediaCommandService mediaCommandService,
        IImageUploadService imageUploadService)
    {
        _mediaQueryService = mediaQueryService;
        _mediaCommandService = mediaCommandService;
        _imageUploadService = imageUploadService;
    }

    public async Task<IActionResult> Index(string? group, string? query, CancellationToken cancellationToken)
    {
        var model = await _mediaQueryService.GetMediaListAsync(group, query, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveSettingImage(MediaSettingImageFormViewModel form, string? group, string? query, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Thông tin ảnh chưa hợp lệ.";
            return RedirectToAction(nameof(Index), new { group, query });
        }

        try
        {
            await _mediaCommandService.SaveMediaSettingImageAsync(form, cancellationToken);
            TempData["Success"] = "Đã cập nhật ảnh Media.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index), new { group, query });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadEditorImage(IFormFile? file, CancellationToken cancellationToken)
    {
        var upload = await _imageUploadService.SaveAsWebpAsync(file, "content", "editor-image", 1800, cancellationToken);
        if (!upload.Success || string.IsNullOrWhiteSpace(upload.Url))
        {
            return BadRequest(new { error = upload.Error ?? "Không tải được ảnh." });
        }

        return Json(new { location = upload.Url });
    }
}
