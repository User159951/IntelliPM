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

public class RunBusinessAgentCommandHandler : IRequestHandler<RunBusinessAgentCommand, BusinessAgentOutput>
{
    private readonly BusinessAgent _businessAgent;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAIDecisionLogger _decisionLogger;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAIAvailabilityService _availabilityService;
    private readonly ICorrelationIdService _correlationIdService;
    private readonly ILogger<RunBusinessAgentCommandHandler> _logger;

    public RunBusinessAgentCommandHandler(
        BusinessAgent businessAgent, 
        IUnitOfWork unitOfWork,
        IAIDecisionLogger decisionLogger,
        ICurrentUserService currentUserService,
        IAIAvailabilityService availabilityService,
        ICorrelationIdService correlationIdService,
        ILogger<RunBusinessAgentCommandHandler> logger)
    {
        _businessAgent = businessAgent;
        _unitOfWork = unitOfWork;
        _decisionLogger = decisionLogger;
        _currentUserService = currentUserService;
        _availabilityService = availabilityService;
        _correlationIdService = correlationIdService;
        _logger = logger;
    }

    public async Task<BusinessAgentOutput> Handle(RunBusinessAgentCommand request, CancellationToken cancellationToken)
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
            "BusinessAgent", request.ProjectId, correlationId);
        
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

        // Calculate TotalStoryPoints from all ProjectTasks in the project
        var taskRepo = _unitOfWork.Repository<ProjectTask>();
        var totalStoryPoints = await taskRepo.Query()
            .Where(t => t.ProjectId == request.ProjectId && t.StoryPoints != null)
            .SumAsync(t => (decimal?)t.StoryPoints!.Value, cancellationToken) ?? 0m;

        // Calculate Velocity: Average story points completed per sprint from last 3 months
        var threeMonthsAgo = DateTimeOffset.UtcNow.AddMonths(-3);
        var completedSprintList = await sprintRepo.Query()
            .Where(s => s.ProjectId == request.ProjectId 
                && s.Status == SprintConstants.Statuses.Completed
                && s.EndDate >= threeMonthsAgo)
            .OrderByDescending(s => s.EndDate)
            .Select(s => new { s.Id, s.Number, s.EndDate, s.StartDate })
            .ToListAsync(cancellationToken);

        // Fetch all completed task story points grouped by sprint in one query
        var sprintIds = completedSprintList.Select(s => s.Id).ToList();
        var sprintPointsMap = new Dictionary<int, decimal>();
        
        if (sprintIds.Count > 0)
        {
            var sprintPointsData = await taskRepo.Query()
                .Where(t => t.SprintId != null && sprintIds.Contains(t.SprintId.Value) && t.Status == TaskConstants.Statuses.Done && t.StoryPoints != null)
                .GroupBy(t => t.SprintId)
                .Select(g => new { SprintId = g.Key!.Value, Points = g.Sum(t => (decimal?)t.StoryPoints!.Value) ?? 0m })
                .ToListAsync(cancellationToken);

            sprintPointsMap = sprintPointsData.ToDictionary(sp => sp.SprintId, sp => sp.Points);
        }

        // Calculate average velocity from completed sprints
        decimal velocity = 0m;
        if (completedSprintList.Count > 0)
        {
            var sprintVelocities = completedSprintList
                .Where(s => sprintPointsMap.ContainsKey(s.Id))
                .Select(s => sprintPointsMap[s.Id])
                .ToList();
            velocity = sprintVelocities.Count > 0 ? sprintVelocities.Average() : 0m;
        }

        // Calculate completed story points
        var completedStoryPoints = await taskRepo.Query()
            .Where(t => t.ProjectId == request.ProjectId && t.Status == TaskConstants.Statuses.Done && t.StoryPoints != null)
            .SumAsync(t => (decimal?)t.StoryPoints!.Value, cancellationToken) ?? 0m;

        // Calculate Progress: Percentage of completed tasks
        var totalTasks = await taskRepo.Query()
            .Where(t => t.ProjectId == request.ProjectId)
            .CountAsync(cancellationToken);

        var completedTasks = await taskRepo.Query()
            .Where(t => t.ProjectId == request.ProjectId && t.Status == TaskConstants.Statuses.Done)
            .CountAsync(cancellationToken);

        // Calculate DefectRate: Bugs / total completed tasks
        var defectRepo = _unitOfWork.Repository<Defect>();
        var bugCount = await defectRepo.Query()
            .Where(d => d.ProjectId == request.ProjectId)
            .CountAsync(cancellationToken);

        decimal defectRate = 0m;
        if (completedTasks > 0)
        {
            defectRate = (decimal)bugCount / completedTasks;
        }

        // Calculate average cycle time: Average days from CreatedAt to UpdatedAt for completed tasks
        var avgCycleTime = await taskRepo.Query()
            .Where(t => t.ProjectId == request.ProjectId 
                && t.Status == TaskConstants.Statuses.Done)
            .Select(t => (t.UpdatedAt - t.CreatedAt).TotalDays)
            .DefaultIfEmpty(0)
            .AverageAsync(cancellationToken);

        // Calculate completion rate: completedStoryPoints / totalStoryPoints
        var completionRate = totalStoryPoints > 0 
            ? completedStoryPoints / totalStoryPoints 
            : 0m;

        decimal progress = totalTasks > 0 ? (completedTasks * 100m) / totalTasks : 0m;

        // Calculate burn-down data: Sprint history with story points completed per sprint
        var burnDownData = new List<Dictionary<string, object>>();
        foreach (var sprint in completedSprintList.OrderBy(s => s.EndDate ?? s.StartDate ?? DateTimeOffset.MinValue))
        {
            var sprintPoints = sprintPointsMap.ContainsKey(sprint.Id) ? sprintPointsMap[sprint.Id] : 0m;

            burnDownData.Add(new Dictionary<string, object>
            {
                { "SprintNumber", sprint.Number },
                { "SprintId", sprint.Id },
                { "EndDate", sprint.EndDate?.ToString("yyyy-MM-dd") ?? sprint.StartDate?.ToString("yyyy-MM-dd") ?? "" },
                { "StoryPointsCompleted", sprintPoints }
            });
        }

        // Calculate velocity trend (last 5 sprints for trend analysis)
        var velocityTrend = completedSprintList
            .OrderBy(s => s.EndDate ?? s.StartDate ?? DateTimeOffset.MinValue)
            .TakeLast(5)
            .Select(s => sprintPointsMap.ContainsKey(s.Id) ? sprintPointsMap[s.Id] : 0m)
            .ToList();

        // Get project name for context
        var projectRepo = _unitOfWork.Repository<Project>();
        var project = await projectRepo.Query()
            .Where(p => p.Id == request.ProjectId)
            .Select(p => new { p.Name })
            .FirstOrDefaultAsync(cancellationToken);

        var kpis = new Dictionary<string, object>
        {
            { "ProjectName", project?.Name ?? "Unknown" },
            { "CompletedSprints", completedSprints },
            { "CompletedFeatures", completedFeatures.Count },
            { "TotalStoryPoints", totalStoryPoints },
            { "CompletedStoryPoints", completedStoryPoints },
            { "TotalTasks", totalTasks },
            { "CompletedTasks", completedTasks },
            { "Progress", progress },
            { "BurnDownData", burnDownData },
            { "VelocityTrend", velocityTrend }
        };

        var businessMetrics = new Dictionary<string, decimal>
        {
            { "Velocity", velocity },
            { "DefectRate", defectRate },
            { "AvgCycleTimeDays", (decimal)avgCycleTime },
            { "CompletionRate", completionRate }
        };

        try
        {
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

            var inputDataJson = JsonSerializer.Serialize(new { kpis, completedFeatures, businessMetrics });
            
            // Estimate token usage (ILlmClient doesn't provide actual token counts)
            var (promptTokens, completionTokens, totalTokens) = TokenEstimationHelper.EstimateTokenUsage(
                inputDataJson + result.ValueDeliverySummary, // Approximate prompt
                result.ValueDeliverySummary // Completion
            );
            tokensUsed = totalTokens;
            
            _logger.LogInformation(
                "Completed {AgentType} execution | Tokens: {TokensUsed} | Duration: {DurationMs}ms | CorrelationId: {CorrelationId}",
                "BusinessAgent", tokensUsed, stopwatch.ElapsedMilliseconds, correlationId);
            
            linkedDecisionId = await _decisionLogger.LogDecisionAsync(
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
                    agentType: AIDecisionConstants.AgentTypes.BusinessAgent,
                    decisionType: AIDecisionConstants.DecisionTypes.BusinessValueAnalysis,
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
                AgentId = "business-agent",
                AgentType = AIDecisionConstants.AgentTypes.BusinessAgent,
                UserId = userId > 0 ? userId.ToString() : "0",
                UserInput = JsonSerializer.Serialize(new { kpis, completedFeatures, businessMetrics }),
                AgentResponse = result.ValueDeliverySummary,
                Status = "Success",
                Success = true,
                ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds,
                TokensUsed = tokensUsed,
                ExecutionCostUsd = 0m,
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
                "BusinessAgent", ex.Message, correlationId);
            
            // Create AgentExecutionLog entry for failed execution
            var executionLogRepo = _unitOfWork.Repository<AgentExecutionLog>();
            var executionLog = new AgentExecutionLog
            {
                Id = Guid.NewGuid(),
                OrganizationId = orgId > 0 ? orgId : throw new InvalidOperationException("OrganizationId is required for AgentExecutionLog"),
                AgentId = "business-agent",
                AgentType = AIDecisionConstants.AgentTypes.BusinessAgent,
                UserId = userId > 0 ? userId.ToString() : "0",
                UserInput = JsonSerializer.Serialize(new { kpis, completedFeatures, businessMetrics }),
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

