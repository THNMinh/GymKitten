using GymKitten.Application.Abstractions.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace GymKitten.Infrastructure.Mail;

public sealed class MailService : IMailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<MailService> _logger;

    public MailService(IOptions<EmailSettings> options, ILogger<MailService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public async Task SendOtpEmailAsync(string toEmail, string otpCode)
    {
        _logger.LogInformation("[DEV OTP CODE] 🔑 OTP code for {ToEmail} is: {OtpCode} (TTL: 5 minutes)", toEmail, otpCode);

        var subject = "[GymKitten] Mã xác thực OTP tài khoản của bạn";
        var htmlBody = $@"
<div style=""font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px; background-color: #ffffff;"">
    <div style=""text-align: center; margin-bottom: 20px;"">
        <h1 style=""color: #e11d48; margin: 0;"">GYMKITTEN 🐱</h1>
        <p style=""color: #64748b; font-size: 14px;"">Thời trang & Phụ kiện Tập Gym Đỉnh Cao</p>
    </div>
    <div style=""padding: 20px; background-color: #f8fafc; border-radius: 6px;"">
        <h2 style=""color: #1e293b; margin-top: 0;"">Xác Thực Tài Khoản Của Bạn</h2>
        <p style=""color: #334155; font-size: 15px; line-height: 1.5;"">
            Cảm ơn bạn đã đăng ký tài khoản tại <strong>GymKitten</strong>. Vui lòng sử dụng mã xác thực OTP dưới đây để hoàn tất kích hoạt tài khoản:
        </p>
        <div style=""text-align: center; margin: 30px 0;"">
            <span style=""display: inline-block; font-size: 32px; font-weight: bold; letter-spacing: 6px; color: #ffffff; background: linear-gradient(135deg, #e11d48, #be123c); padding: 12px 30px; border-radius: 8px; box-shadow: 0 4px 6px -1px rgba(0,0,0,0.1);"">
                {otpCode}
            </span>
        </div>
        <p style=""color: #64748b; font-size: 13px; text-align: center;"">
            ⏳ Mã OTP này có hiệu lực trong vòng <strong>5 phút</strong>. Tuyệt đối không chia sẻ mã này cho bất kỳ ai.
        </p>
    </div>
    <div style=""margin-top: 20px; text-align: center; color: #94a3b8; font-size: 12px;"">
        <p>Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email.</p>
        <p>&copy; {DateTime.UtcNow.Year} GymKitten Store. All rights reserved.</p>
    </div>
</div>";

        await SendEmailInternalAsync(toEmail, subject, htmlBody);
    }

    public async Task SendResetPasswordEmailAsync(string toEmail, string newPassword)
    {
        _logger.LogInformation("[DEV RESET PASSWORD] 🔒 Temporary password for {ToEmail} is: {NewPassword}", toEmail, newPassword);

        var subject = "[GymKitten] Mật khẩu tạm thời mới của bạn";
        var htmlBody = $@"
<div style=""font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px; background-color: #ffffff;"">
    <div style=""text-align: center; margin-bottom: 20px;"">
        <h1 style=""color: #e11d48; margin: 0;"">GYMKITTEN 🐱</h1>
        <p style=""color: #64748b; font-size: 14px;"">Thời trang & Phụ kiện Tập Gym Đỉnh Cao</p>
    </div>
    <div style=""padding: 20px; background-color: #f8fafc; border-radius: 6px;"">
        <h2 style=""color: #1e293b; margin-top: 0;"">Cấp Lại Mật Khẩu Đăng Nhập</h2>
        <p style=""color: #334155; font-size: 15px; line-height: 1.5;"">
            Chúng tôi nhận được yêu cầu cấp lại mật khẩu cho tài khoản liên kết với email này. Mật khẩu tạm thời mới của bạn là:
        </p>
        <div style=""text-align: center; margin: 30px 0;"">
            <span style=""display: inline-block; font-size: 24px; font-weight: bold; letter-spacing: 2px; color: #0f172a; background-color: #e2e8f0; padding: 12px 24px; border-radius: 8px; border: 1px dashed #94a3b8;"">
                {newPassword}
            </span>
        </div>
        <p style=""color: #e11d48; font-size: 13px; text-align: center;"">
            🔒 Vì lý do bảo mật, vui lòng đăng nhập và đổi mật khẩu ngay sau khi nhận được email này.
        </p>
    </div>
    <div style=""margin-top: 20px; text-align: center; color: #94a3b8; font-size: 12px;"">
        <p>&copy; {DateTime.UtcNow.Year} GymKitten Store. All rights reserved.</p>
    </div>
</div>";

        await SendEmailInternalAsync(toEmail, subject, htmlBody);
    }

    private async Task SendEmailInternalAsync(string toEmail, string subject, string htmlBody)
    {
        if (string.IsNullOrWhiteSpace(_settings.User) || string.IsNullOrWhiteSpace(_settings.Password))
        {
            _logger.LogWarning("[MAIL SERVICE] SMTP credentials not configured in appsettings. Email to {ToEmail} skipped. (Subject: {Subject})", toEmail, subject);
            return;
        }

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.DisplayName, _settings.From));
            message.To.Add(new MailboxAddress(toEmail, toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            var secureSocketOptions = _settings.EnableSsl
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.Auto;

            await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, secureSocketOptions);
            await client.AuthenticateAsync(_settings.User, _settings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("[MAIL SERVICE] Successfully sent email to {ToEmail} with subject '{Subject}'", toEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MAIL SERVICE] Failed to send email to {ToEmail} via SMTP {Host}:{Port}", toEmail, _settings.SmtpHost, _settings.SmtpPort);
            throw; // Rethrow to allow Hangfire to retry automatically
        }
    }
}
