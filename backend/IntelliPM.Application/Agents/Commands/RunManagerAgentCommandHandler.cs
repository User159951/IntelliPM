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

public class RunManagerAgentCommandHandler : IRequestHandler<RunManagerAgentCommand, ManagerAgentOutput>
{
    private readonly ManagerAgent _managerAgent;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAIDecisionLogger _decisionLogger;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAIAvailabilityService _availabilityService;

    public RunManagerAgentCommandHandler(
        ManagerAgent managerAgent,
        IUnitOfWork unitOfWork,
        IAIDecisionLogger decisionLogger,
        ICurrentUserService currentUserService,
        IAIAvailabilityService availabilityService)
    {
        _managerAgent = managerAgent;
        _unitOfWork = unitOfWork;
        _decisionLogger = decisionLogger;
        _currentUserService = currentUserService;
        _availabilityService = availabilityService;
    }

    public async Task<ManagerAgentOutput> Handle(RunManagerAgentCommand request, CancellationToken cancellationToken)
    {
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
        
        // Calculate KPIs
        var sprintRepo = _unitOfWork.Repository<Sprint>();
        var completedSprints = await sprintRepo.Query()
            .Where(s => s.ProjectId == request.ProjectId && s.Status == SprintConstants.Statuses.Completed)
            .CountAsync(cancellationToken);

        var taskRepo = _unitOfWork.Repository<ProjectTask>();
        var totalTasks = await taskRepo.Query()
            .Where(t => t.ProjectId == request.ProjectId)
            .CountAsync(cancellationToken);
        
        var completedTasks = await taskRepo.Query()
            .Where(t => t.ProjectId == request.ProjectId && t.Status == TaskConstants.Statuses.Done)
            .CountAsync(cancellationToken);

        var kpis = new Dictionary<string, object>
        {
            { "CompletedSprints", completedSprints },
            { "TotalTasks", totalTasks },
            { "CompletedTasks", completedTasks },
            { "CompletionRate", totalTasks > 0 ? (decimal)completedTasks / totalTasks * 100 : 0 }
        };

        // Get recent changes (simplified - last 7 days tasks)
        var recentChanges = await taskRepo.Query()
            .Where(t => t.ProjectId == request.ProjectId && t.UpdatedAt >= DateTimeOffset.UtcNow.AddDays(-7))
            .OrderByDescending(t => t.UpdatedAt)
            .Take(10)
            .Select(t => $"{t.Status}: {t.Title}")
            .ToListAsync(cancellationToken);

        var changes = string.Join("\n", recentChanges);

        // Get highlights (completed high-priority tasks)
        var highlightsList = await taskRepo.Query()
            .Where(t => t.ProjectId == request.ProjectId && t.Status == TaskConstants.Statuses.Done && (t.Priority == TaskConstants.Priorities.High || t.Priority == TaskConstants.Priorities.Critical))
            .OrderByDescending(t => t.UpdatedAt)
            .Take(5)
            .Select(t => t.Title)
            .ToListAsync(cancellationToken);

        var highlights = string.Join("\n", highlightsList);

        try
        {
            var result = await _managerAgent.RunAsync(request.ProjectId, kpis, changes, highlights, cancellationToken);
            stopwatch.Stop();

            // Store agent run
            var agentRun = new AIAgentRun
            {
                ProjectId = request.ProjectId,
                AgentType = "Manager",
                InputData = JsonSerializer.Serialize(new { kpis, changes, highlights }),
                OutputData = result.ExecutiveSummary,
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
                    { "KPIs", kpis },
                    { "RecentChangesCount", recentChanges.Count },
                    { "HighlightsCount", highlightsList.Count },
                    { "KeyDecisionsNeededCount", result.KeyDecisionsNeeded.Count },
                    { "ExecutionTimeMs", (int)stopwatch.ElapsedMilliseconds }
                };

                var decisionJson = JsonSerializer.Serialize(new
                {
                    ExecutiveSummary = result.ExecutiveSummary,
                    KeyDecisionsNeeded = result.KeyDecisionsNeeded,
                    Highlights = result.Highlights
                });

                var inputDataJson = JsonSerializer.Serialize(new { kpis, changes, highlights });
                
                // Estimate token usage: approximately 4 characters per token
                var estimatedTokens = (inputDataJson.Length + decisionJson.Length + result.ExecutiveSummary.Length) / 4;
                tokensUsed = estimatedTokens;
                
                linkedDecisionId = await _decisionLogger.LogDecisionAsync(
                    agentType: AIDecisionConstants.AgentTypes.ManagerAgent,
                    decisionType: AIDecisionConstants.DecisionTypes.ExecutiveSummary,
                    reasoning: result.ExecutiveSummary,
                    confidenceScore: result.Confidence,
                    metadata: metadata,
                    userId: userId,
                    organizationId: orgId,
                    projectId: request.ProjectId,
                    entityType: "Project",
                    entityId: request.ProjectId,
                    question: "Generate executive summary with key decisions needed",
                    decision: decisionJson,
                    inputData: inputDataJson,
                    outputData: decisionJson,
                    tokensUsed: estimatedTokens,
                    executionTimeMs: (int)stopwatch.ElapsedMilliseconds,
                    cancellationToken: cancellationToken);

                // Record quota usage after successful execution
                var quotaRepo = _unitOfWork.Repository<AIQuota>();
                var quota = await quotaRepo.Query()
                    .FirstOrDefaultAsync(q => q.OrganizationId == orgId && q.IsActive && q.TierName != "Disabled", cancellationToken);

                if (quota != null)
                {
                    var cost = estimatedTokens * AIQuotaConstants.CostPerToken;

                    quota.RecordUsage(
                        tokens: estimatedTokens,
                        agentType: AIDecisionConstants.AgentTypes.ManagerAgent,
                        decisionType: AIDecisionConstants.DecisionTypes.ExecutiveSummary,
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
                AgentId = "manager-agent",
                AgentType = AIDecisionConstants.AgentTypes.ManagerAgent,
                UserId = userId > 0 ? userId.ToString() : "0",
                UserInput = JsonSerializer.Serialize(new { kpis, changes, highlights }),
                AgentResponse = result.ExecutiveSummary,
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
            
            // Create AgentExecutionLog entry for failed execution
            var executionLogRepo = _unitOfWork.Repository<AgentExecutionLog>();
            var executionLog = new AgentExecutionLog
            {
                Id = Guid.NewGuid(),
                OrganizationId = orgId > 0 ? orgId : throw new InvalidOperationException("OrganizationId is required for AgentExecutionLog"),
                AgentId = "manager-agent",
                AgentType = AIDecisionConstants.AgentTypes.ManagerAgent,
                UserId = userId > 0 ? userId.ToString() : "0",
                UserInput = JsonSerializer.Serialize(new { kpis, changes, highlights }),
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

