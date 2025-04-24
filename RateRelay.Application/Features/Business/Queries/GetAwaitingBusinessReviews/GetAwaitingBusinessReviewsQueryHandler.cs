using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RateRelay.Application.DTOs.Business.BusinessReviews.Queries;
using RateRelay.Application.Exceptions;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Enums;
using RateRelay.Domain.Interfaces.DataAccess;
using RateRelay.Infrastructure.Services;

namespace RateRelay.Application.Features.Business.Queries.GetAwaitingBusinessReviews;

public class GetAwaitingBusinessReviewsQueryHandler(
    CurrentUserContext currentUserContext,
    IUnitOfWorkFactory unitOfWorkFactory,
    IMapper mapper
) : IRequestHandler<GetAwaitingBusinessReviewsQuery, List<GetAwaitingBusinessReviewsOutputDto>>
{
    public async Task<List<GetAwaitingBusinessReviewsOutputDto>> Handle(GetAwaitingBusinessReviewsQuery request,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var businessRepository = unitOfWork.GetRepository<BusinessEntity>();

        var business = await businessRepository.GetByIdAsync(request.BusinessId, cancellationToken);

        if (business is null || business.OwnerAccountId != currentUserContext.AccountId)
        {
            throw new NotFoundException(
                $"Business with ID {request.BusinessId} not found or not owned by the user.");
        }

        var businessReviewsRepository = unitOfWork.GetRepository<BusinessReviewEntity>();

        var businessReviews = await businessReviewsRepository.GetBaseQueryable()
            .Where(x => x.BusinessId == request.BusinessId && x.Status == BusinessReviewStatus.Pending)
            .ToListAsync(cancellationToken);

        var output = mapper.Map<List<GetAwaitingBusinessReviewsOutputDto>>(businessReviews);
        return output;
    }
}