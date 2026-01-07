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

public class RunQAAgentCommandHandler : IRequestHandler<RunQAAgentCommand, QAAgentOutput>
{
    private readonly QAAgent _qaAgent;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAIDecisionLogger _decisionLogger;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAIAvailabilityService _availabilityService;
    private readonly ICorrelationIdService _correlationIdService;
    private readonly ILogger<RunQAAgentCommandHandler> _logger;

    public RunQAAgentCommandHandler(
        QAAgent qaAgent, 
        IUnitOfWork unitOfWork,
        IAIDecisionLogger decisionLogger,
        ICurrentUserService currentUserService,
        IAIAvailabilityService availabilityService,
        ICorrelationIdService correlationIdService,
        ILogger<RunQAAgentCommandHandler> logger)
    {
        _qaAgent = qaAgent;
        _unitOfWork = unitOfWork;
        _decisionLogger = decisionLogger;
        _currentUserService = currentUserService;
        _availabilityService = availabilityService;
        _correlationIdService = correlationIdService;
        _logger = logger;
    }

    public async Task<QAAgentOutput> Handle(RunQAAgentCommand request, CancellationToken cancellationToken)
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
        QAAgentOutput? result = null;
        string? errorMessage = null;
        List<string>? recentDefects = null;
        Dictionary<string, int>? defectStats = null;
        
        _logger.LogInformation(
            "Starting {AgentType} execution | Project: {ProjectId} | CorrelationId: {CorrelationId}",
            "QAAgent", request.ProjectId, correlationId);

        try
        {
            // Fetch recent defects
            var defectRepo = _unitOfWork.Repository<Defect>();
            recentDefects = await defectRepo.Query()
                .Where(d => d.ProjectId == request.ProjectId)
                .OrderByDescending(d => d.ReportedAt)
                .Take(20)
                .Select(d => $"{d.Severity}: {d.Title}")
                .ToListAsync(cancellationToken);

            // Calculate defect statistics
            defectStats = new Dictionary<string, int>
            {
                { "Open", await defectRepo.Query().Where(d => d.ProjectId == request.ProjectId && d.Status == "Open").CountAsync(cancellationToken) },
                { "Critical", await defectRepo.Query().Where(d => d.ProjectId == request.ProjectId && d.Severity == "Critical").CountAsync(cancellationToken) },
                { "High", await defectRepo.Query().Where(d => d.ProjectId == request.ProjectId && d.Severity == "High").CountAsync(cancellationToken) }
            };

            result = await _qaAgent.RunAsync(request.ProjectId, recentDefects, defectStats, cancellationToken);

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

            var inputDataJson = JsonSerializer.Serialize(new { recentDefects, defectStats });
            
            // Estimate token usage (ILlmClient doesn't provide actual token counts)
            var (promptTokens, completionTokens, totalTokens) = TokenEstimationHelper.EstimateTokenUsage(
                inputDataJson + result.DefectAnalysis, // Approximate prompt
                result.DefectAnalysis // Completion
            );
            tokensUsed = totalTokens;
            
            _logger.LogInformation(
                "Completed {AgentType} execution | Tokens: {TokensUsed} | Duration: {DurationMs}ms | CorrelationId: {CorrelationId}",
                "QAAgent", tokensUsed, stopwatch.ElapsedMilliseconds, correlationId);
            
            linkedDecisionId = await _decisionLogger.LogDecisionAsync(
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
                    agentType: AIDecisionConstants.AgentTypes.QAAgent,
                    decisionType: AIDecisionConstants.DecisionTypes.QualityAnalysis,
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
                AgentId = "qa-agent",
                AgentType = AIDecisionConstants.AgentTypes.QAAgent,
                UserId = userId > 0 ? userId.ToString() : "0",
                UserInput = JsonSerializer.Serialize(new { recentDefects, defectStats }),
                AgentResponse = result.DefectAnalysis,
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
        catch (Exception ex)
        {
            stopwatch.Stop();
            errorMessage = ex.Message;
            
            _logger.LogError(ex,
                "Failed {AgentType} execution | Error: {ErrorMessage} | CorrelationId: {CorrelationId}",
                "QAAgent", ex.Message, correlationId);
            
            // Create AgentExecutionLog entry for failed execution
            var executionLogRepo = _unitOfWork.Repository<AgentExecutionLog>();
            var executionLog = new AgentExecutionLog
            {
                Id = Guid.NewGuid(),
                OrganizationId = orgId > 0 ? orgId : throw new InvalidOperationException("OrganizationId is required for AgentExecutionLog"),
                AgentId = "qa-agent",
                AgentType = AIDecisionConstants.AgentTypes.QAAgent,
                UserId = userId > 0 ? userId.ToString() : "0",
                UserInput = recentDefects != null && defectStats != null 
                    ? JsonSerializer.Serialize(new { recentDefects, defectStats })
                    : "N/A",
                Status = "Error",
                Success = false,
                ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds,
                TokensUsed = 0,
                ExecutionCostUsd = 0m,
                CreatedAt = executionStartTime,
                ErrorMessage = errorMessage,
                LinkedDecisionId = null
            };
            await executionLogRepo.AddAsync(executionLog, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            throw;
        }
    }
}

