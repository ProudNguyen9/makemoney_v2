using Microsoft.AspNetCore.Mvc;
using ScrapWebsite.Services.Interfaces;

namespace ScrapWebsite.ViewComponents;

public class HeaderViewComponent : ViewComponent
{
    private readonly ISiteChromeService _siteChromeService;
    private readonly IPublicScrapQueryService _scrapQueryService;

    public HeaderViewComponent(ISiteChromeService siteChromeService, IPublicScrapQueryService scrapQueryService)
    {
        _siteChromeService = siteChromeService;
        _scrapQueryService = scrapQueryService;
    }

    public async Task<IViewComponentResult> InvokeAsync(CancellationToken cancellationToken = default)
    {
        var model = await _siteChromeService.GetAsync(cancellationToken);
        var categories = await _scrapQueryService.GetCategoryGroupsAsync(cancellationToken);
        return View(model with { ScrapCategories = categories });
    }
}
