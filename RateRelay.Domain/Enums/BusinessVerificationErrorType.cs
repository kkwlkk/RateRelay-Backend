namespace RateRelay.Domain.Enums;

public enum BusinessVerificationErrorType
{
    None,
    AlreadyVerified,
    InvalidPlaceId,
    AlreadyBeingVerified,
    AlreadyExists,
    BusinessNotFound,
    VerificationNotFound,
    VerificationExpired,
    NotOwnedByAccount,
    TooManyBusinesses
}