using FluentValidation;
using TaskManager.Application.Common.Pagination;

namespace TaskManager.Application.Tasks.List;

public sealed class ListTasksQueryValidator : AbstractValidator<ListTasksQuery>
{
    public ListTasksQueryValidator()
    {
        Include(new PagedQueryValidator());

        RuleFor(x => x.Status).ValidStatus();

        RuleFor(x => x.DueDateFrom)
            .LessThanOrEqualTo(x => x.DueDateTo)
            .When(x => x.DueDateFrom.HasValue && x.DueDateTo.HasValue)
            .WithMessage("'dueDateFrom' deve ser menor ou igual a 'dueDateTo'.");

        RuleFor(x => x.Search)
            .MaximumLength(ListTasksQuery.SearchMaxLength)
            .WithMessage($"O termo de busca deve ter no máximo {ListTasksQuery.SearchMaxLength} caracteres.");

        RuleFor(x => x.SortBy)
            .IsInEnum()
            .WithMessage("'sortBy' inválido. Valores aceitos: DueDate, CreatedAt, Title.");

        RuleFor(x => x.SortDirection)
            .IsInEnum()
            .WithMessage("'sortDirection' inválido. Valores aceitos: Asc, Desc.");
    }
}
