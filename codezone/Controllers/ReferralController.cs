using Microsoft.AspNetCore.Mvc;

namespace ScrapWebsite.Controllers;

public class ReferralController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
