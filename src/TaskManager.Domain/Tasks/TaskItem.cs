using TaskManager.Domain.Common;

namespace TaskManager.Domain.Tasks;

public sealed class TaskItem
{
    public const int TitleMaxLength = 100;
    public const int DescriptionMaxLength = 500;

    private TaskItem() { }

    private TaskItem(Guid id, DateTime createdAtUtc)
    {
        Id = id;
        CreatedAt = createdAtUtc;
    }

    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateOnly? DueDate { get; private set; }
    public TaskItemStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public static TaskItem Create(
        string title,
        string? description,
        DateOnly? dueDate,
        TaskItemStatus status,
        DateTime createdAtUtc)
    {
        var task = new TaskItem(Guid.NewGuid(), createdAtUtc);
        task.Apply(title, description, dueDate, status);
        return task;
    }

    public void Update(
        string title,
        string? description,
        DateOnly? dueDate,
        TaskItemStatus status,
        DateTime updatedAtUtc)
    {
        Apply(title, description, dueDate, status);
        UpdatedAt = updatedAtUtc;
    }

    public bool IsOverdue(DateOnly today) =>
        Status != TaskItemStatus.Completed && DueDate < today;

    private void Apply(string title, string? description, DateOnly? dueDate, TaskItemStatus status)
    {
        var normalizedTitle = title?.Trim();
        if (string.IsNullOrEmpty(normalizedTitle))
            throw new DomainException("O título da tarefa é obrigatório.");

        if (normalizedTitle.Length > TitleMaxLength)
            throw new DomainException($"O título deve ter no máximo {TitleMaxLength} caracteres.");

        var normalizedDescription = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        if (normalizedDescription?.Length > DescriptionMaxLength)
            throw new DomainException($"A descrição deve ter no máximo {DescriptionMaxLength} caracteres.");

        if (!Enum.IsDefined(status))
            throw new DomainException($"Status '{status}' inválido.");

        Title = normalizedTitle;
        Description = normalizedDescription;
        DueDate = dueDate;
        Status = status;
    }
}
