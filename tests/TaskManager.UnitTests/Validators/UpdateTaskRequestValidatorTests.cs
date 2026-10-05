using FluentValidation.TestHelper;
using TaskManager.Application.Tasks.Update;
using TaskManager.Domain.Tasks;

namespace TaskManager.UnitTests.Validators;

public sealed class UpdateTaskRequestValidatorTests
{
    private readonly UpdateTaskRequestValidator _validator = new();

    [Fact]
    public void PastDueDate_IsAllowed() =>
        _validator.TestValidate(new UpdateTaskRequest
        {
            Title = "Tarefa atrasada",
            DueDate = new DateOnly(2000, 1, 1),
            Status = TaskItemStatus.Completed
        }).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void EmptyTitle_IsInvalid() =>
        _validator.TestValidate(new UpdateTaskRequest { Title = "" })
            .ShouldHaveValidationErrorFor(x => x.Title);

    [Fact]
    public void UndefinedStatus_IsInvalid() =>
        _validator.TestValidate(new UpdateTaskRequest { Title = "Ok", Status = (TaskItemStatus)7 })
            .ShouldHaveValidationErrorFor(x => x.Status);
}
