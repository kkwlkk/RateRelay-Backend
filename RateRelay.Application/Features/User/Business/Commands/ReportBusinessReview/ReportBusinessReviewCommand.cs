using MediatR;
using RateRelay.Application.DTOs.Business.BusinessReviews.Commands;

namespace RateRelay.Application.Features.Business.Commands.ReportBusinessReview;

public class ReportBusinessReviewCommand : IRequest<ReportBusinessReviewOutputDto>
{
    public long ReviewId { get; set; }
    public string Content { get; set; } = string.Empty;
    public Domain.Enums.BusinessReviewReportReason Reason { get; set; } = Domain.Enums.BusinessReviewReportReason.Other;
}