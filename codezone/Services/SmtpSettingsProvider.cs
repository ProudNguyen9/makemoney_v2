using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using ScrapWebsite.Data;
using ScrapWebsite.Services.Interfaces;

namespace ScrapWebsite.Services;

/// <summary>
/// Đọc cấu hình SMTP từ bảng SiteSettings (do admin chỉnh trên trang quản trị),
/// fallback về appsettings.json khi chưa có dữ liệu trong DB.
/// </summary>
public sealed class SmtpSettingsProvider : ISmtpSettingsProvider
{
    public const string CacheKey = "admin:smtp-settings";
    private const string KeyPrefix = "smtp.";

    private readonly AppDbContext _dbContext;
    private readonly SmtpOptions _fallbackOptions;
    private readonly IMemoryCache _cache;

    public SmtpSettingsProvider(AppDbContext dbContext, IOptions<SmtpOptions> fallbackOptions, IMemoryCache cache)
    {
        _dbContext = dbContext;
        _fallbackOptions = fallbackOptions.Value;
        _cache = cache;
    }

    public async Task<SmtpOptions> GetAsync(CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(CacheKey, out SmtpOptions? cached) && cached is not null)
        {
            return cached;
        }

        var values = await _dbContext.SiteSettings.AsNoTracking()
            .Where(setting => setting.Key.StartsWith(KeyPrefix))
            .Select(setting => new { setting.Key, setting.Value })
            .ToDictionaryAsync(setting => setting.Key, setting => setting.Value ?? string.Empty, cancellationToken);

        var options = new SmtpOptions
        {
            Host = Pick(values, "host", _fallbackOptions.Host),
            Port = ParsePort(Pick(values, "port", null), _fallbackOptions.Port),
            EnableSsl = ParseBool(Pick(values, "enable_ssl", null), _fallbackOptions.EnableSsl),
            UserName = Pick(values, "username", _fallbackOptions.UserName),
            Password = Pick(values, "password", _fallbackOptions.Password),
            FromEmail = Pick(values, "from_email", _fallbackOptions.FromEmail),
            FromName = FirstNonEmpty(Pick(values, "from_name", null), _fallbackOptions.FromName) ?? string.Empty,
            ToEmail = FirstNonEmpty(Pick(values, "to_email", null), _fallbackOptions.ToEmail)
        };

        _cache.Set(CacheKey, options, TimeSpan.FromSeconds(60));
        return options;
    }

    private static string Pick(IReadOnlyDictionary<string, string> values, string suffix, string? fallback)
    {
        var value = values.TryGetValue(KeyPrefix + suffix, out var raw) && !string.IsNullOrWhiteSpace(raw)
            ? raw.Trim()
            : fallback;
        return value ?? string.Empty;
    }

    private static int ParsePort(string? value, int fallback)
        => int.TryParse(value, out var port) && port is > 0 and <= 65535 ? port : fallback;

    private static bool ParseBool(string? value, bool fallback)
        => bool.TryParse(value, out var flag) ? flag : fallback;

    private static string? FirstNonEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        return null;
    }
}
