using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Organizations.Queries;

/// <summary>
/// Handler for GetOrganizationByIdQuery.
/// Returns a single organization by ID (SuperAdmin only).
/// </summary>
public class GetOrganizationByIdQueryHandler : IRequestHandler<GetOrganizationByIdQuery, OrganizationDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetOrganizationByIdQueryHandler> _logger;

    public GetOrganizationByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<GetOrganizationByIdQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<OrganizationDto> Handle(GetOrganizationByIdQuery request, CancellationToken ct)
    {
        // Verify SuperAdmin permissions
        if (!_currentUserService.IsSuperAdmin())
        {
            throw new UnauthorizedException("Only SuperAdmin can view organization details");
        }

        var organization = await _unitOfWork.Repository<Organization>()
            .Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == request.OrganizationId, ct);

        if (organization == null)
        {
            throw new NotFoundException($"Organization {request.OrganizationId} not found");
        }

        // Get user count
        var userCount = await _unitOfWork.Repository<User>()
            .Query()
            .AsNoTracking()
            .CountAsync(u => u.OrganizationId == request.OrganizationId, ct);

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

