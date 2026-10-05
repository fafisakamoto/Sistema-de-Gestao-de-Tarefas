using FluentValidation;
using Microsoft.Extensions.Logging;
using TaskManager.Application.Abstractions;
using TaskManager.Application.Common.Results;
using TaskManager.Application.Common.Time;
using TaskManager.Application.Common.Validation;
using TaskManager.Domain.Tasks;

namespace TaskManager.Application.Tasks.Create;

public sealed class CreateTaskHandler(
    ITaskRepository repository,
    IUnitOfWork unitOfWork,
    IValidator<CreateTaskRequest> validator,
    TimeProvider timeProvider,
    ILogger<CreateTaskHandler> logger)
{
    public async Task<Result<TaskResponse>> HandleAsync(
        CreateTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        if (await validator.ValidateToErrorAsync(request, cancellationToken) is { } error)
            return error;

        var task = TaskItem.Create(
            request.Title,
            request.Description,
            request.DueDate,
            request.Status,
            timeProvider.GetUtcDateTime());

        repository.Add(task);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.TaskCreated(task.Id, task.Status);
        return task.ToResponse(timeProvider.GetToday());
    }
}
