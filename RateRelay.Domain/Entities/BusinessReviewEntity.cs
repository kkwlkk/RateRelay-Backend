using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using RateRelay.Domain.Enums;

namespace RateRelay.Domain.Entities;

[Table("business_reviews")]
[Index(nameof(BusinessId))]
[Index(nameof(ReviewerId))]
[Index(nameof(Status))]
public class BusinessReviewEntity : BaseEntity
{
    public long BusinessId { get; set; }
    public long ReviewerId { get; set; }

    [MaxLength(64)]
    public BusinessReviewStatus Status { get; set; } = BusinessReviewStatus.Pending;

    public BusinessRating Rating { get; set; }

    [MaxLength(512)]
    public required string Comment { get; set; }

    public bool PostedGoogleReview { get; set; }

    public DateTime? DateAcceptedUtc { get; set; }
    public DateTime? DateRejectedUtc { get; set; }

    [ForeignKey("BusinessId")]
    public virtual BusinessEntity Business { get; set; }

    [ForeignKey("ReviewerId")]
    public virtual AccountEntity Reviewer { get; set; }

    [NotMapped]
    public bool IsAccepted => Status == BusinessReviewStatus.Accepted;

    [NotMapped]
    public bool IsRejected => Status == BusinessReviewStatus.Rejected;

    [NotMapped]
    public bool IsPending => Status == BusinessReviewStatus.Pending;

    public void UpdateStatus(BusinessReviewStatus newStatus)
    {
        if (Status == newStatus) return;

        Status = newStatus;
        var now = DateTime.UtcNow;

        switch (newStatus)
        {
            case BusinessReviewStatus.Accepted:
                DateAcceptedUtc = now;
                DateRejectedUtc = null;
                break;
            case BusinessReviewStatus.Rejected:
                DateRejectedUtc = now;
                DateAcceptedUtc = null;
                break;
            case BusinessReviewStatus.Pending:
                DateAcceptedUtc = null;
                DateRejectedUtc = null;
                break;
        }
    }
}