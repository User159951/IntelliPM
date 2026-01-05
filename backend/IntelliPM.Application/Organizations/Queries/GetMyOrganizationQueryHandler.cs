using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Organizations.Queries;

/// <summary>
/// Handler for GetMyOrganizationQuery.
/// Returns the current user's organization (Admin only).
/// </summary>
public class GetMyOrganizationQueryHandler : IRequestHandler<GetMyOrganizationQuery, OrganizationDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetMyOrganizationQueryHandler> _logger;

    public GetMyOrganizationQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<GetMyOrganizationQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<OrganizationDto> Handle(GetMyOrganizationQuery request, CancellationToken ct)
    {
        // Verify admin permissions
        if (!_currentUserService.IsAdmin())
        {
            throw new UnauthorizedException("Only administrators can view organization details");
        }

        var organizationId = _currentUserService.GetOrganizationId();
        if (organizationId == 0)
        {
            throw new UnauthorizedException("User not authenticated or organization not found");
        }

        var organization = await _unitOfWork.Repository<Organization>()
            .Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == organizationId, ct);

        if (organization == null)
        {
            throw new NotFoundException($"Organization {organizationId} not found");
        }

        // Get user count
        var userCount = await _unitOfWork.Repository<User>()
            .Query()
            .AsNoTracking()
            .CountAsync(u => u.OrganizationId == organizationId, ct);

        return new OrganizationDto(
            organization.Id,
            organization.Name,
            organization.Code,
            organization.CreatedAt,
            organization.UpdatedAt,
            userCount
        );
    }
}

