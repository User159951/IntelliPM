using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.Projects.Queries;

public class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, GetProjectByIdResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProjectByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GetProjectByIdResponse> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        var projectRepo = _unitOfWork.Repository<Project>();
        var project = await projectRepo.Query()
            .Include(p => p.Members)
                .ThenInclude(m => m.User)
            .Include(p => p.Members)
                .ThenInclude(m => m.InvitedBy)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);

        if (project == null)
            throw new NotFoundException($"Project {request.ProjectId} not found");

        var members = project.Members.Select(m => new ProjectMemberDto(
            m.Id,
            m.UserId,
            m.User.Username,
            m.User.Email,
            m.Role,
            m.InvitedAt,
            m.InvitedBy.Username
        )).ToList();

        return new GetProjectByIdResponse(
            project.Id,
            project.Name,
            project.Description,
            project.Type,
            project.Status,
            members,
            project.CreatedAt
        );
    }
}

