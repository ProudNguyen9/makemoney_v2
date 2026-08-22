using ScrapWebsite.Models;

namespace ScrapWebsite.Services.Admin;

public interface IAdminAuthService
{
    Task<AdminUser?> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken);
}
