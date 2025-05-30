using System.Text.Json.Serialization;

public class PagedResponse<T> : ApiResponse<PagedData<T>>
{
    public static PagedResponse<T> Create(
        IEnumerable<T> items,
        int currentPage,
        int pageSize,
        long totalCount,
        Dictionary<string, object>? metadata = null)
    {
        var pagedData = new PagedData<T>(items);

        var pagination = new PaginationInfo
        {
            CurrentPage = currentPage,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
            HasNextPage = currentPage < Math.Ceiling((double)totalCount / pageSize),
            HasPreviousPage = currentPage > 1
        };

        var finalMetadata = metadata is not null
            ? new Dictionary<string, object>(metadata)
            : new Dictionary<string, object>();

        finalMetadata["currentPage"] = pagination.CurrentPage;
        finalMetadata["pageSize"] = pagination.PageSize;
        finalMetadata["totalCount"] = pagination.TotalCount;
        finalMetadata["totalPages"] = pagination.TotalPages;
        finalMetadata["hasNextPage"] = pagination.HasNextPage;
        finalMetadata["hasPreviousPage"] = pagination.HasPreviousPage;

        return new PagedResponse<T>
        {
            Success = true,
            Data = pagedData,
            Error = null,
            Metadata = finalMetadata,
            StatusCode = 200
        };
    }

    public static PagedResponse<T> Empty(int currentPage = 1, int pageSize = 10,
        Dictionary<string, object> metadata = null)
    {
        return Create([], currentPage, pageSize, 0, metadata);
    }
}

public class PagedData<T> : List<T>
{
    public PagedData()
    {
    }

    public PagedData(IEnumerable<T> items) : base(items)
    {
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