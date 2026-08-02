using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Auth;

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
}
