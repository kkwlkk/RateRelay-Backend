using MediatR;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Exceptions;
using RateRelay.Domain.Interfaces.DataAccess;

namespace RateRelay.Application.Features.Admin.Business.Commands.DeleteBusiness;

public class DeleteBusinessCommandHandler(IUnitOfWorkFactory unitOfWorkFactory)
    : IRequestHandler<DeleteBusinessCommand, Unit>
{
    public async Task<Unit> Handle(DeleteBusinessCommand request, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var businessRepository = unitOfWork.GetRepository<BusinessEntity>();

        var business = await businessRepository.GetByIdAsync(request.BusinessId, cancellationToken);
        if (business == null)
        {
            throw new NotFoundException($"Business with ID {request.BusinessId} not found.");
        }

        businessRepository.Remove(business);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}