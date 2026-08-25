using ScrapWebsite.Services;
using ScrapWebsite.Services.Interfaces;
using ScrapWebsite.Services.Media;
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
        services.AddHttpContextAccessor();
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
        services.Configure<SmtpOptions>(configuration.GetSection(SmtpOptions.SectionName));
        services.AddScoped<ISmtpSettingsProvider, SmtpSettingsProvider>();
        services.AddScoped<IEmailNotificationService, EmailNotificationService>();
        services.Configure<SmtpOptions>(configuration.GetSection(SmtpOptions.SectionName));
        services.AddScoped<ISmtpSettingsProvider, SmtpSettingsProvider>();
        services.AddScoped<AdminQueryService>();
        services.AddScoped<IAdminAuthService, AdminAuthService>();
        services.AddScoped<IAdminDashboardQueryService>(provider => provider.GetRequiredService<AdminQueryService>());
        services.AddScoped<IAdminScrapQueryService>(provider => provider.GetRequiredService<AdminQueryService>());
        services.AddScoped<IAdminArticleQueryService>(provider => provider.GetRequiredService<AdminQueryService>());
        services.AddScoped<IAdminPriceQueryService>(provider => provider.GetRequiredService<AdminQueryService>());
        services.AddScoped<IAdminLeadQueryService>(provider => provider.GetRequiredService<AdminQueryService>());
        services.AddScoped<IAdminSeoQueryService>(provider => provider.GetRequiredService<AdminQueryService>());
        services.AddScoped<IAdminSettingsQueryService>(provider => provider.GetRequiredService<AdminQueryService>());
        services.AddScoped<IAdminMediaQueryService>(provider => provider.GetRequiredService<AdminQueryService>());
        services.AddScoped<IAdminServiceQueryService>(provider => provider.GetRequiredService<AdminQueryService>());
        services.AddScoped<IAdminLocationQueryService>(provider => provider.GetRequiredService<AdminQueryService>());
        services.AddScoped<IAdminProjectQueryService>(provider => provider.GetRequiredService<AdminQueryService>());
        services.AddScoped<IAdminFaqQueryService>(provider => provider.GetRequiredService<AdminQueryService>());

        services.AddScoped<IImageUploadService, ImageUploadService>();
        services.AddScoped<AdminCommandService>();
        services.AddScoped<IAdminPriceCommandService>(provider => provider.GetRequiredService<AdminCommandService>());
        services.AddScoped<IAdminLeadCommandService>(provider => provider.GetRequiredService<AdminCommandService>());
        services.AddScoped<IAdminScrapCommandService>(provider => provider.GetRequiredService<AdminCommandService>());
        services.AddScoped<IAdminServiceCommandService>(provider => provider.GetRequiredService<AdminCommandService>());
        services.AddScoped<IAdminLocationCommandService>(provider => provider.GetRequiredService<AdminCommandService>());
        services.AddScoped<IAdminProjectCommandService>(provider => provider.GetRequiredService<AdminCommandService>());
        services.AddScoped<IAdminFaqCommandService>(provider => provider.GetRequiredService<AdminCommandService>());
        services.AddScoped<IAdminArticleCommandService>(provider => provider.GetRequiredService<AdminCommandService>());
        services.AddScoped<IAdminSettingsCommandService>(provider => provider.GetRequiredService<AdminCommandService>());
        services.AddScoped<IAdminMediaCommandService>(provider => provider.GetRequiredService<AdminCommandService>());
        services.AddScoped<IAdminSeoCommandService>(provider => provider.GetRequiredService<AdminCommandService>());

        return services;
    }
}
