using ScrapWebsite.Services;
using ScrapWebsite.Services.Interfaces;
using ScrapWebsite.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using ScrapWebsite.Services.Admin;
using ScrapWebsite.Services.Public;

namespace ScrapWebsite.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddScrapWebsiteServices(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddAuthentication(AdminAuthDefaults.AuthenticationScheme)
            .AddCookie(AdminAuthDefaults.AuthenticationScheme, options =>
            {
                options.Cookie.Name = "ScrapWebsite.Admin";
                options.LoginPath = AdminAuthDefaults.LoginPath;
                options.LogoutPath = AdminAuthDefaults.LogoutPath;
                options.AccessDeniedPath = AdminAuthDefaults.LoginPath;
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
            });

        services.AddAuthorization();
        services.AddControllersWithViews();
        services.AddMemoryCache();
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IPublicSeoQueryService, PublicSeoQueryService>();
        services.AddScoped<IPublicHomeQueryService, PublicHomeQueryService>();
        services.AddScoped<IPublicScrapQueryService, PublicScrapQueryService>();
        services.AddScoped<IPublicPostQueryService, PublicPostQueryService>();
        services.AddScoped<ISiteChromeService, SiteChromeService>();
        services.AddScoped<IPublicPageContentService, PublicPageContentService>();
        services.AddScoped<IContactService, ContactService>();
        services.AddScoped<AdminQueryService>();
        services.AddScoped<IAdminAuthService, AdminAuthService>();
        services.AddScoped<IAdminDashboardQueryService>(provider => provider.GetRequiredService<AdminQueryService>());
        services.AddScoped<IAdminScrapQueryService>(provider => provider.GetRequiredService<AdminQueryService>());
        services.AddScoped<IAdminArticleQueryService>(provider => provider.GetRequiredService<AdminQueryService>());
        services.AddScoped<IAdminPriceQueryService>(provider => provider.GetRequiredService<AdminQueryService>());
        services.AddScoped<IAdminSeoQueryService>(provider => provider.GetRequiredService<AdminQueryService>());
        services.AddScoped<IAdminSettingsQueryService>(provider => provider.GetRequiredService<AdminQueryService>());

        return services;
    }
}
