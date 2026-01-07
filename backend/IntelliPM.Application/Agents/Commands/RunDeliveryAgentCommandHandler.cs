using MediatR;
using IntelliPM.Application.Agents.Services;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Helpers;
using IntelliPM.Application.Interfaces;
using IntelliPM.Application.Services;
using IntelliPM.Domain.Constants;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace IntelliPM.Application.Agents.Commands;

public class RunDeliveryAgentCommandHandler : IRequestHandler<RunDeliveryAgentCommand, DeliveryAgentOutput>
{
    private readonly DeliveryAgent _deliveryAgent;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAIDecisionLogger _decisionLogger;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAIAvailabilityService _availabilityService;
    private readonly ICorrelationIdService _correlationIdService;
    private readonly ILogger<RunDeliveryAgentCommandHandler> _logger;

    public RunDeliveryAgentCommandHandler(
        DeliveryAgent deliveryAgent,
        IUnitOfWork unitOfWork,
        IAIDecisionLogger decisionLogger,
        ICurrentUserService currentUserService,
        IAIAvailabilityService availabilityService,
        ICorrelationIdService correlationIdService,
        ILogger<RunDeliveryAgentCommandHandler> logger)
    {
        _deliveryAgent = deliveryAgent;
        _unitOfWork = unitOfWork;
        _decisionLogger = decisionLogger;
        _currentUserService = currentUserService;
        _availabilityService = availabilityService;
        _correlationIdService = correlationIdService;
        _logger = logger;
    }

