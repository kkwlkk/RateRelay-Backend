using RateRelay.Domain.Entities;
using RateRelay.Domain.Enums;

namespace RateRelay.Domain.Common;

public class BusinessVerificationResult
{
    public bool IsSuccess { get; private set; }
    public bool IsVerified { get; private set; }
    public BusinessEntity? Business { get; private set; }
    public BusinessVerificationEntity? Verification { get; private set; }
    public string? ErrorMessage { get; private set; }
    public BusinessVerificationErrorType ErrorType { get; private set; }

    private BusinessVerificationResult(bool isSuccess, bool isVerified = false, BusinessEntity? business = null,
        BusinessVerificationEntity? verification = null, string? errorMessage = null,
        BusinessVerificationErrorType errorType = BusinessVerificationErrorType.None)
    {
        IsSuccess = isSuccess;
        IsVerified = isVerified;
        Business = business;
        Verification = verification;
        ErrorMessage = errorMessage;
        ErrorType = errorType;
    }

    public static BusinessVerificationResult Success(BusinessEntity business, bool isVerified = false,
        BusinessVerificationEntity? verification = null)
        => new(true, isVerified, business, verification);

    public static BusinessVerificationResult AlreadyVerified(BusinessEntity business)
        => new(false, true, business, null, "Business is already verified",
            BusinessVerificationErrorType.AlreadyVerified);

    public static BusinessVerificationResult InvalidPlaceId()
        => new(false, false, null, null, "Invalid Google Place ID", BusinessVerificationErrorType.InvalidPlaceId);

    public static BusinessVerificationResult AlreadyBeingVerified(BusinessEntity business)
        => new(false, false, business, null, "Business is already being verified by another account",
            BusinessVerificationErrorType.AlreadyBeingVerified);

    public static BusinessVerificationResult BusinessNotFound(string message = "Business not found")
        => new(false, false, null, null, message, BusinessVerificationErrorType.BusinessNotFound);

    public static BusinessVerificationResult VerificationNotFound()
        => new(false, false, null, null, "No active verification found",
            BusinessVerificationErrorType.VerificationNotFound);

    public static BusinessVerificationResult VerificationExpired()
        => new(false, false, null, null, "Verification has expired", BusinessVerificationErrorType.VerificationExpired);

    public static BusinessVerificationResult NotOwnedByAccount()
        => new(false, false, null, null, "Business is not owned by the account",
            BusinessVerificationErrorType.NotOwnedByAccount);
    
    public static BusinessVerificationResult TooManyBusinesses()
        => new(false, false, null, null, "Too many businesses for this account",
            BusinessVerificationErrorType.TooManyBusinesses);
}