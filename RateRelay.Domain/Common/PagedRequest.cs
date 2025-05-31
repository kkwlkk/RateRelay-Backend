using System.ComponentModel.DataAnnotations;

namespace RateRelay.Domain.Common;

public class PagedRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
    public int Page { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
    public int PageSize { get; set; } = 10;

    public string? SortBy { get; set; }

    public SortDirection SortDirection { get; set; } = SortDirection.Ascending;

    public string? Search { get; set; }

    public int GetSkip() => (Page - 1) * PageSize;
    public int GetTake() => PageSize;
}

public enum SortDirection
{
    Ascending = 0,
    Descending = 1
}

public static class PagedRequestExtensions
{
    public static PagedApiResponse<T> ToPagedResponse<T>(
        this PagedRequest request,
        IEnumerable<T> items,
        long totalCount,
        Dictionary<string, object>? metadata = null)
    {
        return PagedApiResponse<T>.Create(
            items,
            request.Page,
            request.PageSize,
            totalCount,
            metadata);
    }

    public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, PagedRequest request)
    {
        return query.Skip(request.GetSkip()).Take(request.GetTake());
    }

    public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, PagedRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.SortBy))
            return query;

        try
        {
            var parameter = System.Linq.Expressions.Expression.Parameter(typeof(T), "x");
            var property = System.Linq.Expressions.Expression.Property(parameter, request.SortBy);
            var lambda = System.Linq.Expressions.Expression.Lambda(property, parameter);

            var methodName = request.SortDirection == SortDirection.Ascending ? "OrderBy" : "OrderByDescending";
            var method = typeof(Queryable).GetMethods()
                .First(m => m.Name == methodName && m.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(T), property.Type);

            return (IQueryable<T>)method.Invoke(null, [query, lambda])!;
        }
        catch
        {
            return query;
        }
    }

    public static IQueryable<T> ApplySearch<T>(this IQueryable<T> query, PagedRequest request,
        System.Linq.Expressions.Expression<Func<T, bool>> searchExpression)
    {
        return string.IsNullOrWhiteSpace(request.Search)
            ? query
            : query.Where(searchExpression);
    }
}

public class PagedRequest<TFilter> : PagedRequest where TFilter : new()
{
    public TFilter? Filters { get; set; }
}