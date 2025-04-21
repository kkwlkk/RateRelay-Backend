namespace RateRelay.Domain.Enums;

public enum BusinessVerificationErrorType
{
    None,
    AlreadyVerified,
    InvalidPlaceId,
    AlreadyBeingVerified,
    BusinessNotFound,
    VerificationNotFound,
    VerificationExpired,
    NotOwnedByAccount,
    TooManyBusinesses
}