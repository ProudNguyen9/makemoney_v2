using Microsoft.AspNetCore.Mvc;

namespace ScrapWebsite.Controllers;

public class ProjectsController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Detail(string? slug)
    {
        return View();
    }
}
