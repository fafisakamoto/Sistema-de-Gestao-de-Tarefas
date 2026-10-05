using FluentValidation;
using TaskManager.Application.Common.Results;

namespace TaskManager.Application.Common.Validation;

internal static class ValidatorExtensions
{
    public static async Task<Error?> ValidateToErrorAsync<T>(
        this IValidator<T> validator,
        T instance,
        CancellationToken cancellationToken)
    {
        var result = await validator.ValidateAsync(instance, cancellationToken);
        if (result.IsValid)
            return null;

        return Error.Validation(result.Errors
            .GroupBy(failure => failure.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(failure => failure.ErrorMessage).Distinct().ToArray()));
    }
}
