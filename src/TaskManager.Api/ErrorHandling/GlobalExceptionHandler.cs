using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Domain.Common;

namespace TaskManager.Api.ErrorHandling;

internal sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    IHostEnvironment environment,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private const int ClientClosedRequest = 499;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var method = httpContext.Request.Method;
        var path = httpContext.Request.Path.ToString();

        if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
        {
            logger.RequestCancelled(method, path);
            httpContext.Response.StatusCode = ClientClosedRequest;
            return true;
        }

        int statusCode;
        string title;

        if (exception is DomainException)
        {
            logger.DomainRuleViolated(exception, method, path);
            statusCode = StatusCodes.Status400BadRequest;
            title = "Regra de negócio violada.";
        }
        else
        {
            logger.UnhandledException(exception, method, path);
            statusCode = StatusCodes.Status500InternalServerError;
            title = "Erro interno no servidor.";
        }

        httpContext.Response.StatusCode = statusCode;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = statusCode < 500 || environment.IsDevelopment() ? exception.Message : null
            }
        });
    }
}
