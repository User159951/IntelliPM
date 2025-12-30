using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.Queries.Metrics;

public class GetTaskDistributionQueryHandler : IRequestHandler<GetTaskDistributionQuery, TaskDistributionResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetTaskDistributionQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<TaskDistributionResponse> Handle(GetTaskDistributionQuery request, CancellationToken cancellationToken)
    {
        var taskRepo = _unitOfWork.Repository<ProjectTask>();

        var query = taskRepo.Query();
        
        if (request.ProjectId.HasValue)
        {
            query = query.Where(t => t.ProjectId == request.ProjectId.Value);
        }

        var tasks = await query.ToListAsync(cancellationToken);

        var distribution = tasks
            .GroupBy(t => t.Status)
            .Select(g => new TaskDistributionData
            {
                Status = g.Key,
                Count = g.Count()
            })
            .ToList();

        // Ensure all statuses are present
        var allStatuses = new[] { "Todo", "InProgress", "Blocked", "Done" };
        var existingStatuses = distribution.Select(d => d.Status).ToHashSet();

        foreach (var status in allStatuses)
        {
            if (!existingStatuses.Contains(status))
            {
                distribution.Add(new TaskDistributionData { Status = status, Count = 0 });
            }
        }

        return new TaskDistributionResponse
        {
            Distribution = distribution.OrderBy(d => d.Status).ToList()
        };
    }
}
