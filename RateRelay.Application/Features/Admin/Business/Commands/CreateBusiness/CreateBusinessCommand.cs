using MediatR;
using RateRelay.Application.DTOs.Admin.Business.Commands.CreateBusiness;

namespace RateRelay.Application.Features.Admin.Business.Commands.CreateBusiness;

public class CreateBusinessCommand : IRequest<CreateBusinessOutputDto>
{
    public required string PlaceId { get; set; }
    public required long OwnerId { get; set; }
    public required bool IsVerified { get; set; }
}