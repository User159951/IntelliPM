using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Authorization;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.Projects.Queries;

public class GetProjectMembersQueryHandler : IRequestHandler<GetProjectMembersQuery, List<ProjectMemberDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProjectMembersQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<ProjectMemberDto>> Handle(GetProjectMembersQuery request, CancellationToken cancellationToken)
    {
        // Get project and verify it exists
        var projectRepo = _unitOfWork.Repository<Project>();
        var project = await projectRepo.Query()
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);

        if (project == null)
            throw new NotFoundException($"Project with ID {request.ProjectId} not found");

        // Check if current user has access (is Admin or is a member)
        var userRepo = _unitOfWork.Repository<User>();
        var currentUser = await userRepo.GetByIdAsync(request.CurrentUserId, cancellationToken);
        
        if (currentUser == null)
            throw new NotFoundException($"User with ID {request.CurrentUserId} not found");

        // Check if user is Admin
        var isAdmin = currentUser.GlobalRole == GlobalRole.Admin;
        
        // Check if user is a member of the project
        var isMember = project.Members.Any(m => m.UserId == request.CurrentUserId);

        if (!isAdmin && !isMember)
            throw new UnauthorizedAccessException($"User {request.CurrentUserId} does not have access to project {request.ProjectId}");

        // Query ProjectMembers with includes
        var memberRepo = _unitOfWork.Repository<ProjectMember>();
        var members = await memberRepo.Query()
            .Include(m => m.User)
            .Include(m => m.InvitedBy)
            .Where(m => m.ProjectId == request.ProjectId)
            .OrderBy(m => m.User.FirstName)
            .ThenBy(m => m.User.LastName)
            .ToListAsync(cancellationToken);

        // Map to ProjectMemberDto
        var memberDtos = members.Select(m => new ProjectMemberDto(
            m.Id,
            m.UserId,
            m.User.Username,
            m.User.Email,
            m.Role,
            m.InvitedAt,
            $"{m.InvitedBy.FirstName} {m.InvitedBy.LastName}".Trim()
        )).ToList();

        return memberDtos;
    }
}

