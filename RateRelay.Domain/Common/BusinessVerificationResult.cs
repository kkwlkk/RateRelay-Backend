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
    public string? ErrorCode { get; private set; }
    public BusinessVerificationErrorType ErrorType { get; private set; }
    public Dictionary<string, object> Metadata { get; private set; } = new();

    private BusinessVerificationResult(bool isSuccess, bool isVerified = false, BusinessEntity? business = null,
        BusinessVerificationEntity? verification = null, string? errorMessage = null, string? errorCode = null,
        BusinessVerificationErrorType errorType = BusinessVerificationErrorType.None,
        Dictionary<string, object>? metadata = null)
    {
        IsSuccess = isSuccess;
        IsVerified = isVerified;
        Business = business;
        Verification = verification;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
        ErrorType = errorType;
        Metadata = metadata ?? new Dictionary<string, object>();
    }

    public static BusinessVerificationResult Success(BusinessEntity business, bool isVerified = false,
        BusinessVerificationEntity? verification = null)
        => new(true, isVerified, business, verification);

    public static BusinessVerificationResult AlreadyVerified(BusinessEntity business)
    {
        var metadata = new Dictionary<string, object>
        {
            { "businessId", business.Id },
            { "placeId", business.PlaceId }
        };
        
        return new(false, true, business, null, "Business is already verified", "ERR_ALREADY_VERIFIED",
            BusinessVerificationErrorType.AlreadyVerified, metadata);
    }

    public static BusinessVerificationResult InvalidPlaceId(string placeId)
    {
        var metadata = new Dictionary<string, object>
        {
            { "placeId", placeId }
        };
        
        return new(false, false, null, null, "Invalid Google Place ID", "ERR_INVALID_PLACE_ID", 
            BusinessVerificationErrorType.InvalidPlaceId, metadata);
    }

    public static BusinessVerificationResult AlreadyBeingVerified(BusinessEntity business)
    {
        var metadata = new Dictionary<string, object>
        {
            { "businessId", business.Id },
            { "placeId", business.PlaceId },
            { "ownerAccountId", business.OwnerAccountId }
        };
        
        return new(false, false, business, null, "Business is already being verified by another account", "ERR_ALREADY_BEING_VERIFIED",
            BusinessVerificationErrorType.AlreadyBeingVerified, metadata);
    }

    public static BusinessVerificationResult BusinessNotFound(string message = "Business not found")
        => new(false, false, null, null, message, "ERR_BUSINESS_NOT_FOUND",
            BusinessVerificationErrorType.BusinessNotFound);

    public static BusinessVerificationResult VerificationNotFound()
        => new(false, false, null, null, "No active verification found", "ERR_VERIFICATION_NOT_FOUND",
            BusinessVerificationErrorType.VerificationNotFound);

    public static BusinessVerificationResult VerificationExpired()
        => new(false, false, null, null, "Verification has expired", "ERR_VERIFICATION_EXPIRED", 
            BusinessVerificationErrorType.VerificationExpired);

    public static BusinessVerificationResult NotOwnedByAccount()
        => new(false, false, null, null, "Business is not owned by the account", "ERR_NOT_OWNED_BY_ACCOUNT",
            BusinessVerificationErrorType.NotOwnedByAccount);
    
    public static BusinessVerificationResult TooManyBusinesses()
        => new(false, false, null, null, "Too many businesses for this account", "ERR_TOO_MANY_BUSINESSES",
            BusinessVerificationErrorType.TooManyBusinesses);
}