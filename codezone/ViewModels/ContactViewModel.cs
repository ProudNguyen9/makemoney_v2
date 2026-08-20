using ScrapWebsite.Models;

namespace ScrapWebsite.ViewModels;

public class ContactViewModel
{
    public SharedSeoViewModel Seo { get; set; } = new()
    {
        Title = "Lien he"
    };

    public ContactRequest Request { get; set; } = new();

    public bool IsSubmitted { get; set; }
}
