using MediatR;
using RateRelay.Application.DTOs.Business.BusinessVerification.Commands;

namespace RateRelay.Application.Features.Business.Queries.GetBusinessVerificationStatus;

// user id from context > get business > get business verification status
public class GetBusinessVerificationStatusQuery : IRequest<BusinessVerificationStatusOutputDto>;