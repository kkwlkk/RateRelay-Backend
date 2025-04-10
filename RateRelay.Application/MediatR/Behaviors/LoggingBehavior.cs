using MediatR;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;
using RateRelay.Application.Helpers;

namespace RateRelay.Application.MediatR.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next(cancellationToken);
        }
        catch (Exception ex)
        {
            if (ExceptionHelper.ShouldSkipLogging(ex))
                throw;

            Log.Error(ex, "{RequestName} failed with error: {ErrorMessage}",
                typeof(TRequest).Name, ex.Message);
            throw;
        }
    }
}