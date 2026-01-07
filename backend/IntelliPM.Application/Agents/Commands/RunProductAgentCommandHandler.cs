using MediatR;
using IntelliPM.Application.Agents.Services;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Interfaces;
using IntelliPM.Application.Services;
using IntelliPM.Domain.Constants;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace IntelliPM.Application.Agents.Commands;

public class RunProductAgentCommandHandler : IRequestHandler<RunProductAgentCommand, ProductAgentOutput>
{
    private readonly ProductAgent _productAgent;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAIDecisionLogger _decisionLogger;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAIAvailabilityService _availabilityService;
    private readonly ICorrelationIdService _correlationIdService;
    private readonly ILogger<RunProductAgentCommandHandler> _logger;

    public RunProductAgentCommandHandler(
        ProductAgent productAgent, 
        IUnitOfWork unitOfWork,
        IAIDecisionLogger decisionLogger,
        ICurrentUserService currentUserService,
        IAIAvailabilityService availabilityService,
        ICorrelationIdService correlationIdService,
        ILogger<RunProductAgentCommandHandler> logger)
    {
        _productAgent = productAgent;
        _unitOfWork = unitOfWork;
        _decisionLogger = decisionLogger;
        _currentUserService = currentUserService;
        _availabilityService = availabilityService;
        _correlationIdService = correlationIdService;
        _logger = logger;
    }

    public async Task<ProductAgentOutput> Handle(RunProductAgentCommand request, CancellationToken cancellationToken)
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
            "ProductAgent", request.ProjectId, correlationId);
        
        // Fetch backlog items
        var backlogRepo = _unitOfWork.Repository<BacklogItem>();
        var backlogItems = await backlogRepo.Query()
            .Where(b => b.ProjectId == request.ProjectId && b.Status == "Backlog")
            .Select(b => $"{b.Id}: {b.Title}")
            .ToListAsync(cancellationToken);

        // Get recent completions from the last 30 days
        var thirtyDaysAgo = DateTimeOffset.UtcNow.AddDays(-30);
        var taskRepo = _unitOfWork.Repository<ProjectTask>();
        var completedTasks = await taskRepo.Query()
            .Where(t => t.ProjectId == request.ProjectId 
                && t.Status == TaskConstants.Statuses.Done 
                && t.UpdatedAt >= thirtyDaysAgo)
            .OrderByDescending(t => t.UpdatedAt)
            .Take(10)
            .Select(t => new {
                t.Title,
                t.UpdatedAt,
                t.StoryPoints
            })
            .ToListAsync(cancellationToken);

        var recentCompletions = completedTasks
            .Select(c => c.StoryPoints != null 
                ? $"{c.Title} ({c.StoryPoints.Value} pts, completed {c.UpdatedAt:MMM dd})"
                : $"{c.Title} (completed {c.UpdatedAt:MMM dd})")
            .ToList();

        // Fallback if no completions
        if (!recentCompletions.Any())
        {
            recentCompletions.Add("No recent completions in the last 30 days");
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
                
                _logger.LogInformation(
                    "Completed {AgentType} execution | Tokens: {TokensUsed} | Duration: {DurationMs}ms | CorrelationId: {CorrelationId}",
                    "ProductAgent", tokensUsed, stopwatch.ElapsedMilliseconds, correlationId);
                
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
                    correlationId: correlationId,
                    cancellationToken: cancellationToken);

                // Record quota usage after successful execution
                var quotaRepo = _unitOfWork.Repository<AIQuota>();
                var quota = await quotaRepo.Query()
                    .FirstOrDefaultAsync(q => q.OrganizationId == orgId && q.IsActive, cancellationToken);

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
            
            _logger.LogError(ex,
                "Failed {AgentType} execution | Error: {ErrorMessage} | CorrelationId: {CorrelationId}",
                "ProductAgent", ex.Message, correlationId);
            
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

