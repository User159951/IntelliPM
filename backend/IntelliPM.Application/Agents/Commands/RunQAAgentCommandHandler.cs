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

public class RunQAAgentCommandHandler : IRequestHandler<RunQAAgentCommand, QAAgentOutput>
{
    private readonly QAAgent _qaAgent;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAIDecisionLogger _decisionLogger;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAIAvailabilityService _availabilityService;

    public RunQAAgentCommandHandler(
        QAAgent qaAgent, 
        IUnitOfWork unitOfWork,
        IAIDecisionLogger decisionLogger,
        ICurrentUserService currentUserService,
        IAIAvailabilityService availabilityService)
    {
        _qaAgent = qaAgent;
        _unitOfWork = unitOfWork;
        _decisionLogger = decisionLogger;
        _currentUserService = currentUserService;
        _availabilityService = availabilityService;
    }

    public async Task<QAAgentOutput> Handle(RunQAAgentCommand request, CancellationToken cancellationToken)
    {
        // Check quota before execution
        var organizationId = _currentUserService.GetOrganizationId();
        if (organizationId > 0)
        {
            await _availabilityService.CheckQuotaAsync(organizationId, "Requests", cancellationToken);
        }

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        // Fetch recent defects
        var defectRepo = _unitOfWork.Repository<Defect>();
        var recentDefects = await defectRepo.Query()
            .Where(d => d.ProjectId == request.ProjectId)
            .OrderByDescending(d => d.ReportedAt)
            .Take(20)
            .Select(d => $"{d.Severity}: {d.Title}")
            .ToListAsync(cancellationToken);

        // Calculate defect statistics
        var defectStats = new Dictionary<string, int>
        {
            { "Open", await defectRepo.Query().Where(d => d.ProjectId == request.ProjectId && d.Status == "Open").CountAsync(cancellationToken) },
            { "Critical", await defectRepo.Query().Where(d => d.ProjectId == request.ProjectId && d.Severity == "Critical").CountAsync(cancellationToken) },
            { "High", await defectRepo.Query().Where(d => d.ProjectId == request.ProjectId && d.Severity == "High").CountAsync(cancellationToken) }
        };

        var result = await _qaAgent.RunAsync(request.ProjectId, recentDefects, defectStats, cancellationToken);

        stopwatch.Stop();

        // Store agent run
        var agentRun = new AIAgentRun
        {
            ProjectId = request.ProjectId,
            AgentType = "QA",
            InputData = JsonSerializer.Serialize(new { recentDefects, defectStats }),
            OutputData = result.DefectAnalysis,
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
                { "RecentDefectsCount", recentDefects.Count },
                { "DefectStats", defectStats },
                { "PatternsCount", result.Patterns.Count },
                { "RecommendationsCount", result.Recommendations.Count },
                { "ExecutionTimeMs", (int)stopwatch.ElapsedMilliseconds }
            };

            var decisionJson = JsonSerializer.Serialize(new
            {
                DefectAnalysis = result.DefectAnalysis,
                Patterns = result.Patterns.Select(p => new
                {
                    p.Pattern,
                    p.Frequency,
                    p.Severity,
                    p.Suggestion
                }).ToList(),
                Recommendations = result.Recommendations
            });

            await _decisionLogger.LogDecisionAsync(
                agentType: AIDecisionConstants.AgentTypes.QAAgent,
                decisionType: AIDecisionConstants.DecisionTypes.QualityAnalysis,
                reasoning: result.DefectAnalysis,
                confidenceScore: result.Confidence,
                metadata: metadata,
                userId: userId,
                organizationId: orgId,
                projectId: request.ProjectId,
                entityType: "Project",
                entityId: request.ProjectId,
                question: "Analyze quality metrics and suggest testing strategies",
                decision: decisionJson,
                inputData: JsonSerializer.Serialize(new { recentDefects, defectStats }),
                outputData: decisionJson,
                executionTimeMs: (int)stopwatch.ElapsedMilliseconds,
                cancellationToken: cancellationToken);
        }

        // Create insights from recommendations
        var insightRepo = _unitOfWork.Repository<Insight>();
        foreach (var recommendation in result.Recommendations.Take(3))
        {
            var insight = new Insight
            {
                ProjectId = request.ProjectId,
                AgentRunId = agentRun.Id,
                AgentType = "QA",
                Category = "Opportunity",
                Title = "Quality Improvement",
                Description = recommendation,
                Recommendation = recommendation,
                Confidence = result.Confidence,
                Priority = "Medium",
                CreatedAt = DateTimeOffset.UtcNow
            };
            await insightRepo.AddAsync(insight, cancellationToken);
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return result;
    }
}

