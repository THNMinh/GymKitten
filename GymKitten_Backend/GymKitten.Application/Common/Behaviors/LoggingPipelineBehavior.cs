using GymKitten.Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace GymKitten.Application.Common.Behaviors;

public class LoggingPipelineBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class
    where TResponse : Result
{
    private readonly ILogger<LoggingPipelineBehavior<TRequest, TResponse>> _logger;

    public LoggingPipelineBehavior(ILogger<LoggingPipelineBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        _logger.LogInformation("Processing request {RequestName}", requestName);

        var result = await next();

        if (result.IsSuccess)
        {
            _logger.LogInformation("Completed request {RequestName}", requestName);
        }
        else
        {
            using (LogContext.PushProperty("Error", result.Error, true))
            {
                _logger.LogError("Completed request {RequestName} with error", requestName);
            }
        }

        return result;
    }
}
