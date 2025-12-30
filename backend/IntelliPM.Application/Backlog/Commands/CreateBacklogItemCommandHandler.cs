using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Authorization;
using IntelliPM.Application.Projects.Queries;
using IntelliPM.Domain.Entities;

namespace IntelliPM.Application.Backlog.Commands;

public class CreateBacklogItemCommandHandler : IRequestHandler<CreateBacklogItemCommand, CreateBacklogItemResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;

    public CreateBacklogItemCommandHandler(IUnitOfWork unitOfWork, IMediator mediator)
    {
        _unitOfWork = unitOfWork;
        _mediator = mediator;
    }

    public async Task<CreateBacklogItemResponse> Handle(CreateBacklogItemCommand request, CancellationToken cancellationToken)
    {
        // Permission check - uses CanCreateTasks as backlog items can become tasks
        var userRole = await _mediator.Send(new GetUserRoleInProjectQuery(request.ProjectId, request.CurrentUserId), cancellationToken);
        if (userRole == null)
            throw new UnauthorizedException("You are not a member of this project");
        if (!ProjectPermissions.CanCreateTasks(userRole.Value))
            throw new UnauthorizedException("You don't have permission to create backlog items in this project");
        BacklogItem item = request.ItemType switch
        {
            "Epic" => new Epic
            {
                ProjectId = request.ProjectId,
                Title = request.Title,
                Description = request.Description,
                CreatedAt = DateTimeOffset.UtcNow
            },
            "Feature" => new Feature
            {
                ProjectId = request.ProjectId,
                Title = request.Title,
                Description = request.Description,
                StoryPoints = request.StoryPoints,
                DomainTag = request.DomainTag,
                EpicId = request.EpicId,
                CreatedAt = DateTimeOffset.UtcNow
            },
            "Story" => new UserStory
            {
                ProjectId = request.ProjectId,
                Title = request.Title,
                Description = request.Description,
                StoryPoints = request.StoryPoints,
                DomainTag = request.DomainTag,
                FeatureId = request.FeatureId,
                AcceptanceCriteria = request.AcceptanceCriteria,
                CreatedAt = DateTimeOffset.UtcNow
            },
            _ => throw new ArgumentException("Invalid item type")
        };

        var repo = _unitOfWork.Repository<BacklogItem>();
        await repo.AddAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateBacklogItemResponse(item.Id, item.Title, request.ItemType);
    }
}

