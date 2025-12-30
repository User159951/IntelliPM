using MediatR;

namespace IntelliPM.Application.Search.Queries;

public record SearchQuery(
    string Query,
    int Limit = 20
) : IRequest<SearchResponse>;

public record SearchResponse(
    List<SearchResultDto> Results
);

public record SearchResultDto(
    string Type, // "project", "task", "user"
    int Id,
    string Title,
    string? Description,
    string? Subtitle,
    string? Url
);
