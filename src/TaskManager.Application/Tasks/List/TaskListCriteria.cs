using TaskManager.Application.Common.Pagination;
using TaskManager.Application.Common.Querying;
using TaskManager.Domain.Tasks;

namespace TaskManager.Application.Tasks.List;

public sealed record TaskListCriteria(
    TaskItemStatus? Status,
    DateOnly? DueDateFrom,
    DateOnly? DueDateTo,
    string? Search,
    TaskSortField SortBy,
    SortDirection SortDirection,
    PageRequest Paging);
