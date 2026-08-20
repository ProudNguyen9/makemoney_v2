using ScrapWebsite.Data;
using ScrapWebsite.Models;
using ScrapWebsite.Services.Interfaces;
using ScrapWebsite.ViewModels;

namespace ScrapWebsite.Services;

public class ScrapService : IScrapService
{
    public Task<ScrapIndexViewModel> GetIndexAsync()
    {
        var items = SeedData.FeaturedScrapItems;

        return Task.FromResult(new ScrapIndexViewModel
        {
            Seo = new SharedSeoViewModel
            {
                Title = "Mat hang phe lieu",
                Description = "Danh sach cac mat hang phe lieu thu mua."
            },
            Items = items
        });
    }

    public Task<ScrapDetailViewModel?> GetDetailAsync(string slug)
    {
        var item = SeedData.FeaturedScrapItems
            .FirstOrDefault(scrap => string.Equals(scrap.Slug, slug, StringComparison.OrdinalIgnoreCase));

        if (item is null)
        {
            return Task.FromResult<ScrapDetailViewModel?>(null);
        }

        var prices = new List<ScrapPrice>
        {
            new()
            {
                ScrapItemId = item.Id,
                PriceLabel = "Lien he bao gia",
                Unit = "kg"
            }
        };

        return Task.FromResult<ScrapDetailViewModel?>(new ScrapDetailViewModel
        {
            Seo = new SharedSeoViewModel
            {
                Title = item.Name,
                Description = item.ShortDescription ?? "Chi tiet mat hang phe lieu."
            },
            Item = item,
            Prices = prices,
            RelatedItems = SeedData.FeaturedScrapItems.Where(scrap => scrap.Id != item.Id).ToList()
        });
    }
}
