namespace GymKitten.Application.Abstractions.Auth;

public interface IEmailJobService
{
    /// <summary>
    /// Enqueues a background job to send the OTP verification email.
    /// </summary>
    void EnqueueSendOtpEmail(string email, string otpCode);

    /// <summary>
    /// Enqueues a background job to send the reset password email.
    /// </summary>
    void EnqueueSendResetPasswordEmail(string email, string newPassword);
}

