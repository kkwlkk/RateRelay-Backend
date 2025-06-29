using RateRelay.Domain.Enums;

namespace RateRelay.Application.DTOs.Business.BusinessReviews.Commands;

public class ReportBusinessReviewInputDto
{
    public long ReviewId { get; set; }
    public string Content { get; set; } = string.Empty;
    public BusinessReviewReportReason Reason { get; set; } = BusinessReviewReportReason.Other;
}