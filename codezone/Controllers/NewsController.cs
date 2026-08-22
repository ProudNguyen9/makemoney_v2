using Microsoft.AspNetCore.Mvc;
using ScrapWebsite.Services.Interfaces;
using ScrapWebsite.ViewModels.Public;

namespace ScrapWebsite.Controllers;

public class NewsController : Controller
{
    private readonly IPublicPostQueryService _postQueryService;

    public NewsController(IPublicPostQueryService postQueryService)
    {
        _postQueryService = postQueryService;
    }

    public async Task<IActionResult> Index(string? cursor, string? previousCursor, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var viewModel = await _postQueryService.GetNewsIndexAsync(new PostListQueryDto(pageSize, cursor, previousCursor, page), cancellationToken);
        ViewData["Seo"] = viewModel.Seo;
        return View(viewModel);
    }

    public async Task<IActionResult> Detail(string slug, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return RedirectToAction(nameof(Index));
        }

        var viewModel = await _postQueryService.GetNewsDetailAsync(slug, cancellationToken);
        if (viewModel is not null)
        {
            ViewData["Seo"] = viewModel.Seo;
        }
        return viewModel is null ? NotFound() : View(viewModel);
    }
}
