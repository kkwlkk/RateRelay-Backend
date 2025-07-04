using MediatR;
using RateRelay.Application.DTOs.User.Referral.Commands;

namespace RateRelay.Application.Features.User.Referral.Commands.LinkReferral;

public class LinkReferralCommand : IRequest<LinkReferralOutputDto>
{
    public required string ReferralCode { get; set; }
}