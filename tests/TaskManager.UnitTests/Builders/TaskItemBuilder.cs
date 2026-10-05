using TaskManager.Domain.Tasks;
using TaskManager.UnitTests.Fakes;

namespace TaskManager.UnitTests.Builders;

internal sealed class TaskItemBuilder
{
    private string _title = "Tarefa de teste";
    private string? _description = "Descrição de teste";
    private DateOnly? _dueDate = DateOnly.FromDateTime(FixedTimeProvider.DefaultNow.UtcDateTime).AddDays(5);
    private TaskItemStatus _status = TaskItemStatus.Pending;
    private DateTime _createdAt = FixedTimeProvider.DefaultNow.UtcDateTime.AddDays(-1);

    public TaskItemBuilder WithTitle(string title) { _title = title; return this; }
    public TaskItemBuilder WithDescription(string? description) { _description = description; return this; }
    public TaskItemBuilder WithDueDate(DateOnly? dueDate) { _dueDate = dueDate; return this; }
    public TaskItemBuilder WithStatus(TaskItemStatus status) { _status = status; return this; }
    public TaskItemBuilder CreatedAt(DateTime createdAt) { _createdAt = createdAt; return this; }

    public TaskItem Build() => TaskItem.Create(_title, _description, _dueDate, _status, _createdAt);
}
