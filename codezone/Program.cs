using ScrapWebsite.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScrapWebsiteServices();

var app = builder.Build();

app.UseScrapWebsitePipeline();
app.MapScrapWebsiteRoutes();

app.Run();
