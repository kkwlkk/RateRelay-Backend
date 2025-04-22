using RateRelay.Domain.Common;
using RateRelay.Domain.Entities;

namespace RateRelay.Domain.Interfaces.Services;

public interface IBusinessVerificationService
{
    Task<BusinessVerificationResult> InitiateVerificationAsync(string placeId, long accountId);
    Task<BusinessVerificationResult> CheckVerificationStatusAsync(long accountId);
    Task<BusinessVerificationEntity?> GetActiveVerificationChallengeAsync(long accountId);
}