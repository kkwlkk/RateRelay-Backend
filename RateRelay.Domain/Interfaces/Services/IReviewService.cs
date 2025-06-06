using RateRelay.Domain.Entities;
using RateRelay.Domain.Enums;

namespace RateRelay.Domain.Interfaces;

public interface IReviewService
{
    Task<bool> AddUserReviewAsync(long businessId, long reviewerId, BusinessRating rating, string comment, bool postedGoogleReview,CancellationToken cancellationToken);
    Task<bool> AcceptUserReviewAsync(long businessId, long reviewerId, CancellationToken cancellationToken);
    Task<bool> RejectUserReviewAsync(long businessId, long reviewerId, CancellationToken cancellationToken);
    Task<IEnumerable<BusinessReviewEntity>> GetUserReviewsAsync(long reviewerId, CancellationToken cancellationToken);
    Task<IEnumerable<BusinessReviewEntity>> GetBusinessReviewsAsync(long businessId, CancellationToken cancellationToken);
    Task<BusinessReviewEntity?> GetUserReviewByBusinessIdAsync(long businessId, long reviewerId, CancellationToken cancellationToken);
    Task<BusinessReviewEntity?> GetBusinessReviewByUserIdAsync(long businessId, long reviewerId, CancellationToken cancellationToken);
}