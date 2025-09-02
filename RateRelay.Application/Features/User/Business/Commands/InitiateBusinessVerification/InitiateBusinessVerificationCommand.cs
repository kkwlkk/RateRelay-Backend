using MediatR;
using RateRelay.Application.DTOs.Business.BusinessVerification.Commands;

namespace RateRelay.Application.Features.User.Business.Commands.InitiateBusinessVerification;

public class InitiateBusinessVerificationCommand : IRequest<BusinessVerificationOutputDto>
{
    public required string PlaceId { get; set; }
}