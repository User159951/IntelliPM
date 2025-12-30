using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.Defects.Queries;

public class GetProjectDefectsQueryHandler : IRequestHandler<GetProjectDefectsQuery, GetProjectDefectsResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProjectDefectsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GetProjectDefectsResponse> Handle(GetProjectDefectsQuery request, CancellationToken cancellationToken)
    {
        var defectRepo = _unitOfWork.Repository<Defect>();
        var query = defectRepo.Query()
            .Include(d => d.UserStory)
            .Where(d => d.ProjectId == request.ProjectId);

        if (!string.IsNullOrEmpty(request.Status))
        {
            query = query.Where(d => d.Status == request.Status);
        }

        if (!string.IsNullOrEmpty(request.Severity))
        {
            query = query.Where(d => d.Severity == request.Severity);
        }

        if (request.AssignedToId.HasValue)
        {
            query = query.Where(d => d.AssignedToId == request.AssignedToId.Value);
        }

        var defects = await query
            .Include(d => d.ReportedBy)
            .Include(d => d.AssignedTo)
            .OrderByDescending(d => d.ReportedAt)
            .ToListAsync(cancellationToken);

        var defectDtos = defects.Select(d => new DefectDto(
            d.Id,
            d.Title,
            d.Description,
            d.Severity,
            d.Status,
            d.UserStoryId,
            d.UserStory?.Title,
            d.AssignedToId,
            d.AssignedTo != null ? $"{d.AssignedTo.FirstName} {d.AssignedTo.LastName}" : null,
            d.ReportedById,
            d.ReportedBy != null ? $"{d.ReportedBy.FirstName} {d.ReportedBy.LastName}" : null,
            d.FoundInEnvironment,
            d.ReportedAt,
            d.ResolvedAt,
            d.UpdatedAt
        )).ToList();

        return new GetProjectDefectsResponse(defectDtos, defectDtos.Count);
    }
}

