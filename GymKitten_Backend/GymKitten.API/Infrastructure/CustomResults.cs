using GymKitten.Domain.Common;

namespace GymKitten.API.Infrastructure;

public static class CustomResults
{
    public static IResult Problem(Result result)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("Cannot create a problem result for a successful result.");
        }

        var statusCode = GetStatusCode(result.Error.Code);

        return Microsoft.AspNetCore.Http.Results.Problem(
            title: GetTitle(statusCode),
            detail: result.Error.Message,
            statusCode: statusCode,
            extensions: new Dictionary<string, object?>
            {
                { "code", result.Error.Code }
            });
    }

    private static int GetStatusCode(string errorCode)
    {
        if (string.IsNullOrWhiteSpace(errorCode)) return StatusCodes.Status400BadRequest;

        if (errorCode.Contains("NotFound", StringComparison.OrdinalIgnoreCase))
            return StatusCodes.Status404NotFound;

        if (errorCode.Contains("AlreadyExists", StringComparison.OrdinalIgnoreCase) ||
            errorCode.Contains("Conflict", StringComparison.OrdinalIgnoreCase))
            return StatusCodes.Status409Conflict;

        if (errorCode.Contains("InvalidCredentials", StringComparison.OrdinalIgnoreCase) ||
            errorCode.Contains("Unauthorized", StringComparison.OrdinalIgnoreCase))
            return StatusCodes.Status401Unauthorized;

        if (errorCode.Contains("Forbidden", StringComparison.OrdinalIgnoreCase) ||
            errorCode.Contains("AccessDenied", StringComparison.OrdinalIgnoreCase))
            return StatusCodes.Status403Forbidden;

        return StatusCodes.Status400BadRequest;
    }

    private static string GetTitle(int statusCode) =>
        statusCode switch
        {
            StatusCodes.Status400BadRequest => "Bad Request",
            StatusCodes.Status401Unauthorized => "Unauthorized",
            StatusCodes.Status403Forbidden => "Forbidden",
            StatusCodes.Status404NotFound => "Not Found",
            StatusCodes.Status409Conflict => "Conflict",
            _ => "An error occurred"
        };
}
