using MediatR;
using RateRelay.Application.DTOs.User.Referral.Commands;

namespace RateRelay.Application.Features.User.Referral.Commands.GenerateReferralCode;

public class GenerateReferralCodeCommand : IRequest<GenerateReferralCodeOutputDto>;