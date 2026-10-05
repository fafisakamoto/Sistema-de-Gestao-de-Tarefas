using TaskManager.Domain.Tasks;

namespace TaskManager.Application.Tasks;

/// <summary>
/// Representação de uma tarefa retornada pela API.
/// </summary>
public sealed record TaskResponse
{
    /// <summary>Código único da tarefa.</summary>
    public Guid Id { get; init; }

    /// <summary>Título.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Descrição.</summary>
    public string? Description { get; init; }

    /// <summary>Data de vencimento.</summary>
    public DateOnly? DueDate { get; init; }

    /// <summary>Status atual.</summary>
    public TaskItemStatus Status { get; init; }

    /// <summary>Indica se a tarefa está vencida e ainda não foi concluída.</summary>
    public bool IsOverdue { get; init; }

    /// <summary>Data/hora de criação (UTC).</summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>Data/hora da última atualização (UTC).</summary>
    public DateTime? UpdatedAt { get; init; }
}
