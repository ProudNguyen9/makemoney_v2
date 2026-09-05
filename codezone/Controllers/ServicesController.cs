using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScrapWebsite.Data;
using ScrapWebsite.Models;
using ScrapWebsite.Services.Interfaces;
using ScrapWebsite.Services.Public;
using ScrapWebsite.ViewModels.Public;
using codezone.ViewModels.Shared;

namespace ScrapWebsite.Controllers;

public class ServicesController : Controller
{
    private readonly AppDbContext _dbContext;
    private readonly IPublicSeoQueryService _seoQueryService;

    public ServicesController(AppDbContext dbContext, IPublicSeoQueryService seoQueryService)
    {
        _dbContext = dbContext;
        _seoQueryService = seoQueryService;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = 9, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = pageSize <= 0 ? 9 : pageSize;

        var baseQuery = _dbContext.Services
            .AsNoTracking()
            .Where(service => service.DeletedAt == null && service.Status == PublicConstants.Published);

        var totalItems = await baseQuery.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        if (totalPages > 0 && page > totalPages)
        {
            page = totalPages;
        }

        var services = await baseQuery
            .OrderByDescending(service => service.IsFeatured)
            .ThenBy(service => service.SortOrder)
            .ThenBy(service => service.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(service => new ServiceCardDto(
                service.Id,
                service.Title,
                service.Slug,
                service.Excerpt,
                service.CoverImage))
            .ToListAsync(cancellationToken);

        var pager = new NumberedPageDto<ServiceCardDto>(
            services,
            page,
            pageSize,
            totalItems,
            totalPages);

        ViewData["Services"] = services;
        ViewData["Pager"] = pager;
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
                item.CoverImage,
                item.SeoKeywords))
            .FirstOrDefaultAsync(cancellationToken);

        if (service is null)
        {
            return NotFound();
        }

        ViewData["Service"] = service;
        ViewData["Seo"] = await _seoQueryService.GetByEntityAsync(
            "Service",
            service.Id,
            $"/dich-vu/{service.Slug}",
            new SeoDto(
                service.Title,
                service.Excerpt ?? "Chi tiết dịch vụ thu mua phế liệu chuyên nghiệp.",
                service.SeoKeywords,
                CanonicalUrl: $"/dich-vu/{service.Slug}",
                OgImage: service.CoverImage,
                OgType: "website"),
            cancellationToken);
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

public sealed record ServiceDetailDto(int Id, string Title, string Slug, string? Excerpt, string? ContentHtml, string? CoverImage, string? SeoKeywords = null);
