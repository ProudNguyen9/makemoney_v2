using Microsoft.AspNetCore.Mvc;

namespace ScrapWebsite.ViewComponents;

public class ContactCtaViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        return View();
    }
}
