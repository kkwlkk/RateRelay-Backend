namespace RateRelay.Application.DTOs.Accounts.Commands;

public class CreateAccountCommandResponseDto
{
    public long AccountId { get; set; }
    public string Username { get; set; }
    public DateTime DateCreatedUtc { get; set; }
}