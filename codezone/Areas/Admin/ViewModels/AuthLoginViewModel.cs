using System.ComponentModel.DataAnnotations;

namespace ScrapWebsite.Areas.Admin.ViewModels;

public sealed class AuthLoginViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập email.")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
    public string Password { get; set; } = string.Empty;

    public bool Remember { get; set; }

    public string? ReturnUrl { get; set; }
}
