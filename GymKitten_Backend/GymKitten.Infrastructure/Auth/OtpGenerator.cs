using System.Security.Cryptography;
using GymKitten.Application.Abstractions.Auth;

namespace GymKitten.Infrastructure.Auth;

public sealed class OtpGenerator : IOtpGenerator
{
    public string Generate6Digits()
    {
        // Generate a cryptographically secure random 6-digit OTP
        var randomNumber = RandomNumberGenerator.GetInt32(100000, 999999 + 1);
        return randomNumber.ToString("D6");
    }
}