    public async Task<DeliveryAgentOutput> Handle(RunDeliveryAgentCommand request, CancellationToken cancellationToken)
    {
        // Get correlation ID for tracing
        var correlationId = _correlationIdService.GetCorrelationId() ?? Guid.NewGuid().ToString();
        
        // Check quota before execution
        var organizationId = _currentUserService.GetOrganizationId();
        if (organizationId > 0)
        {
            await _availabilityService.CheckQuotaAsync(organizationId, "Requests", cancellationToken);
        }

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var executionStartTime = DateTime.UtcNow;
        var userId = _currentUserService.GetUserId();
        var orgId = organizationId;
        int? linkedDecisionId = null;
        int tokensUsed = 0;
        
        _logger.LogInformation(
            "Starting {AgentType} execution | Project: {ProjectId} | CorrelationId: {CorrelationId}",
            "DeliveryAgent", request.ProjectId, correlationId);
        
        // Get active sprint progress
        var sprintRepo = _unitOfWork.Repository<Sprint>();
        var activeSprint = await sprintRepo.Query()
            .Where(s => s.ProjectId == request.ProjectId && s.Status == SprintConstants.Statuses.Active)
            .FirstOrDefaultAsync(cancellationToken);

        var sprintProgress = activeSprint != null
            ? $"Sprint {activeSprint.Number}: {activeSprint.Status}"
            : "No active sprint";

        // Calculate velocity trend (last 5 sprints)
        var completedSprints = await sprintRepo.Query()
            .Where(s => s.ProjectId == request.ProjectId && s.Status == SprintConstants.Statuses.Completed)
            .OrderByDescending(s => s.EndDate)
            .Take(5)
            .ToListAsync(cancellationToken);

        // Calculate velocity (story points completed per sprint) - simplified
        var velocityTrend = new List<decimal>();
        foreach (var sprint in completedSprints.OrderBy(s => s.EndDate))
        {
            var taskRepo = _unitOfWork.Repository<ProjectTask>();
            var completedPoints = await taskRepo.Query()
                .Where(t => t.SprintId == sprint.Id && t.Status == TaskConstants.Statuses.Done && t.StoryPoints != null)
                .SumAsync(t => (decimal?)t.StoryPoints!.Value, cancellationToken) ?? 0;
            velocityTrend.Add(completedPoints);
        }

        // Get active risks
        var riskRepo = _unitOfWork.Repository<Risk>();
        var activeRisks = await riskRepo.Query()
            .Where(r => r.ProjectId == request.ProjectId && r.Status == "Open")
            .Select(r => $"Risk {r.Id}: {r.Title} (Impact: {r.Impact}, Probability: {r.Probability})")
            .ToListAsync(cancellationToken);

        try
        {
            var result = await _deliveryAgent.RunAsync(request.ProjectId, sprintProgress, velocityTrend, activeRisks, cancellationToken);
            stopwatch.Stop();

            // Store agent run
            var agentRun = new AIAgentRun
            {
                ProjectId = request.ProjectId,
                AgentType = "Delivery",
                InputData = JsonSerializer.Serialize(new { sprintProgress, velocityTrend, activeRisks }),
                OutputData = result.RiskAssessment,
                Confidence = result.Confidence,
                ExecutedAt = DateTimeOffset.UtcNow
            };
            var runRepo = _unitOfWork.Repository<AIAgentRun>();
            await runRepo.AddAsync(agentRun, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Log decision to AIDecisionLog
            if (userId > 0 && orgId > 0)
            {
                var metadata = new Dictionary<string, object>
                {
                    { "SprintProgress", sprintProgress },
                    { "VelocityTrendCount", velocityTrend.Count },
                    { "ActiveRisksCount", activeRisks.Count },
                    { "RecommendedActionsCount", result.RecommendedActions.Count },
                    { "ExecutionTimeMs", (int)stopwatch.ElapsedMilliseconds }
                };

                var decisionJson = JsonSerializer.Serialize(new
                {
                    RiskAssessment = result.RiskAssessment,
                    RecommendedActions = result.RecommendedActions
                });

                var inputDataJson = JsonSerializer.Serialize(new { sprintProgress, velocityTrend, activeRisks });
                
                // Estimate token usage (ILlmClient doesn't provide actual token counts)
                var (promptTokens, completionTokens, totalTokens) = TokenEstimationHelper.EstimateTokenUsage(
                    inputDataJson + result.RiskAssessment, // Approximate prompt
                    result.RiskAssessment // Completion
                );
                tokensUsed = totalTokens;
                
                _logger.LogInformation(
                    "Completed {AgentType} execution | Tokens: {TokensUsed} | Duration: {DurationMs}ms | CorrelationId: {CorrelationId}",
                    "DeliveryAgent", tokensUsed, stopwatch.ElapsedMilliseconds, correlationId);

                linkedDecisionId = await _decisionLogger.LogDecisionAsync(
                    agentType: AIDecisionConstants.AgentTypes.DeliveryAgent,
                    decisionType: AIDecisionConstants.DecisionTypes.DeliveryAnalysis,
                    reasoning: result.RiskAssessment,
                    confidenceScore: result.Confidence,
                    metadata: metadata,
                    userId: userId,
                    organizationId: orgId,
                    projectId: request.ProjectId,
                    entityType: "Project",
                    entityId: request.ProjectId,
                    question: "Assess delivery risk and provide recommendations",
                    decision: decisionJson,
                    inputData: inputDataJson,
                    outputData: decisionJson,
                    tokensUsed: totalTokens,
                    promptTokens: promptTokens,
                    completionTokens: completionTokens,
                    executionTimeMs: (int)stopwatch.ElapsedMilliseconds,
                    correlationId: correlationId,
                    cancellationToken: cancellationToken);

                // Record quota usage after successful execution
                var quotaRepo = _unitOfWork.Repository<AIQuota>();
                var quota = await quotaRepo.Query()
                    .FirstOrDefaultAsync(q => q.OrganizationId == orgId && q.IsActive, cancellationToken);

                if (quota != null)
                {
                    var cost = totalTokens * AIQuotaConstants.CostPerToken;

                    quota.RecordUsage(
                        tokens: totalTokens,
                        agentType: AIDecisionConstants.AgentTypes.DeliveryAgent,
                        decisionType: AIDecisionConstants.DecisionTypes.DeliveryAnalysis,
                        cost: cost);

                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
            }

            // Create AgentExecutionLog entry
            var executionLogRepo = _unitOfWork.Repository<AgentExecutionLog>();
            var executionLog = new AgentExecutionLog
            {
                Id = Guid.NewGuid(),
                OrganizationId = orgId > 0 ? orgId : throw new InvalidOperationException("OrganizationId is required for AgentExecutionLog"),
                AgentId = "delivery-agent",
                AgentType = AIDecisionConstants.AgentTypes.DeliveryAgent,
                UserId = userId > 0 ? userId.ToString() : "0",
                UserInput = JsonSerializer.Serialize(new { sprintProgress, velocityTrend, activeRisks }),
                AgentResponse = result.RiskAssessment,
                Status = "Success",
                Success = true,
                ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds,
                TokensUsed = tokensUsed,
                ExecutionCostUsd = 0m, // Local LLM
                CreatedAt = executionStartTime,
                LinkedDecisionId = linkedDecisionId
            };
            await executionLogRepo.AddAsync(executionLog, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            _logger.LogError(ex,
                "Failed {AgentType} execution | Error: {ErrorMessage} | CorrelationId: {CorrelationId}",
                "DeliveryAgent", ex.Message, correlationId);
            
            // Create AgentExecutionLog entry for failed execution
            var executionLogRepo = _unitOfWork.Repository<AgentExecutionLog>();
            var executionLog = new AgentExecutionLog
            {
                Id = Guid.NewGuid(),
                OrganizationId = orgId > 0 ? orgId : throw new InvalidOperationException("OrganizationId is required for AgentExecutionLog"),
                AgentId = "delivery-agent",
                AgentType = AIDecisionConstants.AgentTypes.DeliveryAgent,
                UserId = userId > 0 ? userId.ToString() : "0",
                UserInput = JsonSerializer.Serialize(new { sprintProgress, velocityTrend, activeRisks }),
                Status = "Error",
                Success = false,
                ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds,
                TokensUsed = 0,
                ExecutionCostUsd = 0m,
                CreatedAt = executionStartTime,
                ErrorMessage = ex.Message,
                LinkedDecisionId = null
            };
            await executionLogRepo.AddAsync(executionLog, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            throw;
        }
    }
}

