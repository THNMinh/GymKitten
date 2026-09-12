namespace GymKitten.Application.Abstractions.Services;

public interface IMailService
{
    Task SendOtpEmailAsync(string toEmail, string otpCode);
    Task SendResetPasswordEmailAsync(string toEmail, string newPassword);
}
