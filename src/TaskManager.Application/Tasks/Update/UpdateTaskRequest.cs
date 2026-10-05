using TaskManager.Domain.Tasks;

namespace TaskManager.Application.Tasks.Update;

/// <summary>
/// Dados para atualização de uma tarefa (semântica de PUT: todos os campos editáveis são substituídos).
/// </summary>
public sealed record UpdateTaskRequest
{
    /// <summary>Título (obrigatório, até 100 caracteres).</summary>
    /// <example>Preparar apresentação da sprint</example>
    public string Title { get; init; } = string.Empty;

    /// <summary>Descrição (opcional). Enviar <c>null</c> remove a descrição.</summary>
    /// <example>Versão revisada com o PO</example>
    public string? Description { get; init; }

    /// <summary>Data de vencimento (opcional). Pode estar no passado, para tratar tarefas atrasadas.</summary>
    /// <example>2026-12-31</example>
    public DateOnly? DueDate { get; init; }

    /// <summary>Status da tarefa.</summary>
    /// <example>InProgress</example>
    public TaskItemStatus Status { get; init; } = TaskItemStatus.Pending;
}
