using MediatR;
using Microsoft.EntityFrameworkCore;
using RateRelay.Application.DTOs.Admin.Business.Queries.GetSpecificBusinessDetails;
using RateRelay.Domain.Constants;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Enums;
using RateRelay.Domain.Exceptions;
using RateRelay.Domain.Interfaces;
using RateRelay.Domain.Interfaces.DataAccess;

namespace RateRelay.Application.Features.Admin.Business.Queries.GetSpecificBusinessDetails;

public class GetSpecificBusinessDetailsQueryHandler(
    IUnitOfWorkFactory unitOfWorkFactory,
    IBusinessQueueService businessQueueService,
    IGoogleMapsService googleMapsService
) : IRequestHandler<GetSpecificBusinessDetailsQuery, AdminBusinessDetailDto>
{
    public async Task<AdminBusinessDetailDto> Handle(GetSpecificBusinessDetailsQuery request, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var businessRepository = unitOfWork.GetRepository<BusinessEntity>();
        var reviewRepository = unitOfWork.GetRepository<BusinessReviewEntity>();
        var historyRepository = unitOfWork.GetRepository<BusinessPriorityHistoryEntity>();
        var businessBoostsRepository = unitOfWork.GetRepository<BusinessBoostEntity>();

        var business = await businessRepository.GetBaseQueryable()
            .Include(b => b.OwnerAccount)
            .FirstOrDefaultAsync(b => b.Id == request.BusinessId, cancellationToken);

        if (business == null)
            throw new NotFoundException($"Business with ID {request.BusinessId} not found");

        var reviewStats = await reviewRepository.GetBaseQueryable()
            .Where(r => r.BusinessId == request.BusinessId)
            .Select(r => new { r.Status, r.Rating, r.DateCreatedUtc })
            .ToListAsync(cancellationToken);

        var totalReviews = reviewStats.Count;
        var acceptedReviews = reviewStats.Count(r => r.Status == BusinessReviewStatus.Accepted);
        var pendingReviews = reviewStats.Count(r => r.Status == BusinessReviewStatus.Pending);
        var rejectedReviews = reviewStats.Count(r => r.Status == BusinessReviewStatus.Rejected);
        var averageRating = reviewStats.Where(r => r.Status == BusinessReviewStatus.Accepted)
                                     .Average(r => (decimal?)r.Rating) ?? 0;
        var lastReviewDate = reviewStats.OrderByDescending(r => r.DateCreatedUtc)
                                      .FirstOrDefault()?.DateCreatedUtc;

        var boostHistory = await historyRepository.GetBaseQueryable()
            .Where(h => h.BusinessId == request.BusinessId)
            .Include(h => h.ChangedBy)
            .OrderByDescending(h => h.DateCreatedUtc)
            .Select(h => new BusinessBoostHistoryDto
            {
                OldPriority = h.OldPriority,
                NewPriority = h.NewPriority,
                Reason = h.Reason ?? "",
                ChangedByName = h.ChangedBy.GoogleUsername,
                ChangedAt = h.DateCreatedUtc,
                WasBoosted = h.NewPriority > h.OldPriority
            })
            .ToListAsync(cancellationToken);

        var isEligible = await businessQueueService.IsBusinessEligibleForQueueAsync(business.Id, cancellationToken);
        var boostTargetReviews = await businessBoostsRepository.GetBaseQueryable()
            .Where(b => b.BusinessId == request.BusinessId && b.IsActive)
            .Select(b => b.TargetReviews)
            .FirstOrDefaultAsync(cancellationToken);

        return new AdminBusinessDetailDto
        {
            Id = business.Id,
            BusinessName = business.BusinessName,
            PlaceId = business.PlaceId,
            Cid = business.Cid,
            MapUrl = googleMapsService.GenerateMapUrlFromCid(business.Cid),
            OwnerAccountId = business.OwnerAccount?.Id ?? 0,
            OwnerName = business.OwnerAccount?.GoogleUsername ?? "",
            OwnerEmail = business.OwnerAccount?.Email ?? "",
            OwnerPointBalance = business.OwnerAccount?.PointBalance ?? 0,
            Priority = business.Priority,
            IsVerified = business.IsVerified,
            IsBoosted = business.Priority >= BusinessQueueConstants.BoostedPriorityThreshold,
            DateCreatedUtc = business.DateCreatedUtc,
            TotalReviews = totalReviews,
            AcceptedReviews = acceptedReviews,
            PendingReviews = pendingReviews,
            RejectedReviews = rejectedReviews,
            AverageRating = averageRating,
            LastReviewDate = lastReviewDate,
            IsEligibleForQueue = isEligible,
            BoostHistory = boostHistory,
            BoostTargetReviews = boostTargetReviews > 0 ? boostTargetReviews : null
        };
    }
}