using TaskManager.Application.Common.Results;

namespace TaskManager.Application.Tasks;

public static class TaskErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("Task.NotFound", $"Tarefa '{id}' não encontrada.");
}
