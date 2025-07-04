using MediatR;
using RateRelay.Application.DTOs.User.Referral.Queries;

namespace RateRelay.Application.Features.User.Referral.Queries.ReferralStats;

public class GetReferralStatsQuery : IRequest<ReferralStatsOutputDto>;