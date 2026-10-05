using FluentValidation.TestHelper;
using TaskManager.Application.Common.Pagination;
using TaskManager.Application.Common.Querying;
using TaskManager.Application.Tasks.List;
using TaskManager.Domain.Tasks;

namespace TaskManager.UnitTests.Validators;

public sealed class ListTasksQueryValidatorTests
{
    private readonly ListTasksQueryValidator _validator = new();

    [Fact]
    public void DefaultQuery_IsValid() =>
        _validator.TestValidate(new ListTasksQuery()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void AllParametersCombined_IsValid() =>
        _validator.TestValidate(new ListTasksQuery
        {
            Status = TaskItemStatus.InProgress,
            DueDateFrom = new DateOnly(2026, 1, 1),
            DueDateTo = new DateOnly(2026, 1, 31),
            Search = "relatório",
            SortBy = TaskSortField.CreatedAt,
            SortDirection = SortDirection.Desc,
            Page = 2,
            PageSize = PagedQuery.MaxPageSize
        }).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void SameDayRange_IsValid() =>
        _validator.TestValidate(new ListTasksQuery
        {
            DueDateFrom = new DateOnly(2026, 1, 15),
            DueDateTo = new DateOnly(2026, 1, 15)
        }).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void InvertedDateRange_IsInvalid() =>
        _validator.TestValidate(new ListTasksQuery
        {
            DueDateFrom = new DateOnly(2026, 2, 1),
            DueDateTo = new DateOnly(2026, 1, 1)
        }).ShouldHaveValidationErrorFor(x => x.DueDateFrom);

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void PageBelowOne_IsInvalid(int page) =>
        _validator.TestValidate(new ListTasksQuery { Page = page })
            .ShouldHaveValidationErrorFor(x => x.Page);

    [Theory]
    [InlineData(0)]
    [InlineData(PagedQuery.MaxPageSize + 1)]
    public void PageSizeOutOfRange_IsInvalid(int pageSize) =>
        _validator.TestValidate(new ListTasksQuery { PageSize = pageSize })
            .ShouldHaveValidationErrorFor(x => x.PageSize);

    [Fact]
    public void SearchTooLong_IsInvalid() =>
        _validator.TestValidate(new ListTasksQuery { Search = new string('a', ListTasksQuery.SearchMaxLength + 1) })
            .ShouldHaveValidationErrorFor(x => x.Search);

    [Fact]
    public void UndefinedSortValues_AreInvalid()
    {
        var result = _validator.TestValidate(new ListTasksQuery
        {
            SortBy = (TaskSortField)99,
            SortDirection = (SortDirection)99
        });

        result.ShouldHaveValidationErrorFor(x => x.SortBy);
        result.ShouldHaveValidationErrorFor(x => x.SortDirection);
    }
}
