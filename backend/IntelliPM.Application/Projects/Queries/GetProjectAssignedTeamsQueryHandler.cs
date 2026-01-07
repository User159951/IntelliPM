using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Projects.Queries;

/// <summary>
/// Handler for GetProjectAssignedTeamsQuery.
/// Retrieves all teams assigned to a project with proper authorization.
/// </summary>
public class GetProjectAssignedTeamsQueryHandler : IRequestHandler<GetProjectAssignedTeamsQuery, List<ProjectAssignedTeamDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetProjectAssignedTeamsQueryHandler> _logger;

    public GetProjectAssignedTeamsQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<GetProjectAssignedTeamsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<ProjectAssignedTeamDto>> Handle(GetProjectAssignedTeamsQuery request, CancellationToken cancellationToken)
    {
        var organizationId = _currentUserService.GetOrganizationId();
        if (organizationId == 0)
        {
            throw new UnauthorizedException("User must be authenticated");
        }

        // Verify project exists and belongs to organization
        var project = await _unitOfWork.Repository<Project>()
            .Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId && p.OrganizationId == organizationId, cancellationToken);

        if (project == null)
        {
            throw new NotFoundException($"Project with ID {request.ProjectId} not found");
        }

        // Get assigned teams for the project
        var assignedTeams = await _unitOfWork.Repository<ProjectTeam>()
            .Query()
            .AsNoTracking()
            .Include(pt => pt.Team)
            .Include(pt => pt.AssignedBy)
            .Where(pt => pt.ProjectId == request.ProjectId && pt.OrganizationId == organizationId)
            .OrderByDescending(pt => pt.AssignedAt)
            .ToListAsync(cancellationToken);

        _logger.LogInformation(
            "Retrieved {Count} assigned teams for project {ProjectId}",
            assignedTeams.Count,
            request.ProjectId);

        return assignedTeams.Select(pt => new ProjectAssignedTeamDto(
            pt.TeamId,
            pt.Team.Name,
            null, // Team entity doesn't have a Description property
            pt.AssignedAt,
            pt.AssignedById,
            pt.AssignedBy?.Username,
            pt.IsActive
        )).ToList();
    }
}

