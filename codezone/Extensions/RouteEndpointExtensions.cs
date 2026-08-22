namespace ScrapWebsite.Extensions;

public static class RouteEndpointExtensions
{
    public static WebApplication MapScrapWebsiteRoutes(this WebApplication app)
    {
        app.MapStaticAssets();

        app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}")
            .WithStaticAssets();

        app.MapControllerRoute(
                name: "admin-root",
                pattern: "admin",
                defaults: new { area = "Admin", controller = "Home", action = "Index" })
            .WithStaticAssets();

        app.MapControllerRoute(
                name: "admin-login",
                pattern: "admin/login",
                defaults: new { area = "Admin", controller = "Auth", action = "Login" })
            .WithStaticAssets();

        app.MapControllerRoute(
                name: "admin-static-action",
                pattern: "admin/{controller}/{action}",
                defaults: new { area = "Admin", action = "Index" })
            .WithStaticAssets();

        app.MapControllerRoute(
                name: "admin-static-index",
                pattern: "admin/{controller}",
                defaults: new { area = "Admin", action = "Index" })
            .WithStaticAssets();

        app.MapControllerRoute(
                name: "about",
                pattern: "gioi-thieu",
                defaults: new { controller = "Home", action = "About" })
            .WithStaticAssets();

        app.MapControllerRoute(
                name: "capability",
                pattern: "nang-luc",
                defaults: new { controller = "Capability", action = "Index" })
            .WithStaticAssets();

        app.MapControllerRoute(
                name: "scrap-category",
                pattern: "phe-lieu/danh-muc",
                defaults: new { controller = "Scrap", action = "Category" })
            .WithStaticAssets();

        app.MapControllerRoute(
                name: "scrap-category-group",
                pattern: "phe-lieu/nhom/{slug?}",
                defaults: new { controller = "Scrap", action = "Category" })
            .WithStaticAssets();

        app.MapControllerRoute(
                name: "scrap-index",
                pattern: "phe-lieu",
                defaults: new { controller = "Scrap", action = "Index" })
            .WithStaticAssets();

        app.MapControllerRoute(
                name: "scrap-detail",
                pattern: "phe-lieu/{slug}",
                defaults: new { controller = "Scrap", action = "Detail" })
            .WithStaticAssets();

        app.MapControllerRoute(
                name: "services-index",
                pattern: "dich-vu",
                defaults: new { controller = "Services", action = "Index" })
            .WithStaticAssets();

        app.MapControllerRoute(
                name: "services-detail",
                pattern: "dich-vu/{slug}",
                defaults: new { controller = "Services", action = "Detail" })
            .WithStaticAssets();

        app.MapControllerRoute(
                name: "prices",
                pattern: "bang-gia",
                defaults: new { controller = "Prices", action = "Index" })
            .WithStaticAssets();

        app.MapControllerRoute(
                name: "locations-index",
                pattern: "khu-vuc",
                defaults: new { controller = "Locations", action = "Index" })
            .WithStaticAssets();

        app.MapControllerRoute(
                name: "locations-detail",
                pattern: "khu-vuc/{slug}",
                defaults: new { controller = "Locations", action = "Detail" })
            .WithStaticAssets();

        app.MapControllerRoute(
                name: "projects-index",
                pattern: "du-an",
                defaults: new { controller = "Projects", action = "Index" })
            .WithStaticAssets();

        app.MapControllerRoute(
                name: "projects-detail",
                pattern: "du-an/{slug}",
                defaults: new { controller = "Projects", action = "Detail" })
            .WithStaticAssets();

        app.MapControllerRoute(
                name: "news-index",
                pattern: "tin-tuc",
                defaults: new { controller = "News", action = "Index" })
            .WithStaticAssets();

        app.MapControllerRoute(
                name: "news-detail",
                pattern: "tin-tuc/{slug}",
                defaults: new { controller = "News", action = "Detail" })
            .WithStaticAssets();

        app.MapControllerRoute(
                name: "referral",
                pattern: "hoa-hong",
                defaults: new { controller = "Referral", action = "Index" })
            .WithStaticAssets();

        app.MapControllerRoute(
                name: "contact",
                pattern: "lien-he",
                defaults: new { controller = "Contact", action = "Index" })
            .WithStaticAssets();

        app.MapControllerRoute(
                name: "search",
                pattern: "tim-kiem",
                defaults: new { controller = "Search", action = "Index" })
            .WithStaticAssets();

        app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
            .WithStaticAssets();

        return app;
    }
}
