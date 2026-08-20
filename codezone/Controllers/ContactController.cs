using Microsoft.AspNetCore.Mvc;
using ScrapWebsite.Models;
using ScrapWebsite.Services.Interfaces;
using ScrapWebsite.ViewModels;

namespace ScrapWebsite.Controllers;

public class ContactController : Controller
{
    private readonly IContactService _contactService;

    public ContactController(IContactService contactService)
    {
        _contactService = contactService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View(new ContactViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ContactRequest request)
    {
        if (!ModelState.IsValid)
        {
            return View(new ContactViewModel { Request = request });
        }

        await _contactService.SaveRequestAsync(request);

        return View(new ContactViewModel
        {
            Request = new ContactRequest(),
            IsSubmitted = true
        });
    }
}
