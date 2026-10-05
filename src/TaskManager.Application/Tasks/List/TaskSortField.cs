namespace TaskManager.Application.Tasks.List;

/// <summary>
/// Campos disponíveis para ordenação: <c>DueDate</c>, <c>CreatedAt</c> ou <c>Title</c>.
/// </summary>
public enum TaskSortField
{
    DueDate = 0,
    CreatedAt = 1,
    Title = 2
}
