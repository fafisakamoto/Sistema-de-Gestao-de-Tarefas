using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.Common.Pagination;
using TaskManager.Application.Tasks;
using TaskManager.Domain.Tasks;

namespace TaskManager.IntegrationTests.Api;

public sealed class TasksApiTests(TaskManagerApiFactory factory) : IClassFixture<TaskManagerApiFactory>
{
    private const string Route = "/api/tasks";

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    private readonly HttpClient _client = factory.CreateClient();

    private async Task<TaskResponse> CreateAsync(string title, string status = "Pending", DateOnly? dueDate = null)
    {
        var response = await _client.PostAsJsonAsync(Route, new { title, status, dueDate }, Json);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TaskResponse>(Json))!;
    }

    private static string UniqueToken() => Guid.NewGuid().ToString("N");

    [Fact]
    public async Task Post_ValidPayload_Returns201WithLocationAndId()
    {
        var response = await _client.PostAsJsonAsync(Route, new
        {
            title = "Nova tarefa",
            description = "Criada pelo teste",
            dueDate = Today.AddDays(7),
            status = "InProgress"
        }, Json);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var task = await response.Content.ReadFromJsonAsync<TaskResponse>(Json);
        Assert.NotNull(task);
        Assert.NotEqual(Guid.Empty, task.Id);
        Assert.Equal(TaskItemStatus.InProgress, task.Status);
        Assert.Equal(Today.AddDays(7), task.DueDate);
        Assert.EndsWith($"{Route}/{task.Id}", response.Headers.Location!.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Post_InvalidPayload_Returns400ProblemDetailsWithFieldErrors()
    {
        var response = await _client.PostAsJsonAsync(Route, new { title = "", dueDate = Today.AddDays(-1) }, Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(Json);
        Assert.NotNull(problem);
        Assert.Contains("Title", problem.Errors.Keys);
        Assert.Contains("DueDate", problem.Errors.Keys);
    }

    [Theory]
    [InlineData("\"Inexistente\"")]
    [InlineData("99")]
    public async Task Post_InvalidStatus_Returns400(string rawStatus)
    {
        using var content = new StringContent($$"""{ "title": "X", "status": {{rawStatus}} }""", System.Text.Encoding.UTF8, "application/json");

        var response = await _client.PostAsync(Route, content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetById_Existing_Returns200()
    {
        var created = await CreateAsync("Buscar por id");

        var task = await _client.GetFromJsonAsync<TaskResponse>($"{Route}/{created.Id}", Json);

        Assert.Equal(created.Id, task!.Id);
        Assert.Equal("Buscar por id", task.Title);
    }

    [Fact]
    public async Task GetById_Missing_Returns404ProblemDetailsWithErrorCode()
    {
        var response = await _client.GetAsync($"{Route}/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(Json);
        Assert.Equal("Task.NotFound", problem!.Extensions["errorCode"]?.ToString());
    }

    [Fact]
    public async Task GetById_InvalidGuid_Returns404()
    {
        var response = await _client.GetAsync($"{Route}/nao-e-um-guid");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task List_FiltersByStatusAndSearch()
    {
        var token = UniqueToken();
        await CreateAsync($"{token} pendente", "Pending");
        var completed = await CreateAsync($"{token} concluída", "Completed");

        var page = await _client.GetFromJsonAsync<PagedResult<TaskResponse>>(
            $"{Route}?status=Completed&search={token}", Json);

        var single = Assert.Single(page!.Items);
        Assert.Equal(completed.Id, single.Id);
    }

    [Fact]
    public async Task List_FiltersByDueDateRange_ExcludingTasksWithoutDate()
    {
        var token = UniqueToken();
        var start = Today.AddDays(30);
        var inside = await CreateAsync($"{token} dentro", dueDate: start);
        await CreateAsync($"{token} fora", dueDate: start.AddDays(10));
        await CreateAsync($"{token} sem data");

        var page = await _client.GetFromJsonAsync<PagedResult<TaskResponse>>(
            $"{Route}?dueDateFrom={start:yyyy-MM-dd}&dueDateTo={start.AddDays(1):yyyy-MM-dd}&search={token}", Json);

        var single = Assert.Single(page!.Items);
        Assert.Equal(inside.Id, single.Id);
    }

    [Fact]
    public async Task List_PaginatesResults()
    {
        var token = UniqueToken();
        for (var i = 0; i < 5; i++)
            await CreateAsync($"{token} item {i}", dueDate: Today.AddDays(i));

        var page = await _client.GetFromJsonAsync<PagedResult<TaskResponse>>(
            $"{Route}?search={token}&page=2&pageSize=2", Json);

        Assert.NotNull(page);
        Assert.Equal(2, page.Items.Count);
        Assert.Equal(5, page.TotalCount);
        Assert.Equal(3, page.TotalPages);
        Assert.True(page.HasNextPage);
        Assert.True(page.HasPreviousPage);
        Assert.Equal($"{token} item 2", page.Items[0].Title);
    }

    [Fact]
    public async Task List_SortsByRequestedFieldAndDirection()
    {
        var token = UniqueToken();
        var b = await CreateAsync($"{token} B");
        var c = await CreateAsync($"{token} C");
        var a = await CreateAsync($"{token} A");

        var page = await _client.GetFromJsonAsync<PagedResult<TaskResponse>>(
            $"{Route}?search={token}&sortBy=Title&sortDirection=Desc", Json);

        Assert.Equal(new[] { c.Id, b.Id, a.Id }, page!.Items.Select(t => t.Id));
    }

    [Fact]
    public async Task List_InvalidSortField_Returns400()
    {
        var response = await _client.GetAsync($"{Route}?sortBy=Inexistente");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task List_InvalidRange_Returns400()
    {
        var response = await _client.GetAsync($"{Route}?dueDateFrom=2026-12-31&dueDateTo=2026-01-01");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Put_Existing_UpdatesAndReturns200()
    {
        var created = await CreateAsync("Antes");

        var response = await _client.PutAsJsonAsync($"{Route}/{created.Id}",
            new { title = "Depois", description = "Editada", status = "Completed" }, Json);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var fetched = await _client.GetFromJsonAsync<TaskResponse>($"{Route}/{created.Id}", Json);
        Assert.Equal("Depois", fetched!.Title);
        Assert.Equal("Editada", fetched.Description);
        Assert.Equal(TaskItemStatus.Completed, fetched.Status);
        Assert.NotNull(fetched.UpdatedAt);
    }

    [Fact]
    public async Task Put_Missing_Returns404()
    {
        var response = await _client.PutAsJsonAsync($"{Route}/{Guid.NewGuid()}",
            new { title = "Qualquer", status = "Pending" }, Json);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Existing_Returns204AndTaskIsGone()
    {
        var created = await CreateAsync("Para excluir");

        var response = await _client.DeleteAsync($"{Route}/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"{Route}/{created.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.DeleteAsync($"{Route}/{created.Id}")).StatusCode);
    }
}
