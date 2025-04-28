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
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object> Metadata { get; private set; }

    [JsonIgnore]
    public int StatusCode { get; private set; }

    private ApiResponse()
    {
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

    public static ApiResponse<T> SuccessResponse(T data, Dictionary<string, object> metadata, int statusCode = 200)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            StatusCode = statusCode,
            Error = null,
            Metadata = metadata
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
                Code = code,
                ValidationErrors = null
            },
            Metadata = null
        };
    }

    public static ApiResponse<T> ErrorResponse(string message, string code = null, Dictionary<string, object> metadata = null, int statusCode = 400)
    {
        var response = new ApiResponse<T>
        {
            Success = false,
            Data = default,
            StatusCode = statusCode,
            Error = new ErrorResponse
            {
                Message = message,
                Code = code,
                ValidationErrors = null
            },
            Metadata = metadata
        };

        return response;
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

    public static ApiResponse<T> ErrorResponse(ErrorResponse errorResponse, Dictionary<string, object> metadata, int statusCode = 400)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Data = default,
            StatusCode = statusCode,
            Error = errorResponse,
            Metadata = metadata
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

    public static ApiResponse<T> ValidationErrorResponse(IEnumerable<ValidationError> validationErrors,
        Dictionary<string, object> metadata, int statusCode = 400)
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
            Metadata = metadata
        };
    }

    // Add metadata to an existing response
    public ApiResponse<T> WithMetadata(Dictionary<string, object> metadata)
    {
        this.Metadata = metadata;
        return this;
    }

    // Add or update a single metadata value
    public ApiResponse<T> WithMetadataValue(string key, object value)
    {
        if (this.Metadata == null)
        {
            this.Metadata = new Dictionary<string, object>();
        }
        
        this.Metadata[key] = value;
        return this;
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
    public List<ValidationError> ValidationErrors { get; set; } = [];
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