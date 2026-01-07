using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Reports.DTOs;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Reports.Queries;

/// <summary>
/// Handler for GetActivityByRoleQuery that groups activities by user role.
/// </summary>
public class GetActivityByRoleQueryHandler : IRequestHandler<GetActivityByRoleQuery, List<RoleActivityReportDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetActivityByRoleQueryHandler> _logger;

    public GetActivityByRoleQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetActivityByRoleQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<List<RoleActivityReportDto>> Handle(GetActivityByRoleQuery request, CancellationToken cancellationToken)
    {
        var activityRepo = _unitOfWork.Repository<IntelliPM.Domain.Entities.Activity>();
        var projectMemberRepo = _unitOfWork.Repository<ProjectMember>();

        var query = activityRepo.Query()
            .AsNoTracking()
            .Include(a => a.User)
            .Include(a => a.Project)
            .AsQueryable();

        // Apply date range filter
        if (request.StartDate.HasValue)
        {
            query = query.Where(a => a.CreatedAt >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(a => a.CreatedAt <= request.EndDate.Value);
        }

        // Apply organization filter
        if (request.OrganizationId.HasValue)
        {
            query = query.Where(a => a.Project.OrganizationId == request.OrganizationId.Value);
        }

        // Apply action type filter
        if (!string.IsNullOrWhiteSpace(request.ActionTypeFilter))
        {
            query = query.Where(a => a.ActivityType == request.ActionTypeFilter);
        }

        var activities = await query.ToListAsync(cancellationToken);

        // Get all project members for projects in the activities
        var projectIds = activities.Select(a => a.ProjectId).Distinct().ToList();
        var projectMembers = await projectMemberRepo.Query()
            .AsNoTracking()
            .Where(pm => projectIds.Contains(pm.ProjectId))
            .ToListAsync(cancellationToken);

        // Create a dictionary for quick lookup: (ProjectId, UserId) -> ProjectRole
        var roleLookup = projectMembers
            .GroupBy(pm => new { pm.ProjectId, pm.UserId })
            .ToDictionary(
                g => g.Key,
                g => g.First().Role.ToString()
            );

        // Group activities by role and action type
        var grouped = activities
            .Select(a =>
            {
                // Try to get ProjectRole first, fallback to GlobalRole
                string role = "Unknown";
                
                if (roleLookup.TryGetValue(new { ProjectId = a.ProjectId, UserId = a.UserId }, out var projectRole))
                {
                    role = projectRole;
                }
                else
                {
                    // Fallback to GlobalRole
                    role = a.User.GlobalRole.ToString();
                }

                return new
                {
                    Role = role,
                    ActionType = a.ActivityType,
                    CreatedAt = a.CreatedAt,
                    UserId = a.UserId
                };
            })
            .GroupBy(a => new { a.Role, a.ActionType })
            .Select(g => new RoleActivityReportDto
            {
                Role = g.Key.Role,
                ActionType = g.Key.ActionType,
                Count = g.Count(),
                LastPerformed = g.Max(a => a.CreatedAt),
                UniqueUsers = g.Select(a => a.UserId).Distinct().Count()
            })
            .ToList();

        // Apply role filter if specified
        if (!string.IsNullOrWhiteSpace(request.RoleFilter))
        {
            grouped = grouped.Where(g => g.Role.Equals(request.RoleFilter, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return grouped.OrderByDescending(g => g.Count).ToList();
    }
}

