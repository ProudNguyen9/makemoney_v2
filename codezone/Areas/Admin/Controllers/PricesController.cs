using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScrapWebsite.Services.Admin;

namespace ScrapWebsite.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(AuthenticationSchemes = AdminAuthDefaults.AuthenticationScheme)]
public class PricesController : Controller
{
    private readonly IAdminPriceQueryService _priceQueryService;

    public PricesController(IAdminPriceQueryService priceQueryService)
    {
        _priceQueryService = priceQueryService;
    }

    public async Task<IActionResult> Index(string? group, string? q, CancellationToken cancellationToken)
    {
        var model = await _priceQueryService.GetPriceListAsync(group, q, cancellationToken);
        return View(model);
    }
}
