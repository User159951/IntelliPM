using MediatR;
using IntelliPM.Application.Agents.Services;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.Agents.Commands;

public class RunProductAgentCommandHandler : IRequestHandler<RunProductAgentCommand, ProductAgentOutput>
{
    private readonly ProductAgent _productAgent;
    private readonly IUnitOfWork _unitOfWork;

    public RunProductAgentCommandHandler(ProductAgent productAgent, IUnitOfWork unitOfWork)
    {
        _productAgent = productAgent;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProductAgentOutput> Handle(RunProductAgentCommand request, CancellationToken cancellationToken)
    {
        // Fetch backlog items
        var backlogRepo = _unitOfWork.Repository<BacklogItem>();
        var backlogItems = await backlogRepo.Query()
            .Where(b => b.ProjectId == request.ProjectId && b.Status == "Backlog")
            .Select(b => $"{b.Id}: {b.Title}")
            .ToListAsync(cancellationToken);

        // Stub recent completions
        var recentCompletions = new List<string> { "Story X completed", "Feature Y delivered" };

        var result = await _productAgent.RunAsync(request.ProjectId, backlogItems, recentCompletions, cancellationToken);

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

        return result;
    }
}

