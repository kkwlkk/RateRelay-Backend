using RateRelay.Domain.Entities;

namespace RateRelay.Domain.Common;

public class TicketSubjects
{
    public long? BusinessId { get; set; }
    public long? ReviewId { get; set; }

    public void ApplyTo(TicketEntity ticket)
    {
        ticket.SubjectBusinessId = BusinessId;
        ticket.SubjectReviewId = ReviewId;
    }
}