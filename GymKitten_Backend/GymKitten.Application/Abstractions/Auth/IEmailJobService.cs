namespace GymKitten.Application.Abstractions.Auth;

public interface IEmailJobService
{
    /// <summary>
    /// Enqueues a background job to send the OTP verification email.
    /// </summary>
    void EnqueueSendOtpEmail(string email, string otpCode);
}
