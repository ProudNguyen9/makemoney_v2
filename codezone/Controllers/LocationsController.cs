using Microsoft.AspNetCore.Mvc;

namespace ScrapWebsite.Controllers;

public class LocationsController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    // Trang chi tiết khu vực đã được bỏ — mọi link cũ chuyển về trang danh sách.
    public IActionResult Detail(string? slug)
    {
        return RedirectPermanent("/khu-vuc");
    }
}
