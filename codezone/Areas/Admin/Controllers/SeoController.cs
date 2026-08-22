using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScrapWebsite.Services.Admin;

namespace ScrapWebsite.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(AuthenticationSchemes = AdminAuthDefaults.AuthenticationScheme)]
public class SeoController : Controller
{
    private readonly IAdminSeoQueryService _seoQueryService;

    public SeoController(IAdminSeoQueryService seoQueryService)
    {
        _seoQueryService = seoQueryService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = await _seoQueryService.GetSeoListAsync(cancellationToken);
        return View(model);
    }
}
