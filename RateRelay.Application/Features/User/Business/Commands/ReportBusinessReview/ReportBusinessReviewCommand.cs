using MediatR;
using RateRelay.Application.DTOs.Business.BusinessReviews.Commands;
using RateRelay.Application.DTOs.User.Business.BusinessReviews.Commands;

namespace RateRelay.Application.Features.User.Business.Commands.ReportBusinessReview;

public class ReportBusinessReviewCommand : IRequest<ReportBusinessReviewOutputDto>
{
    public long BusinessId { get; set; }
    public long ReviewId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Domain.Enums.BusinessReviewReportReason Reason { get; set; } = Domain.Enums.BusinessReviewReportReason.Other;
}