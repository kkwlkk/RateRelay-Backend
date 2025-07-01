using RateRelay.Domain.Entities;
using RateRelay.Domain.Enums;

namespace RateRelay.Domain.Interfaces;

public interface IReviewService
{
    Task<bool> AddUserReviewAsync(long businessId, long reviewerId, BusinessRating rating, string comment,
        bool postedGoogleReview, CancellationToken cancellationToken);

    Task<bool> AcceptUserReviewAsync(long reviewId, CancellationToken cancellationToken);
    Task<bool> RejectUserReviewAsync(long reviewId, CancellationToken cancellationToken);
    
    Task<bool> UpdateReviewStatusAsync(long reviewId, BusinessReviewStatus status,
        CancellationToken cancellationToken);

    Task<IEnumerable<BusinessReviewEntity>> GetUserReviewsAsync(long reviewerId, CancellationToken cancellationToken);

    Task<IEnumerable<BusinessReviewEntity>> GetBusinessReviewsAsync(long businessId,
        CancellationToken cancellationToken);

    Task<BusinessReviewEntity?> GetBusinessReviewAsync(long reviewId, bool includeBusiness = false, CancellationToken cancellationToken = default);

    Task<BusinessReviewEntity?> GetUserReviewByBusinessIdAsync(long businessId, long reviewerId,
        CancellationToken cancellationToken);

    Task<BusinessReviewEntity?> GetBusinessReviewByUserIdAsync(long businessId, long reviewerId,
        CancellationToken cancellationToken);
}