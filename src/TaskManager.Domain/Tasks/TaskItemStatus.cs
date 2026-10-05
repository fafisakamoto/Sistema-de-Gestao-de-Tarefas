namespace TaskManager.Domain.Tasks;

/// <summary>
/// Status de uma tarefa: <c>Pending</c> (Pendente), <c>InProgress</c> (Em progresso), <c>Completed</c> (Concluída).
/// </summary>
public enum TaskItemStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2
}
