using Microsoft.AspNetCore.Mvc;

namespace ScrapWebsite.Controllers;

public class PricesController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
