using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Interfaces;
using RateRelay.Domain.Interfaces.DataAccess;
using RateRelay.Infrastructure.DataAccess.Context;
using RateRelay.Infrastructure.Services;

namespace RateRelay.Infrastructure.Authorization;

public class VerifiedBusinessAuthorizationHandler(
    IDbContextFactory<RateRelayDbContext> dbContextFactory,
    ICurrentUserDataResolver currentUserDataResolver,
    IUnitOfWorkFactory unitOfWorkFactory)
    : AuthorizationHandler<VerifiedBusinessRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        VerifiedBusinessRequirement requirement)
    {
        if (!currentUserDataResolver.TryGetAccountId(out var accountId))
        {
            return;
        }

        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var businessRepository = unitOfWork.GetRepository<BusinessEntity>();

        var hasVerifiedBusiness = await businessRepository.GetBaseQueryable()
            .AnyAsync(b => b.OwnerAccountId == accountId && b.IsVerified);

        if (hasVerifiedBusiness)
        {
            context.Succeed(requirement);
        }
    }
}