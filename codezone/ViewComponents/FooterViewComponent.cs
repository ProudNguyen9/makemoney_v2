using Microsoft.AspNetCore.Mvc;

namespace ScrapWebsite.ViewComponents;

public class FooterViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        return View();
    }
}
