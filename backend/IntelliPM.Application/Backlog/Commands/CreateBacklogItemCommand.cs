using MediatR;

namespace IntelliPM.Application.Backlog.Commands;

public record CreateBacklogItemCommand(
    int ProjectId,
    int CurrentUserId,
    string ItemType, // "Epic", "Feature", "Story"
    string Title,
    string Description,
    int? StoryPoints,
    string? DomainTag,
    int? EpicId,
    int? FeatureId,
    string? AcceptanceCriteria
) : IRequest<CreateBacklogItemResponse>;

public record CreateBacklogItemResponse(int Id, string Title, string ItemType);

