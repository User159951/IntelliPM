using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.Agent.Queries;

public class GetAgentAuditLogsQueryHandler : IRequestHandler<GetAgentAuditLogsQuery, GetAgentAuditLogsResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAgentAuditLogsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GetAgentAuditLogsResponse> Handle(GetAgentAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var logsRepo = _unitOfWork.Repository<AgentExecutionLog>();
        
        // Build query with filters
        var query = logsRepo.Query().AsNoTracking();

        if (!string.IsNullOrEmpty(request.AgentId))
        {
            query = query.Where(log => log.AgentId == request.AgentId);
        }

        if (!string.IsNullOrEmpty(request.UserId))
        {
            query = query.Where(log => log.UserId == request.UserId);
        }

        if (!string.IsNullOrEmpty(request.Status))
        {
            query = query.Where(log => log.Status == request.Status);
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Calculate pagination
        var pageSize = Math.Max(1, Math.Min(request.PageSize, 100)); // Max 100 items per page
        var page = Math.Max(1, request.Page);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        // Get paginated logs ordered by CreatedAt descending
        var logs = await query
            .OrderByDescending(log => log.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(log => new AgentExecutionLogDto(
                log.Id,
                log.AgentId,
                log.UserId,
                log.UserInput,
                log.AgentResponse,
                log.ToolsCalled,
                log.Status,
                log.ExecutionTimeMs,
                log.ExecutionCostUsd,
                log.CreatedAt,
                log.ErrorMessage
            ))
            .ToListAsync(cancellationToken);

        return new GetAgentAuditLogsResponse(
            logs,
            totalCount,
            page,
            pageSize,
            totalPages
        );
    }
}

