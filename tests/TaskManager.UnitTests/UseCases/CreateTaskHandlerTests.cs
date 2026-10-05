using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TaskManager.Application.Common.Results;
using TaskManager.Application.Tasks.Create;
using TaskManager.Domain.Tasks;

namespace TaskManager.UnitTests.UseCases;

public sealed class CreateTaskHandlerTests : HandlerTestBase
{
    private readonly CreateTaskHandler _sut;

    public CreateTaskHandlerTests() =>
        _sut = new CreateTaskHandler(
            Repository.Object,
            UnitOfWork.Object,
            new CreateTaskRequestValidator(Clock),
            Clock,
            NullLogger<CreateTaskHandler>.Instance);

    [Fact]
    public async Task ValidRequest_AddsCommitsAndReturnsTask()
    {
        var request = new CreateTaskRequest
        {
            Title = "Estudar .NET",
            Description = "EF Core",
            DueDate = Today.AddDays(3),
            Status = TaskItemStatus.InProgress
        };

        var result = await _sut.HandleAsync(request);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
        Assert.Equal(request.Title, result.Value.Title);
        Assert.Equal(request.DueDate, result.Value.DueDate);
        Assert.Equal(TaskItemStatus.InProgress, result.Value.Status);
        Assert.Equal(Clock.GetUtcNow().UtcDateTime, result.Value.CreatedAt);
        Assert.False(result.Value.IsOverdue);
        Repository.Verify(r => r.Add(It.Is<TaskItem>(t => t.Id == result.Value.Id)), Times.Once);
        VerifyCommitted();
    }

    [Fact]
    public async Task WithoutStatus_DefaultsToPending()
    {
        var result = await _sut.HandleAsync(new CreateTaskRequest { Title = "Sem status" });

        Assert.Equal(TaskItemStatus.Pending, result.Value.Status);
    }

    [Fact]
    public async Task InvalidRequest_ReturnsErrorsPerFieldAndPersistsNothing()
    {
        var result = await _sut.HandleAsync(new CreateTaskRequest { Title = "", DueDate = Today.AddDays(-1) });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Contains(nameof(CreateTaskRequest.Title), result.Error.ValidationErrors.Keys);
        Assert.Contains(nameof(CreateTaskRequest.DueDate), result.Error.ValidationErrors.Keys);
        Repository.Verify(r => r.Add(It.IsAny<TaskItem>()), Times.Never);
        VerifyNothingCommitted();
    }

    [Fact]
    public async Task ForwardsCancellationToken()
    {
        using var cts = new CancellationTokenSource();

        await _sut.HandleAsync(new CreateTaskRequest { Title = "Com token" }, cts.Token);

        UnitOfWork.Verify(u => u.SaveChangesAsync(cts.Token), Times.Once);
    }
}
