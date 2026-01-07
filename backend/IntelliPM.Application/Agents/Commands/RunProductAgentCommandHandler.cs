using MediatR;
using IntelliPM.Application.Agents.Services;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Interfaces;
using IntelliPM.Application.Services;
using IntelliPM.Domain.Constants;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace IntelliPM.Application.Agents.Commands;

public class RunProductAgentCommandHandler : IRequestHandler<RunProductAgentCommand, ProductAgentOutput>
{
    private readonly ProductAgent _productAgent;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAIDecisionLogger _decisionLogger;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAIAvailabilityService _availabilityService;

    public RunProductAgentCommandHandler(
        ProductAgent productAgent, 
        IUnitOfWork unitOfWork,
        IAIDecisionLogger decisionLogger,
        ICurrentUserService currentUserService,
        IAIAvailabilityService availabilityService)
    {
        _productAgent = productAgent;
        _unitOfWork = unitOfWork;
        _decisionLogger = decisionLogger;
        _currentUserService = currentUserService;
        _availabilityService = availabilityService;
    }

    public async Task<ProductAgentOutput> Handle(RunProductAgentCommand request, CancellationToken cancellationToken)
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
        
        // Fetch backlog items
        var backlogRepo = _unitOfWork.Repository<BacklogItem>();
        var backlogItems = await backlogRepo.Query()
            .Where(b => b.ProjectId == request.ProjectId && b.Status == "Backlog")
            .Select(b => $"{b.Id}: {b.Title}")
            .ToListAsync(cancellationToken);

        // Fetch real recent completions from database
        var recentCompletions = new List<string>();
        var thirtyDaysAgo = DateTimeOffset.UtcNow.AddDays(-30);

        // Fetch completed UserStories (recent, last 30 days)
        var userStoryRepo = _unitOfWork.Repository<UserStory>();
        var completedStories = await userStoryRepo.Query()
            .Where(s => s.ProjectId == request.ProjectId 
                && s.Status == "Done" 
                && s.UpdatedAt >= thirtyDaysAgo)
            .OrderByDescending(s => s.UpdatedAt)
            .Take(10)
            .Select(s => new { s.Id, s.Title, s.StoryPoints, s.UpdatedAt })
            .ToListAsync(cancellationToken);

        foreach (var story in completedStories)
        {
            var storyPointsText = story.StoryPoints.HasValue ? $" ({story.StoryPoints} SP)" : "";
            var dateText = story.UpdatedAt.ToString("yyyy-MM-dd");
            recentCompletions.Add($"Story '{story.Title}' completed{storyPointsText} on {dateText}");
        }

        // Fetch completed Features (recent, last 30 days)
        var featureRepo = _unitOfWork.Repository<Feature>();
        var completedFeatures = await featureRepo.Query()
            .Where(f => f.ProjectId == request.ProjectId 
                && f.Status == "Done" 
                && f.UpdatedAt >= thirtyDaysAgo)
            .OrderByDescending(f => f.UpdatedAt)
            .Take(10)
            .Select(f => new { f.Id, f.Title, f.StoryPoints, f.UpdatedAt })
            .ToListAsync(cancellationToken);

        foreach (var feature in completedFeatures)
        {
            var storyPointsText = feature.StoryPoints.HasValue ? $" ({feature.StoryPoints} SP)" : "";
            var dateText = feature.UpdatedAt.ToString("yyyy-MM-dd");
            recentCompletions.Add($"Feature '{feature.Title}' delivered{storyPointsText} on {dateText}");
        }

        // Fetch completed ProjectTasks (recent, last 30 days)
        var taskRepo = _unitOfWork.Repository<ProjectTask>();
        var completedTasks = await taskRepo.Query()
            .Include(t => t.Assignee)
            .Where(t => t.ProjectId == request.ProjectId 
                && t.Status == TaskConstants.Statuses.Done 
                && t.UpdatedAt >= thirtyDaysAgo)
            .OrderByDescending(t => t.UpdatedAt)
            .Take(15)
            .ToListAsync(cancellationToken);

