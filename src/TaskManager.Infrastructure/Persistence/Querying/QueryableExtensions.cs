using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Common.Pagination;
using TaskManager.Application.Common.Querying;

namespace TaskManager.Infrastructure.Persistence.Querying;

internal static class QueryableExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        PageRequest paging,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip(paging.Skip)
            .Take(paging.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>(items, paging.Page, paging.PageSize, totalCount);
    }

    public static IOrderedQueryable<T> OrderByDirection<T, TKey>(
        this IQueryable<T> query,
        Expression<Func<T, TKey>> keySelector,
        SortDirection direction) =>
        direction == SortDirection.Desc ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);

    public static IOrderedQueryable<T> ThenByDirection<T, TKey>(
        this IOrderedQueryable<T> query,
        Expression<Func<T, TKey>> keySelector,
        SortDirection direction) =>
        direction == SortDirection.Desc ? query.ThenByDescending(keySelector) : query.ThenBy(keySelector);
}
