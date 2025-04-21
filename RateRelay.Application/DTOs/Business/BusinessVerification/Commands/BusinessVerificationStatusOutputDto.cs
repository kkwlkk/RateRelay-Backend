namespace RateRelay.Application.DTOs.Business.BusinessVerification.Commands;

public class BusinessVerificationStatusOutputDto
{
    public required long VerificationId { get; set; }
    public bool IsVerified { get; set; }
    public required string Status { get; set; }
}