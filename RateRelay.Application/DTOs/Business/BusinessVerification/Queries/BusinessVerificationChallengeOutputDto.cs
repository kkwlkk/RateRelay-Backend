namespace RateRelay.Application.DTOs.Business.BusinessVerification.Queries;

public class BusinessVerificationChallengeOutputDto
{
    public required byte VerificationDay { get; set; }
    public required TimeSpan VerificationOpeningTime { get; set; }
    public required TimeSpan VerificationClosingTime { get; set; }
}