using TaskManager.Domain.Common;
using TaskManager.Domain.Tasks;
using TaskManager.UnitTests.Builders;

namespace TaskManager.UnitTests.Domain;

public sealed class TaskItemTests
{
    private static readonly DateTime Now = new(2026, 1, 10, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateOnly Today = DateOnly.FromDateTime(Now);

    [Fact]
    public void Create_WithValidData_InitializesAllFields()
    {
        var dueDate = Today.AddDays(3);

        var task = TaskItem.Create("Estudar EF Core", "Capítulo 4", dueDate, TaskItemStatus.InProgress, Now);

        Assert.NotEqual(Guid.Empty, task.Id);
        Assert.Equal("Estudar EF Core", task.Title);
        Assert.Equal("Capítulo 4", task.Description);
        Assert.Equal(dueDate, task.DueDate);
        Assert.Equal(TaskItemStatus.InProgress, task.Status);
        Assert.Equal(Now, task.CreatedAt);
        Assert.Null(task.UpdatedAt);
    }

    [Fact]
    public void Create_GeneratesUniqueIds()
    {
        var first = new TaskItemBuilder().Build();
        var second = new TaskItemBuilder().Build();

        Assert.NotEqual(first.Id, second.Id);
    }

    [Fact]
    public void Create_NormalizesTitleAndBlankDescription()
    {
        var task = new TaskItemBuilder().WithTitle("  Título  ").WithDescription("   ").Build();

        Assert.Equal("Título", task.Title);
        Assert.Null(task.Description);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithoutTitle_ThrowsDomainException(string? title)
    {
        Assert.Throws<DomainException>(() => new TaskItemBuilder().WithTitle(title!).Build());
    }

    [Fact]
    public void Create_WithTitleTooLong_ThrowsDomainException()
    {
        var title = new string('a', TaskItem.TitleMaxLength + 1);

        Assert.Throws<DomainException>(() => new TaskItemBuilder().WithTitle(title).Build());
    }

    [Fact]
    public void Create_WithDescriptionTooLong_ThrowsDomainException()
    {
        var description = new string('a', TaskItem.DescriptionMaxLength + 1);

        Assert.Throws<DomainException>(() => new TaskItemBuilder().WithDescription(description).Build());
    }

    [Fact]
    public void Create_WithUndefinedStatus_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => new TaskItemBuilder().WithStatus((TaskItemStatus)99).Build());
    }

    [Fact]
    public void Update_ChangesFieldsAndSetsUpdatedAt_PreservingIdAndCreatedAt()
    {
        var task = new TaskItemBuilder().CreatedAt(Now).Build();
        var originalId = task.Id;
        var later = Now.AddHours(2);

        task.Update("Novo título", null, Today.AddDays(10), TaskItemStatus.Completed, later);

        Assert.Equal(originalId, task.Id);
        Assert.Equal(Now, task.CreatedAt);
        Assert.Equal("Novo título", task.Title);
        Assert.Null(task.Description);
        Assert.Equal(Today.AddDays(10), task.DueDate);
        Assert.Equal(TaskItemStatus.Completed, task.Status);
        Assert.Equal(later, task.UpdatedAt);
    }

    [Fact]
    public void Update_WithInvalidData_DoesNotChangeAnything()
    {
        var task = new TaskItemBuilder().WithTitle("Original").WithStatus(TaskItemStatus.Pending).Build();

        Assert.Throws<DomainException>(() =>
            task.Update(" ", "Outra descrição", null, TaskItemStatus.Completed, Now));

        Assert.Equal("Original", task.Title);
        Assert.Equal("Descrição de teste", task.Description);
        Assert.Equal(TaskItemStatus.Pending, task.Status);
        Assert.Null(task.UpdatedAt);
    }

    [Theory]
    [InlineData(-1, TaskItemStatus.Pending, true)]
    [InlineData(-1, TaskItemStatus.InProgress, true)]
    [InlineData(-1, TaskItemStatus.Completed, false)]
    [InlineData(0, TaskItemStatus.Pending, false)]
    [InlineData(1, TaskItemStatus.Pending, false)]
    public void IsOverdue_DependsOnDueDateAndStatus(int daysFromToday, TaskItemStatus status, bool expected)
    {
        var task = new TaskItemBuilder().WithDueDate(Today.AddDays(daysFromToday)).WithStatus(status).Build();

        Assert.Equal(expected, task.IsOverdue(Today));
    }

    [Fact]
    public void IsOverdue_WithoutDueDate_IsFalse()
    {
        var task = new TaskItemBuilder().WithDueDate(null).Build();

        Assert.False(task.IsOverdue(Today));
    }
}
