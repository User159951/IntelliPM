using MediatR;
using IntelliPM.Application.Agents.Services;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.Agents.Commands;

public class RunQAAgentCommandHandler : IRequestHandler<RunQAAgentCommand, QAAgentOutput>
{
    private readonly QAAgent _qaAgent;
    private readonly IUnitOfWork _unitOfWork;

    public RunQAAgentCommandHandler(QAAgent qaAgent, IUnitOfWork unitOfWork)
    {
        _qaAgent = qaAgent;
        _unitOfWork = unitOfWork;
    }

    public async Task<QAAgentOutput> Handle(RunQAAgentCommand request, CancellationToken cancellationToken)
    {
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

        // Store agent run
        var agentRun = new AIAgentRun
        {
            ProjectId = request.ProjectId,
            AgentType = "QA",
            InputData = System.Text.Json.JsonSerializer.Serialize(new { recentDefects, defectStats }),
            OutputData = result.DefectAnalysis,
            Confidence = result.Confidence,
            ExecutedAt = DateTimeOffset.UtcNow
        };
        var runRepo = _unitOfWork.Repository<AIAgentRun>();
        await runRepo.AddAsync(agentRun, cancellationToken);
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
}

