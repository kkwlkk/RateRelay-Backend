using MediatR;

namespace RateRelay.Application.Features.Admin.Business.Commands.DeleteBusiness;

public class DeleteBusinessCommand : IRequest<Unit>
{
    public long BusinessId { get; set; }
}