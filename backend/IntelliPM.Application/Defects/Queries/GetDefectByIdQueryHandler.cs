using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.Defects.Queries;

public class GetDefectByIdQueryHandler : IRequestHandler<GetDefectByIdQuery, DefectDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetDefectByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DefectDetailDto> Handle(GetDefectByIdQuery request, CancellationToken cancellationToken)
    {
        var defectRepo = _unitOfWork.Repository<Defect>();
        
        var defect = await defectRepo.Query()
            .Where(d => d.Id == request.DefectId)
            // Tenant filter automatically applied via global filter
            .Include(d => d.UserStory)
            .Include(d => d.Sprint)
            .Include(d => d.ReportedBy)
            .Include(d => d.AssignedTo)
            .FirstOrDefaultAsync(cancellationToken);

        if (defect == null)
            throw new NotFoundException($"Defect not found");

        return new DefectDetailDto(
            defect.Id,
            defect.Title,
            defect.Description,
            defect.Severity,
            defect.Status,
            defect.ProjectId,
            defect.UserStoryId,
            defect.UserStory?.Title,
            defect.SprintId,
            defect.Sprint != null ? $"Sprint {defect.Sprint.Number}" : null,
            defect.ReportedById,
            defect.ReportedBy != null ? $"{defect.ReportedBy.FirstName} {defect.ReportedBy.LastName}" : null,
            defect.AssignedToId,
            defect.AssignedTo != null ? $"{defect.AssignedTo.FirstName} {defect.AssignedTo.LastName}" : null,
            defect.FoundInEnvironment,
            defect.StepsToReproduce,
            defect.Resolution,
            defect.ReportedAt,
            defect.ResolvedAt,
            defect.UpdatedAt
        );
    }
}
