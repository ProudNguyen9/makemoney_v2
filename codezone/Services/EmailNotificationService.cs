using System.Net;
using System.Net.Mail;
using System.Text;
using ScrapWebsite.Models;
using ScrapWebsite.Services.Interfaces;

namespace ScrapWebsite.Services;

public sealed class EmailNotificationService : IEmailNotificationService
{
    private const string VietnamTimeZoneId = "SE Asia Standard Time";
    private const string DefaultPublicBaseUrl = "https://phelieuminhduc.com";

    // Bảng màu đồng bộ nhận diện website (xanh lá phế liệu + vàng nhấn).
    private const string BrandDark = "#133d2a";
    private const string BrandGreen = "#167447";
    private const string BrandAmber = "#f5b301";
    private const string InkColor = "#1f2a24";
    private const string MutedColor = "#7b857f";
    private const string LineColor = "#e3e8e2";

    private readonly ISmtpSettingsProvider _smtpSettingsProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailNotificationService> _logger;

    public EmailNotificationService(
        ISmtpSettingsProvider smtpSettingsProvider,
        IConfiguration configuration,
        ILogger<EmailNotificationService> logger)
    {
        _smtpSettingsProvider = smtpSettingsProvider;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendContactLeadEmailAsync(ContactRequest request, string requestCode, string? adminEmail)
    {
        var options = await _smtpSettingsProvider.GetAsync();

        // Ưu tiên "Email nhận thông báo liên hệ" đã lưu trong phần cài đặt SMTP,
        // sau đó mới tới email liên hệ hiển thị trên website và email người gửi.
        var toEmail = FirstNonEmpty(options.ToEmail, adminEmail, options.FromEmail);
        var fromEmail = FirstNonEmpty(options.FromEmail, options.UserName);

        if (string.IsNullOrWhiteSpace(options.Host) || string.IsNullOrWhiteSpace(toEmail) || string.IsNullOrWhiteSpace(fromEmail))
        {
            _logger.LogWarning(
                "Bỏ qua gửi email yêu cầu {Code}: chưa cấu hình Smtp Host hoặc địa chỉ email người nhận.",
                requestCode);
            return;
        }

        var subject = $"🔔 {requestCode} – Yêu cầu mới từ {request.Name ?? "Khách hàng"} ({request.Phone})";
        var body = BuildBody(request, requestCode);
        var fromName = options.FromName;

        // Fire-and-forget: chỉ dùng giá trị đã resolve, không chạm DbContext trong background thread.
        _ = Task.Run(async () =>
        {
            try
            {
                using var message = new MailMessage
                {
                    From = new MailAddress(fromEmail, EncodeDisplayName(fromName), Encoding.UTF8),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                    BodyEncoding = Encoding.UTF8,
                    SubjectEncoding = Encoding.UTF8
                };
                message.To.Add(new MailAddress(toEmail));

                using var client = CreateClient(options);
                await client.SendMailAsync(message);

                _logger.LogInformation("Đã gửi email thông báo yêu cầu {Code} tới {To}.", requestCode, toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Không gửi được email thông báo yêu cầu {Code} tới {To}.", requestCode, toEmail);
            }
        });
    }

    public async Task SendTestEmailAsync(string toEmail)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
        {
            throw new InvalidOperationException("Vui lòng nhập địa chỉ nhận thư thử.");
        }

        var options = await _smtpSettingsProvider.GetAsync();
        if (string.IsNullOrWhiteSpace(options.Host) || string.IsNullOrWhiteSpace(options.FromEmail))
        {
            throw new InvalidOperationException("Chưa cấu hình máy chủ SMTP hoặc email người gửi.");
        }

        var body = BuildTestBody(options.FromName);

        using var message = new MailMessage
        {
            From = new MailAddress(options.FromEmail, EncodeDisplayName(options.FromName), Encoding.UTF8),
            Subject = "✅ Email thử nghiệm – cấu hình SMTP hoạt động",
            Body = body,
            IsBodyHtml = true,
            BodyEncoding = Encoding.UTF8,
            SubjectEncoding = Encoding.UTF8
        };
        message.To.Add(new MailAddress(toEmail.Trim()));

        using var client = CreateClient(options);
        await client.SendMailAsync(message);

        _logger.LogInformation("Đã gửi email thử nghiệm tới {To}.", toEmail);
    }

    private static SmtpClient CreateClient(SmtpOptions options)
    {
        var client = new SmtpClient(options.Host, options.Port)
        {
            EnableSsl = options.EnableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Timeout = 15000
        };

        if (!string.IsNullOrWhiteSpace(options.UserName))
        {
            client.Credentials = new NetworkCredential(options.UserName, options.Password);
        }

        return client;
    }

    private static string EncodeDisplayName(string displayName)
    {
        return displayName.Replace("\"", string.Empty).Trim();
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        return null;
    }

    private string ResolveBaseUrl()
    {
        var configured = _configuration["PublicBaseUrl"];
        return FirstNonEmpty(configured, DefaultPublicBaseUrl)!;
    }

    private string AbsoluteUrl(string url)
    {
        if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return url;
        }

        return ResolveBaseUrl().TrimEnd('/') + "/" + url.TrimStart('/');
    }

