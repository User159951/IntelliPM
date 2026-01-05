using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Models;
using IntelliPM.Application.Identity.DTOs;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Organizations.Queries;

/// <summary>
/// Handler for GetOrganizationMembersQuery.
/// Returns a paginated list of members in the current user's organization (Admin only).
/// </summary>
public class GetOrganizationMembersQueryHandler : IRequestHandler<GetOrganizationMembersQuery, PagedResponse<UserListDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetOrganizationMembersQueryHandler> _logger;

    public GetOrganizationMembersQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<GetOrganizationMembersQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<PagedResponse<UserListDto>> Handle(GetOrganizationMembersQuery request, CancellationToken ct)
    {
        // Verify admin permissions
        if (!_currentUserService.IsAdmin())
        {
            throw new UnauthorizedException("Only administrators can view organization members");
        }

        var organizationId = _currentUserService.GetOrganizationId();
        if (organizationId == 0)
        {
            throw new UnauthorizedException("User not authenticated or organization not found");
        }

        var userRepo = _unitOfWork.Repository<User>();
        var query = userRepo.Query()
            .AsNoTracking()
            .Include(u => u.Organization)
            .Where(u => u.OrganizationId == organizationId);

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(u =>
                u.Email.ToLower().Contains(searchTerm) ||
                u.FirstName.ToLower().Contains(searchTerm) ||
                u.LastName.ToLower().Contains(searchTerm) ||
                u.Username.ToLower().Contains(searchTerm));
        }

        // Get total count
        var totalCount = await query.CountAsync(ct);

        // Apply pagination
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Max(1, Math.Min(request.PageSize, 100));

        var users = await query
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserListDto(
                u.Id,
                u.Username,
                u.Email,
                u.FirstName ?? string.Empty,
                u.LastName ?? string.Empty,
                u.GlobalRole,
                u.IsActive,
                u.OrganizationId,
                u.Organization.Name,
                u.CreatedAt,
                u.LastLoginAt
            ))
            .ToListAsync(ct);

        return new PagedResponse<UserListDto>(
            users,
            page,
            pageSize,
            totalCount
        );
    }
}

