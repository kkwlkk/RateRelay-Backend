using MediatR;
using RateRelay.Domain.Constants.ErrorCodes;
using RateRelay.Domain.Exceptions;
using RateRelay.Domain.Interfaces;
using RateRelay.Infrastructure.Services;
using ILogger = Serilog.ILogger;

namespace RateRelay.API.Middleware;

public class BannedAccountMiddleware<TRequest, TResponse>(
    CurrentUserContext currentUser,
    ILogger logger,
    IUserService userService
) : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated)
        {
            return await next(cancellationToken);
        }

        var accountId = currentUser.AccountId;
        var activeBan = await userService.HasActiveBanAsync(accountId, cancellationToken);

        if (activeBan is null) return await next(cancellationToken);

        logger.Warning(
            "Banned user {AccountId} attempted to access {RequestType}. Ban reason: {Reason}, expires: {ExpiresAt}",
            accountId, typeof(TRequest).Name, activeBan.Reason, activeBan.ExpiresAtUtc);

        var banMetadata = new Dictionary<string, object>
        {
            { "AccountId", accountId },
            { "BanReason", activeBan.Reason },
            { "BanExpiresAt", activeBan.ExpiresAtUtc! }
        };

        throw new ForbiddenException(
            "Your account is banned.",
            AuthErrorCodes.AccountBanned,
            metadata: banMetadata
        );
    }
}