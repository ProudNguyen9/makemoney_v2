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
        app.UseRouting();
        app.UseAuthorization();

        return app;
    }
}
