using MediatR;
using RateRelay.Application.DTOs.Business.BusinessVerification.Queries;

namespace RateRelay.Application.Features.Business.Queries.GetBusinessVerificationChallenge;

public class GetBusinessVerificationChallengeQuery : IRequest<BusinessVerificationChallengeOutputDto>;