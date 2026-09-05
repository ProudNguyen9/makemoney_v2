using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScrapWebsite.Areas.Admin.ViewModels.Forms;
using ScrapWebsite.Services;
using ScrapWebsite.Services.Admin;
using ScrapWebsite.Services.Interfaces;

namespace ScrapWebsite.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(AuthenticationSchemes = AdminAuthDefaults.AuthenticationScheme)]
public class SettingsController : Controller
{
    private readonly IAdminSettingsQueryService _settingsQueryService;
    private readonly IAdminSettingsCommandService _settingsCommandService;
    private readonly IEmailNotificationService _emailNotificationService;

    public SettingsController(
        IAdminSettingsQueryService settingsQueryService,
        IAdminSettingsCommandService settingsCommandService,
        IEmailNotificationService emailNotificationService)
    {
        _settingsQueryService = settingsQueryService;
        _settingsCommandService = settingsCommandService;
        _emailNotificationService = emailNotificationService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = await _settingsQueryService.GetSettingsAsync(cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveCompany(CompanySettingsFormViewModel form, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Thông tin cài đặt chưa hợp lệ.";
            return RedirectToAction(nameof(Index));
        }

        await _settingsCommandService.SaveCompanySettingsAsync(form, cancellationToken);
        TempData["Success"] = "Đã cập nhật thông tin công ty.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveFavicon(FaviconFormViewModel form, CancellationToken cancellationToken)
    {
        try
        {
            await _settingsCommandService.SaveFaviconAsync(form, cancellationToken);
            TempData["Success"] = "Đã cập nhật favicon.";
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveSmtp(SmtpSettingsFormViewModel form, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Cấu hình SMTP chưa hợp lệ. Vui lòng kiểm tra lại Host, Port và địa chỉ email.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await _settingsCommandService.SaveSmtpSettingsAsync(form, cancellationToken);
            TempData["Success"] = "Đã lưu cấu hình email (SMTP).";
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendSmtpTest(string? testEmail, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(testEmail))
        {
            var settings = await _settingsQueryService.GetSettingsAsync(cancellationToken);
            testEmail = settings.SmtpToEmail;
        }

        try
        {
            await _emailNotificationService.SendTestEmailAsync(testEmail!);
            TempData["Success"] = $"Đã gửi email thử nghiệm tới {testEmail}. Vui lòng kiểm tra hộp thư.";
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            TempData["Error"] = ex.Message;
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Không gửi được email thử nghiệm: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }
}
