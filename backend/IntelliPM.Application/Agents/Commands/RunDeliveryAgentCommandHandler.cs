using MediatR;
using IntelliPM.Application.Agents.Services;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Interfaces;
using IntelliPM.Application.Services;
using IntelliPM.Domain.Constants;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace IntelliPM.Application.Agents.Commands;

public class RunDeliveryAgentCommandHandler : IRequestHandler<RunDeliveryAgentCommand, DeliveryAgentOutput>
{
    private readonly DeliveryAgent _deliveryAgent;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAIDecisionLogger _decisionLogger;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAIAvailabilityService _availabilityService;

    public RunDeliveryAgentCommandHandler(
        DeliveryAgent deliveryAgent,
        IUnitOfWork unitOfWork,
        IAIDecisionLogger decisionLogger,
        ICurrentUserService currentUserService,
        IAIAvailabilityService availabilityService)
    {
        _deliveryAgent = deliveryAgent;
        _unitOfWork = unitOfWork;
        _decisionLogger = decisionLogger;
        _currentUserService = currentUserService;
        _availabilityService = availabilityService;
    }

    public async Task<DeliveryAgentOutput> Handle(RunDeliveryAgentCommand request, CancellationToken cancellationToken)
    {
        // Check quota before execution
        var organizationId = _currentUserService.GetOrganizationId();
        if (organizationId > 0)
        {
            await _availabilityService.CheckQuotaAsync(organizationId, "Requests", cancellationToken);
        }

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
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
        var userId = _currentUserService.GetUserId();
        var orgId = organizationId;
        
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

            await _decisionLogger.LogDecisionAsync(
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
                inputData: JsonSerializer.Serialize(new { sprintProgress, velocityTrend, activeRisks }),
                outputData: decisionJson,
                executionTimeMs: (int)stopwatch.ElapsedMilliseconds,
                cancellationToken: cancellationToken);
        }

        return result;
    }
}

