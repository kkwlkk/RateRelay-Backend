using MediatR;
using Microsoft.EntityFrameworkCore;
using RateRelay.Application.DTOs.Admin.Business;
using RateRelay.Domain.Common;
using RateRelay.Domain.Constants;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Enums;
using RateRelay.Domain.Interfaces;
using RateRelay.Domain.Interfaces.DataAccess;

namespace RateRelay.Application.Features.Admin.Business.Queries.GetBusinessesForAdmin;

public class GetBusinessesForAdminQueryHandler(
    IUnitOfWorkFactory unitOfWorkFactory,
    IBusinessQueueService businessQueueService
) : IRequestHandler<GetBusinessesForAdminQuery, PagedApiResponse<AdminBusinessListDto>>
{
    public async Task<PagedApiResponse<AdminBusinessListDto>> Handle(GetBusinessesForAdminQuery request,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var businessRepository = unitOfWork.GetRepository<BusinessEntity>();
        var reviewRepository = unitOfWork.GetRepository<BusinessReviewEntity>();
        var historyRepository = unitOfWork.GetRepository<BusinessPriorityHistoryEntity>();
        var businessBoostsRepository = unitOfWork.GetRepository<BusinessBoostEntity>();

        IQueryable<BusinessEntity> query = businessRepository.GetBaseQueryable()
            .Include(b => b.OwnerAccount);

        if (request.Filters?.IsVerified.HasValue == true)
            query = query.Where(b => b.IsVerified == request.Filters.IsVerified.Value);

        if (request.Filters?.OwnerId.HasValue == true)
            query = query.Where(b => b.OwnerAccountId == request.Filters.OwnerId.Value);

        query = query.ApplySearch(request, b =>
            b.BusinessName.Contains(request.Search!) ||
            b.OwnerAccount!.GoogleUsername.Contains(request.Search!) ||
            b.OwnerAccount!.Email.Contains(request.Search!));

        var totalCount = await query.CountAsync(cancellationToken);
        var businesses = await query
            .ApplySorting(request)
            .ApplyPaging(request)
            .ToListAsync(cancellationToken);

        var businessIds = businesses.Select(b => b.Id).ToList();

        var reviewStats = await reviewRepository.GetBaseQueryable()
            .Where(r => businessIds.Contains(r.BusinessId))
            .GroupBy(r => r.BusinessId)
            .Select(g => new
            {
                BusinessId = g.Key,
                TotalReviews = g.Count(r => r.Status == BusinessReviewStatus.Accepted),
                PendingReviews = g.Count(r => r.Status == BusinessReviewStatus.Pending),
                AverageRating = g.Where(r => r.Status == BusinessReviewStatus.Accepted)
                    .Average(r => (double?)r.Rating) ?? 0
            })
            .ToListAsync(cancellationToken);

        var reviewStatsDict = reviewStats.ToDictionary(r => r.BusinessId);

        var lastBoosts = await historyRepository.GetBaseQueryable()
            .Where(h => businessIds.Contains(h.BusinessId))
            .GroupBy(h => h.BusinessId)
            .Select(g => new
            {
                BusinessId = g.Key,
                LastReason = g.OrderByDescending(h => h.DateCreatedUtc).First().Reason
            })
            .ToListAsync(cancellationToken);

        var lastBoostDict = lastBoosts.ToDictionary(b => b.BusinessId, b => b.LastReason);

        var businessDtos = new List<AdminBusinessListDto>();

        foreach (var business in businesses)
        {
            var stats = reviewStatsDict.GetValueOrDefault(business.Id);
            var currentReviews = stats?.TotalReviews ?? 0;

            if (request.Filters?.MinReviews.HasValue == true && currentReviews < request.Filters.MinReviews.Value)
                continue;
            if (request.Filters?.MaxReviews.HasValue == true && currentReviews > request.Filters.MaxReviews.Value)
                continue;

            var isEligible = await businessQueueService.IsBusinessEligibleForQueueAsync(business.Id, cancellationToken);
            var boostTargetReviews = await businessBoostsRepository.GetBaseQueryable()
                .Where(b => b.BusinessId == business.Id && b.IsActive)
                .Select(b => b.TargetReviews)
                .FirstOrDefaultAsync(cancellationToken);

            businessDtos.Add(new AdminBusinessListDto
            {
                Id = business.Id,
                BusinessName = business.BusinessName,
                OwnerName = business.OwnerAccount?.GoogleUsername ?? "Unknown",
                OwnerEmail = business.OwnerAccount?.Email ?? "Unknown",
                CurrentReviews = currentReviews,
                PendingReviews = stats?.PendingReviews ?? 0,
                AverageRating = (decimal)(stats?.AverageRating ?? 0),
                Priority = business.Priority,
                IsVerified = business.IsVerified,
                IsBoosted = business.Priority >= BusinessQueueConstants.BoostedPriorityThreshold,
                DateCreatedUtc = business.DateCreatedUtc,
                IsEligibleForQueue = isEligible,
                LastBoostReason = lastBoostDict.GetValueOrDefault(business.Id),
                BoostTargetReviews = boostTargetReviews > 0 ? boostTargetReviews : null
            });
        }

        return request.ToPagedResponse(businessDtos, totalCount);
    }
}