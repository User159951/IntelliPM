using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.AI.Queries;

/// <summary>
/// Handler for getting a specific AI decision by ID.
/// </summary>
public class GetAIDecisionByIdQueryHandler : IRequestHandler<GetAIDecisionByIdQuery, AIDecisionLogDetailDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetAIDecisionByIdQueryHandler> _logger;

    public GetAIDecisionByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetAIDecisionByIdQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async System.Threading.Tasks.Task<AIDecisionLogDetailDto?> Handle(GetAIDecisionByIdQuery request, CancellationToken ct)
    {
        var decision = await _unitOfWork.Repository<AIDecisionLog>()
            .Query()
            .AsNoTracking()
            .Where(d => d.DecisionId == request.DecisionId && d.OrganizationId == request.OrganizationId)
            .Select(d => new AIDecisionLogDetailDto(
                d.DecisionId,
                d.DecisionType,
                d.AgentType,
                d.EntityType,
                d.EntityId,
                d.EntityName,
                d.Question,
                d.Decision,
                d.Reasoning,
                d.ConfidenceScore,
                d.ModelName,
                d.ModelVersion,
                d.TokensUsed,
                d.PromptTokens,
                d.CompletionTokens,
                d.Status,
                d.RequiresHumanApproval,
                d.ApprovedByHuman,
                d.ApprovedByUserId,
                d.ApprovedAt,
                d.ApprovalNotes,
                d.WasApplied,
                d.AppliedAt,
                d.ActualOutcome,
                d.CreatedAt,
                d.ExecutionTimeMs,
                d.IsSuccess,
                d.ErrorMessage
            ))
            .FirstOrDefaultAsync(ct);

        return decision;
    }
}

