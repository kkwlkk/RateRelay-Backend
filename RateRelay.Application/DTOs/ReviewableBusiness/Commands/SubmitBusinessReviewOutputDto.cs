namespace RateRelay.Application.DTOs.ReviewableBusiness.Commands;

public class SubmitBusinessReviewOutputDto
{
    public required long BusinessId { get; set; }
    public required long ReviewId { get; set; }
    public required DateTime SubmittedOn { get; set; }
}