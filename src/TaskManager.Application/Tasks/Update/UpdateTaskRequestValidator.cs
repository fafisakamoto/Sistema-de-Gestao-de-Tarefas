using FluentValidation;

namespace TaskManager.Application.Tasks.Update;

public sealed class UpdateTaskRequestValidator : AbstractValidator<UpdateTaskRequest>
{
    public UpdateTaskRequestValidator()
    {
        RuleFor(x => x.Title).ValidTitle();
        RuleFor(x => x.Description).ValidDescription();
        RuleFor(x => x.Status).ValidStatus();
    }
}
