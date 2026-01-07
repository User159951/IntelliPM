using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Reports.DTOs;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace IntelliPM.Application.Reports.Queries;

/// <summary>
/// Handler for GetWorkflowTransitionsByRoleQuery that groups workflow transitions by user role.
/// </summary>
public class GetWorkflowTransitionsByRoleQueryHandler : IRequestHandler<GetWorkflowTransitionsByRoleQuery, List<WorkflowRoleReportDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetWorkflowTransitionsByRoleQueryHandler> _logger;

    public GetWorkflowTransitionsByRoleQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetWorkflowTransitionsByRoleQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<List<WorkflowRoleReportDto>> Handle(GetWorkflowTransitionsByRoleQuery request, CancellationToken cancellationToken)
    {
        var activityRepo = _unitOfWork.Repository<IntelliPM.Domain.Entities.Activity>();
        var projectMemberRepo = _unitOfWork.Repository<ProjectMember>();

        // Get activities that represent status changes
        var query = activityRepo.Query()
            .AsNoTracking()
            .Include(a => a.User)
            .Include(a => a.Project)
            .Where(a => a.ActivityType.Contains("status_changed") || 
                       a.ActivityType.Contains("completed") ||
                       a.ActivityType.Contains("started") ||
                       a.ActivityType.Contains("updated"))
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

        // Apply entity type filter
        if (!string.IsNullOrWhiteSpace(request.EntityTypeFilter))
        {
            query = query.Where(a => a.EntityType == request.EntityTypeFilter);
        }

        // Apply organization filter
        if (request.OrganizationId.HasValue)
        {
            query = query.Where(a => a.Project.OrganizationId == request.OrganizationId.Value);
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

        // Parse activities and extract status transitions
        var transitions = new List<(string Role, string FromStatus, string ToStatus, string EntityType, DateTimeOffset CreatedAt, int UserId)>();

        foreach (var activity in activities)
        {
            // Try to get ProjectRole first, fallback to GlobalRole
            string role = "Unknown";
            
            if (roleLookup.TryGetValue(new { ProjectId = activity.ProjectId, UserId = activity.UserId }, out var projectRole))
            {
                role = projectRole;
            }
            else
            {
                // Fallback to GlobalRole
                role = activity.User.GlobalRole.ToString();
            }

            // Parse metadata to extract status transition
            string fromStatus = "Unknown";
            string toStatus = "Unknown";

            if (!string.IsNullOrWhiteSpace(activity.Metadata))
            {
                try
                {
                    var metadata = JsonSerializer.Deserialize<JsonElement>(activity.Metadata);
                    
                    // Try different property names that might be used
                    if (metadata.TryGetProperty("oldStatus", out var oldStatusElement))
                    {
                        fromStatus = oldStatusElement.GetString() ?? "Unknown";
                    }
                    else if (metadata.TryGetProperty("OldStatus", out var oldStatusElement2))
                    {
                        fromStatus = oldStatusElement2.GetString() ?? "Unknown";
                    }

                    if (metadata.TryGetProperty("newStatus", out var newStatusElement))
                    {
                        toStatus = newStatusElement.GetString() ?? "Unknown";
                    }
                    else if (metadata.TryGetProperty("NewStatus", out var newStatusElement2))
                    {
                        toStatus = newStatusElement2.GetString() ?? "Unknown";
                    }
                    else if (metadata.TryGetProperty("Status", out var statusElement))
                    {
                        // Sometimes status change is stored as "old -> new"
                        var statusStr = statusElement.GetString() ?? "";
                        var parts = statusStr.Split("->", StringSplitOptions.TrimEntries);
                        if (parts.Length == 2)
                        {
                            fromStatus = parts[0];
                            toStatus = parts[1];
                        }
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to parse metadata for activity {ActivityId}: {Metadata}", activity.Id, activity.Metadata);
                }
            }

            // If we couldn't parse from metadata, try to infer from activity type
            if (fromStatus == "Unknown" && toStatus == "Unknown")
            {
                if (activity.ActivityType == "task_completed")
                {
                    fromStatus = "InProgress"; // Common assumption
                    toStatus = "Done";
                }
                else if (activity.ActivityType.Contains("started"))
                {
                    fromStatus = "Pending";
                    toStatus = "InProgress";
                }
            }

            transitions.Add((role, fromStatus, toStatus, activity.EntityType, activity.CreatedAt, activity.UserId));
        }

        // Group transitions by role, from status, to status, and entity type
        var grouped = transitions
            .GroupBy(t => new { t.Role, t.FromStatus, t.ToStatus, t.EntityType })
            .Select(g => new WorkflowRoleReportDto
            {
                Role = g.Key.Role,
                FromStatus = g.Key.FromStatus,
                ToStatus = g.Key.ToStatus,
                EntityType = g.Key.EntityType,
                TransitionCount = g.Count(),
                LastTransition = g.Max(t => t.CreatedAt),
                UniqueUsers = g.Select(t => t.UserId).Distinct().Count()
            })
            .ToList();

        // Apply role filter if specified
        if (!string.IsNullOrWhiteSpace(request.RoleFilter))
        {
            grouped = grouped.Where(g => g.Role.Equals(request.RoleFilter, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return grouped.OrderByDescending(g => g.TransitionCount).ToList();
    }
}

