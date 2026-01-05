using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Models;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Organizations.Queries;

/// <summary>
/// Handler for GetOrganizationsQuery.
/// Returns a paginated list of organizations (SuperAdmin only).
/// </summary>
public class GetOrganizationsQueryHandler : IRequestHandler<GetOrganizationsQuery, PagedResponse<OrganizationDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetOrganizationsQueryHandler> _logger;

    public GetOrganizationsQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<GetOrganizationsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<PagedResponse<OrganizationDto>> Handle(GetOrganizationsQuery request, CancellationToken ct)
    {
        // Verify SuperAdmin permissions
        if (!_currentUserService.IsSuperAdmin())
        {
            throw new UnauthorizedException("Only SuperAdmin can list all organizations");
        }

        var orgRepo = _unitOfWork.Repository<Organization>();
        var query = orgRepo.Query().AsNoTracking();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(o => o.Name.ToLower().Contains(searchTerm) ||
                                     o.Code.ToLower().Contains(searchTerm));
        }

        // Get total count
        var totalCount = await query.CountAsync(ct);

        // Apply pagination
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Max(1, Math.Min(request.PageSize, 100));

        var organizations = await query
            .OrderBy(o => o.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        // Get user counts for each organization
        var orgIds = organizations.Select(o => o.Id).ToList();
        var userCounts = await _unitOfWork.Repository<User>()
            .Query()
            .AsNoTracking()
            .Where(u => orgIds.Contains(u.OrganizationId))
            .GroupBy(u => u.OrganizationId)
            .Select(g => new { OrganizationId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.OrganizationId, x => x.Count, ct);

        var orgDtos = organizations.Select(o => new OrganizationDto(
            o.Id,
            o.Name,
            o.Code,
            o.CreatedAt,
            o.UpdatedAt,
            userCounts.GetValueOrDefault(o.Id, 0)
        )).ToList();

        return new PagedResponse<OrganizationDto>(
            orgDtos,
            page,
            pageSize,
            totalCount
        );
    }
}

