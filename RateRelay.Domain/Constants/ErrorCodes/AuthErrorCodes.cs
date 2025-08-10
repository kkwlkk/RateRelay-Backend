namespace RateRelay.Domain.Constants.ErrorCodes;

public static class AuthErrorCodes
{
    public const string BackendUnavailable = "BACKEND_UNAVAILABLE";
    public const string InvalidCredentials = "INVALID_CREDENTIALS";
    public const string AccountDeleted = "ACCOUNT_DELETED";
    public const string AccountExists = "ACCOUNT_EXISTS";
    public const string SessionExpired = "SESSION_EXPIRED";
    public const string InvalidToken = "INVALID_TOKEN";
    public const string MissingGoogleData = "MISSING_GOOGLE_DATA";
    public const string ReferralLinkFailed = "REFERRAL_LINK_FAILED";
    public const string NetworkError = "NETWORK_ERROR";
    public const string UnknownError = "UNKNOWN_ERROR";
}