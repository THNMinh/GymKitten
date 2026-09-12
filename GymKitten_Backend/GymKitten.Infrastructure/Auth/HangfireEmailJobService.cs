using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Services;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace GymKitten.Infrastructure.Auth;

public sealed class HangfireEmailJobService : IEmailJobService
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly ILogger<HangfireEmailJobService> _logger;

    public HangfireEmailJobService(
        IBackgroundJobClient backgroundJobClient,
        ILogger<HangfireEmailJobService> logger)
    {
        _backgroundJobClient = backgroundJobClient;
        _logger = logger;
    }

    public void EnqueueSendOtpEmail(string email, string otpCode)
    {
        _backgroundJobClient.Enqueue<IMailService>(m => m.SendOtpEmailAsync(email, otpCode));
        _logger.LogInformation("[HANGFIRE JOB] Enqueued background job to send OTP email to {Email}", email);
    }

    public void EnqueueSendResetPasswordEmail(string email, string newPassword)
    {
        _backgroundJobClient.Enqueue<IMailService>(m => m.SendResetPasswordEmailAsync(email, newPassword));
        _logger.LogInformation("[HANGFIRE JOB] Enqueued background job to send Reset Password email to {Email}", email);
    }
}
