using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.Sprints.Queries;

public class GetSprintByIdQueryHandler : IRequestHandler<GetSprintByIdQuery, SprintDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetSprintByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<SprintDetailDto> Handle(GetSprintByIdQuery request, CancellationToken cancellationToken)
    {
        var sprintRepo = _unitOfWork.Repository<Sprint>();
        
        var sprint = await sprintRepo.Query()
            .Where(s => s.Id == request.SprintId)
            // Tenant filter automatically applied via global filter
            .Include(s => s.Project)
            .Include(s => s.Items)
                .ThenInclude(i => i.UserStory)
            .Select(s => new SprintDetailDto(
                s.Id,
                s.ProjectId,
                s.Project.Name,
                $"Sprint {s.Number}",
                s.StartDate ?? DateTimeOffset.UtcNow,
                s.EndDate ?? DateTimeOffset.UtcNow.AddDays(14),
                s.Goal,
                s.Status,
                s.Items.Select(i => new SprintTaskDto(
                    i.UserStoryId,
                    i.UserStory.Title,
                    i.Status,
                    i.UserStory.Priority.ToString(),
                    i.SnapshotStoryPoints ?? i.UserStory.StoryPoints
                )).ToList(),
                s.CreatedAt,
                s.CreatedAt // No UpdatedAt in Sprint entity
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (sprint == null)
            throw new NotFoundException($"Sprint not found");

        return sprint;
    }
}

