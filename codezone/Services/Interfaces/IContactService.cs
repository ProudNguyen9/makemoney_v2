using ScrapWebsite.Models;

namespace ScrapWebsite.Services.Interfaces;

public interface IContactService
{
    Task SaveRequestAsync(ContactRequest request);
}
