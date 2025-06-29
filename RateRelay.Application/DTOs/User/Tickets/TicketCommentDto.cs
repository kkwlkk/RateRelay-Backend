namespace RateRelay.Application.DTOs.Tickets;

public class TicketCommentDto
{
    public long Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public long AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;

    public bool IsInternal { get; set; } = false;
    public bool IsSystemGenerated { get; set; } = false;
}