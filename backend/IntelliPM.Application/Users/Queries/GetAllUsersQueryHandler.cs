using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Models;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.Users.Queries;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, PagedResponse<UserListDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetAllUsersQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResponse<UserListDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var userRepo = _unitOfWork.Repository<User>();
        var projectMemberRepo = _unitOfWork.Repository<ProjectMember>();
        var organizationId = _currentUserService.GetOrganizationId();

        // Filter by organization (multi-tenancy)
        var query = userRepo.Query()
            .AsNoTracking()
            .Include(u => u.Organization)
            .Where(u => u.OrganizationId == organizationId);

        // Apply filters
        if (request.Role.HasValue)
        {
            query = query.Where(u => u.GlobalRole == request.Role.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == request.IsActive.Value);
        }

        // Get total count for pagination (before sorting)
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "name" or "firstname" => request.SortDescending
                ? query.OrderByDescending(u => u.FirstName).ThenByDescending(u => u.LastName)
                : query.OrderBy(u => u.FirstName).ThenBy(u => u.LastName),
            "email" => request.SortDescending
                ? query.OrderByDescending(u => u.Email)
                : query.OrderBy(u => u.Email),
            "role" or "globalrole" => request.SortDescending
                ? query.OrderByDescending(u => u.GlobalRole)
                : query.OrderBy(u => u.GlobalRole),
            "createdat" or "created" => request.SortDescending
                ? query.OrderByDescending(u => u.CreatedAt)
                : query.OrderBy(u => u.CreatedAt),
            "status" or "isactive" => request.SortDescending
                ? query.OrderByDescending(u => u.IsActive)
                : query.OrderBy(u => u.IsActive),
            _ => query.OrderBy(u => u.FirstName).ThenBy(u => u.LastName) // Default sorting
        };

        // Apply pagination
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Max(1, Math.Min(request.PageSize, 100)); // Max 100 per page

        var users = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new
            {
                u.Id,
                u.Username,
                u.Email,
                u.FirstName,
                u.LastName,
                u.GlobalRole,
                u.OrganizationId,
                OrganizationName = u.Organization.Name,
                u.CreatedAt,
                u.IsActive,
                u.LastLoginAt
            })
            .ToListAsync(cancellationToken);

        // Get project counts for each user
        var userIds = users.Select(u => u.Id).ToList();
        var projectCounts = await projectMemberRepo.Query()
            .AsNoTracking()
            .Where(pm => userIds.Contains(pm.UserId))
            .GroupBy(pm => pm.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.UserId, x => x.Count, cancellationToken);

        var userDtos = users.Select(u => new UserListDto(
            u.Id,
            u.Username,
            u.Email,
            u.FirstName,
            u.LastName,
            u.GlobalRole.ToString(),
            u.OrganizationId,
            u.OrganizationName,
            u.CreatedAt,
            u.IsActive,
            projectCounts.GetValueOrDefault(u.Id, 0),
            u.LastLoginAt
        )).ToList();

        return new PagedResponse<UserListDto>(
            userDtos,
            page,
            pageSize,
            totalCount
        );
    }
}
