using Microsoft.EntityFrameworkCore;
using ScrapWebsite.Data;
using ScrapWebsite.Models;

namespace ScrapWebsite.Services.Admin;

public sealed class AdminAuthService : IAdminAuthService
{
    private readonly AppDbContext _dbContext;

    public AdminAuthService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AdminUser?> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalizedEmail) || string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        var admin = await _dbContext.AdminUsers
            .FirstOrDefaultAsync(user =>
                user.Email == normalizedEmail &&
                user.IsActive &&
                user.Status == "active" &&
                user.DeletedAt == null,
                cancellationToken);

        if (admin is null || !AdminPasswordHasher.Verify(password, admin.PasswordHash))
        {
            return null;
        }

        admin.LastLoginAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return admin;
    }
}
