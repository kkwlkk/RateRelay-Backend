using MediatR;
using RateRelay.Application.DTOs.User.Referral.Queries;

namespace RateRelay.Application.Features.User.Referral.Queries.ReferralGoals;

public class GetReferralGoalsQuery : IRequest<List<ReferralGoalOutputDto>>;