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

        // LEAD-001: form riêng của trang /lien-he luôn gắn nguồn "contact" phía server.
        request.SourceForm = "contact";
        request.SourceUrl ??= "/lien-he";
        request.Status = "new";

        await _contactService.SaveRequestAsync(request);

        return View(new ContactViewModel
        {
            Request = new ContactRequest(),
            IsSubmitted = true
        });
    }

    [HttpPost("/contact/quick-quote")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuickQuote(QuickQuoteRequestViewModel request, CancellationToken cancellationToken)
    {
        if (request.Images.Count(file => file is { Length: > 0 }) > 3)
        {
            return BadRequest(new { ok = false, message = "Bạn chỉ gửi tối đa 3 ảnh." });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(new { ok = false, message = "Vui lòng kiểm tra lại số điện thoại, loại phế liệu và khu vực." });
        }

        try
        {
            var id = await _contactService.SaveQuickQuoteAsync(request, cancellationToken);
            return Json(new { ok = true, id, code = $"LE-{id:0000}" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { ok = false, message = ex.Message });
        }
        catch
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { ok = false, message = "Không lưu được yêu cầu, vui lòng thử lại hoặc gọi hotline." });
        }
    }
}
