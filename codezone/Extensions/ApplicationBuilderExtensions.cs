namespace ScrapWebsite.Extensions;

public static class ApplicationBuilderExtensions
{
    public static WebApplication UseScrapWebsitePipeline(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseMiddleware<ScrapWebsite.Middleware.PublicHtmlDataMiddleware>();

        return app;
    }
}
