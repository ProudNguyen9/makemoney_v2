using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScrapWebsite.Data;

namespace ScrapWebsite.Controllers;

public class SeoController : Controller
{
    private const string BaseUrl = "https://phelieuminhduc.com";
    private const string Published = "published";
    private readonly AppDbContext _dbContext;

    public SeoController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("sitemap.xml")]
    public async Task<IActionResult> Sitemap(CancellationToken cancellationToken)
    {
        var entries = new Dictionary<string, SitemapItem>(StringComparer.OrdinalIgnoreCase);

        var blockedSeo = await _dbContext.SeoMetadata.AsNoTracking()
            .Where(seo => seo.Status != "active" || !seo.RobotsIndex)
            .Select(seo => new { seo.RoutePath, seo.EntityType, seo.EntityId })
            .ToListAsync(cancellationToken);

        var blocked = blockedSeo
            .Where(seo => !string.IsNullOrWhiteSpace(seo.RoutePath))
            .Select(seo => NormalizeRoute(seo.RoutePath!))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var blockedEntities = blockedSeo
            .Where(seo => seo.EntityId.HasValue)
            .Select(seo => $"{seo.EntityType}:{seo.EntityId}")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        bool IsBlockedEntity(string entityType, int entityId) => blockedEntities.Contains($"{entityType}:{entityId}");
        void Add(string route, DateTime? lastModified = null, string changeFrequency = "weekly", decimal priority = 0.5m)
        {
            route = NormalizeRoute(route);
            if (blocked.Contains(route))
            {
                return;
            }

            entries[route] = new SitemapItem(route, lastModified, changeFrequency, priority);
        }

        var manualEntries = await _dbContext.SeoSitemapEntries.AsNoTracking()
            .Where(entry => entry.IncludeInSitemap)
            .Select(entry => new
            {
                entry.RoutePath,
                entry.LastModifiedAt,
                entry.ChangeFrequency,
                entry.Priority
            })
            .ToListAsync(cancellationToken);

        foreach (var entry in manualEntries)
        {
            Add(entry.RoutePath, entry.LastModifiedAt, entry.ChangeFrequency, entry.Priority);
        }

        Add("/", DateTime.UtcNow, "daily", 1.0m);
        Add("/phe-lieu", DateTime.UtcNow, "daily", 0.9m);
        Add("/tin-tuc", DateTime.UtcNow, "daily", 0.8m);
        Add("/dich-vu", DateTime.UtcNow, "weekly", 0.8m);
        Add("/khu-vuc", DateTime.UtcNow, "weekly", 0.8m);
        Add("/du-an", DateTime.UtcNow, "weekly", 0.7m);
        Add("/bang-gia", DateTime.UtcNow, "daily", 0.9m);
        Add("/lien-he", DateTime.UtcNow, "monthly", 0.6m);

        var scrapItems = await _dbContext.ScrapItems.AsNoTracking()
            .Where(item => item.Status == Published)
            .Select(item => new { item.Id, item.Slug, item.UpdatedAt, item.PublishedAt })
            .ToListAsync(cancellationToken);
        foreach (var item in scrapItems)
        {
            if (IsBlockedEntity("ScrapItem", item.Id)) continue;
            Add($"/phe-lieu/{item.Slug}", item.UpdatedAt != default ? item.UpdatedAt : item.PublishedAt, "weekly", 0.8m);
        }

        var posts = await _dbContext.Posts.AsNoTracking()
            .Where(post => post.Status == Published && post.DeletedAt == null)
            .Select(post => new { post.Id, post.Slug, post.UpdatedAt, post.PublishedAt })
            .ToListAsync(cancellationToken);
        foreach (var post in posts)
        {
            if (IsBlockedEntity("Post", post.Id)) continue;
            Add($"/tin-tuc/{post.Slug}", post.UpdatedAt != default ? post.UpdatedAt : post.PublishedAt, "weekly", 0.7m);
        }

        var services = await _dbContext.Services.AsNoTracking()
            .Where(service => service.Status == Published && service.DeletedAt == null)
            .Select(service => new { service.Id, service.Slug, service.UpdatedAt, service.PublishedAt, service.CreatedAt })
            .ToListAsync(cancellationToken);
        foreach (var service in services)
        {
            if (IsBlockedEntity("Service", service.Id)) continue;
            Add($"/dich-vu/{service.Slug}", service.UpdatedAt ?? service.PublishedAt ?? service.CreatedAt, "weekly", 0.7m);
        }

        var locations = await _dbContext.Locations.AsNoTracking()
            .Where(location => location.Status == Published && location.DeletedAt == null)
            .Select(location => new { location.Id, location.Slug, location.UpdatedAt, location.PublishedAt, location.CreatedAt })
            .ToListAsync(cancellationToken);
        foreach (var location in locations)
        {
            if (IsBlockedEntity("Location", location.Id)) continue;
            Add($"/khu-vuc/{location.Slug}", location.UpdatedAt ?? location.PublishedAt ?? location.CreatedAt, "weekly", 0.6m);
        }

        var projects = await _dbContext.Projects.AsNoTracking()
            .Where(project => project.Status == Published && project.DeletedAt == null)
            .Select(project => new { project.Id, project.Slug, project.UpdatedAt, project.PublishedAt, project.CreatedAt })
            .ToListAsync(cancellationToken);
        foreach (var project in projects)
        {
            if (IsBlockedEntity("Project", project.Id)) continue;
            Add($"/du-an/{project.Slug}", project.UpdatedAt ?? project.PublishedAt ?? project.CreatedAt, "monthly", 0.6m);
        }

        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        var document = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement(ns + "urlset",
                entries.Values
                    .OrderByDescending(entry => entry.Priority)
                    .ThenBy(entry => entry.Route)
                    .Select(entry => new XElement(ns + "url",
                        new XElement(ns + "loc", $"{BaseUrl}{entry.Route}"),
                        entry.LastModified is null ? null : new XElement(ns + "lastmod", entry.LastModified.Value.ToString("yyyy-MM-dd")),
                        new XElement(ns + "changefreq", entry.ChangeFrequency),
                        new XElement(ns + "priority", entry.Priority.ToString("0.0#"))))));

        return Content(document.ToString(SaveOptions.DisableFormatting), "application/xml; charset=utf-8");
    }

    [HttpGet("robots.txt")]
    public IActionResult Robots()
        => Content($"User-agent: *\nAllow: /\nSitemap: {BaseUrl}/sitemap.xml\n", "text/plain; charset=utf-8");

    private static string NormalizeRoute(string route)
    {
        route = string.IsNullOrWhiteSpace(route) ? "/" : route.Trim();
        if (route.StartsWith(BaseUrl, StringComparison.OrdinalIgnoreCase))
        {
            route = route[BaseUrl.Length..];
        }

        if (!route.StartsWith('/'))
        {
            route = $"/{route}";
        }

        return route == "/" ? route : route.TrimEnd('/');
    }

    private sealed record SitemapItem(string Route, DateTime? LastModified, string ChangeFrequency, decimal Priority);
}
