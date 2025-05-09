using MediatR;
using RateRelay.Application.DTOs.Business.BusinessVerification.Commands;

namespace RateRelay.Application.Features.Business.Commands.ProcessBusinessVerificationChallenge;

public class ProcessBusinessVerificationChallengeCommand : IRequest<BusinessVerificationStatusOutputDto>;