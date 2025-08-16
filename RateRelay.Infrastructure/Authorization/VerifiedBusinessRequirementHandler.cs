using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Enums;
using RateRelay.Domain.Exceptions;
using RateRelay.Domain.Interfaces.DataAccess;
using RateRelay.Infrastructure.Services;
using Serilog;

namespace RateRelay.Infrastructure.Authorization;

public class VerifiedBusinessAuthorizationHandler(
    CurrentUserContext currentUserContext,
    IUnitOfWorkFactory unitOfWorkFactory)
    : AuthorizationHandler<VerifiedBusinessRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        VerifiedBusinessRequirement requirement)
    {
        if (!currentUserContext.IsAuthenticated)
        {
            context.Fail();
            return;
        }

        if (currentUserContext.AccountFlags.HasFlag(AccountFlags.BypassVerifiedBusinessRequirement)) 
        {
            context.Succeed(requirement);
            return;
        }

        var accountId = currentUserContext.AccountId;

        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var businessRepository = unitOfWork.GetRepository<BusinessEntity>();

        var hasVerifiedBusiness = await businessRepository.GetBaseQueryable()
            .AnyAsync(b => b.OwnerAccountId == accountId && b.IsVerified);

        if (!hasVerifiedBusiness)
            throw new ForbiddenException(
                "You do not have a verified business. Please verify your business to access this resource.",
                "ERR_FORBIDDEN");

        context.Succeed(requirement);
    }
}