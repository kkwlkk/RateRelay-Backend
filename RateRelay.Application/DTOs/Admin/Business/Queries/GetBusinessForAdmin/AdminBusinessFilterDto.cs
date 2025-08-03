namespace RateRelay.Application.DTOs.Admin.Business.Queries.GetBusinessForAdmin;

public class AdminBusinessFilterDto
{
    public int? MinReviews { get; set; }
    public int? MaxReviews { get; set; }
    public bool? IsVerified { get; set; }
    public long? OwnerId { get; set; }
}