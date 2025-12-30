using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.Teams.Queries;

public class GetTeamCapacityQueryHandler : IRequestHandler<GetTeamCapacityQuery, TeamCapacityDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetTeamCapacityQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<TeamCapacityDto> Handle(GetTeamCapacityQuery request, CancellationToken cancellationToken)
    {
        var teamRepo = _unitOfWork.Repository<Team>();
        
        // Get team with members
        var team = await teamRepo.Query()
            .Include(t => t.Members)
                .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(t => t.Id == request.TeamId, cancellationToken);

        if (team == null)
            throw new InvalidOperationException($"Team with ID {request.TeamId} not found");

        // Get member IDs
        var memberIds = team.Members.Select(m => m.UserId).ToList();

        // Calculate assigned story points for team members in active sprints
        // Find tasks assigned to team members that are in active sprints
        var taskRepo = _unitOfWork.Repository<ProjectTask>();
        var sprintRepo = _unitOfWork.Repository<Sprint>();

        // Find active sprints (Status = "Active")
        var activeSprints = await sprintRepo.Query()
            .Where(s => s.Status == "Active")
            .ToListAsync(cancellationToken);

        int assignedStoryPoints = 0;
        int? activeSprintId = null;
        string? activeSprintName = null;

        if (activeSprints.Any())
        {
            // For simplicity, consider the first active sprint
            var activeSprint = activeSprints.First();
            activeSprintId = activeSprint.Id;
            activeSprintName = $"Sprint {activeSprint.Number}";

            // Calculate assigned story points for team members in this sprint
            var assignedTasks = await taskRepo.Query()
                .Where(t => t.SprintId == activeSprint.Id && memberIds.Contains(t.AssigneeId ?? 0))
                .ToListAsync(cancellationToken);

            assignedStoryPoints = assignedTasks
                .Where(t => t.StoryPoints != null)
                .Sum(t => t.StoryPoints!.Value);
        }

        var availableCapacity = team.Capacity - assignedStoryPoints;

        return new TeamCapacityDto(
            team.Id,
            team.Name,
            team.Capacity,
            assignedStoryPoints,
            availableCapacity > 0 ? availableCapacity : 0,
            activeSprintId,
            activeSprintName
        );
    }
}

