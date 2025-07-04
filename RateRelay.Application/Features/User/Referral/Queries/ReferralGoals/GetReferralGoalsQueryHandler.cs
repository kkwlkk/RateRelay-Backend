using AutoMapper;
using MediatR;
using RateRelay.Application.DTOs.User.Referral.Queries;
using RateRelay.Domain.Interfaces;

namespace RateRelay.Application.Features.User.Referral.Queries.ReferralGoals;

public class GetReferralGoalsQueryHandler(
    IReferralService referralService,
    IMapper mapper
) : IRequestHandler<GetReferralGoalsQuery, List<ReferralGoalOutputDto>>
{
    public async Task<List<ReferralGoalOutputDto>> Handle(GetReferralGoalsQuery request, CancellationToken cancellationToken)
    {
        var goals = await referralService.GetActiveGoalsAsync(cancellationToken);
        return mapper.Map<List<ReferralGoalOutputDto>>(goals);
    }
}