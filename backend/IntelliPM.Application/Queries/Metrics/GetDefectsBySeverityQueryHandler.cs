using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.Queries.Metrics;

public class GetDefectsBySeverityQueryHandler : IRequestHandler<GetDefectsBySeverityQuery, DefectsBySeverityResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetDefectsBySeverityQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DefectsBySeverityResponse> Handle(GetDefectsBySeverityQuery request, CancellationToken cancellationToken)
    {
        var defectRepo = _unitOfWork.Repository<Defect>();

        var query = defectRepo.Query()
            .Where(d => d.Status != "Closed" && d.Status != "Resolved");

        if (request.ProjectId.HasValue)
        {
            query = query.Where(d => d.ProjectId == request.ProjectId.Value);
        }

        var defects = await query.ToListAsync(cancellationToken);

        var distribution = defects
            .GroupBy(d => d.Severity)
            .Select(g => new DefectSeverityData
            {
                Severity = g.Key,
                Count = g.Count()
            })
            .ToList();

        // Ensure all severities are present
        var allSeverities = new[] { "Critical", "High", "Medium", "Low" };
        var existingSeverities = distribution.Select(d => d.Severity).ToHashSet();

        foreach (var severity in allSeverities)
        {
            if (!existingSeverities.Contains(severity))
            {
                distribution.Add(new DefectSeverityData { Severity = severity, Count = 0 });
            }
        }

        return new DefectsBySeverityResponse
        {
            Defects = distribution.OrderByDescending(d => 
                d.Severity == "Critical" ? 4 :
                d.Severity == "High" ? 3 :
                d.Severity == "Medium" ? 2 : 1).ToList()
        };
    }
}
