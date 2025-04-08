using System.Text.Json.Serialization;

namespace RateRelay.Domain.Common;

public class ApiResponse<T>
{
    [JsonPropertyName("success")]
    public bool Success { get; private set; }

    [JsonPropertyName("data")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T Data { get; private set; }

    [JsonPropertyName("error")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ErrorResponse Error { get; private set; }

    [JsonPropertyName("metadata")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public MetadataResponse Metadata { get; private set; }

    [JsonIgnore]
    public int StatusCode { get; private set; }

    private ApiResponse()
    {
        Metadata = new MetadataResponse();
    }

    public static ApiResponse<T> SuccessResponse(T data, int statusCode = 200)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            StatusCode = statusCode,
            Error = null,
            Metadata = null
        };
    }

    public static ApiResponse<T> ErrorResponse(string message, string code = null, int statusCode = 400)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Data = default,
            StatusCode = statusCode,
            Error = new ErrorResponse
            {
                Message = message,
                Code = code
            },
            Metadata = null
        };
    }

    public static ApiResponse<T> ErrorResponse(ErrorResponse errorResponse, int statusCode = 400)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Data = default,
            StatusCode = statusCode,
            Error = errorResponse,
            Metadata = null
        };
    }

    public static ApiResponse<T> ValidationErrorResponse(IEnumerable<ValidationError> validationErrors,
        int statusCode = 400)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Data = default,
            StatusCode = statusCode,
            Error = new ErrorResponse
            {
                Message = "Validation failed",
                ValidationErrors = validationErrors.ToList()
            },
            Metadata = null
        };
    }

    public void WithMetadata(Action<MetadataResponse> configureMetadata)
    {
        Metadata = new MetadataResponse();
        configureMetadata(Metadata);
    }
}

public class ErrorResponse
{
    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("code")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Code { get; set; }

    [JsonPropertyName("validationErrors")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ValidationError> ValidationErrors { get; set; } = new();
}

public class ValidationError
{
    [JsonPropertyName("property")]
    public string Property { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("code")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Code { get; set; }

    [JsonPropertyName("attemptedValue")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object AttemptedValue { get; set; }
}

public class MetadataResponse
{
    [JsonPropertyName("totalCount")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int TotalCount { get; set; }

    [JsonPropertyName("pageSize")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int PageSize { get; set; }

    [JsonPropertyName("currentPage")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int CurrentPage { get; set; }

    [JsonPropertyName("totalPages")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int TotalPages { get; set; }

    [JsonPropertyName("additionalInfo")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Dictionary<string, object> AdditionalInfo { get; private set; } = new();
}