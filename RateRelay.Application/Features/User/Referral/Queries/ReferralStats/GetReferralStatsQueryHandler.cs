using AutoMapper;
using MediatR;
using RateRelay.Application.DTOs.User.Referral;
using RateRelay.Application.DTOs.User.Referral.Queries;
using RateRelay.Domain.Interfaces;
using RateRelay.Infrastructure.Services;

namespace RateRelay.Application.Features.User.Referral.Queries.ReferralStats;

public class GetReferralStatsQueryHandler(
    CurrentUserContext currentUserContext,
    IReferralService referralService,
    IMapper mapper
) : IRequestHandler<GetReferralStatsQuery, ReferralStatsOutputDto>
{
    public async Task<ReferralStatsOutputDto> Handle(GetReferralStatsQuery request, CancellationToken cancellationToken)
    {
        var stats = await referralService.GetReferralStatsAsync(currentUserContext.AccountId, cancellationToken);
        var recentRewards =
            await referralService.GetReferralRewardsAsync(currentUserContext.AccountId, cancellationToken);

        var result = mapper.Map<ReferralStatsOutputDto>(stats);
        result.RecentRewards = recentRewards
            .Take(5)
            .Select(r => new ReferralRewardDto
            {
                GoalName = r.Goal.Name,
                RewardPoints = r.RewardPoints,
                DateAwarded = r.DateAwardedUtc,
                ReferredUserName = r.Referred.GoogleUsername
            })
            .ToList();

        return result;
    }
}