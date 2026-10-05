using TaskManager.Domain.Tasks;

namespace TaskManager.Application.Tasks;

internal static class TaskMappings
{
    public static TaskResponse ToResponse(this TaskItem task, DateOnly today) => new()
    {
        Id = task.Id,
        Title = task.Title,
        Description = task.Description,
        DueDate = task.DueDate,
        Status = task.Status,
        IsOverdue = task.IsOverdue(today),
        CreatedAt = task.CreatedAt,
        UpdatedAt = task.UpdatedAt
    };
}
