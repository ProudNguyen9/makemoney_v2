using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScrapWebsite.Services.Admin;

namespace ScrapWebsite.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(AuthenticationSchemes = AdminAuthDefaults.AuthenticationScheme)]
public class ArticlesController : Controller
{
    private readonly IAdminArticleQueryService _articleQueryService;

    public ArticlesController(IAdminArticleQueryService articleQueryService)
    {
        _articleQueryService = articleQueryService;
    }

    public async Task<IActionResult> Index(string? category, string? status, string? q, CancellationToken cancellationToken)
    {
        var model = await _articleQueryService.GetArticleListAsync(category, status, q, cancellationToken);
        return View(model);
    }

    public IActionResult Form()
    {
        return View();
    }
}
