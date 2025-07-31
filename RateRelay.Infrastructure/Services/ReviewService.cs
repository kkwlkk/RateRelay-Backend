using Microsoft.EntityFrameworkCore;
using RateRelay.Domain.Constants;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Enums;
using RateRelay.Domain.Exceptions;
using RateRelay.Domain.Interfaces;
using RateRelay.Domain.Interfaces.DataAccess;

namespace RateRelay.Infrastructure.Services;

public class ReviewService(
    IUnitOfWorkFactory unitOfWorkFactory,
    IPointService pointService,
    IReferralService referralService,
    IBusinessBoostService businessBoostService
) : IReviewService
{
    public async Task<bool> AddUserReviewAsync(
        long businessId,
        long reviewerId,
        BusinessRating rating,
        string comment,
        bool postedGoogleReview,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var businessRepository = unitOfWork.GetRepository<BusinessEntity>();

        var business = await businessRepository.GetByIdAsync(businessId, cancellationToken);

        if (business is null)
        {
            throw new AppException(
                $"Business with ID {businessId} not found.", "BUSINESS_NOT_FOUND");
        }

        var reviewRepository = unitOfWork.GetRepository<BusinessReviewEntity>();

        var existingReview = await reviewRepository
            .FindAsync(r => r.BusinessId == businessId && r.ReviewerId == reviewerId &&
                            (r.Status == BusinessReviewStatus.Pending || r.Status == BusinessReviewStatus.Accepted),
                cancellationToken);

        if (existingReview.Any())
        {
            throw new InvalidOperationException(
                $"User with ID {reviewerId} has already reviewed business with ID {businessId}.");
        }

        var businessBoostRepository = unitOfWork.GetRepository<BusinessBoostEntity>();
        var businessHasActiveBoost = await businessBoostService.IsBusinessBoostedAsync(
            businessId, cancellationToken);

        var review = new BusinessReviewEntity
        {
            BusinessId = businessId,
            ReviewerId = reviewerId,
            Status = BusinessReviewStatus.Pending,
            Rating = rating,
            Comment = comment,
            PostedGoogleReview = postedGoogleReview
        };

        if (!businessHasActiveBoost)
        {
            await pointService.DeductPointsAsync(
                business.OwnerAccountId,
                PointConstants.BasicReviewPoints,
                PointTransactionType.ReviewSubmissionLock,
                null,
                cancellationToken
            );

            if (postedGoogleReview)
            {
                await pointService.DeductPointsAsync(
                    business.OwnerAccountId,
                    PointConstants.GoogleMapsReviewPoints,
                    PointTransactionType.GoogleMapsReviewLock,
                    null,
                    cancellationToken
                );
            }
        }

        if (businessHasActiveBoost)
        {
            var boost = await businessBoostRepository
                .GetBaseQueryable()
                .FirstOrDefaultAsync(b => b.BusinessId == businessId && b.IsActive, cancellationToken);

            if (boost is not null)
            {
                var pendingBusinessReviews = await reviewRepository
                    .CountAsync(r => r.BusinessId == businessId && r.Status == BusinessReviewStatus.Pending,
                        cancellationToken) + 1; // +1 for the current review

                var targetReviews = boost.TargetReviews + boost.ReviewsAtBoostStart;
                if (pendingBusinessReviews >= targetReviews)
                {
                    await businessBoostService.UnboostBusinessAsync(
                        businessId,
                        null,
                        "Osiągnięto wyznaczony cel recenzji.",
                        cancellationToken
                    );
                }
            }
        }

        await reviewRepository.InsertAsync(review, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> AcceptUserReviewAsync(long reviewId, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var reviewRepository = unitOfWork.GetRepository<BusinessReviewEntity>();

        var review = await reviewRepository.GetByIdAsync(reviewId, cancellationToken);

        if (review is null)
        {
            throw new ArgumentException($"Review with ID {reviewId} not found.");
        }

        if (review.Status == BusinessReviewStatus.Accepted)
        {
            throw new AppException(
                $"Review with ID {reviewId} has already been accepted.", "REVIEW_ALREADY_ACCEPTED");
        }

        if (review.Status == BusinessReviewStatus.Rejected)
        {
            throw new AppException(
                $"Review with ID {reviewId} has already been rejected.", "REVIEW_ALREADY_REJECTED");
        }

        reviewRepository.Update(review);
        review.Status = BusinessReviewStatus.Accepted;
        review.DateAcceptedUtc = DateTime.UtcNow;
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await pointService.AddPointsAsync(
            review.ReviewerId,
            PointConstants.BasicReviewPoints,
            PointTransactionType.ReviewAcceptedReward,
            null,
            cancellationToken
        );

        if (review.PostedGoogleReview)
        {
            await pointService.AddPointsAsync(
                review.ReviewerId,
                PointConstants.GoogleMapsReviewPoints,
                PointTransactionType.GoogleMapsReviewBonus,
                null,
                cancellationToken
            );
        }

        await referralService.UpdateReferralProgressAsync(
            review.ReviewerId,
            ReferralGoalType.ReviewsCompleted,
            1,
            cancellationToken
        );

        var currentReviewerPoints = await pointService.GetPointBalanceAsync(review.ReviewerId, cancellationToken);

        await referralService.ProcessGoalCompletionAsync(
            review.ReviewerId,
            ReferralGoalType.PointsEarned,
            currentReviewerPoints,
            cancellationToken
        );

        return true;
    }

    public async Task<bool> RejectUserReviewAsync(long reviewId, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var reviewRepository = unitOfWork.GetRepository<BusinessReviewEntity>();

        var review = await reviewRepository.GetBaseQueryable()
            .Include(r => r.Business)
            .FirstOrDefaultAsync(r => r.Id == reviewId, cancellationToken);

        if (review is null)
        {
            throw new AppException(
                $"Review with ID {reviewId} not found.", "REVIEW_NOT_FOUND");
        }

        if (review.Status == BusinessReviewStatus.Rejected)
        {
            throw new AppException(
                $"Review with ID {reviewId} has already been rejected.", "REVIEW_ALREADY_REJECTED");
        }

        if (review.Status == BusinessReviewStatus.Accepted)
        {
            throw new AppException(
                $"Review with ID {reviewId} has already been accepted.", "REVIEW_ALREADY_ACCEPTED");
        }

        reviewRepository.Update(review);
        review.Status = BusinessReviewStatus.Rejected;
        review.DateRejectedUtc = DateTime.UtcNow;
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await pointService.AddPointsAsync(
            review.Business.OwnerAccountId,
            PointConstants.BasicReviewPoints,
            PointTransactionType.ReviewRejectionReturn,
            null,
            cancellationToken
        );

        if (review.PostedGoogleReview)
        {
            await pointService.AddPointsAsync(
                review.Business.OwnerAccountId,
                PointConstants.GoogleMapsReviewPoints,
                PointTransactionType.GoogleMapsReviewReturn,
                null,
                cancellationToken
            );
        }

        return true;
    }

    public async Task<bool> UpdateReviewStatusAsync(long reviewId, BusinessReviewStatus status,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var reviewRepository = unitOfWork.GetRepository<BusinessReviewEntity>();
        var review = await reviewRepository.GetByIdAsync(reviewId, cancellationToken);

        if (review is null)
        {
            throw new NotFoundException($"Review with ID {reviewId} not found.", "ReviewNotFound");
        }

        if (review.Status == status)
        {
            throw new AppException(
                $"Review with ID {reviewId} is already in status {status}.", "ReviewAlreadyInStatus");
        }

        reviewRepository.Update(review);
        review.UpdateStatus(status);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<IEnumerable<BusinessReviewEntity>> GetUserReviewsAsync(long reviewerId,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var reviewRepository = unitOfWork.GetRepository<BusinessReviewEntity>();

        var reviews = await reviewRepository
            .FindAsync(r => r.ReviewerId == reviewerId, cancellationToken);

        return reviews;
    }

    public async Task<IEnumerable<BusinessReviewEntity>> GetBusinessReviewsAsync(long businessId,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var reviewRepository = unitOfWork.GetRepository<BusinessReviewEntity>();

        var reviews = await reviewRepository
            .FindAsync(r => r.BusinessId == businessId, cancellationToken);

        return reviews;
    }

    public async Task<BusinessReviewEntity?> GetBusinessReviewAsync(long reviewId, bool includeBusiness = false,
        CancellationToken cancellationToken = default)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var reviewRepository = unitOfWork.GetRepository<BusinessReviewEntity>();

        var queryable = reviewRepository.GetBaseQueryable();

        if (includeBusiness)
        {
            queryable = queryable.Include(r => r.Business);
        }

        var review = await queryable
            .FirstOrDefaultAsync(r => r.Id == reviewId, cancellationToken);

        return review;
    }

    public async Task<BusinessReviewEntity?> GetUserReviewByBusinessIdAsync(long businessId, long reviewerId,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var reviewRepository = unitOfWork.GetRepository<BusinessReviewEntity>();

        var review = await reviewRepository.GetBaseQueryable()
            .FirstOrDefaultAsync(r => r.BusinessId == businessId && r.ReviewerId == reviewerId, cancellationToken);

        if (review is null)
        {
            return null;
        }

        if (review.BusinessId != businessId)
        {
            throw new InvalidOperationException(
                $"Review with ID {reviewerId} does not belong to business with ID {businessId}.");
        }

        return review;
    }

    public async Task<BusinessReviewEntity?> GetBusinessReviewByUserIdAsync(long businessId, long reviewerId,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var reviewRepository = unitOfWork.GetRepository<BusinessReviewEntity>();

        var review = await reviewRepository.GetBaseQueryable()
            .FirstOrDefaultAsync(r => r.BusinessId == businessId && r.ReviewerId == reviewerId, cancellationToken);

        if (review is null)
        {
            return null;
        }

        if (review.BusinessId != businessId)
        {
            throw new InvalidOperationException(
                $"Review with ID {reviewerId} does not belong to business with ID {businessId}.");
        }

        return review;
    }
}