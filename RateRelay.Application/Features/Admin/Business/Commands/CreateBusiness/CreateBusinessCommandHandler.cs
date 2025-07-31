using MediatR;
using Microsoft.EntityFrameworkCore;
using RateRelay.Application.DTOs.Admin.Business;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Exceptions;
using RateRelay.Domain.Interfaces;
using RateRelay.Domain.Interfaces.DataAccess;

namespace RateRelay.Application.Features.Admin.Business.Commands.CreateBusiness;

public class CreateBusinessCommandHandler(IUnitOfWorkFactory unitOfWorkFactory, IGooglePlacesService googlePlacesService)
    : IRequestHandler<CreateBusinessCommand, CreateBusinessOutputDto>
{
    public async Task<CreateBusinessOutputDto> Handle(CreateBusinessCommand request, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var businessRepository = unitOfWork.GetRepository<BusinessEntity>();
        var accountRepository = unitOfWork.GetRepository<AccountEntity>();

        var existingBusiness = await businessRepository.GetBaseQueryable()
            .FirstOrDefaultAsync(b => b.PlaceId == request.PlaceId, cancellationToken);
        
        if (existingBusiness is not null) 
        {
            throw new AppException($"Business with Place ID {request.PlaceId} already exists.", "BusinessAlreadyExists");
        }
        
        var ownerAccount = await accountRepository.GetByIdAsync(request.OwnerId, cancellationToken);

        if (ownerAccount is null)
        {
            throw new NotFoundException($"Owner account with ID {request.OwnerId} not found.", "OwnerNotFound");
        }
        
        var googleBusinessData = await googlePlacesService.GetPlaceDetailsAsync(request.PlaceId, cancellationToken);

        if (googleBusinessData is null)
        {
            throw new AppException($"Google Places data for Place ID {request.PlaceId} not found.", "GoogleDataNotFound");
        }
        
        var newBusiness = new BusinessEntity
        {
            PlaceId = request.PlaceId,
            Cid = googleBusinessData.Cid,
            BusinessName = googleBusinessData.DisplayName.Text,
            IsVerified = request.IsVerified,
            OwnerAccountId = ownerAccount.Id,
        };
        
        await businessRepository.InsertAsync(newBusiness, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return new CreateBusinessOutputDto
        {
            Id = newBusiness.Id,
        };
    }
}