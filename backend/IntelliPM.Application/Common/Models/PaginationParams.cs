namespace IntelliPM.Application.Common.Models;

public record PaginationParams
{
    private const int MaxPageSize = 100;
    private int _pageSize = 20;

    public int Page { get; init; } = 1;

    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }

    public int Skip => (Page - 1) * PageSize;
}

