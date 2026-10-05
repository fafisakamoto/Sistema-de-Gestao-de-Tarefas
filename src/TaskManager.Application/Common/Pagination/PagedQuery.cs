namespace TaskManager.Application.Common.Pagination;

/// <summary>
/// Base para qualquer consulta paginada da API.
/// </summary>
public abstract class PagedQuery
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    /// <summary>Página desejada (começa em 1).</summary>
    /// <example>1</example>
    public int Page { get; set; } = 1;

    /// <summary>Itens por página (1 a 100).</summary>
    /// <example>20</example>
    public int PageSize { get; set; } = DefaultPageSize;

    public PageRequest ToPageRequest() => new(Page, PageSize);
}
