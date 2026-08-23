using ScrapWebsite.Models;
using ScrapWebsite.ViewModels;

namespace ScrapWebsite.Services.Interfaces;

public interface IContactService
{
    Task<int> SaveRequestAsync(ContactRequest request, CancellationToken cancellationToken = default);

    Task<int> SaveQuickQuoteAsync(QuickQuoteRequestViewModel form, CancellationToken cancellationToken);
}
