using FluentValidation;
using Microsoft.Extensions.Logging;
using TaskManager.Application.Abstractions;
using TaskManager.Application.Common.Results;
using TaskManager.Application.Common.Time;
using TaskManager.Application.Common.Validation;

namespace TaskManager.Application.Tasks.Update;

public sealed class UpdateTaskHandler(
    ITaskRepository repository,
    IUnitOfWork unitOfWork,
    IValidator<UpdateTaskRequest> validator,
    TimeProvider timeProvider,
    ILogger<UpdateTaskHandler> logger)
{
    public async Task<Result<TaskResponse>> HandleAsync(
        Guid id,
        UpdateTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        if (await validator.ValidateToErrorAsync(request, cancellationToken) is { } error)
            return error;

        var task = await repository.GetByIdAsync(id, cancellationToken);
        if (task is null)
            return TaskErrors.NotFound(id);

        task.Update(
            request.Title,
            request.Description,
            request.DueDate,
            request.Status,
            timeProvider.GetUtcDateTime());

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.TaskUpdated(task.Id, task.Status);
        return task.ToResponse(timeProvider.GetToday());
    }
}
