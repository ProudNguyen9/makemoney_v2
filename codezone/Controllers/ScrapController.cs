using Microsoft.AspNetCore.Mvc;
using ScrapWebsite.Services.Interfaces;

namespace ScrapWebsite.Controllers;

public class ScrapController : Controller
{
    private readonly IScrapService _scrapService;

    public ScrapController(IScrapService scrapService)
    {
        _scrapService = scrapService;
    }

    public async Task<IActionResult> Index()
    {
        var viewModel = await _scrapService.GetIndexAsync();
        return View(viewModel);
    }

    public IActionResult Category()
    {
        return View();
    }

    public async Task<IActionResult> Detail(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return RedirectToAction(nameof(Index));
        }

        var viewModel = await _scrapService.GetDetailAsync(slug);
        return viewModel is null ? View() : View(viewModel);
    }
}
