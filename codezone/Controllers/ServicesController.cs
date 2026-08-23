using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScrapWebsite.Data;
using ScrapWebsite.Services.Public;
using codezone.ViewModels.Shared;

namespace ScrapWebsite.Controllers;

public class ServicesController : Controller
{
    private readonly AppDbContext _dbContext;

    public ServicesController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewData["Faq"] = await GetFaqAsync("faqServices", cancellationToken);
        return View();
    }

    public async Task<IActionResult> Detail(string? slug, CancellationToken cancellationToken)
    {
        ViewData["Faq"] = await GetFaqAsync("faqServiceDetail", cancellationToken);
        return View();
    }

    private async Task<FaqAccordionViewModel> GetFaqAsync(string id, CancellationToken cancellationToken)
    {
        var items = await _dbContext.FaqItems
            .AsNoTracking()
            .Where(faq => faq.DeletedAt == null &&
                          faq.Status == PublicConstants.Published &&
                          (faq.EntityType == "services" || faq.EntityType == "service"))
            .OrderBy(faq => faq.SortOrder)
            .ThenBy(faq => faq.Id)
            .Select(faq => new FaqItemViewModel(faq.Question, faq.Answer))
            .ToListAsync(cancellationToken);

        return new FaqAccordionViewModel(id, items);
    }
}
