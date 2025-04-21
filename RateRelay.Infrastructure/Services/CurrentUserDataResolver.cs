using Microsoft.AspNetCore.Http;
using RateRelay.Domain.Interfaces;
using RateRelay.Infrastructure.Extensions;

namespace RateRelay.Infrastructure.Services;

public class CurrentUserDataResolver(IHttpContextAccessor httpContextAccessor) : ICurrentUserDataResolver
{
    public long GetAccountId()
    {
        return httpContextAccessor.HttpContext?.User.GetUserId() ??
               throw new InvalidOperationException("Account ID not found in claims.");
    }

    public string GetUsername()
    {
        return httpContextAccessor.HttpContext?.User.GetUserName() ??
               throw new InvalidOperationException("Username not found in claims.");
    }
}