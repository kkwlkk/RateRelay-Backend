using RateRelay.Domain.Enums;

namespace RateRelay.Application.DTOs.User.Business.BusinessReviews.Commands;

public class ReportBusinessReviewInputDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public BusinessReviewReportReason Reason { get; set; } = BusinessReviewReportReason.Other;
}