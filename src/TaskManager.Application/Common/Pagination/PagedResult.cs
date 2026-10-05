namespace TaskManager.Application.Common.Pagination;

/// <summary>
/// Página de resultados.
/// </summary>
/// <param name="Items">Itens da página atual.</param>
/// <param name="Page">Número da página (começa em 1).</param>
/// <param name="PageSize">Quantidade máxima de itens por página.</param>
/// <param name="TotalCount">Total de itens que atendem aos filtros.</param>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    /// <summary>Total de páginas.</summary>
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>Indica se existe próxima página.</summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>Indica se existe página anterior.</summary>
    public bool HasPreviousPage => Page > 1;

    public PagedResult<TResult> Map<TResult>(Func<T, TResult> selector) =>
        new(Items.Select(selector).ToList(), Page, PageSize, TotalCount);
}
