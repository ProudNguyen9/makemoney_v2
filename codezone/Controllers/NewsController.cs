using Microsoft.AspNetCore.Mvc;
using ScrapWebsite.Services.Interfaces;

namespace ScrapWebsite.Controllers;

public class NewsController : Controller
{
    private readonly IPostService _postService;

    public NewsController(IPostService postService)
    {
        _postService = postService;
    }

    public async Task<IActionResult> Index()
    {
        var viewModel = await _postService.GetIndexAsync();
        return View(viewModel);
    }

    public async Task<IActionResult> Detail(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return RedirectToAction(nameof(Index));
        }

        var viewModel = await _postService.GetDetailAsync(slug);
        return viewModel is null ? View() : View(viewModel);
    }
}
