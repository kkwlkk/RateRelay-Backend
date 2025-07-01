namespace RateRelay.Application.DTOs.User.Tickets;

public class TicketCommentDto
{
    public long Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? EditedAtUtc { get; set; }

    public long AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;

    public bool IsInternal { get; set; } = false;
    public bool IsSystemGenerated { get; set; } = false;
}