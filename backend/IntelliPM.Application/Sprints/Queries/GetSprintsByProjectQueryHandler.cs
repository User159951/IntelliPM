using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.Sprints.Queries;

public class GetSprintsByProjectQueryHandler : IRequestHandler<GetSprintsByProjectQuery, GetSprintsByProjectResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetSprintsByProjectQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GetSprintsByProjectResponse> Handle(GetSprintsByProjectQuery request, CancellationToken cancellationToken)
    {
        var sprintRepo = _unitOfWork.Repository<Sprint>();
        
        var sprints = await sprintRepo.Query()
            .Include(s => s.Items)
            .Where(s => s.ProjectId == request.ProjectId)
            .OrderByDescending(s => s.StartDate)
            .Select(s => new SprintListDto(
                s.Id,
                $"Sprint {s.Number}",
                s.StartDate ?? DateTimeOffset.UtcNow,
                s.EndDate ?? DateTimeOffset.UtcNow.AddDays(14),
                s.Goal,
                s.Status,
                s.Items.Count,
                s.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return new GetSprintsByProjectResponse(sprints);
    }
}

