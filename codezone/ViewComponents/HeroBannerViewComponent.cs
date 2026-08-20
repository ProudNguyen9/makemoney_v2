using Microsoft.AspNetCore.Mvc;
using ScrapWebsite.Models;

namespace ScrapWebsite.ViewComponents;

public class HeroBannerViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(Banner banner)
    {
        return View(banner);
    }
}
