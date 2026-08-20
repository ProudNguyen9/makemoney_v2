using Microsoft.AspNetCore.Mvc;
using ScrapWebsite.Models;

namespace ScrapWebsite.ViewComponents;

public class LatestNewsViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(IReadOnlyList<Post> posts)
    {
        return View(posts);
    }
}
