using MediatR;
using Serilog;
using System.Diagnostics;

namespace RateRelay.Application.MediatR.Behaviors;

public class LoggingBehavior<TRequest, TResponse>(ILogger logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestType = typeof(TRequest).Name;
        var requestId = Guid.NewGuid().ToString();

        _logger.Information("[START] {RequestType} {RequestId}", requestType, requestId);

        try
        {
            var stopwatch = Stopwatch.StartNew();
            var response = await next(cancellationToken);
            stopwatch.Stop();

            _logger.Information(
                "[END] {RequestType} {RequestId} completed in {ElapsedMilliseconds}ms", 
                requestType, 
                requestId, 
                stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            _logger.Error(
                ex, 
                "[ERROR] {RequestType} {RequestId} failed with error: {ErrorMessage}", 
                requestType, 
                requestId, 
                ex.Message);
            
            throw;
        }
    }
}