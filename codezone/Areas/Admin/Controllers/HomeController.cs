using Microsoft.AspNetCore.Mvc;

namespace ScrapWebsite.Areas.Admin.Controllers;

[Area("Admin")]
public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
