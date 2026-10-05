using TaskManager.Application.Common.Results;
using TaskManager.Application.Common.Time;

namespace TaskManager.Application.Tasks.GetById;

public sealed class GetTaskByIdHandler(ITaskRepository repository, TimeProvider timeProvider)
{
    public async Task<Result<TaskResponse>> HandleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var task = await repository.GetByIdAsync(id, cancellationToken);
        if (task is null)
            return TaskErrors.NotFound(id);

        return task.ToResponse(timeProvider.GetToday());
    }
}
