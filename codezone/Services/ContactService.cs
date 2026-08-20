using ScrapWebsite.Models;
using ScrapWebsite.Services.Interfaces;

namespace ScrapWebsite.Services;

public class ContactService : IContactService
{
    public Task SaveRequestAsync(ContactRequest request)
    {
        request.CreatedAt = DateTime.UtcNow;
        return Task.CompletedTask;
    }
}
