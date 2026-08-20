using Microsoft.AspNetCore.Mvc;
using ScrapWebsite.Models;

namespace ScrapWebsite.ViewComponents;

public class ScrapCarouselViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(IReadOnlyList<ScrapItem> items)
    {
        return View(items);
    }
}
