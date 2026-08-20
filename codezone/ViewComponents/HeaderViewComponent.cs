using Microsoft.AspNetCore.Mvc;

namespace ScrapWebsite.ViewComponents;

public class HeaderViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        return View();
    }
}
