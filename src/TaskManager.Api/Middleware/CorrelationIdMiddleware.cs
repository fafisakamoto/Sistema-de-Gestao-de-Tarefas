namespace TaskManager.Api.Middleware;

internal sealed class CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
{
    public const string HeaderName = "X-Correlation-Id";
    private const int MaxLength = 64;

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = ResolveCorrelationId(context.Request.Headers[HeaderName].ToString());
        context.Response.Headers[HeaderName] = correlationId;

        using (logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
        {
            await next(context);
        }
    }

    private static string ResolveCorrelationId(string incoming) =>
        !string.IsNullOrWhiteSpace(incoming)
        && incoming.Length <= MaxLength
        && incoming.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_')
            ? incoming
            : Guid.NewGuid().ToString("N");
}
