using System.Text.Json.Serialization;

namespace RateRelay.Domain.Common;

public class PagedApiResponse<T> : ApiResponse<List<T>>
{
    [JsonPropertyName("pagination")]
    [JsonPropertyOrder(100)]
    public PaginationInfo Pagination { get; init; }

    public static PagedApiResponse<T> Create(
        IEnumerable<T> items,
        int currentPage,
        int pageSize,
        long totalCount,
        Dictionary<string, object>? metadata = null)
    {
        var pagination = new PaginationInfo
        {
            CurrentPage = currentPage,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
            HasNextPage = currentPage < Math.Ceiling((double)totalCount / pageSize),
            HasPreviousPage = currentPage > 1
        };

        return new PagedApiResponse<T>
        {
            Success = true,
            Data = items.ToList(),
            Pagination = pagination,
            Error = null,
            Metadata = metadata,
            StatusCode = 200
        };
    }

    public static PagedApiResponse<T> Empty(int currentPage = 1, int pageSize = 10,
        Dictionary<string, object>? metadata = null)
    {
        return Create([], currentPage, pageSize, 0, metadata);
    }
}

public class PaginationInfo
{
    [JsonPropertyName("currentPage")]
    public int CurrentPage { get; init; }

    [JsonPropertyName("pageSize")]
    public int PageSize { get; init; }

    [JsonPropertyName("totalCount")]
    public long TotalCount { get; init; }

    [JsonPropertyName("totalPages")]
    public int TotalPages { get; init; }

    [JsonPropertyName("hasNextPage")]
    public bool HasNextPage { get; init; }

    [JsonPropertyName("hasPreviousPage")]
    public bool HasPreviousPage { get; init; }
}