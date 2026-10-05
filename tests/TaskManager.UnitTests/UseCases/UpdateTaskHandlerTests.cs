using Microsoft.Extensions.Logging.Abstractions;
using TaskManager.Application.Common.Results;
using TaskManager.Application.Tasks.Update;
using TaskManager.Domain.Tasks;
using TaskManager.UnitTests.Builders;

namespace TaskManager.UnitTests.UseCases;

public sealed class UpdateTaskHandlerTests : HandlerTestBase
{
    private readonly UpdateTaskHandler _sut;

    public UpdateTaskHandlerTests() =>
        _sut = new UpdateTaskHandler(
            Repository.Object,
            UnitOfWork.Object,
            new UpdateTaskRequestValidator(),
            Clock,
            NullLogger<UpdateTaskHandler>.Instance);

    [Fact]
    public async Task ExistingTask_ReplacesFieldsAndCommits()
    {
        var task = new TaskItemBuilder().Build();
        GivenTask(task);

        var result = await _sut.HandleAsync(task.Id, new UpdateTaskRequest
        {
            Title = "Título editado",
            Description = null,
            DueDate = Today.AddDays(10),
            Status = TaskItemStatus.Completed
        });

        Assert.True(result.IsSuccess);
        Assert.Equal("Título editado", result.Value.Title);
        Assert.Null(result.Value.Description);
        Assert.Equal(Today.AddDays(10), result.Value.DueDate);
        Assert.Equal(TaskItemStatus.Completed, result.Value.Status);
        Assert.Equal(Clock.GetUtcNow().UtcDateTime, result.Value.UpdatedAt);
        VerifyCommitted();
    }

    [Fact]
    public async Task MissingTask_ReturnsNotFoundAndCommitsNothing()
    {
        var id = Guid.NewGuid();
        GivenNoTask(id);

        var result = await _sut.HandleAsync(id, new UpdateTaskRequest { Title = "Qualquer" });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        VerifyNothingCommitted();
    }

    [Fact]
    public async Task InvalidRequest_FailsFastWithoutTouchingRepository()
    {
        var result = await _sut.HandleAsync(Guid.NewGuid(), new UpdateTaskRequest { Title = "" });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Repository.VerifyNoOtherCalls();
        VerifyNothingCommitted();
    }
}
