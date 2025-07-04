using System.ComponentModel.DataAnnotations;

namespace RateRelay.Application.DTOs.User.Referral.Commands;

public class LinkReferralInputDto
{
    [Required(ErrorMessage = "Referral code is required")]
    [StringLength(16, MinimumLength = 6, ErrorMessage = "Referral code must be between 6 and 16 characters")]
    public required string ReferralCode { get; set; }
}