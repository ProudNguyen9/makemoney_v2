using Microsoft.EntityFrameworkCore;
using ScrapWebsite.Data;
using ScrapWebsite.Helpers;
using ScrapWebsite.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace ScrapWebsite.Services.Media;

public sealed record ImageUploadResult(
    bool Success,
    string? Error,
    string? Url,
    long OriginalBytes,
    long OptimizedBytes,
    int Width,
    int Height)
{
    public static ImageUploadResult Failure(string error) => new(false, error, null, 0, 0, 0, 0);
}

public interface IImageUploadService
{
    /// <summary>
    /// Validates the uploaded image, auto-orients, downsizes to maxWidth when needed,
    /// re-encodes as WebP and stores it under wwwroot/uploads. Original bytes are discarded.
    /// </summary>
    Task<ImageUploadResult> SaveAsWebpAsync(
        IFormFile? file,
        string folder,
        string? nameHint,
        int maxWidth,
        CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a previously uploaded file (physical file + MediaFile row) when the image is replaced or removed.
    /// Only URLs under /uploads/ are touched.
    /// </summary>
    Task DeleteUploadedImageAsync(string? url, CancellationToken cancellationToken);
}

public sealed class ImageUploadService : IImageUploadService
{
    private const long FallbackMaxUploadBytes = 10 * 1024 * 1024;
    private const int FallbackQuality = 80;

    private static readonly string[] AllowedFolders = ["scrap", "service", "location", "project", "content", "brand"];

    private readonly AppDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;
    private readonly long _maxUploadBytes;
    private readonly int _quality;

    public ImageUploadService(AppDbContext dbContext, IWebHostEnvironment environment, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _environment = environment;
        _maxUploadBytes = configuration.GetValue<long?>("Media:MaxUploadBytes") ?? FallbackMaxUploadBytes;
        _quality = configuration.GetValue<int?>("Media:Quality") ?? FallbackQuality;
    }

    public async Task<ImageUploadResult> SaveAsWebpAsync(
        IFormFile? file,
        string folder,
        string? nameHint,
        int maxWidth,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return ImageUploadResult.Failure("Chưa chọn tệp ảnh.");
        }

        if (file.Length > _maxUploadBytes)
        {
            return ImageUploadResult.Failure($"Tệp quá lớn ({file.Length / 1024.0 / 1024.0:N1} MB). Tối đa {_maxUploadBytes / 1024 / 1024} MB.");
        }

        var safeFolder = AllowedFolders.Contains(folder, StringComparer.OrdinalIgnoreCase) ? folder.ToLowerInvariant() : "content";

        await using var sourceStream = file.OpenReadStream();
        IImageFormat format;
        try
        {
            // Decoding doubles as content validation: magic bytes are checked by ImageSharp,
            // a mismatched content-type or a renamed file still fails here.
            format = Image.DetectFormat(sourceStream);
        }
        catch (UnknownImageFormatException)
        {
            return ImageUploadResult.Failure("Định dạng ảnh không hợp lệ. Chỉ nhận JPG, PNG, WebP hoặc GIF.");
        }

        if (format is not (JpegFormat or PngFormat or WebpFormat or GifFormat))
        {
            return ImageUploadResult.Failure("Định dạng ảnh không được hỗ trợ. Chỉ nhận JPG, PNG, WebP hoặc GIF.");
        }

        sourceStream.Seek(0, SeekOrigin.Begin);
        try
        {
            using var image = await Image.LoadAsync(sourceStream, cancellationToken);
            image.Mutate(context => context.AutoOrient());
            if (image.Width > maxWidth)
            {
                image.Mutate(context => context.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(maxWidth, 0)
                }));
            }

            var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
            var monthFolder = DateTime.UtcNow.ToString("yyyyMM");
            var directory = Path.Combine(webRoot, "uploads", safeFolder, monthFolder);
            Directory.CreateDirectory(directory);

            var baseName = SlugHelper.ToSlug(string.IsNullOrWhiteSpace(nameHint) ? Path.GetFileNameWithoutExtension(file.FileName) : nameHint);
            var fileName = $"{baseName}-{Guid.NewGuid().ToString("N")[..8]}.webp";
            var fullPath = Path.Combine(directory, fileName);

            await using (var targetStream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                await image.SaveAsync(targetStream, new WebpEncoder { Quality = _quality }, cancellationToken);
            }

            var url = $"/uploads/{safeFolder}/{monthFolder}/{fileName}";
            var optimizedBytes = new FileInfo(fullPath).Length;

            _dbContext.MediaFiles.Add(new MediaFile
            {
                FileName = file.FileName,
                Url = url,
                Folder = $"{safeFolder}/{monthFolder}",
                MimeType = "image/webp",
                Status = "active"
            });
            // No SaveChanges here: the MediaFile row commits atomically with the entity
            // that references the image in the calling command service.

            return new ImageUploadResult(true, null, url, file.Length, optimizedBytes, image.Width, image.Height);
        }
        catch (UnknownImageFormatException)
        {
            return ImageUploadResult.Failure("Không đọc được nội dung ảnh. Tệp có thể bị hỏng.");
        }
    }

    public async Task DeleteUploadedImageAsync(string? url, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return;
        }

        const string prefix = "/uploads/";
        if (!url.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return; // never touch static assets shipped with the site
        }

        var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
        var relative = url[prefix.Length..].Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(webRoot, "uploads", relative));
        var uploadsRoot = Path.GetFullPath(Path.Combine(webRoot, "uploads"));
        if (!fullPath.StartsWith(uploadsRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        var mediaRow = await _dbContext.MediaFiles.FirstOrDefaultAsync(media => media.Url == url, cancellationToken);
        if (mediaRow is not null)
        {
            _dbContext.MediaFiles.Remove(mediaRow);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
