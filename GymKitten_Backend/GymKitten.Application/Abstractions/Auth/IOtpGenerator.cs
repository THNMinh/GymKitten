namespace GymKitten.Application.Abstractions.Auth;

public interface IOtpGenerator
{
    /// <summary>
    /// Generates a cryptographically secure 6-digit OTP code.
    /// </summary>
    string Generate6Digits();
}
