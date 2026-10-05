using FluentValidation.TestHelper;
using TaskManager.Application.Tasks.Create;
using TaskManager.Domain.Tasks;
using TaskManager.UnitTests.Fakes;

namespace TaskManager.UnitTests.Validators;

public sealed class CreateTaskRequestValidatorTests
{
    private static readonly DateOnly Today = DateOnly.FromDateTime(FixedTimeProvider.DefaultNow.UtcDateTime);
    private readonly CreateTaskRequestValidator _validator = new(new FixedTimeProvider());

    private static CreateTaskRequest ValidRequest() => new()
    {
        Title = "Tarefa válida",
        Description = "Descrição",
        DueDate = Today.AddDays(1),
        Status = TaskItemStatus.Pending
    };

    [Fact]
    public void FullValidRequest_HasNoErrors() =>
        _validator.TestValidate(ValidRequest()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void OnlyTitle_IsEnough() =>
        _validator.TestValidate(new CreateTaskRequest { Title = "Só o título" }).ShouldNotHaveAnyValidationErrors();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void MissingTitle_IsInvalid(string? title) =>
        _validator.TestValidate(ValidRequest() with { Title = title! })
            .ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("O título é obrigatório.");

    [Fact]
    public void TitleAtMaxLength_IsValid() =>
        _validator.TestValidate(ValidRequest() with { Title = new string('a', TaskItem.TitleMaxLength) })
            .ShouldNotHaveValidationErrorFor(x => x.Title);

    [Fact]
    public void TitleAboveMaxLength_IsInvalid() =>
        _validator.TestValidate(ValidRequest() with { Title = new string('a', TaskItem.TitleMaxLength + 1) })
            .ShouldHaveValidationErrorFor(x => x.Title);

    [Fact]
    public void DescriptionAboveMaxLength_IsInvalid() =>
        _validator.TestValidate(ValidRequest() with { Description = new string('a', TaskItem.DescriptionMaxLength + 1) })
            .ShouldHaveValidationErrorFor(x => x.Description);

    [Fact]
    public void DueDateYesterday_IsInvalid() =>
        _validator.TestValidate(ValidRequest() with { DueDate = Today.AddDays(-1) })
            .ShouldHaveValidationErrorFor(x => x.DueDate);

    [Fact]
    public void DueDateToday_IsValid() =>
        _validator.TestValidate(ValidRequest() with { DueDate = Today })
            .ShouldNotHaveValidationErrorFor(x => x.DueDate);

    [Fact]
    public void UndefinedStatus_IsInvalid() =>
        _validator.TestValidate(ValidRequest() with { Status = (TaskItemStatus)42 })
            .ShouldHaveValidationErrorFor(x => x.Status);

    [Fact]
    public void Today_IsEvaluatedInBusinessTimeZone()
    {
        var utcNow = new DateTimeOffset(2026, 1, 11, 1, 0, 0, TimeSpan.Zero);
        var saoPaulo = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
        var validator = new CreateTaskRequestValidator(new FixedTimeProvider(utcNow, saoPaulo));

        var result = validator.TestValidate(new CreateTaskRequest { Title = "Hoje local", DueDate = new DateOnly(2026, 1, 10) });

        result.ShouldNotHaveValidationErrorFor(x => x.DueDate);
    }
}
