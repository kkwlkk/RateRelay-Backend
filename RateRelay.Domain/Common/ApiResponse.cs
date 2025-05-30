using System.Text.Json.Serialization;

public class ApiResponse<T>
{
    [JsonPropertyName("success")]
    public bool Success { get; init; }

    [JsonPropertyName("data")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T Data { get; init; }

    [JsonPropertyName("error")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ErrorResponse Error { get; init; }

    [JsonPropertyName("metadata")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object> Metadata { get; init; }

    [JsonIgnore]
    public int StatusCode { get; init; }

    public static ApiResponse<T> Create(
        bool success = true,
        T data = default,
        string errorMessage = null,
        string errorCode = null,
        IEnumerable<ValidationError> validationErrors = null,
        Dictionary<string, object> metadata = null,
        int statusCode = 200)
    {
        return new ApiResponse<T>
        {
            Success = success,
            Data = data,
            Error = success
                ? null
                : new ErrorResponse
                {
                    Message = errorMessage,
                    Code = errorCode,
                    ValidationErrors = validationErrors?.ToList()
                },
            Metadata = metadata,
            StatusCode = success ? statusCode : (statusCode == 200 ? 400 : statusCode)
        };
    }
}

public class ErrorResponse
{
    [JsonPropertyName("message")]
    public string Message { get; init; }

    [JsonPropertyName("code")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Code { get; init; }

    [JsonPropertyName("validationErrors")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ValidationError> ValidationErrors { get; init; }
}

public class ValidationError
{
    [JsonPropertyName("property")]
    public string Property { get; init; }

    [JsonPropertyName("message")]
    public string Message { get; init; }

    [JsonPropertyName("code")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Code { get; init; }

    [JsonPropertyName("attemptedValue")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object AttemptedValue { get; init; }
}