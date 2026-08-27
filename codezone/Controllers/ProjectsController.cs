using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScrapWebsite.Data;
using ScrapWebsite.Models;
using ScrapWebsite.Services.Public;

namespace ScrapWebsite.Controllers;

public class ProjectsController : Controller
{
    private readonly AppDbContext _dbContext;

    public ProjectsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var projects = await _dbContext.Projects
            .AsNoTracking()
            .Where(project => project.DeletedAt == null && project.Status == PublicConstants.Published)
            .OrderByDescending(project => project.IsFeatured)
            .ThenBy(project => project.SortOrder)
            .ThenBy(project => project.Id)
            .Select(project => new ProjectCardDto(
                project.Id,
                project.Title,
                project.Slug,
                project.ProjectType,
                project.LocationText,
                project.Excerpt,
                project.CoverImage,
                project.CompletedAt,
                project.QuantityText,
                project.DurationText))
            .ToListAsync(cancellationToken);

        var projectTypes = projects
            .Select(project => project.ProjectType)
            .Where(type => !string.IsNullOrWhiteSpace(type))
            .Distinct()
            .OrderBy(type => type)
            .ToList();

        ViewData["Projects"] = projects;
        ViewData["ProjectTypes"] = projectTypes;
        return View();
    }

    public async Task<IActionResult> Detail(string? slug, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return RedirectToAction(nameof(Index));
        }

        // PRJ-003: dự án nháp / đã xóa → 404, không render template tĩnh.
        var project = await _dbContext.Projects
            .AsNoTracking()
            .Where(item => item.DeletedAt == null && item.Status == PublicConstants.Published && item.Slug == slug)
            .Select(item => new ProjectDetailDto(
                item.Id,
                item.Title,
                item.Slug,
                item.ProjectType,
                item.LocationText,
                item.Excerpt,
                item.ContentHtml,
                item.CoverImage,
                item.CompletedAt,
                item.QuantityText,
                item.DurationText,
                item.Images
                    .OrderBy(image => image.SortOrder)
                    .ThenBy(image => image.Id)
                    .Select(image => new ProjectGalleryImageDto(image.ImageUrl, image.AltText))
                    .ToList()))
            .FirstOrDefaultAsync(cancellationToken);

        if (project is null)
        {
            return NotFound();
        }

        ViewData["Project"] = project;
        return View();
    }
}

public sealed record ProjectCardDto(
    int Id,
    string Title,
    string Slug,
    string? ProjectType,
    string? LocationText,
    string? Excerpt,
    string? CoverImage,
    DateOnly? CompletedAt,
    string? QuantityText,
    string? DurationText);

public sealed record ProjectGalleryImageDto(string ImageUrl, string? AltText);

public sealed record ProjectDetailDto(
    int Id,
    string Title,
    string Slug,
    string? ProjectType,
    string? LocationText,
    string? Excerpt,
    string? ContentHtml,
    string? CoverImage,
    DateOnly? CompletedAt,
    string? QuantityText,
    string? DurationText,
    IReadOnlyList<ProjectGalleryImageDto> Images);
