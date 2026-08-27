using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScrapWebsite.Data;
using ScrapWebsite.Models;
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
        var services = await _dbContext.Services
            .AsNoTracking()
            .Where(service => service.DeletedAt == null && service.Status == PublicConstants.Published)
            .OrderByDescending(service => service.IsFeatured)
            .ThenBy(service => service.SortOrder)
            .ThenBy(service => service.Id)
            .Select(service => new ServiceCardDto(
                service.Id,
                service.Title,
                service.Slug,
                service.Excerpt,
                service.CoverImage))
            .ToListAsync(cancellationToken);

        ViewData["Services"] = services;
        ViewData["Faq"] = await GetFaqAsync("faqServices", cancellationToken);
        return View();
    }

    public async Task<IActionResult> Detail(string? slug, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return RedirectToAction(nameof(Index));
        }

        // SRV-003: bài chưa xuất bản / đã xóa → 404, không render template tĩnh.
        var service = await _dbContext.Services
            .AsNoTracking()
            .Where(item => item.DeletedAt == null && item.Status == PublicConstants.Published && item.Slug == slug)
            .Select(item => new ServiceDetailDto(
                item.Id,
                item.Title,
                item.Slug,
                item.Excerpt,
                item.ContentHtml,
                item.CoverImage))
            .FirstOrDefaultAsync(cancellationToken);

        if (service is null)
        {
            return NotFound();
        }

        ViewData["Service"] = service;
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

public sealed record ServiceCardDto(int Id, string Title, string Slug, string? Excerpt, string? CoverImage);

public sealed record ServiceDetailDto(int Id, string Title, string Slug, string? Excerpt, string? ContentHtml, string? CoverImage);