    // ------------------------------------------------------------------
    // Template email thông báo lead
    // ------------------------------------------------------------------
    private string BuildBody(ContactRequest request, string requestCode)
    {
        var customerName = WebUtility.HtmlEncode(FirstNonEmpty(request.Name, "Khách hàng")!);
        var phone = WebUtility.HtmlEncode(request.Phone ?? string.Empty);
        var timeText = WebUtility.HtmlEncode(FormatVietnamTime(request.CreatedAt));
        var telLink = "tel:" + (request.Phone ?? string.Empty).Replace(" ", string.Empty);
        var zaloLink = string.IsNullOrWhiteSpace(request.Zalo)
            ? null
            : "https://zalo.me/" + WebUtility.HtmlEncode(request.Zalo.Replace(" ", string.Empty));

        var builder = new StringBuilder();

        builder.Append("<!DOCTYPE html><html><body style=\"margin:0;padding:0;background:#f0f2ef;\">");
        builder.Append("<table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"background:#f0f2ef;padding:24px 12px;\"><tr><td align=\"center\">");

        // Khung chính
        builder.Append("<table role=\"presentation\" width=\"640\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"max-width:640px;width:100%;background:#ffffff;border-radius:14px;overflow:hidden;box-shadow:0 2px 10px rgba(19,61,42,.08);font-family:Arial,Helvetica,sans-serif;color:#1f2a24;\">");

        // ---- Header ----
        builder.Append("<tr><td style=\"background:#133d2a;padding:26px 32px 22px;\">");
        builder.Append("<div style=\"font-size:11px;letter-spacing:3px;color:#f5b301;font-weight:bold;text-transform:uppercase;margin-bottom:6px;\">Phế Liệu Thành Trung</div>");
        builder.Append("<div style=\"font-size:22px;line-height:28px;color:#ffffff;font-weight:bold;\">Yêu cầu báo giá mới 🚛</div>");
        builder.Append("<div style=\"font-size:13px;color:#bcd4c6;margin-top:4px;\">Vừa có khách gửi form trên website</div>");
        builder.Append("</td></tr>");

        // ---- Dải mã yêu cầu ----
        builder.Append("<tr><td style=\"background:#f5b301;padding:10px 32px;\">");
        builder.Append("<span style=\"font-size:13px;font-weight:bold;color:#133d2a;\">MÃ YÊU CẦU:&nbsp; ").Append(WebUtility.HtmlEncode(requestCode))
               .Append("</span>")
               .Append("<span style=\"float:right;font-size:12px;color:#133d2a;\">").Append(timeText).Append("</span>");
        builder.Append("</td></tr>");

        // ---- Thông tin khách ----
        builder.Append("<tr><td style=\"padding:26px 32px 6px;\">");
        builder.Append("<div style=\"font-size:11px;letter-spacing:2px;text-transform:uppercase;color:#7b857f;margin-bottom:4px;\">Khách hàng</div>");
        builder.Append("<div style=\"font-size:20px;font-weight:bold;color:#133d2a;\">").Append(customerName).Append("</div>");
        builder.Append("</td></tr>");

        // Nút hành động: Gọi + Zalo
        builder.Append("<tr><td style=\"padding:12px 32px 4px;\"><table role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr>");
        if (!string.IsNullOrWhiteSpace(request.Phone))
        {
            builder.Append("<td style=\"padding-right:10px;\"><a href=\"").Append(telLink)
                   .Append("\" style=\"display:inline-block;background:#167447;color:#ffffff;text-decoration:none;font-weight:bold;font-size:15px;padding:12px 26px;border-radius:8px;\">📞 Gọi ngay ")
                   .Append(phone).Append("</a></td>");
        }

        if (zaloLink != null)
        {
            builder.Append("<td><a href=\"").Append(zaloLink)
                   .Append("\" style=\"display:inline-block;background:#ffffff;color:#167447;border:2px solid #167447;text-decoration:none;font-weight:bold;font-size:15px;padding:10px 24px;border-radius:8px;\">Nhắn Zalo</a></td>");
        }

        builder.Append("</tr></table></td></tr>");

        // ---- Lưới thông tin nhanh ----
        builder.Append("<tr><td style=\"padding:18px 32px 0;\"><table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\">");

        AppendFactRow(builder,
            ("Loại phế liệu", request.ScrapType),
            ("Khu vực", request.Area));
        AppendFactRow(builder,
            ("Số lượng", request.QuantityText),
            ("Nguồn", DescribeSource(request.SourceForm)));

        builder.Append("</table></td></tr>");

        // ---- Nội dung khách nhắn ----
        if (!string.IsNullOrWhiteSpace(request.Message))
        {
            builder.Append("<tr><td style=\"padding:16px 32px 0;\"><div style=\"border-left:4px solid #f5b301;background:#f7f8f6;border-radius:0 8px 8px 0;padding:12px 16px;\">");
            builder.Append("<div style=\"font-size:11px;letter-spacing:2px;text-transform:uppercase;color:#7b857f;margin-bottom:4px;\">Ghi chú của khách</div>");
            builder.Append("<div style=\"font-size:14px;line-height:21px;color:#1f2a24;white-space:pre-line;\">")
                   .Append(WebUtility.HtmlEncode(request.Message)).Append("</div></div></td></tr>");
        }

        // ---- Ảnh đính kèm ----
        var files = request.Files
            .Where(file => !string.IsNullOrWhiteSpace(file.FileUrl))
            .ToList();

        if (files.Count > 0)
        {
            builder.Append("<tr><td style=\"padding:20px 32px 0;\">");
            builder.Append("<div style=\"font-size:11px;letter-spacing:2px;text-transform:uppercase;color:#7b857f;margin-bottom:8px;\">Ảnh phế liệu đính kèm (").Append(files.Count).Append(")</div>");
            builder.Append("<table role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr>");

            foreach (var file in files.Take(3))
            {
                var fullUrl = WebUtility.HtmlEncode(AbsoluteUrl(file.FileUrl!));
                builder.Append("<td style=\"padding-right:10px;\"><a href=\"").Append(fullUrl)
                       .Append("\"><img src=\"").Append(fullUrl)
                       .Append("\" alt=\"Ảnh phế liệu\" width=\"120\" height=\"90\" style=\"display:block;border-radius:8px;border:1px solid #e3e8e2;object-fit:cover;\"/></a></td>");
            }

            builder.Append("</tr></table></td></tr>");
        }

        // ---- CTA quản trị ----
        builder.Append("<tr><td style=\"padding:24px 32px 0;\"><table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"background:#f7f8f6;border-radius:10px;\"><tr><td align=\"center\" style=\"padding:16px;\">");
        builder.Append("<a href=\"").Append(WebUtility.HtmlEncode(ResolveBaseUrl())).Append("/admin/leads")
               .Append("\" style=\"display:inline-block;background:#133d2a;color:#ffffff;text-decoration:none;font-weight:bold;font-size:14px;padding:11px 30px;border-radius:8px;\">Xử lý trong trang quản trị →</a>");
        builder.Append("<div style=\"font-size:12px;color:#7b857f;margin-top:8px;\">Phản hồi khách trong 10 phút để giữ uy tín dịch vụ.</div>");
        builder.Append("</td></tr></table></td></tr>");

        // ---- Footer ----
        builder.Append("<tr><td style=\"padding:20px 32px 26px;border-top:1px solid #e3e8e2;margin-top:20px;\">");
        builder.Append("<div style=\"font-size:12px;line-height:18px;color:#7b857f;\">Email tự động từ hệ thống website thu mua phế liệu.<br/>Trang gửi: ")
               .Append(WebUtility.HtmlEncode(request.SourceUrl ?? "/"))
               .Append(" · Gửi lúc: ").Append(timeText)
               .Append("</div></td></tr>");

        builder.Append("</table></td></tr></table></body></html>");
        return builder.ToString();
    }

    private static void AppendFactRow(StringBuilder builder, (string Label, string? Value) left, (string Label, string? Value) right)
    {
        void AppendCell(StringBuilder b, (string Label, string? Value) fact, bool hasBorder)
        {
            b.Append("<td width=\"50%\" valign=\"top\" style=\"padding:10px 14px;")
             .Append(hasBorder ? "border-left:1px solid #e3e8e2;" : "")
             .Append("\">");

            if (!string.IsNullOrWhiteSpace(fact.Value))
            {
                b.Append("<div style=\"font-size:11px;letter-spacing:1px;text-transform:uppercase;color:#7b857f;\">")
                 .Append(WebUtility.HtmlEncode(fact.Label)).Append("</div>")
                 .Append("<div style=\"font-size:14px;font-weight:bold;color:#1f2a24;margin-top:2px;\">")
                 .Append(WebUtility.HtmlEncode(fact.Value)).Append("</div>");
            }

            b.Append("</td>");
        }

        if (string.IsNullOrWhiteSpace(left.Value) && string.IsNullOrWhiteSpace(right.Value))
        {
            return;
        }

        builder.Append("<tr>");
        AppendCell(builder, left, hasBorder: false);
        AppendCell(builder, right, hasBorder: true);
        builder.Append("</tr>");
    }

    // ------------------------------------------------------------------
    // Template email thử nghiệm
    // ------------------------------------------------------------------
    private string BuildTestBody(string? fromName)
    {
        var brandName = WebUtility.HtmlEncode(FirstNonEmpty(fromName, "Website Phế Liệu Thành Trung")!);

        var builder = new StringBuilder();
        builder.Append("<!DOCTYPE html><html><body style=\"margin:0;padding:0;background:#f0f2ef;\">");
        builder.Append("<table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"background:#f0f2ef;padding:24px 12px;\"><tr><td align=\"center\">");
        builder.Append("<table role=\"presentation\" width=\"560\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"max-width:560px;width:100%;background:#ffffff;border-radius:14px;overflow:hidden;font-family:Arial,Helvetica,sans-serif;\">");

        builder.Append("<tr><td style=\"background:#133d2a;padding:24px 32px;\">");
        builder.Append("<div style=\"font-size:11px;letter-spacing:3px;color:#f5b301;font-weight:bold;text-transform:uppercase;margin-bottom:6px;\">Phế Liệu Thành Trung</div>");
        builder.Append("<div style=\"font-size:20px;color:#ffffff;font-weight:bold;\">✅ Email thử nghiệm thành công</div>");
        builder.Append("</td></tr>");

        builder.Append("<tr><td style=\"padding:26px 32px;\">");
        builder.Append("<p style=\"font-size:14px;line-height:22px;color:#1f2a24;margin:0 0 10px;\">Chúc mừng! Nếu bạn đọc được email này nghĩa là cấu hình SMTP đang hoạt động bình thường.</p>");
        builder.Append("<p style=\"font-size:14px;line-height:22px;color:#1f2a24;margin:0;\">Từ giờ, mỗi khi có khách gửi yêu cầu báo giá trên website, hệ thống sẽ gửi thư thông báo chi tiết đến hộp thư này.</p>");
        builder.Append("</td></tr>");

        builder.Append("<tr><td style=\"padding:0 32px 26px;\">");
        builder.Append("<div style=\"font-size:12px;color:#7b857f;border-top:1px solid #e3e8e2;padding-top:14px;\">Email tự động từ ").Append(brandName).Append(" – vui lòng không trả lời email này.</div>");
        builder.Append("</td></tr>");

        builder.Append("</table></td></tr></table></body></html>");
        return builder.ToString();
    }

    private static string DescribeSource(string? sourceForm)
    {
        return sourceForm switch
        {
            "quick_quote" => "Form báo giá nhanh",
            "contact" => "Trang liên hệ",
            _ => sourceForm ?? string.Empty
        };
    }

    private static string FormatVietnamTime(DateTime utcTime)
    {
        try
        {
            var zone = TimeZoneInfo.FindSystemTimeZoneById(VietnamTimeZoneId);
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utcTime, DateTimeKind.Utc), zone)
                .ToString("HH:mm dd/MM/yyyy");
        }
        catch
        {
            return utcTime.ToString("HH:mm dd/MM/yyyy 'UTC'");
        }
    }
}
