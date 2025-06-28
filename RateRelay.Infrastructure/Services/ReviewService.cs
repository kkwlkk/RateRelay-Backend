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
    IPointService pointService
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

        var review = new BusinessReviewEntity
        {
            BusinessId = businessId,
            ReviewerId = reviewerId,
            Status = BusinessReviewStatus.Pending,
            Rating = rating,
            Comment = comment,
            PostedGoogleReview = postedGoogleReview
        };

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

    public async Task<BusinessReviewEntity?> GetBusinessReviewAsync(long reviewId, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var reviewRepository = unitOfWork.GetRepository<BusinessReviewEntity>();

        var review = await reviewRepository.GetByIdAsync(reviewId, cancellationToken);

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

    public async Task<bool> ReportBusinessReviewAsync(long reporterId, long reviewId, string content, BusinessReviewReportReason reason,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var businessRepository = unitOfWork.GetRepository<BusinessEntity>();
        
        var review = await GetBusinessReviewAsync(reviewId, cancellationToken);
        
        if (review is null)
        {
            throw new NotFoundException(
                $"Business review with ID {reviewId} not found.", "BusinessReviewNotFound");
        }
        
        var business = await businessRepository.GetByIdAsync(review.BusinessId, cancellationToken);
        
        if (business is null)
        {
            throw new NotFoundException(
                $"Business with ID {review.BusinessId} not found.", "BusinessNotFound");
        }
        
        if (review.ReviewerId == reporterId)
        {
            throw new AppException(
                $"You cannot report your own review with ID {reviewId}.",
                "BusinessReviewCannotReportOwn");
        }
        
        if (business.OwnerAccountId != reporterId)
        {
            throw new AppException(
                $"You do not have permission to report this review with ID {reviewId}.",
                "BusinessReviewNoPermissionToReport");
        }

        if (review.Status is BusinessReviewStatus.UnderDispute)
        {
            throw new AppException(
                $"Business review with ID {reviewId} is already under dispute.",
                "BusinessReviewAlreadyUnderDispute");
        }
        
        if (review.Status is not BusinessReviewStatus.Pending and not BusinessReviewStatus.Accepted)
        {
            throw new AppException(
                $"Business review with ID {reviewId} cannot be reported.",
                "BusinessReviewCannotBeReported");
        }
        
        // review.Status = BusinessReviewStatus.UnderDispute;
        
        
        return true;
        
        
        
    }
}