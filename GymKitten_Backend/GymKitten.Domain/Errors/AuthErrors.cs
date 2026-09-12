using GymKitten.Domain.Common;

namespace GymKitten.Domain.Errors;

public static class AuthErrors
{
    public static readonly Error InvalidCredentials = new(
        "Auth.InvalidCredentials",
        "The provided email or password is incorrect.");

    public static readonly Error InvalidRefreshToken = new(
        "Auth.InvalidRefreshToken",
        "The refresh token is invalid, has been used, or has been revoked.");

    public static readonly Error ExpiredRefreshToken = new(
        "Auth.ExpiredRefreshToken",
        "The refresh token has expired.");

    public static readonly Error AccountInactive = new(
        "Auth.AccountInactive",
        "This account has been deactivated.");

    public static readonly Error EmailAlreadyExists = new(
        "Auth.EmailAlreadyExists",
        "An account with this email address already exists.");

    public static readonly Error EmailNotVerified = new(
        "Auth.EmailNotVerified",
        "Email address has not been verified yet.");

    public static readonly Error UserAlreadyVerified = new(
        "Auth.UserAlreadyVerified",
        "This user's email address is already verified.");

    public static readonly Error OtpExpired = new(
        "Auth.OtpExpired",
        "The OTP verification code has expired. Please request a new one.");

    public static readonly Error WrongOtp = new(
        "Auth.WrongOtp",
        "The provided OTP verification code is incorrect.");

    public static readonly Error TokenReuseDetected = new(
        "Auth.TokenReuseDetected",
        "Suspicious token reuse detected. All active sessions have been revoked for your security.");

    public static readonly Error WrongPassword = new(
        "Auth.WrongPassword",
        "The current password provided is incorrect.");

    public static readonly Error UserNotFound = new(
        "Auth.UserNotFound",
        "User with the specified email or identifier was not found.");
}
