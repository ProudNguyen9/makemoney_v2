using Microsoft.AspNetCore.Mvc;
using ScrapWebsite.Models;

namespace ScrapWebsite.ViewComponents;

public class PriceTableViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(IReadOnlyList<ScrapPrice> prices)
    {
        return View(prices);
    }
}
