using MediatR;
using Microsoft.EntityFrameworkCore;
using RateRelay.Application.DTOs.User.Business.UserBusiness.Queries;
using RateRelay.Domain.Common;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Enums;
using RateRelay.Domain.Interfaces;
using RateRelay.Domain.Interfaces.DataAccess;

namespace RateRelay.Application.Features.User.Business.Queries.GetAllUserBusinesses;

public class GetAllUserBusinessesQueryHandler(
    ICurrentUserDataResolver currentUserDataResolver,
    IUnitOfWorkFactory unitOfWorkFactory,
    IBusinessQueueService businessQueueService
) : IRequestHandler<GetAllUserBusinessesQuery, PagedApiResponse<GetBusinessQueryOutputDto>>
{
    public async Task<PagedApiResponse<GetBusinessQueryOutputDto>> Handle(GetAllUserBusinessesQuery request,
        CancellationToken cancellationToken)
    {
        var accountId = currentUserDataResolver.GetAccountId();
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var businessesRepository = unitOfWork.GetRepository<BusinessEntity>();
        var businessReviewsRepository = unitOfWork.GetRepository<BusinessReviewEntity>();

        var query = businessesRepository.GetBaseQueryable()
            .Where(x => x.OwnerAccountId == accountId)
            .ApplySearch(request, x =>
                x.BusinessName.Contains(request.Search!) ||
                x.Id.ToString().Contains(request.Search!));

        var totalCount = await query.CountAsync(cancellationToken);

        var businessesData = await query
            .OrderBy(x => x.Id)
            .ApplySorting(request)
            .ApplyPaging(request)
            .ToListAsync(cancellationToken);

        var businessIds = businessesData.Select(b => b.Id).ToList();

        var allReviews = await businessReviewsRepository.GetBaseQueryable()
            .Where(r => businessIds.Contains(r.BusinessId))
            .ToListAsync(cancellationToken);

        var businesses = businessesData.Select(business =>
        {
            var businessReviews = allReviews.Where(r => r.BusinessId == business.Id).ToList();
            var acceptedReviews = businessReviews.Where(r => r.Status == BusinessReviewStatus.Accepted).ToList();
            var isBusinessEligibleForQueue = businessQueueService.IsBusinessEligibleForQueueAsync(business.Id, cancellationToken).Result;
            
            return new GetBusinessQueryOutputDto
            {
                Id = business.Id,
                PlaceId = business.PlaceId,
                Cid = business.Cid,
                BusinessName = business.BusinessName,
                IsVerified = business.IsVerified,
                IsEligibleForQueue = isBusinessEligibleForQueue,
                DateCreatedUtc = business.DateCreatedUtc,
                Reviews = new GetBusinessQueryOutputReviewsDto
                {
                    TotalCount = businessReviews.Count,
                    PendingCount = businessReviews.Count(r => r.Status == BusinessReviewStatus.Pending),
                    AcceptedCount = acceptedReviews.Count,
                    RejectedCount = businessReviews.Count(r => r.Status == BusinessReviewStatus.Rejected)
                },
                AverageRating = acceptedReviews.Any() 
                    ? (decimal)acceptedReviews.Average(r => (int)r.Rating)
                    : 0m
            };
        }).ToList();

        return request.ToPagedResponse(businesses, totalCount);
    }
}