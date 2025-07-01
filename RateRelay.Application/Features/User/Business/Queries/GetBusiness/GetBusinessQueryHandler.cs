using MediatR;
using Microsoft.EntityFrameworkCore;
using RateRelay.Application.DTOs.User.Business.UserBusiness.Queries;
using RateRelay.Application.Features.Business.Queries.GetBusiness;
using RateRelay.Domain.Enums;
using RateRelay.Domain.Exceptions;
using RateRelay.Domain.Interfaces;
using RateRelay.Domain.Interfaces.DataAccess;

namespace RateRelay.Application.Features.User.Business.Queries.GetBusiness;

public class GetBusinessQueryHandler(
    ICurrentUserDataResolver currentUserDataResolver,
    IUnitOfWorkFactory unitOfWorkFactory,
    IBusinessQueueService businessQueueService
) : IRequestHandler<GetBusinessQuery, GetBusinessQueryOutputDto>
{
    public async Task<GetBusinessQueryOutputDto> Handle(GetBusinessQuery request, CancellationToken cancellationToken)
    {
        var accountId = currentUserDataResolver.GetAccountId();
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var businessesRepository = unitOfWork.GetRepository<Domain.Entities.BusinessEntity>();
        var businessReviewsRepository = unitOfWork.GetRepository<Domain.Entities.BusinessReviewEntity>();

        var businessEntity = await businessesRepository.GetBaseQueryable()
            .Where(x => x.Id == request.BusinessId && x.OwnerAccountId == accountId)
            .FirstOrDefaultAsync(cancellationToken);

        if (businessEntity is null)
            throw new NotFoundException(
                "The requested business could not be found or you do not have permission to access it."
            );

        var businessReviews = await businessReviewsRepository.GetBaseQueryable()
            .Where(r => r.BusinessId == request.BusinessId)
            .ToListAsync(cancellationToken);

        var acceptedReviews = businessReviews.Where(r => r.Status == BusinessReviewStatus.Accepted).ToList();
        var isBusinessEligibleForQueue = await businessQueueService.IsBusinessEligibleForQueueAsync(businessEntity.Id, cancellationToken);
        
        var business = new GetBusinessQueryOutputDto
        {
            Id = businessEntity.Id,
            PlaceId = businessEntity.PlaceId,
            Cid = businessEntity.Cid,
            BusinessName = businessEntity.BusinessName,
            IsVerified = businessEntity.IsVerified,
            IsEligibleForQueue = isBusinessEligibleForQueue,
            DateCreatedUtc = businessEntity.DateCreatedUtc,
            AverageRating = acceptedReviews.Any() 
                ? (decimal)acceptedReviews.Average(r => (int)r.Rating)
                : 0m,
            Reviews = new GetBusinessQueryOutputReviewsDto
            {
                TotalCount = businessReviews.Count,
                PendingCount = businessReviews.Count(r => r.Status == BusinessReviewStatus.Pending),
                AcceptedCount = acceptedReviews.Count,
                RejectedCount = businessReviews.Count(r => r.Status == BusinessReviewStatus.Rejected)
            }
        };

        return business;
    }
}