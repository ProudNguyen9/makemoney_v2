using ScrapWebsite.Services;
using ScrapWebsite.Services.Interfaces;

namespace ScrapWebsite.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddScrapWebsiteServices(this IServiceCollection services)
    {
        services.AddControllersWithViews();

        services.AddScoped<IScrapService, ScrapService>();
        services.AddScoped<IPostService, PostService>();
        services.AddScoped<IContactService, ContactService>();
        services.AddScoped<ISiteSettingService, SiteSettingService>();

        return services;
    }
}
