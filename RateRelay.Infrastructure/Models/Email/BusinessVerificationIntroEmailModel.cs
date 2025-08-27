using RateRelay.Infrastructure.Models.Email.Base;

namespace RateRelay.Infrastructure.Models.Email;

public class BusinessVerificationIntroEmailModel : BaseEmailModel
{
    public List<string> Benefits { get; set; } = [];
    public List<string> Reasons { get; set; } = [];
    public Cta Cta { get; set; } = new();
    public string VerificationUrl { get; set; } = string.Empty;
    public string? SecondaryUrl { get; set; }
    public int DaysFromRegistration { get; set; }
}