using ScrapWebsite.Models;
using ScrapWebsite.Services.Interfaces;
using ScrapWebsite.Services.Media;
using ScrapWebsite.Data;
using ScrapWebsite.ViewModels;

namespace ScrapWebsite.Services;

public class ContactService : IContactService
{
    private readonly AppDbContext _dbContext;
    private readonly IImageUploadService _imageUploadService;

    public ContactService(AppDbContext dbContext, IImageUploadService imageUploadService)
    {
        _dbContext = dbContext;
        _imageUploadService = imageUploadService;
    }

    public async Task<int> SaveRequestAsync(ContactRequest request, CancellationToken cancellationToken = default)
    {
        request.CreatedAt = DateTime.UtcNow;
        request.Status = string.IsNullOrWhiteSpace(request.Status) ? "new" : request.Status;
        _dbContext.ContactRequests.Add(request);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return request.Id;
    }

    public async Task<int> SaveQuickQuoteAsync(QuickQuoteRequestViewModel form, CancellationToken cancellationToken)
    {
        var request = new ContactRequest
        {
            Name = Clean(form.Name),
            Phone = Clean(form.Phone) ?? string.Empty,
            Zalo = Clean(form.Zalo),
            ScrapType = Clean(form.Scrap),
            QuantityText = Clean(form.Quantity),
            Area = Clean(form.Area),
            Message = Clean(form.Note),
            SourceForm = "quick_quote",
            SourceUrl = Clean(form.SourceUrl),
            Status = "new",
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.ContactRequests.Add(request);

        var imageFiles = form.Images
            .Where(file => file is { Length: > 0 })
            .ToList();

        if (imageFiles.Count > 3)
        {
            throw new InvalidOperationException("Bạn chỉ gửi tối đa 3 ảnh.");
        }

        foreach (var file in imageFiles)
        {
            var upload = await _imageUploadService.SaveAsWebpAsync(file, "content", "quote-image", 1600, cancellationToken);
            if (!upload.Success || string.IsNullOrWhiteSpace(upload.Url))
            {
                throw new InvalidOperationException(upload.Error ?? "Không tải được ảnh báo giá.");
            }

            request.Files.Add(new ContactRequestFile
            {
                FileUrl = upload.Url,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return request.Id;
    }

    private static string? Clean(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
