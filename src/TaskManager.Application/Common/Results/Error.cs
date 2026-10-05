namespace TaskManager.Application.Common.Results;

public enum ErrorType
{
    Validation = 1,
    NotFound = 2
}

public sealed record Error(string Code, string Message, ErrorType Type)
{
    public IReadOnlyDictionary<string, string[]> ValidationErrors { get; init; } =
        new Dictionary<string, string[]>();

    public static Error NotFound(string code, string message) => new(code, message, ErrorType.NotFound);

    public static Error Validation(IReadOnlyDictionary<string, string[]> errors) =>
        new("Validation.Failed", "Um ou mais erros de validação ocorreram.", ErrorType.Validation)
        {
            ValidationErrors = errors
        };
}
