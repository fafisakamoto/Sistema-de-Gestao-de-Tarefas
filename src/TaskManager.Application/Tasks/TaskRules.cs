using FluentValidation;
using TaskManager.Domain.Tasks;

namespace TaskManager.Application.Tasks;

internal static class TaskRules
{
    public static IRuleBuilderOptions<T, string> ValidTitle<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().WithMessage("O título é obrigatório.")
            .MaximumLength(TaskItem.TitleMaxLength)
            .WithMessage($"O título deve ter no máximo {TaskItem.TitleMaxLength} caracteres.");

    public static IRuleBuilderOptions<T, string?> ValidDescription<T>(this IRuleBuilder<T, string?> rule) =>
        rule.MaximumLength(TaskItem.DescriptionMaxLength)
            .WithMessage($"A descrição deve ter no máximo {TaskItem.DescriptionMaxLength} caracteres.");

    public static IRuleBuilderOptions<T, TStatus> ValidStatus<T, TStatus>(this IRuleBuilder<T, TStatus> rule) =>
        rule.IsInEnum().WithMessage("Status inválido. Valores aceitos: Pending, InProgress, Completed.");
}
