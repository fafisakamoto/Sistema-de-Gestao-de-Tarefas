using Microsoft.Extensions.Logging;
using TaskManager.Domain.Tasks;

namespace TaskManager.Application.Tasks;

internal static partial class TaskLog
{
    [LoggerMessage(EventId = 1000, Level = LogLevel.Information, Message = "Tarefa {TaskId} criada com status {Status}")]
    public static partial void TaskCreated(this ILogger logger, Guid taskId, TaskItemStatus status);

    [LoggerMessage(EventId = 1001, Level = LogLevel.Information, Message = "Tarefa {TaskId} atualizada para status {Status}")]
    public static partial void TaskUpdated(this ILogger logger, Guid taskId, TaskItemStatus status);

    [LoggerMessage(EventId = 1002, Level = LogLevel.Information, Message = "Tarefa {TaskId} excluída")]
    public static partial void TaskDeleted(this ILogger logger, Guid taskId);
}
