using Microsoft.EntityFrameworkCore;
using RateRelay.Domain.Constants;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Enums;
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
            throw new ArgumentException($"Business with ID {businessId} not found.");
        }

        var reviewRepository = unitOfWork.GetRepository<BusinessReviewEntity>();

        var existingReview = await reviewRepository
            .FindAsync(r => r.BusinessId == businessId && r.ReviewerId == reviewerId, cancellationToken);

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

        await reviewRepository.InsertAsync(review, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> AcceptUserReviewAsync(long businessId, long reviewerId, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var reviewRepository = unitOfWork.GetRepository<BusinessReviewEntity>();

        var review = await reviewRepository.GetBaseQueryable()
            .FirstOrDefaultAsync(r => r.BusinessId == businessId && r.ReviewerId == reviewerId, cancellationToken);

        if (review is null)
        {
            throw new ArgumentException($"Review with ID {reviewerId} not found.");
        }

        if (review.BusinessId != businessId)
        {
            throw new InvalidOperationException(
                $"Review with ID {reviewerId} does not belong to business with ID {businessId}.");
        }

        switch (review.Status)
        {
            case BusinessReviewStatus.Accepted:
                throw new InvalidOperationException(
                    $"Review with ID {reviewerId} has already been accepted.");
            case BusinessReviewStatus.Rejected:
                throw new InvalidOperationException(
                    $"Review with ID {reviewerId} has already been rejected.");
        }

        reviewRepository.Update(review);
        review.Status = BusinessReviewStatus.Accepted;
        review.DateAcceptedUtc = DateTime.UtcNow;
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await pointService.AddPointsAsync(
            reviewerId,
            PointConstants.BusinessReviewPoints,
            PointTransactionType.BusinessReview,
            $"Accepted review for business with ID {businessId}",
            cancellationToken
        );

        return true;
    }

    public async Task<bool> RejectUserReviewAsync(long businessId, long reviewerId, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var reviewRepository = unitOfWork.GetRepository<BusinessReviewEntity>();

        var review = await reviewRepository
            .GetBaseQueryable()
            .FirstOrDefaultAsync(r => r.BusinessId == businessId && r.ReviewerId == reviewerId, cancellationToken);

        if (review is null)
        {
            throw new ArgumentException($"Review with ID {reviewerId} not found.");
        }

        if (review.BusinessId != businessId)
        {
            throw new InvalidOperationException(
                $"Review with ID {reviewerId} does not belong to business with ID {businessId}.");
        }

        switch (review.Status)
        {
            case BusinessReviewStatus.Rejected:
                throw new InvalidOperationException(
                    $"Review with ID {reviewerId} has already been rejected.");
            case BusinessReviewStatus.Accepted:
                throw new InvalidOperationException(
                    $"Review with ID {reviewerId} has already been accepted.");
        }

        reviewRepository.Update(review);
        review.Status = BusinessReviewStatus.Rejected;
        review.DateRejectedUtc = DateTime.UtcNow;
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