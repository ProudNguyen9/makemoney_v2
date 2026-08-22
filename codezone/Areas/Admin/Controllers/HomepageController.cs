using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScrapWebsite.Services.Admin;

namespace ScrapWebsite.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(AuthenticationSchemes = AdminAuthDefaults.AuthenticationScheme)]
public class HomepageController : Controller
{
    private readonly IAdminSettingsQueryService _settingsQueryService;

    public HomepageController(IAdminSettingsQueryService settingsQueryService)
    {
        _settingsQueryService = settingsQueryService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = await _settingsQueryService.GetSettingsAsync(cancellationToken);
        return View(model);
    }
}
