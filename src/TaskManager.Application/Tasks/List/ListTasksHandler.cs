using FluentValidation;
using TaskManager.Application.Common.Pagination;
using TaskManager.Application.Common.Results;
using TaskManager.Application.Common.Time;
using TaskManager.Application.Common.Validation;

namespace TaskManager.Application.Tasks.List;

public sealed class ListTasksHandler(
    ITaskRepository repository,
    IValidator<ListTasksQuery> validator,
    TimeProvider timeProvider)
{
    public async Task<Result<PagedResult<TaskResponse>>> HandleAsync(
        ListTasksQuery query,
        CancellationToken cancellationToken = default)
    {
        if (await validator.ValidateToErrorAsync(query, cancellationToken) is { } error)
            return error;

        var page = await repository.ListAsync(query.ToCriteria(), cancellationToken);

        var today = timeProvider.GetToday();
        return page.Map(task => task.ToResponse(today));
    }
}
