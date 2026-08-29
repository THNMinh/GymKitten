namespace GymKitten.API.Infrastructure;

public class ApiResult<T>
{
    public bool IsSuccess { get; init; }
    public T? Data { get; init; }
    public ErrorResponse? Error { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    public static ApiResult<T> Success(T? data) => new()
    {
        IsSuccess = true,
        Data = data
    };

    public static ApiResult<T> Failure(string code, string message) => new()
    {
        IsSuccess = false,
        Error = new ErrorResponse(code, message)
    };
}

public record ErrorResponse(string Code, string Message);
