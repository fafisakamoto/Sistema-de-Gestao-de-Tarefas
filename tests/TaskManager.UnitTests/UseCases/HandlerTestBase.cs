using Moq;
using TaskManager.Application.Abstractions;
using TaskManager.Application.Tasks;
using TaskManager.Domain.Tasks;
using TaskManager.UnitTests.Fakes;

namespace TaskManager.UnitTests.UseCases;

public abstract class HandlerTestBase
{
    internal FixedTimeProvider Clock { get; } = new();
    internal Mock<ITaskRepository> Repository { get; } = new();
    internal Mock<IUnitOfWork> UnitOfWork { get; } = new();

    internal DateOnly Today => DateOnly.FromDateTime(Clock.GetUtcNow().UtcDateTime);

    internal void GivenTask(TaskItem task) =>
        Repository.Setup(r => r.GetByIdAsync(task.Id, It.IsAny<CancellationToken>())).ReturnsAsync(task);

    internal void GivenNoTask(Guid id) =>
        Repository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((TaskItem?)null);

    internal void VerifyCommitted() =>
        UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

    internal void VerifyNothingCommitted() =>
        UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
}
