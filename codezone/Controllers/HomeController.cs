using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ScrapWebsite.Models;
using ScrapWebsite.Services.Interfaces;

namespace ScrapWebsite.Controllers;

public class HomeController : Controller
{
    private readonly IPublicHomeQueryService _homeQueryService;

    public HomeController(IPublicHomeQueryService homeQueryService)
    {
        _homeQueryService = homeQueryService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var viewModel = await _homeQueryService.GetHomeAsync(cancellationToken);
        ViewData["Seo"] = viewModel.Seo;
        return View(viewModel);
    }

    public IActionResult About()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
