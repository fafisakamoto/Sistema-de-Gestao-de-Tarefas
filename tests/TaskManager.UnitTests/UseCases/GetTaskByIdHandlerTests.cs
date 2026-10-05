using TaskManager.Application.Common.Results;
using TaskManager.Application.Tasks.GetById;
using TaskManager.UnitTests.Builders;

namespace TaskManager.UnitTests.UseCases;

public sealed class GetTaskByIdHandlerTests : HandlerTestBase
{
    private readonly GetTaskByIdHandler _sut;

    public GetTaskByIdHandlerTests() => _sut = new GetTaskByIdHandler(Repository.Object, Clock);

    [Fact]
    public async Task ExistingTask_ReturnsMappedResponse()
    {
        var task = new TaskItemBuilder().WithTitle("Existente").Build();
        GivenTask(task);

        var result = await _sut.HandleAsync(task.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(task.Id, result.Value.Id);
        Assert.Equal("Existente", result.Value.Title);
        Assert.Equal(task.Description, result.Value.Description);
        Assert.Equal(task.DueDate, result.Value.DueDate);
    }

    [Fact]
    public async Task OverdueTask_IsFlaggedAsOverdue()
    {
        var task = new TaskItemBuilder().WithDueDate(Today.AddDays(-2)).Build();
        GivenTask(task);

        var result = await _sut.HandleAsync(task.Id);

        Assert.True(result.Value.IsOverdue);
    }

    [Fact]
    public async Task MissingTask_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        GivenNoTask(id);

        var result = await _sut.HandleAsync(id);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal("Task.NotFound", result.Error.Code);
    }
}
