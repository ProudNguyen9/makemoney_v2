using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScrapWebsite.Services.Admin;

namespace ScrapWebsite.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(AuthenticationSchemes = AdminAuthDefaults.AuthenticationScheme)]
public class ScrapController : Controller
{
    private readonly IAdminScrapQueryService _scrapQueryService;

    public ScrapController(IAdminScrapQueryService scrapQueryService)
    {
        _scrapQueryService = scrapQueryService;
    }

    public async Task<IActionResult> Index(string? group, string? status, string? q, CancellationToken cancellationToken)
    {
        var model = await _scrapQueryService.GetScrapListAsync(group, status, q, cancellationToken);
        return View(model);
    }

    public IActionResult Form()
    {
        return View();
    }
}
