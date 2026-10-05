using TaskManager.Application.Common.Pagination;
using TaskManager.Application.Tasks.List;
using TaskManager.Domain.Tasks;

namespace TaskManager.Application.Tasks;

public interface ITaskRepository
{
    Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResult<TaskItem>> ListAsync(TaskListCriteria criteria, CancellationToken cancellationToken = default);

    void Add(TaskItem task);

    void Remove(TaskItem task);
}