        foreach (var task in completedTasks)
        {
            var storyPointsText = task.StoryPoints != null ? $" ({task.StoryPoints.Value} SP)" : "";
            var assigneeName = task.Assignee != null 
                ? $" by {string.Join(" ", new[] { task.Assignee.FirstName, task.Assignee.LastName }.Where(n => !string.IsNullOrEmpty(n)))}"
                : "";
            var dateText = task.UpdatedAt.ToString("yyyy-MM-dd");
            recentCompletions.Add($"Task '{task.Title}' completed{storyPointsText}{assigneeName} on {dateText}");
        }

        // Fetch deployed Releases (recent, last 90 days)
        var releaseRepo = _unitOfWork.Repository<Release>();
        var deployedReleases = await releaseRepo.Query()
            .Where(r => r.ProjectId == request.ProjectId 
                && r.Status == ReleaseStatus.Deployed 
                && r.ActualReleaseDate >= DateTimeOffset.UtcNow.AddDays(-90))
            .OrderByDescending(r => r.ActualReleaseDate)
            .Take(5)
            .Select(r => new { r.Name, r.Version, r.ActualReleaseDate })
            .ToListAsync(cancellationToken);

        foreach (var release in deployedReleases)
        {
            var dateText = release.ActualReleaseDate?.ToString("yyyy-MM-dd") ?? "Unknown";
            recentCompletions.Add($"Release '{release.Name}' v{release.Version} deployed on {dateText}");
        }

        try
        {
            var result = await _productAgent.RunAsync(request.ProjectId, backlogItems, recentCompletions, cancellationToken);
            stopwatch.Stop();

            // Store agent run
            var agentRun = new AIAgentRun
            {
                ProjectId = request.ProjectId,
                AgentType = "Product",
                InputData = string.Join(", ", backlogItems),
                OutputData = result.Rationale,
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
                    { "BacklogItemsCount", backlogItems.Count },
                    { "RecentCompletionsCount", recentCompletions.Count },
                    { "PrioritizedItemsCount", result.PrioritizedItems.Count },
                    { "ExecutionTimeMs", (int)stopwatch.ElapsedMilliseconds }
                };

                var decisionJson = JsonSerializer.Serialize(new
                {
                    PrioritizedItems = result.PrioritizedItems.Select(item => new
                    {
                        item.ItemId,
                        item.Title,
                        item.Priority,
                        item.Rationale
                    }).ToList(),
                    Summary = result.Rationale
                });

                var inputDataJson = JsonSerializer.Serialize(new { backlogItems, recentCompletions });
                
                // Estimate token usage: approximately 4 characters per token
                var estimatedTokens = (inputDataJson.Length + decisionJson.Length + result.Rationale.Length) / 4;
                tokensUsed = estimatedTokens;
                
                linkedDecisionId = await _decisionLogger.LogDecisionAsync(
                    agentType: AIDecisionConstants.AgentTypes.ProductAgent,
                    decisionType: AIDecisionConstants.DecisionTypes.BacklogPrioritization,
                    reasoning: result.Rationale,
                    confidenceScore: result.Confidence,
                    metadata: metadata,
                    userId: userId,
                    organizationId: orgId,
                    projectId: request.ProjectId,
                    entityType: "Project",
                    entityId: request.ProjectId,
                    question: "Prioritize backlog items based on ROI and risk",
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
                        agentType: AIDecisionConstants.AgentTypes.ProductAgent,
                        decisionType: AIDecisionConstants.DecisionTypes.BacklogPrioritization,
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
                AgentId = "product-agent",
                AgentType = AIDecisionConstants.AgentTypes.ProductAgent,
                UserId = userId > 0 ? userId.ToString() : "0",
                UserInput = JsonSerializer.Serialize(new { backlogItems, recentCompletions }),
                AgentResponse = result.Rationale,
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
                AgentId = "product-agent",
                AgentType = AIDecisionConstants.AgentTypes.ProductAgent,
                UserId = userId > 0 ? userId.ToString() : "0",
                UserInput = JsonSerializer.Serialize(new { backlogItems, recentCompletions }),
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

