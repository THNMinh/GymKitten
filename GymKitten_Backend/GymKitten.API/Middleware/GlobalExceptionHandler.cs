using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace GymKitten.API.Middleware;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);

        var (statusCode, title, detail) = exception switch
        {
            ValidationException validationException => (
                StatusCodes.Status400BadRequest,
                "Validation Error",
                string.Join(" | ", validationException.Errors.Select(e => e.ErrorMessage))),
            Microsoft.EntityFrameworkCore.DbUpdateException dbEx when dbEx.InnerException is Npgsql.PostgresException pgEx && pgEx.SqlState == "23505" => (
                StatusCodes.Status409Conflict,
                "Conflict",
                pgEx.ConstraintName != null && pgEx.ConstraintName.Contains("sku", StringComparison.OrdinalIgnoreCase)
                    ? "Mã SKU này đã tồn tại trong hệ thống. Vui lòng nhập mã SKU khác."
                    : "Dữ liệu bị trùng lặp vi phạm ràng buộc duy nhất trong cơ sở dữ liệu."),
            Microsoft.EntityFrameworkCore.DbUpdateException dbEx when dbEx.InnerException is Npgsql.PostgresException pgEx && pgEx.SqlState == "23503" => (
                StatusCodes.Status409Conflict,
                "Conflict",
                "Không thể thực hiện thao tác do có dữ liệu liên quan đang tồn tại trong hệ thống."),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                "An unexpected error occurred. Please try again later.")
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Type = statusCode switch
            {
                400 => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1"
            }
        };

        // Add validation errors as extension if applicable
        if (exception is ValidationException validationEx)
        {
            problemDetails.Extensions["errors"] = validationEx.Errors
                .Select(e => new { e.PropertyName, e.ErrorMessage })
                .ToList();
        }

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
