using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Common.Pagination;
using TaskManager.Application.Common.Querying;
using TaskManager.Application.Tasks;
using TaskManager.Application.Tasks.List;
using TaskManager.Domain.Tasks;
using TaskManager.Infrastructure.Persistence.Querying;

namespace TaskManager.Infrastructure.Persistence.Repositories;

internal sealed class TaskRepository(AppDbContext context) : ITaskRepository
{
    public Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Tasks.FirstOrDefaultAsync(task => task.Id == id, cancellationToken);

    public Task<PagedResult<TaskItem>> ListAsync(TaskListCriteria criteria, CancellationToken cancellationToken = default)
    {
        var query = Filter(context.Tasks.AsNoTracking(), criteria);
        return Sort(query, criteria.SortBy, criteria.SortDirection)
            .ToPagedResultAsync(criteria.Paging, cancellationToken);
    }

    public void Add(TaskItem task) => context.Tasks.Add(task);

    public void Remove(TaskItem task) => context.Tasks.Remove(task);

    private static IQueryable<TaskItem> Filter(IQueryable<TaskItem> query, TaskListCriteria criteria)
    {
        if (criteria.Status is { } status)
            query = query.Where(task => task.Status == status);

        if (criteria.DueDateFrom is { } from)
            query = query.Where(task => task.DueDate >= from);

        if (criteria.DueDateTo is { } to)
            query = query.Where(task => task.DueDate <= to);

        if (criteria.Search is { } search)
        {
            var term = search.ToLowerInvariant();
            query = query.Where(task =>
                task.Title.ToLower().Contains(term) ||
                (task.Description != null && task.Description.ToLower().Contains(term)));
        }

        return query;
    }

    private static IOrderedQueryable<TaskItem> Sort(IQueryable<TaskItem> query, TaskSortField field, SortDirection direction)
    {
        var ordered = field switch
        {
            TaskSortField.CreatedAt => query.OrderByDirection(task => task.CreatedAt, direction),
            TaskSortField.Title => query.OrderByDirection(task => task.Title, direction),
            _ => query
                .OrderBy(task => task.DueDate == null)
                .ThenByDirection(task => task.DueDate, direction)
        };

        return ordered
            .ThenBy(task => task.CreatedAt)
            .ThenBy(task => task.Id);
    }
}
