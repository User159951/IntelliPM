using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Models;
using IntelliPM.Application.Identity.DTOs;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Identity.Queries;

/// <summary>
/// Handler for GetUsersQuery that retrieves a paginated list of users with filtering, sorting, and search.
/// </summary>
public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PagedResponse<UserListDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetUsersQueryHandler> _logger;

    public GetUsersQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<GetUsersQueryHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PagedResponse<UserListDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var organizationId = _currentUserService.GetOrganizationId();

        _logger.LogInformation(
            "Retrieving users - Page: {Page}, PageSize: {PageSize}, Role: {Role}, IsActive: {IsActive}, SortField: {SortField}, SortDescending: {SortDescending}, SearchTerm: {SearchTerm}",
            request.Page,
            request.PageSize,
            request.Role,
            request.IsActive,
            request.SortField,
            request.SortDescending,
            request.SearchTerm);

        var userRepo = _unitOfWork.Repository<User>();

        // Build base query with organization filter (multi-tenancy)
        var query = userRepo.Query()
            .AsNoTracking()
            .Include(u => u.Organization)
            .Where(u => u.OrganizationId == organizationId);

        // Apply role filter
        if (request.Role.HasValue)
        {
            query = query.Where(u => u.GlobalRole == request.Role.Value);
        }

        // Apply active status filter
        if (request.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == request.IsActive.Value);
        }

        // Apply search term filter (case-insensitive search in username, email, firstName, lastName)
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.Trim();
            query = query.Where(u =>
                u.Username.Contains(searchTerm) ||
                u.Email.Contains(searchTerm) ||
                (u.FirstName != null && u.FirstName.Contains(searchTerm)) ||
                (u.LastName != null && u.LastName.Contains(searchTerm)));
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        query = ApplySorting(query, request.SortField, request.SortDescending);

        // Apply pagination
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Max(1, Math.Min(request.PageSize, 100)); // Max 100 per page

        var users = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserListDto(
                u.Id,
                u.Username,
                u.Email,
                u.FirstName,
                u.LastName,
                u.GlobalRole,
                u.IsActive,
                u.OrganizationId,
                u.Organization.Name,
                u.CreatedAt,
                u.LastLoginAt
            ))
            .ToListAsync(cancellationToken);

        _logger.LogInformation(
            "Retrieved {Count} users out of {TotalCount} total users",
            users.Count,
            totalCount);

        return new PagedResponse<UserListDto>(
            users,
            page,
            pageSize,
            totalCount
        );
    }

    /// <summary>
    /// Applies sorting to the query based on the sort field and direction.
    /// </summary>
    private static IQueryable<User> ApplySorting(IQueryable<User> query, string? sortField, bool sortDescending)
    {
        var normalizedSortField = sortField?.Trim().ToLowerInvariant();

        return normalizedSortField switch
        {
            "username" => sortDescending
                ? query.OrderByDescending(u => u.Username)
                : query.OrderBy(u => u.Username),

            "email" => sortDescending
                ? query.OrderByDescending(u => u.Email)
                : query.OrderBy(u => u.Email),

            "createdat" or "created" => sortDescending
                ? query.OrderByDescending(u => u.CreatedAt)
                : query.OrderBy(u => u.CreatedAt),

            "lastloginat" or "lastlogin" => sortDescending
                ? query.OrderByDescending(u => u.LastLoginAt ?? DateTimeOffset.MinValue)
                : query.OrderBy(u => u.LastLoginAt ?? DateTimeOffset.MinValue),

            "role" or "globalrole" => sortDescending
                ? query.OrderByDescending(u => u.GlobalRole)
                : query.OrderBy(u => u.GlobalRole),

            "isactive" or "status" => sortDescending
                ? query.OrderByDescending(u => u.IsActive)
                : query.OrderBy(u => u.IsActive),

            _ => query.OrderByDescending(u => u.CreatedAt) // Default: CreatedAt descending
        };
    }
}

