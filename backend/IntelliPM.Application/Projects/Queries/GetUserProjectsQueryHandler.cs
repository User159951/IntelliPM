using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Models;
using IntelliPM.Application.Interfaces;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.Projects.Queries;

public class GetUserProjectsQueryHandler : IRequestHandler<GetUserProjectsQuery, PagedResponse<ProjectListDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    public GetUserProjectsQueryHandler(IUnitOfWork unitOfWork, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<PagedResponse<ProjectListDto>> Handle(GetUserProjectsQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"user-projects:{request.UserId}:page:{request.Page}:size:{request.PageSize}";

        // Try get from cache
        var cachedResponse = await _cache.GetAsync<PagedResponse<ProjectListDto>>(cacheKey, cancellationToken);
        if (cachedResponse != null)
        {
            return cachedResponse;
        }

        // Load from database
        var projectRepo = _unitOfWork.Repository<Project>();
        var query = projectRepo.Query()
            .AsNoTracking()
            .Include(p => p.Members)
                .ThenInclude(pm => pm.User)
            .Where(p => p.OwnerId == request.UserId || p.Members.Any(m => m.UserId == request.UserId));

        // Get total count for pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var projects = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var projectDtos = projects.Select(p => new ProjectListDto(
            p.Id,
            p.Name,
            p.Description,
            p.Type,
            p.Status,
            p.CreatedAt,
            p.Members.Select(pm => new ProjectMemberListDto(
                pm.UserId,
                pm.User.FirstName,
                pm.User.LastName,
                pm.User.Email,
                null // Avatar not implemented yet
            )).ToList()
        )).ToList();

        var response = new PagedResponse<ProjectListDto>(
            projectDtos,
            request.Page,
            request.PageSize,
            totalCount
        );

        // Cache for 5 minutes
        await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5), cancellationToken);

        return response;
    }
}

