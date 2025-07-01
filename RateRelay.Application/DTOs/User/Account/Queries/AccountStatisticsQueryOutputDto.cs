namespace RateRelay.Application.DTOs.User.Account.Queries;

public class AccountStatisticsQueryOutputDto
{
    public int TotalBusinesses { get; set; }
    public int TotalTickets { get; set; }
    public int TotalAwaitingBusinessReviews { get; set; }
    public int TotalCompletedBusinessReviews { get; set; }
}