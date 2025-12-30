using MediatR;
using IntelliPM.Application.Agents.Services;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.Agents.Commands;

public class RunBusinessAgentCommandHandler : IRequestHandler<RunBusinessAgentCommand, BusinessAgentOutput>
{
    private readonly BusinessAgent _businessAgent;
    private readonly IUnitOfWork _unitOfWork;

    public RunBusinessAgentCommandHandler(BusinessAgent businessAgent, IUnitOfWork unitOfWork)
    {
        _businessAgent = businessAgent;
        _unitOfWork = unitOfWork;
    }

    public async Task<BusinessAgentOutput> Handle(RunBusinessAgentCommand request, CancellationToken cancellationToken)
    {
        // Fetch completed features
        var storyRepo = _unitOfWork.Repository<UserStory>();
        var completedFeatures = await storyRepo.Query()
            .Where(s => s.ProjectId == request.ProjectId && s.Status == "Done")
            .Select(s => s.Title)
            .ToListAsync(cancellationToken);

        // Calculate KPIs
        var sprintRepo = _unitOfWork.Repository<Sprint>();
        var completedSprints = await sprintRepo.Query()
            .Where(s => s.ProjectId == request.ProjectId && s.Status == "Completed")
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

        // Store agent run
        var agentRun = new AIAgentRun
        {
            ProjectId = request.ProjectId,
            AgentType = "Business",
            InputData = System.Text.Json.JsonSerializer.Serialize(new { kpis, completedFeatures }),
            OutputData = result.ValueDeliverySummary,
            Confidence = result.Confidence,
            ExecutedAt = DateTimeOffset.UtcNow
        };
        var runRepo = _unitOfWork.Repository<AIAgentRun>();
        await runRepo.AddAsync(agentRun, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return result;
    }
}

