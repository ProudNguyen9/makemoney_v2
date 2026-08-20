using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ScrapWebsite.Models;
using ScrapWebsite.Services.Interfaces;

namespace ScrapWebsite.Controllers;

public class HomeController : Controller
{
    private readonly ISiteSettingService _siteSettingService;

    public HomeController(ISiteSettingService siteSettingService)
    {
        _siteSettingService = siteSettingService;
    }

    public async Task<IActionResult> Index()
    {
        var viewModel = await _siteSettingService.GetHomeAsync();
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
