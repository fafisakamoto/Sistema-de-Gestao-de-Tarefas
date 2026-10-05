namespace TaskManager.Application.Common.Pagination;

public sealed record PageRequest(int Page, int PageSize)
{
    public int Skip => (Page - 1) * PageSize;
}
