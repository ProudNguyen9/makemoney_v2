using ScrapWebsite.Data;
using ScrapWebsite.Services.Interfaces;
using ScrapWebsite.ViewModels;

namespace ScrapWebsite.Services;

public class PostService : IPostService
{
    public Task<NewsIndexViewModel> GetIndexAsync()
    {
        return Task.FromResult(new NewsIndexViewModel
        {
            Seo = new SharedSeoViewModel
            {
                Title = "Tin tuc",
                Description = "Tin tuc va kinh nghiem thu mua phe lieu."
            },
            Posts = SeedData.LatestPosts
        });
    }

    public Task<NewsDetailViewModel?> GetDetailAsync(string slug)
    {
        var post = SeedData.LatestPosts
            .FirstOrDefault(item => string.Equals(item.Slug, slug, StringComparison.OrdinalIgnoreCase));

        if (post is null)
        {
            return Task.FromResult<NewsDetailViewModel?>(null);
        }

        return Task.FromResult<NewsDetailViewModel?>(new NewsDetailViewModel
        {
            Seo = new SharedSeoViewModel
            {
                Title = post.Title,
                Description = post.Excerpt ?? "Chi tiet tin tuc."
            },
            Post = post,
            RelatedPosts = SeedData.LatestPosts.Where(item => item.Id != post.Id).ToList()
        });
    }
}
