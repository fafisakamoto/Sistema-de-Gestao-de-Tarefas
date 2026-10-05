using Microsoft.Extensions.Logging;
using TaskManager.Application.Abstractions;
using TaskManager.Application.Common.Results;

namespace TaskManager.Application.Tasks.Delete;

public sealed class DeleteTaskHandler(
    ITaskRepository repository,
    IUnitOfWork unitOfWork,
    ILogger<DeleteTaskHandler> logger)
{
    public async Task<Result> HandleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var task = await repository.GetByIdAsync(id, cancellationToken);
        if (task is null)
            return TaskErrors.NotFound(id);

        repository.Remove(task);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.TaskDeleted(id);
        return Result.Success();
    }
}
