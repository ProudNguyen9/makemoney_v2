using Microsoft.AspNetCore.Mvc;
using ScrapWebsite.Services.Interfaces;

namespace ScrapWebsite.ViewComponents;

public class FooterViewComponent : ViewComponent
{
    private readonly ISiteChromeService _siteChromeService;

    public FooterViewComponent(ISiteChromeService siteChromeService)
    {
        _siteChromeService = siteChromeService;
    }

    public async Task<IViewComponentResult> InvokeAsync(CancellationToken cancellationToken = default)
    {
        var model = await _siteChromeService.GetAsync(cancellationToken);
        return View(model);
    }
}
