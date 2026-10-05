using System.Net;

namespace TaskManager.IntegrationTests.Api;

public sealed class CrossCuttingTests(TaskManagerApiFactory factory) : IClassFixture<TaskManagerApiFactory>
{
    private const string Header = "X-Correlation-Id";
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task EveryResponse_ContainsGeneratedCorrelationId()
    {
        var response = await _client.GetAsync("/api/tasks");

        Assert.True(response.Headers.TryGetValues(Header, out var values));
        Assert.False(string.IsNullOrWhiteSpace(values.Single()));
    }

    [Fact]
    public async Task ValidIncomingCorrelationId_IsPropagated()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/tasks");
        request.Headers.Add(Header, "pedido-123");

        var response = await _client.SendAsync(request);

        Assert.Equal("pedido-123", response.Headers.GetValues(Header).Single());
    }

    [Theory]
    [InlineData("has spaces and symbols!")]
    [InlineData("<script>alert(1)</script>")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    public async Task InvalidCorrelationId_IsReplacedByGeneratedOne(string incoming)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/tasks");
        request.Headers.TryAddWithoutValidation(Header, incoming);

        var response = await _client.SendAsync(request);

        var returned = response.Headers.GetValues(Header).Single();
        Assert.NotEqual(incoming, returned);
        Assert.Matches("^[a-f0-9]{32}$", returned);
    }

    [Fact]
    public async Task HealthCheck_ReturnsHealthy()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task SwaggerDocument_IsGeneratedWithTaskEndpoints()
    {
        var response = await _client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var document = await response.Content.ReadAsStringAsync();
        Assert.Contains("/api/tasks/{id}", document);
    }
}
