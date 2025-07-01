namespace RateRelay.Application.DTOs.User.Tickets.Commands;

public class AddTicketCommandInputDto
{
    public string Comment { get; set; } = string.Empty;
    public bool IsInternal { get; set; } = false;
}