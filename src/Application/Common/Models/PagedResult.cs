namespace Application.Common.Models;

public record PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];

    public int PageNumber { get; init; }

    public int PageSize { get; init; }

    public int TotalCount { get; init; }

    public int TotalPages =>
        PageSize <= 0
            ? 0
            : (int)Math.Ceiling(
                (double)TotalCount / PageSize);

    public bool HasNextPage =>
        PageNumber < TotalPages;

    public bool HasPreviousPage =>
        PageNumber > 1;

    public static PagedResult<T> Create(
        IEnumerable<T> items,
        int pageNumber,
        int pageSize,
        int totalCount)
    {
        return new PagedResult<T>
        {
            Items = items.ToList(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}