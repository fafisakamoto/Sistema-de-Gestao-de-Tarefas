namespace TaskManager.Api.ErrorHandling;

internal static partial class ApiLog
{
    [LoggerMessage(EventId = 5000, Level = LogLevel.Error, Message = "Erro não tratado em {Method} {Path}")]
    public static partial void UnhandledException(this ILogger logger, Exception exception, string method, string path);

    [LoggerMessage(EventId = 5001, Level = LogLevel.Warning, Message = "Regra de domínio violada em {Method} {Path}")]
    public static partial void DomainRuleViolated(this ILogger logger, Exception exception, string method, string path);

    [LoggerMessage(EventId = 5002, Level = LogLevel.Information, Message = "Requisição {Method} {Path} cancelada pelo cliente")]
    public static partial void RequestCancelled(this ILogger logger, string method, string path);
}
