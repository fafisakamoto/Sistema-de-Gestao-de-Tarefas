using TaskManager.Application.Common.Pagination;
using TaskManager.Application.Common.Querying;
using TaskManager.Domain.Tasks;

namespace TaskManager.Application.Tasks.List;

/// <summary>
/// Filtros, ordenação e paginação da listagem. Todos opcionais e combináveis.
/// </summary>
public sealed class ListTasksQuery : PagedQuery
{
    public const int SearchMaxLength = 100;

    /// <summary>Filtra pelo status.</summary>
    public TaskItemStatus? Status { get; set; }

    /// <summary>Vencimento a partir desta data, inclusive (yyyy-MM-dd).</summary>
    public DateOnly? DueDateFrom { get; set; }

    /// <summary>Vencimento até esta data, inclusive (yyyy-MM-dd).</summary>
    public DateOnly? DueDateTo { get; set; }

    /// <summary>Busca no título e na descrição, sem diferenciar maiúsculas de minúsculas.</summary>
    public string? Search { get; set; }

    /// <summary>Campo de ordenação. Padrão: <c>DueDate</c> (tarefas sem vencimento ficam por último).</summary>
    public TaskSortField SortBy { get; set; } = TaskSortField.DueDate;

    /// <summary>Direção da ordenação. Padrão: <c>Asc</c>.</summary>
    public SortDirection SortDirection { get; set; } = SortDirection.Asc;

    public TaskListCriteria ToCriteria() => new(
        Status,
        DueDateFrom,
        DueDateTo,
        string.IsNullOrWhiteSpace(Search) ? null : Search.Trim(),
        SortBy,
        SortDirection,
        ToPageRequest());
}
