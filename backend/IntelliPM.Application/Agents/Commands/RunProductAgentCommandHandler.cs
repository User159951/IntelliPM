using MediatR;
using IntelliPM.Application.Agents.Services;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Interfaces;
using IntelliPM.Domain.Constants;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace IntelliPM.Application.Agents.Commands;

public class RunProductAgentCommandHandler : IRequestHandler<RunProductAgentCommand, ProductAgentOutput>
{
    private readonly ProductAgent _productAgent;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAIDecisionLogger _decisionLogger;
    private readonly ICurrentUserService _currentUserService;

    public RunProductAgentCommandHandler(
        ProductAgent productAgent, 
        IUnitOfWork unitOfWork,
        IAIDecisionLogger decisionLogger,
        ICurrentUserService currentUserService)
    {
        _productAgent = productAgent;
        _unitOfWork = unitOfWork;
        _decisionLogger = decisionLogger;
        _currentUserService = currentUserService;
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
        
        // Fetch backlog items
        var backlogRepo = _unitOfWork.Repository<BacklogItem>();
        var backlogItems = await backlogRepo.Query()
            .Where(b => b.ProjectId == request.ProjectId && b.Status == "Backlog")
            .Select(b => $"{b.Id}: {b.Title}")
            .ToListAsync(cancellationToken);

        // Stub recent completions
        var recentCompletions = new List<string> { "Story X completed", "Feature Y delivered" };

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
        var userId = _currentUserService.GetUserId();
        var organizationId = _currentUserService.GetOrganizationId();
        
        if (userId > 0 && organizationId > 0)
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

            await _decisionLogger.LogDecisionAsync(
                agentType: AIDecisionConstants.AgentTypes.ProductAgent,
                decisionType: AIDecisionConstants.DecisionTypes.BacklogPrioritization,
                reasoning: result.Rationale,
                confidenceScore: result.Confidence,
                metadata: metadata,
                userId: userId,
                organizationId: organizationId,
                projectId: request.ProjectId,
                entityType: "Project",
                entityId: request.ProjectId,
                question: "Prioritize backlog items based on ROI and risk",
                decision: decisionJson,
                inputData: JsonSerializer.Serialize(new { backlogItems, recentCompletions }),
                outputData: decisionJson,
                executionTimeMs: (int)stopwatch.ElapsedMilliseconds,
                cancellationToken: cancellationToken);
        }

        return result;
    }
}

