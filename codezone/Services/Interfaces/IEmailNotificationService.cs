using ScrapWebsite.Models;

namespace ScrapWebsite.Services.Interfaces;

public interface IEmailNotificationService
{
    Task SendContactLeadEmailAsync(ContactRequest request, string requestCode, string? adminEmail);

    Task SendTestEmailAsync(string toEmail);
}
