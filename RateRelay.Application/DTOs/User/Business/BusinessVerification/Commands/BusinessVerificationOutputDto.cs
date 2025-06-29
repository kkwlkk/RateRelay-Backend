namespace RateRelay.Application.DTOs.Business.BusinessVerification.Commands;

public class BusinessVerificationOutputDto
{
    // public required string PlaceId { get; set; }
    public required ulong VerificationId { get; set; }
    public required string Status { get; set; }
    public required byte VerificationDay { get; set; }
    public required TimeSpan VerificationOpeningTime { get; set; }
    public required TimeSpan VerificationClosingTime { get; set; }
}