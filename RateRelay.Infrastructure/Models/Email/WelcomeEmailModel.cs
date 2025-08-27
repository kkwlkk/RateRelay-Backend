using RateRelay.Infrastructure.Models.Email.Base;

namespace RateRelay.Infrastructure.Models.Email;

public class WelcomeEmailModel : BaseEmailModel
{
    public List<string> Features { get; set; }
    public Cta Cta { get; set; }
    public string DashboardUrl { get; set; }
    public string? SecondaryUrl { get; set; }
}

public class Cta
{
    public string PrimaryText { get; set; }
    public string SecondaryText { get; set; }
}