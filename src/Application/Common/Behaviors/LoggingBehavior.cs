using System.Diagnostics;
using Domain.Common;                     // ← adjust if your Result lives elsewhere
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Common.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var sw = Stopwatch.StartNew();

        logger.LogInformation("Handling {RequestName} {@Request}", requestName, request);

        try
        {
            var response = await next();
            sw.Stop();

            // Special handling when the response is a Result / Result<T>
            if (response is Result result)
            {
                if (result.IsFailure)
                {
                    logger.LogWarning(
                        "Handled {RequestName} in {ElapsedMilliseconds}ms → FAILURE {@Errors}",
                        requestName, sw.ElapsedMilliseconds, result.Errors);
                }
                else
                {
                    logger.LogInformation(
                        "Handled {RequestName} in {ElapsedMilliseconds}ms → SUCCESS",
                        requestName, sw.ElapsedMilliseconds);
                }
            }
            else
            {
                logger.LogInformation(
                    "Handled {RequestName} in {ElapsedMilliseconds}ms",
                    requestName, sw.ElapsedMilliseconds);
            }

            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();
            logger.LogError(ex,
                "{RequestName} failed after {ElapsedMilliseconds}ms",
                requestName, sw.ElapsedMilliseconds);
            throw;
        }
    }
}