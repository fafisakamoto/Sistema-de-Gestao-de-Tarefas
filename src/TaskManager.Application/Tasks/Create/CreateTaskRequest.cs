using TaskManager.Domain.Tasks;

namespace TaskManager.Application.Tasks.Create;

/// <summary>
/// Dados para criação de uma tarefa.
/// </summary>
public sealed record CreateTaskRequest
{
    /// <summary>Título (obrigatório, até 100 caracteres).</summary>
    /// <example>Preparar apresentação da sprint</example>
    public string Title { get; init; } = string.Empty;

    /// <summary>Descrição (opcional, até 500 caracteres).</summary>
    /// <example>Consolidar métricas e principais entregas do time</example>
    public string? Description { get; init; }

    /// <summary>Data de vencimento (opcional, yyyy-MM-dd). Não pode ser anterior a hoje.</summary>
    /// <example>2026-12-31</example>
    public DateOnly? DueDate { get; init; }

    /// <summary>Status inicial. Padrão: <c>Pending</c>.</summary>
    /// <example>Pending</example>
    public TaskItemStatus Status { get; init; } = TaskItemStatus.Pending;
}
