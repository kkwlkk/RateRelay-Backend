namespace RateRelay.Domain.Common.DTOs;

public class BusinessBoostResultDto
{
    public long BusinessId { get; set; }
    public required string BusinessName { get; set; }
    public byte OldPriority { get; set; }
    public byte NewPriority { get; set; }
    public int CurrentReviews { get; set; }
    public int ReviewsNeededForTarget { get; set; }
    public required string Reason { get; set; }
    public DateTime BoostedAt { get; set; }
    public bool IsNowBoosted { get; set; }
}