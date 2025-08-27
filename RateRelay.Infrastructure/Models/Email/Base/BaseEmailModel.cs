namespace RateRelay.Infrastructure.Models.Email.Base;

public class BaseEmailModel
{
    public string Subject { get; set; } = string.Empty;
    public Company Company { get; set; }
    public User User { get; set; }
    public Links Links { get; set; }
}

public class Company
{
    public string Name { get; set; }
    public string LogoUrl { get; set; }
    public int LogoWidth { get; set; } = 120;
    public int LogoHeight { get; set; } = 40;
    public string Address { get; set; }
    public string Website { get; set; }
    public string PrivacyEmail { get; set; }
    public List<SocialLink> SocialLinks { get; set; }
}

public class User
{
    public string Name { get; set; }
    public string Email { get; set; }
}

public class Links
{
    public string Support { get; set; }
    public string Unsubscribe { get; set; }
    public string Preferences { get; set; }
}

public class SocialLink
{
    public string Platform { get; set; }
    public string Url { get; set; }
}