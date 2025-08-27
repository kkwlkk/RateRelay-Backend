using RateRelay.Infrastructure.Models.Email.Base;

namespace RateRelay.Infrastructure.Models.Email;

public class IncompleteVerificationEmailModel : BaseEmailModel
{
    public List<string> PossibleReasons { get; set; } = [];
    public Cta Cta { get; set; } = new();
    public string RestartVerificationUrl { get; set; } = string.Empty;
    public string FeedbackUrl { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public int HoursUntilRemoval { get; set; } = 24;
    public DateTime RemovedAt { get; set; }
}