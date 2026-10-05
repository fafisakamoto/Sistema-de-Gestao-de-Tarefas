using Moq;
using TaskManager.Application.Common.Pagination;
using TaskManager.Application.Common.Querying;
using TaskManager.Application.Common.Results;
using TaskManager.Application.Tasks.List;
using TaskManager.Domain.Tasks;
using TaskManager.UnitTests.Builders;

namespace TaskManager.UnitTests.UseCases;

public sealed class ListTasksHandlerTests : HandlerTestBase
{
    private readonly ListTasksHandler _sut;
    private TaskListCriteria? _captured;

    public ListTasksHandlerTests() => _sut = new ListTasksHandler(Repository.Object, new ListTasksQueryValidator(), Clock);

    private void GivenPage(PagedResult<TaskItem> page) =>
        Repository
            .Setup(r => r.ListAsync(It.IsAny<TaskListCriteria>(), It.IsAny<CancellationToken>()))
            .Callback<TaskListCriteria, CancellationToken>((criteria, _) => _captured = criteria)
            .ReturnsAsync(page);

    [Fact]
    public async Task BuildsNormalizedCriteriaAndMapsPage()
    {
        var tasks = new[] { new TaskItemBuilder().Build(), new TaskItemBuilder().Build() };
        GivenPage(new PagedResult<TaskItem>(tasks, 2, 10, 12));

        var result = await _sut.HandleAsync(new ListTasksQuery
        {
            Status = TaskItemStatus.Pending,
            DueDateFrom = Today,
            DueDateTo = Today.AddDays(30),
            Search = "  relatório  ",
            SortBy = TaskSortField.Title,
            SortDirection = SortDirection.Desc,
            Page = 2,
            PageSize = 10
        });

        Assert.True(result.IsSuccess);
        Assert.Equal(
            new TaskListCriteria(TaskItemStatus.Pending, Today, Today.AddDays(30), "relatório",
                TaskSortField.Title, SortDirection.Desc, new PageRequest(2, 10)),
            _captured);
        Assert.Equal(tasks.Select(t => t.Id), result.Value.Items.Select(i => i.Id));
        Assert.Equal(12, result.Value.TotalCount);
        Assert.Equal(2, result.Value.TotalPages);
    }

    [Fact]
    public async Task DefaultQuery_UsesDefaultSortingAndPaging()
    {
        GivenPage(new PagedResult<TaskItem>(Array.Empty<TaskItem>(), 1, PagedQuery.DefaultPageSize, 0));

        await _sut.HandleAsync(new ListTasksQuery());

        Assert.NotNull(_captured);
        Assert.Null(_captured.Search);
        Assert.Equal(TaskSortField.DueDate, _captured.SortBy);
        Assert.Equal(SortDirection.Asc, _captured.SortDirection);
        Assert.Equal(new PageRequest(1, PagedQuery.DefaultPageSize), _captured.Paging);
    }

    [Fact]
    public async Task InvalidQuery_ReturnsValidationErrorWithoutQuerying()
    {
        var result = await _sut.HandleAsync(new ListTasksQuery
        {
            DueDateFrom = new DateOnly(2026, 3, 1),
            DueDateTo = new DateOnly(2026, 2, 1)
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Repository.Verify(r => r.ListAsync(It.IsAny<TaskListCriteria>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
