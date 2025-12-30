using MediatR;
using IntelliPM.Application.Common.Models;

namespace IntelliPM.Application.Backlog.Queries;

/// <summary>
/// Query to retrieve unassigned tasks (backlog) sorted by priority with pagination and filtering.
/// </summary>
public record GetBacklogQuery : IRequest<PagedResponse<BacklogTaskDto>>
{
    public int ProjectId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
    public string? Priority { get; init; } // Optional filter: "Critical", "High", "Medium", "Low"
    public string? Status { get; init; } // Optional filter: "Todo", "InProgress"
    public string? SearchTerm { get; init; } // Search in title/description
}

/// <summary>
/// DTO for a backlog task (unassigned to any sprint).
/// </summary>
public record BacklogTaskDto(
    int Id,
    string Title,
    string Description,
    string Priority,
    string Status,
    int? StoryPoints,
    int? AssigneeId,
    string? AssigneeName,
    DateTimeOffset CreatedAt,
    int PriorityOrder // For sorting: Critical=1, High=2, Medium=3, Low=4
);

