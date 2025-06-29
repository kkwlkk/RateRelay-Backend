using RateRelay.Domain.Enums;

namespace RateRelay.Application.DTOs.Account.ReviewHistory.Queries;

public class AccountReviewHistoryQueryOutputDto
{
    public required long Id { get; set; }
    public required string BusinessName { get; set; }
    public required string Cid { get; set; }
    public required string MapUrl { get; set; }
    public required BusinessReviewStatus Status { get; set; }
    public required BusinessRating Rating { get; set; }
    public required string Comment { get; set; } = string.Empty;
    public required DateTime DateCreatedUtc { get; set; }
}