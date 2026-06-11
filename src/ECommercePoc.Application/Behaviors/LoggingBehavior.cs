using MediatR;
using Microsoft.Extensions.Logging;

namespace ECommercePoc.Application.Behaviors;

/// <summary>
/// Logs every command/query at the start of execution, including how long it took.
/// Demonstrates cross-cutting concerns handled via MediatR pipeline — no handler code duplication.
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        _logger.LogInformation("Handling {RequestName}: {@Request}", requestName, request);

        var start = DateTime.UtcNow;
        var response = await next();
        var elapsed = DateTime.UtcNow - start;

        _logger.LogInformation(
            "Handled {RequestName} in {ElapsedMs}ms",
            requestName, elapsed.TotalMilliseconds);

        return response;
    }
}
