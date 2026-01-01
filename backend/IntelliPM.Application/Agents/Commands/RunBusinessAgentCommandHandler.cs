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

public class RunBusinessAgentCommandHandler : IRequestHandler<RunBusinessAgentCommand, BusinessAgentOutput>
{
    private readonly BusinessAgent _businessAgent;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAIDecisionLogger _decisionLogger;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAIAvailabilityService _availabilityService;

    public RunBusinessAgentCommandHandler(
        BusinessAgent businessAgent, 
        IUnitOfWork unitOfWork,
        IAIDecisionLogger decisionLogger,
        ICurrentUserService currentUserService,
        IAIAvailabilityService availabilityService)
    {
        _businessAgent = businessAgent;
        _unitOfWork = unitOfWork;
        _decisionLogger = decisionLogger;
        _currentUserService = currentUserService;
        _availabilityService = availabilityService;
    }

    public async Task<BusinessAgentOutput> Handle(RunBusinessAgentCommand request, CancellationToken cancellationToken)
    {
        // Check quota before execution
        var organizationId = _currentUserService.GetOrganizationId();
        if (organizationId > 0)
        {
            await _availabilityService.CheckQuotaAsync(organizationId, "Requests", cancellationToken);
        }

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        // Fetch completed features
        var storyRepo = _unitOfWork.Repository<UserStory>();
        var completedFeatures = await storyRepo.Query()
            .Where(s => s.ProjectId == request.ProjectId && s.Status == "Done")
            .Select(s => s.Title)
            .ToListAsync(cancellationToken);

        // Calculate KPIs
        var sprintRepo = _unitOfWork.Repository<Sprint>();
        var completedSprints = await sprintRepo.Query()
            .Where(s => s.ProjectId == request.ProjectId && s.Status == SprintConstants.Statuses.Completed)
            .CountAsync(cancellationToken);

        var kpis = new Dictionary<string, object>
        {
            { "CompletedSprints", completedSprints },
            { "CompletedFeatures", completedFeatures.Count },
            { "TotalStoryPoints", 0 } // Stub
        };

        var businessMetrics = new Dictionary<string, decimal>
        {
            { "Velocity", 25.5m },
            { "DefectRate", 0.12m }
        };

        var result = await _businessAgent.RunAsync(request.ProjectId, kpis, completedFeatures, businessMetrics, cancellationToken);

        stopwatch.Stop();

        // Store agent run
        var agentRun = new AIAgentRun
        {
            ProjectId = request.ProjectId,
            AgentType = "Business",
            InputData = JsonSerializer.Serialize(new { kpis, completedFeatures }),
            OutputData = result.ValueDeliverySummary,
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
                { "CompletedFeaturesCount", completedFeatures.Count },
                { "CompletedSprints", completedSprints },
                { "KPIs", kpis },
                { "BusinessMetrics", businessMetrics },
                { "ValueMetrics", result.ValueMetrics },
                { "HighlightsCount", result.BusinessHighlights.Count },
                { "RecommendationsCount", result.StrategicRecommendations.Count },
                { "ExecutionTimeMs", (int)stopwatch.ElapsedMilliseconds }
            };

            var decisionJson = JsonSerializer.Serialize(new
            {
                ValueDeliverySummary = result.ValueDeliverySummary,
                ValueMetrics = result.ValueMetrics,
                BusinessHighlights = result.BusinessHighlights,
                StrategicRecommendations = result.StrategicRecommendations
            });

            await _decisionLogger.LogDecisionAsync(
                agentType: AIDecisionConstants.AgentTypes.BusinessAgent,
                decisionType: AIDecisionConstants.DecisionTypes.BusinessValueAnalysis,
                reasoning: result.ValueDeliverySummary,
                confidenceScore: result.Confidence,
                metadata: metadata,
                userId: userId,
                organizationId: orgId,
                projectId: request.ProjectId,
                entityType: "Project",
                entityId: request.ProjectId,
                question: "Evaluate business value and ROI for features and initiatives",
                decision: decisionJson,
                inputData: JsonSerializer.Serialize(new { kpis, completedFeatures, businessMetrics }),
                outputData: decisionJson,
                executionTimeMs: (int)stopwatch.ElapsedMilliseconds,
                cancellationToken: cancellationToken);
        }

        return result;
    }
}

