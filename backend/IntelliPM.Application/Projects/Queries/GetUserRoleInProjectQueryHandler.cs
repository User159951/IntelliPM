using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.Projects.Queries;

public class GetUserRoleInProjectQueryHandler : IRequestHandler<GetUserRoleInProjectQuery, ProjectRole?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUserRoleInProjectQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ProjectRole?> Handle(GetUserRoleInProjectQuery request, CancellationToken cancellationToken)
    {
        var memberRepo = _unitOfWork.Repository<ProjectMember>();
        
        var projectMember = await memberRepo.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                m => m.ProjectId == request.ProjectId && m.UserId == request.UserId,
                cancellationToken);

        return projectMember?.Role;
    }
}

