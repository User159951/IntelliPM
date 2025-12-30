using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Models;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.AI.Queries;

/// <summary>
/// Handler for getting AI decision logs with filtering and pagination.
/// </summary>
public class GetAIDecisionLogsQueryHandler : IRequestHandler<GetAIDecisionLogsQuery, PagedResponse<AIDecisionLogDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetAIDecisionLogsQueryHandler> _logger;

    public GetAIDecisionLogsQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetAIDecisionLogsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async System.Threading.Tasks.Task<PagedResponse<AIDecisionLogDto>> Handle(GetAIDecisionLogsQuery request, CancellationToken ct)
    {
        var query = _unitOfWork.Repository<AIDecisionLog>()
            .Query()
            .AsNoTracking()
            .Where(d => d.OrganizationId == request.OrganizationId);

        // Apply filters
        if (!string.IsNullOrEmpty(request.DecisionType))
        {
            query = query.Where(d => d.DecisionType == request.DecisionType);
        }

        if (!string.IsNullOrEmpty(request.AgentType))
        {
            query = query.Where(d => d.AgentType == request.AgentType);
        }

        if (!string.IsNullOrEmpty(request.EntityType))
        {
            query = query.Where(d => d.EntityType == request.EntityType);
        }

        if (request.EntityId.HasValue)
        {
            query = query.Where(d => d.EntityId == request.EntityId.Value);
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(d => d.CreatedAt >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(d => d.CreatedAt <= request.EndDate.Value);
        }

        if (request.RequiresApproval.HasValue)
        {
            query = query.Where(d => d.RequiresHumanApproval == request.RequiresApproval.Value);
        }

        // Get total count
        var totalCount = await query.CountAsync(ct);

        // Apply pagination
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Max(1, Math.Min(request.PageSize, 100));

        var decisions = await query
            .OrderByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new AIDecisionLogDto(
                d.DecisionId,
                d.DecisionType,
                d.AgentType,
                d.EntityType,
                d.EntityId,
                d.EntityName,
                d.Question,
                d.Decision,
                d.ConfidenceScore,
                d.Status,
                d.RequiresHumanApproval,
                d.ApprovedByHuman,
                d.CreatedAt,
                d.TokensUsed
            ))
            .ToListAsync(ct);

        return new PagedResponse<AIDecisionLogDto>(decisions, page, pageSize, totalCount);
    }
}

