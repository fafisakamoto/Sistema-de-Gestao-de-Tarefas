using FluentValidation;

namespace TaskManager.Application.Common.Pagination;

internal sealed class PagedQueryValidator : AbstractValidator<PagedQuery>
{
    public PagedQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("'page' deve ser maior ou igual a 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, PagedQuery.MaxPageSize)
            .WithMessage($"'pageSize' deve estar entre 1 e {PagedQuery.MaxPageSize}.");
    }
}
