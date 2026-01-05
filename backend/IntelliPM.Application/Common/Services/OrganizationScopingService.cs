using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace IntelliPM.Application.Common.Services;

/// <summary>
/// Service for enforcing organization scoping in queries and commands.
/// SuperAdmin can access all organizations; Admin can only access their own organization.
/// </summary>
public class OrganizationScopingService
{
    private readonly ICurrentUserService _currentUserService;

    public OrganizationScopingService(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Gets the effective organization ID for scoping.
    /// Returns 0 for SuperAdmin (all organizations), or the user's OrganizationId for Admin.
    /// </summary>
    public int GetScopedOrganizationId()
    {
        if (_currentUserService.IsSuperAdmin())
        {
            return 0; // 0 means "all organizations" for SuperAdmin
        }

        return _currentUserService.GetOrganizationId();
    }

    /// <summary>
    /// Ensures that the requested organization ID is accessible by the current user.
    /// Throws UnauthorizedException if Admin tries to access a different organization.
    /// </summary>
    /// <param name="requestedOrganizationId">The organization ID being accessed</param>
    /// <exception cref="UnauthorizedException">Thrown if Admin tries to access a different organization</exception>
    public void EnsureOrganizationAccess(int requestedOrganizationId)
    {
        if (_currentUserService.IsSuperAdmin())
        {
            // SuperAdmin can access any organization
            return;
        }

        var currentOrganizationId = _currentUserService.GetOrganizationId();
        if (currentOrganizationId == 0)
        {
            throw new UnauthorizedException("User not authenticated or organization not found");
        }

        if (requestedOrganizationId != currentOrganizationId)
        {
            throw new UnauthorizedException(
                $"Access denied. You can only access resources from your own organization (OrganizationId: {currentOrganizationId}).");
        }
    }

    /// <summary>
    /// Applies organization scoping to an IQueryable.
    /// SuperAdmin: no filter (returns all).
    /// Admin: filters by OrganizationId == current user's OrganizationId.
    /// </summary>
    /// <typeparam name="T">Entity type that has OrganizationId property</typeparam>
    /// <param name="query">The query to scope</param>
    /// <returns>Scoped query</returns>
    public IQueryable<T> ApplyOrganizationScope<T>(IQueryable<T> query) where T : class
    {
        if (_currentUserService.IsSuperAdmin())
        {
            // SuperAdmin can see all organizations
            return query;
        }

        var organizationId = _currentUserService.GetOrganizationId();
        if (organizationId == 0)
        {
            // User not authenticated or no organization - return empty query
            return query.Where(x => false);
        }

        // Build expression: x.OrganizationId == organizationId
        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(parameter, "OrganizationId");
        var constant = Expression.Constant(organizationId);
        var equality = Expression.Equal(property, constant);
        var lambda = Expression.Lambda<Func<T, bool>>(equality, parameter);

        return query.Where(lambda);
    }

    /// <summary>
    /// Checks if the current user can access the specified organization.
    /// </summary>
    /// <param name="organizationId">The organization ID to check</param>
    /// <returns>True if accessible, false otherwise</returns>
    public bool CanAccessOrganization(int organizationId)
    {
        if (_currentUserService.IsSuperAdmin())
        {
            return true;
        }

        return _currentUserService.GetOrganizationId() == organizationId;
    }
}

