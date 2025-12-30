namespace IntelliPM.Application.Common.Models;

public record PaginatedList<T>(List<T> Items, int Total, int Skip, int Take)
{
    public int PageCount => (Total + Take - 1) / Take;
}

