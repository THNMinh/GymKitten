using GymKitten.Domain.Common;

namespace GymKitten.Domain.Errors;

public static class UserErrors
{
    public static readonly Error NotFound = new(
        "User.NotFound",
        "The specified user was not found.");

    public static readonly Error Unauthorized = new(
        "User.Unauthorized",
        "Authentication is required to access this resource.");

    public static readonly Error Forbidden = new(
        "User.Forbidden",
        "You do not have administrative privileges to perform this operation.");

    public static readonly Error AccessDenied = new(
        "User.AccessDenied",
        "Access denied. You do not have permission to perform this action.");
}
