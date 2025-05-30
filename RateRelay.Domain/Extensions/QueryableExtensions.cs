using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using RateRelay.Domain.Common;

namespace RateRelay.Domain.Extensions;

public static class QueryableExtensions
{
    public static async Task<PagedApiResponse<T>> ToPagedApiResponseAsync<T>(
        this IQueryable<T> query,
        int page = 1,
        int pageSize = 10,
        Dictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        var (validatedPage, validatedPageSize) = ValidatePageParameters(page, pageSize);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await GetPagedItemsAsync(query, validatedPage, validatedPageSize, cancellationToken);

        return PagedApiResponse<T>.Create(
            items,
            validatedPage,
            validatedPageSize,
            totalCount,
            metadata);
    }

    public static async Task<PagedApiResponse<TResult>> ToPagedApiResponseAsync<T, TResult>(
        this IQueryable<T> query,
        Expression<Func<T, TResult>> selector,
        int page = 1,
        int pageSize = 10,
        Dictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        var (validatedPage, validatedPageSize) = ValidatePageParameters(page, pageSize);
        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((validatedPage - 1) * validatedPageSize)
            .Take(validatedPageSize)
            .Select(selector)
            .ToListAsync(cancellationToken);

        return PagedApiResponse<TResult>.Create(
            items,
            validatedPage,
            validatedPageSize,
            totalCount,
            metadata);
    }

    public static async Task<PagedApiResponse<TResult>> ToPagedApiResponseAsync<T, TResult>(
        this IQueryable<T> query,
        Func<T, TResult> selector,
        int page = 1,
        int pageSize = 10,
        Dictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        var (validatedPage, validatedPageSize) = ValidatePageParameters(page, pageSize);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await GetPagedItemsAsync(query, validatedPage, validatedPageSize, cancellationToken);
        var projectedItems = items.Select(selector);

        return PagedApiResponse<TResult>.Create(
            projectedItems,
            validatedPage,
            validatedPageSize,
            totalCount,
            metadata);
    }

    private static (int page, int pageSize) ValidatePageParameters(int page, int pageSize)
    {
        return (Math.Max(1, page), Math.Max(1, pageSize));
    }

    private static async Task<List<T>> GetPagedItemsAsync<T>(
        IQueryable<T> query,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var skip = (page - 1) * pageSize;
        return await query
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }
}