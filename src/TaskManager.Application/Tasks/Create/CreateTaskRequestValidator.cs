using FluentValidation;
using TaskManager.Application.Common.Time;

namespace TaskManager.Application.Tasks.Create;

public sealed class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>
{
    public CreateTaskRequestValidator(TimeProvider timeProvider)
    {
        RuleFor(x => x.Title).ValidTitle();
        RuleFor(x => x.Description).ValidDescription();
        RuleFor(x => x.Status).ValidStatus();

        RuleFor(x => x.DueDate)
            .Must(dueDate => dueDate >= timeProvider.GetToday())
            .When(x => x.DueDate.HasValue)
            .WithMessage("A data de vencimento não pode ser anterior à data atual.");
    }
}
