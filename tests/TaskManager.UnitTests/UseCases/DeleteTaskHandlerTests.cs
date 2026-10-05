using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TaskManager.Application.Common.Results;
using TaskManager.Application.Tasks.Delete;
using TaskManager.Domain.Tasks;
using TaskManager.UnitTests.Builders;

namespace TaskManager.UnitTests.UseCases;

public sealed class DeleteTaskHandlerTests : HandlerTestBase
{
    private readonly DeleteTaskHandler _sut;

    public DeleteTaskHandlerTests() =>
        _sut = new DeleteTaskHandler(Repository.Object, UnitOfWork.Object, NullLogger<DeleteTaskHandler>.Instance);

    [Fact]
    public async Task ExistingTask_RemovesAndCommits()
    {
        var task = new TaskItemBuilder().Build();
        GivenTask(task);

        var result = await _sut.HandleAsync(task.Id);

        Assert.True(result.IsSuccess);
        Repository.Verify(r => r.Remove(task), Times.Once);
        VerifyCommitted();
    }

    [Fact]
    public async Task MissingTask_ReturnsNotFoundAndCommitsNothing()
    {
        var id = Guid.NewGuid();
        GivenNoTask(id);

        var result = await _sut.HandleAsync(id);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Repository.Verify(r => r.Remove(It.IsAny<TaskItem>()), Times.Never);
        VerifyNothingCommitted();
    }
}
