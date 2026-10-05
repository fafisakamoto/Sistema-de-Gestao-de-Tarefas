using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Common.Pagination;
using TaskManager.Application.Common.Querying;
using TaskManager.Application.Tasks.List;
using TaskManager.Domain.Tasks;
using TaskManager.Infrastructure.Persistence;
using TaskManager.Infrastructure.Persistence.Repositories;

namespace TaskManager.IntegrationTests.Persistence;

public sealed class TaskRepositoryTests : IDisposable
{
    private static readonly DateTime Now = new(2026, 1, 10, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateOnly Today = DateOnly.FromDateTime(Now);

    private readonly AppDbContext _context;
    private readonly TaskRepository _repository;

    public TaskRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"RepositoryTests-{Guid.NewGuid():N}")
            .Options;

        _context = new AppDbContext(options);
        _repository = new TaskRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    private async Task<TaskItem> SeedAsync(
        string title,
        TaskItemStatus status = TaskItemStatus.Pending,
        DateOnly? dueDate = null,
        string? description = null,
        int createdOffsetMinutes = 0)
    {
        var task = TaskItem.Create(title, description, dueDate, status, Now.AddMinutes(createdOffsetMinutes));
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
        return task;
    }

    private static TaskListCriteria Criteria(
        TaskItemStatus? status = null,
        DateOnly? from = null,
        DateOnly? to = null,
        string? search = null,
        TaskSortField sortBy = TaskSortField.DueDate,
        SortDirection direction = SortDirection.Asc,
        int page = 1,
        int pageSize = 20) =>
        new(status, from, to, search, sortBy, direction, new PageRequest(page, pageSize));

    private async Task<Guid[]> ListIdsAsync(TaskListCriteria criteria) =>
        (await _repository.ListAsync(criteria)).Items.Select(t => t.Id).ToArray();

    [Fact]
    public async Task DefaultSort_ByDueDateAscending_WithUndatedLast()
    {
        var undated = await SeedAsync("Sem data");
        var later = await SeedAsync("Depois", dueDate: Today.AddDays(5));
        var sooner = await SeedAsync("Antes", dueDate: Today.AddDays(1));

        Assert.Equal(new[] { sooner.Id, later.Id, undated.Id }, await ListIdsAsync(Criteria()));
    }

    [Fact]
    public async Task DueDateDescending_StillKeepsUndatedLast()
    {
        var undated = await SeedAsync("Sem data");
        var sooner = await SeedAsync("Antes", dueDate: Today.AddDays(1));
        var later = await SeedAsync("Depois", dueDate: Today.AddDays(5));

        Assert.Equal(new[] { later.Id, sooner.Id, undated.Id },
            await ListIdsAsync(Criteria(direction: SortDirection.Desc)));
    }

    [Fact]
    public async Task SortByCreatedAtDescending_ReturnsNewestFirst()
    {
        var oldest = await SeedAsync("Antiga", createdOffsetMinutes: 0);
        var newest = await SeedAsync("Nova", createdOffsetMinutes: 20);
        var middle = await SeedAsync("Meio", createdOffsetMinutes: 10);

        Assert.Equal(new[] { newest.Id, middle.Id, oldest.Id },
            await ListIdsAsync(Criteria(sortBy: TaskSortField.CreatedAt, direction: SortDirection.Desc)));
    }

    [Fact]
    public async Task SortByTitleAscending_IsAlphabetical()
    {
        var c = await SeedAsync("C");
        var a = await SeedAsync("A");
        var b = await SeedAsync("B");

        Assert.Equal(new[] { a.Id, b.Id, c.Id }, await ListIdsAsync(Criteria(sortBy: TaskSortField.Title)));
    }

    [Fact]
    public async Task Ties_AreBrokenByCreationTime()
    {
        var second = await SeedAsync("Segunda", dueDate: Today, createdOffsetMinutes: 10);
        var first = await SeedAsync("Primeira", dueDate: Today, createdOffsetMinutes: 0);

        Assert.Equal(new[] { first.Id, second.Id }, await ListIdsAsync(Criteria()));
    }

    [Fact]
    public async Task FiltersByStatus()
    {
        await SeedAsync("Pendente", TaskItemStatus.Pending);
        var inProgress = await SeedAsync("Em progresso", TaskItemStatus.InProgress);

        Assert.Equal(new[] { inProgress.Id }, await ListIdsAsync(Criteria(status: TaskItemStatus.InProgress)));
    }

    [Fact]
    public async Task DateRange_IsInclusiveOnBothEnds_AndExcludesUndated()
    {
        await SeedAsync("Antes", dueDate: Today.AddDays(-1));
        var start = await SeedAsync("Início", dueDate: Today);
        var end = await SeedAsync("Fim", dueDate: Today.AddDays(2));
        await SeedAsync("Depois", dueDate: Today.AddDays(3));
        await SeedAsync("Sem data");

        Assert.Equal(new[] { start.Id, end.Id }, await ListIdsAsync(Criteria(from: Today, to: Today.AddDays(2))));
    }

    [Fact]
    public async Task Search_MatchesTitleOrDescription_CaseInsensitive()
    {
        var byTitle = await SeedAsync("Relatório mensal", dueDate: Today);
        var byDescription = await SeedAsync("Outra tarefa", dueDate: Today.AddDays(1), description: "anexar RELATÓRIO");
        await SeedAsync("Sem relação");

        Assert.Equal(new[] { byTitle.Id, byDescription.Id }, await ListIdsAsync(Criteria(search: "relatório")));
    }

    [Fact]
    public async Task Paginates_AndReportsTotals()
    {
        for (var i = 0; i < 5; i++)
            await SeedAsync($"Tarefa {i}", dueDate: Today.AddDays(i));

        var result = await _repository.ListAsync(Criteria(page: 3, pageSize: 2));

        Assert.Equal("Tarefa 4", Assert.Single(result.Items).Title);
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
        Assert.False(result.HasNextPage);
        Assert.True(result.HasPreviousPage);
    }

    [Fact]
    public async Task PageBeyondLast_ReturnsEmptyWithTotals()
    {
        await SeedAsync("Única");

        var result = await _repository.ListAsync(Criteria(page: 5, pageSize: 10));

        Assert.Empty(result.Items);
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task List_DoesNotTrackEntities()
    {
        await SeedAsync("Qualquer");
        _context.ChangeTracker.Clear();

        await _repository.ListAsync(Criteria());

        Assert.Empty(_context.ChangeTracker.Entries());
    }
}
