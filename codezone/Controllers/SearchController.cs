using Microsoft.AspNetCore.Mvc;

namespace ScrapWebsite.Controllers;

public class SearchController : Controller
{
    public IActionResult Index(string? q)
    {
        ViewData["Query"] = q;
        return View();
    }
}
