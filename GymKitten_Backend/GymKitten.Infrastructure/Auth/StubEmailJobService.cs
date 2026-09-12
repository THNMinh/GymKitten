using GymKitten.Application.Abstractions.Auth;
using Microsoft.Extensions.Logging;

namespace GymKitten.Infrastructure.Auth;

/// <summary>
/// Stub email job service that logs the OTP email to the console.
/// Replace with Hangfire + MailKit implementation later.
/// </summary>
public sealed class StubEmailJobService : IEmailJobService
{
    private readonly ILogger<StubEmailJobService> _logger;

    public StubEmailJobService(ILogger<StubEmailJobService> logger)
    {
        _logger = logger;
    }

    public void EnqueueSendOtpEmail(string email, string otpCode)
    {
        _logger.LogInformation(
            "[STUB EMAIL] Enqueued OTP email to {Email} with code: {OtpCode}",
            email, otpCode);
    }

    public void EnqueueSendResetPasswordEmail(string email, string newPassword)
    {
        _logger.LogInformation(
            "[STUB EMAIL] Enqueued Reset Password email to {Email} with new password: {NewPassword}",
            email, newPassword);
    }
}
