using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.Insights.Queries;

public class GetProjectInsightsQueryHandler : IRequestHandler<GetProjectInsightsQuery, GetProjectInsightsResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProjectInsightsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GetProjectInsightsResponse> Handle(GetProjectInsightsQuery request, CancellationToken cancellationToken)
    {
        var insightRepo = _unitOfWork.Repository<Insight>();
        var query = insightRepo.Query()
            .Where(i => i.ProjectId == request.ProjectId);

        if (!string.IsNullOrEmpty(request.Status))
        {
            query = query.Where(i => i.Status == request.Status);
        }

        if (!string.IsNullOrEmpty(request.AgentType))
        {
            query = query.Where(i => i.AgentType == request.AgentType);
        }

        var insights = await query
            .OrderByDescending(i => i.CreatedAt)
            .Select(i => new InsightDto(
                i.Id,
                i.AgentType,
                i.Category,
                i.Title,
                i.Description,
                i.Recommendation,
                i.Confidence,
                i.Priority,
                i.Status,
                i.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return new GetProjectInsightsResponse(insights, insights.Count);
    }
}

