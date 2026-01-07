using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Reports.DTOs;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Reports.Queries;

/// <summary>
/// Handler for GetAIDecisionsByApproverRoleQuery that groups AI decisions by approver role.
/// </summary>
public class GetAIDecisionsByApproverRoleQueryHandler : IRequestHandler<GetAIDecisionsByApproverRoleQuery, List<AIDecisionRoleReportDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetAIDecisionsByApproverRoleQueryHandler> _logger;

    public GetAIDecisionsByApproverRoleQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetAIDecisionsByApproverRoleQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<List<AIDecisionRoleReportDto>> Handle(GetAIDecisionsByApproverRoleQuery request, CancellationToken cancellationToken)
    {
        var decisionRepo = _unitOfWork.Repository<AIDecisionLog>();
        var projectMemberRepo = _unitOfWork.Repository<ProjectMember>();

        var query = decisionRepo.Query()
            .AsNoTracking()
            .Include(d => d.ApprovedByUser)
            .Include(d => d.RejectedByUser)
            .AsQueryable();

        // Apply date range filter
        if (request.StartDate.HasValue)
        {
            query = query.Where(d => d.CreatedAt >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(d => d.CreatedAt <= request.EndDate.Value);
        }

        // Apply organization filter
        if (request.OrganizationId.HasValue)
        {
            query = query.Where(d => d.OrganizationId == request.OrganizationId.Value);
        }

        var decisions = await query.ToListAsync(cancellationToken);

        // Get project members for decisions that have project context
        // We need to find projects related to decisions through EntityType and EntityId
        var projectIds = new List<int>();
        foreach (var decision in decisions.Where(d => d.EntityType == "Project" || d.EntityType == "Task" || d.EntityType == "Sprint"))
        {
            if (decision.EntityType == "Project")
            {
                projectIds.Add(decision.EntityId);
            }
            else
            {
                // For Task/Sprint, we'd need to join, but for now we'll use a simpler approach
                // Get all project members for all projects
            }
        }

        // Get all project members (we'll filter by project when needed)
        var allProjectMembers = await projectMemberRepo.Query()
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var roleLookup = allProjectMembers
            .GroupBy(pm => new { pm.ProjectId, pm.UserId })
            .ToDictionary(
                g => g.Key,
                g => g.First().Role.ToString()
            );

        // Process decisions and group by approver role
        var decisionGroups = decisions
            .Where(d => d.ApprovedByUserId.HasValue || d.RejectedByUserId.HasValue)
            .Select(d =>
            {
                var approverId = d.ApprovedByUserId ?? d.RejectedByUserId ?? 0;
                var approver = d.ApprovedByUser ?? d.RejectedByUser;
                
                string role = "Unknown";
                
                if (approver != null)
                {
                    // Try to find ProjectRole first
                    // For AI decisions, we need to determine which project they relate to
                    // Since decisions can be at different levels, we'll use GlobalRole as fallback
                    role = approver.GlobalRole.ToString();
                    
                    // If we can determine the project, try to get ProjectRole
                    // This is a simplified approach - in a real scenario, you might need more complex logic
                }

                var responseTime = d.ApprovedAt.HasValue 
                    ? (d.ApprovedAt.Value - d.CreatedAt).TotalHours
                    : d.RejectedAt.HasValue 
                        ? (d.RejectedAt.Value - d.CreatedAt).TotalHours 
                        : (double?)null;

                return new
                {
                    Role = role,
                    IsApproved = d.ApprovedByUserId.HasValue,
                    IsRejected = d.RejectedByUserId.HasValue,
                    IsPending = !d.ApprovedByUserId.HasValue && !d.RejectedByUserId.HasValue,
                    ResponseTimeHours = responseTime,
                    ApproverId = approverId,
                    ConfidenceScore = d.ConfidenceScore
                };
            })
            .GroupBy(d => d.Role)
            .Select(g => new AIDecisionRoleReportDto
            {
                Role = g.Key,
                DecisionsApproved = g.Count(d => d.IsApproved),
                DecisionsRejected = g.Count(d => d.IsRejected),
                DecisionsPending = g.Count(d => d.IsPending),
                AverageResponseTimeHours = g.Where(d => d.ResponseTimeHours.HasValue).Any()
                    ? g.Where(d => d.ResponseTimeHours.HasValue).Average(d => d.ResponseTimeHours!.Value)
                    : 0,
                UniqueApprovers = g.Select(d => d.ApproverId).Distinct().Count(),
                AverageConfidenceScore = g.Any() ? (decimal)g.Average(d => (double)d.ConfidenceScore) : 0
            })
            .ToList();

        // Apply role filter if specified
        if (!string.IsNullOrWhiteSpace(request.RoleFilter))
        {
            decisionGroups = decisionGroups.Where(g => g.Role.Equals(request.RoleFilter, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return decisionGroups.OrderByDescending(g => g.DecisionsApproved + g.DecisionsRejected).ToList();
    }
}

