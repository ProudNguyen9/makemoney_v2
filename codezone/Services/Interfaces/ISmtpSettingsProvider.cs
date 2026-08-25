using ScrapWebsite.Services;

namespace ScrapWebsite.Services.Interfaces;

public interface ISmtpSettingsProvider
{
    Task<SmtpOptions> GetAsync(CancellationToken cancellationToken = default);
}
