using Microsoft.AspNetCore.Mvc;
using ScrapWebsite.Services.Interfaces;
using ScrapWebsite.ViewModels.Public;

namespace ScrapWebsite.Controllers;

public class ScrapController : Controller
{
    private readonly IPublicScrapQueryService _scrapQueryService;

    public ScrapController(IPublicScrapQueryService scrapQueryService)
    {
        _scrapQueryService = scrapQueryService;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = 12, CancellationToken cancellationToken = default)
    {
        var viewModel = await _scrapQueryService.GetScrapIndexAsync(new ScrapListQueryDto(page, pageSize), cancellationToken);
        ViewData["Seo"] = viewModel.Seo;
        return View(viewModel);
    }

    public async Task<IActionResult> Category(string? slug, CancellationToken cancellationToken = default)
    {
        var viewModel = await _scrapQueryService.GetScrapCategoryPageAsync(slug, cancellationToken);
        if (viewModel is null)
        {
            return NotFound();
        }
        ViewData["Seo"] = viewModel.Seo;
        return View(viewModel);
    }

    public async Task<IActionResult> Detail(string slug, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return RedirectToAction(nameof(Index));
        }

        var viewModel = await _scrapQueryService.GetScrapDetailAsync(slug, cancellationToken);
        if (viewModel is not null)
        {
            ViewData["Seo"] = viewModel.Seo;
        }
        return viewModel is null ? NotFound() : View(viewModel);
    }
}
