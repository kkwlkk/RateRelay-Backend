using MediatR;
using RateRelay.Application.DTOs.User.Referral.Commands;
using RateRelay.Domain.Interfaces;
using RateRelay.Infrastructure.Services;

namespace RateRelay.Application.Features.User.Referral.Commands.GenerateReferralCode;

public class GenerateReferralCodeCommandHandler(
    CurrentUserContext currentUserContext,
    IReferralService referralService
) : IRequestHandler<GenerateReferralCodeCommand, GenerateReferralCodeOutputDto>
{
    public async Task<GenerateReferralCodeOutputDto> Handle(GenerateReferralCodeCommand request,
        CancellationToken cancellationToken)
    {
        var referralCode =
            await referralService.GenerateReferralCodeAsync(currentUserContext.AccountId, cancellationToken);

        return new GenerateReferralCodeOutputDto
        {
            ReferralCode = referralCode
        };
    }
}