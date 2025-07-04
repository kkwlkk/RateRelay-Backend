using MediatR;
using RateRelay.Application.DTOs.User.Referral.Commands;
using RateRelay.Domain.Constants;
using RateRelay.Domain.Exceptions;
using RateRelay.Domain.Interfaces;
using RateRelay.Infrastructure.Services;

namespace RateRelay.Application.Features.User.Referral.Commands.LinkReferral;

public class LinkReferralCommandHandler(
    CurrentUserContext currentUserContext,
    IReferralService referralService
) : IRequestHandler<LinkReferralCommand, LinkReferralOutputDto>
{
    public async Task<LinkReferralOutputDto> Handle(LinkReferralCommand request, CancellationToken cancellationToken)
    {
        var referrerAccount =
            await referralService.GetAccountByReferralCodeAsync(request.ReferralCode, cancellationToken);

        if (referrerAccount is null)
        {
            throw new AppException(
                "Invalid referral code. Please check the code and try again.",
                "ReferralCodeInvalid");
        }

        var success = await referralService.LinkReferralAsync(
            currentUserContext.AccountId,
            request.ReferralCode,
            cancellationToken);

        if (!success)
        {
            throw new AppException("Unable to link referral. You may already have a referrer or the code is invalid.",
                "ReferralLinkFailed");
        }

        return new LinkReferralOutputDto
        {
            ReferrerName = referrerAccount.Username,
            WelcomeBonus = PointConstants.ReferralWelcomeBonusPoints
        };
    }
}