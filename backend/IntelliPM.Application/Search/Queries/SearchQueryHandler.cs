using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Search.Queries;

public class SearchQueryHandler : IRequestHandler<SearchQuery, SearchResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<SearchQueryHandler> _logger;

    public SearchQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<SearchQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<SearchResponse> Handle(SearchQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
        {
            return new SearchResponse(new List<SearchResultDto>());
        }

        // CRITICAL SECURITY: Extract organization ID from current user context
        // This MUST be applied before any search logic to prevent cross-tenant data exposure
        var userOrganizationId = _currentUserService.GetOrganizationId();
        
        if (userOrganizationId == 0)
        {
            _logger.LogWarning("Search attempted by user without organization context. UserId: {UserId}", _currentUserService.GetUserId());
            return new SearchResponse(new List<SearchResultDto>());
        }

        var query = request.Query.Trim().ToLower();
        _logger.LogDebug(
            "Performing search query: {Query}, OrganizationId: {OrganizationId}, Limit: {Limit}",
            request.Query,
            userOrganizationId,
            request.Limit);

        var results = new List<SearchResultDto>();

        // Search Projects - MANDATORY filter by OrganizationId
        var projectRepo = _unitOfWork.Repository<Project>();
        var projects = await projectRepo.Query()
            .AsNoTracking()
            .Where(p => p.OrganizationId == userOrganizationId) // SECURITY: Filter by org FIRST
            .Where(p => p.Name.ToLower().Contains(query) || 
                       (p.Description != null && p.Description.ToLower().Contains(query)))
            .Take(5)
            .Select(p => new SearchResultDto(
                "project",
                p.Id,
                p.Name,
                p.Description,
                $"Project • {p.Type}",
                $"/projects/{p.Id}"
            ))
            .ToListAsync(cancellationToken);

        results.AddRange(projects);
        _logger.LogDebug("Found {Count} projects matching search query", projects.Count);

        // Search Tasks - Filter by OrganizationId (Tasks have OrganizationId directly)
        var taskRepo = _unitOfWork.Repository<ProjectTask>();
        var tasks = await taskRepo.Query()
            .AsNoTracking()
            .Include(t => t.Project)
            .Where(t => t.OrganizationId == userOrganizationId) // SECURITY: Filter by org FIRST
            .Where(t => t.Title.ToLower().Contains(query) ||
                       t.Id.ToString().Contains(query))
            .Take(5)
            .Select(t => new SearchResultDto(
                "task",
                t.Id,
                t.Title,
                t.Description,
                $"Task #{t.Id} • {t.Project.Name}",
                $"/projects/{t.ProjectId}?task={t.Id}"
            ))
            .ToListAsync(cancellationToken);

        results.AddRange(tasks);
        _logger.LogDebug("Found {Count} tasks matching search query", tasks.Count);

        // Search Comments - Filter by OrganizationId
        var commentRepo = _unitOfWork.Repository<Comment>();
        var comments = await commentRepo.Query()
            .AsNoTracking()
            .Where(c => c.OrganizationId == userOrganizationId) // SECURITY: Filter by org FIRST
            .Where(c => !c.IsDeleted && // Exclude soft-deleted comments
                       c.Content.ToLower().Contains(query))
            .OrderByDescending(c => c.CreatedAt)
            .Take(5)
            .Select(c => new SearchResultDto(
                "comment",
                c.Id,
                c.Content.Length > 100 ? c.Content.Substring(0, 100) + "..." : c.Content,
                $"Comment on {c.EntityType} #{c.EntityId}",
                $"Comment • {c.EntityType}",
                null // Comments don't have direct URLs, they're shown in context
            ))
            .ToListAsync(cancellationToken);

        results.AddRange(comments);
        _logger.LogDebug("Found {Count} comments matching search query", comments.Count);

        // Search Users - Filter by OrganizationId (users belong to organizations)
        var userRepo = _unitOfWork.Repository<User>();
        var users = await userRepo.Query()
            .AsNoTracking()
            .Where(u => u.OrganizationId == userOrganizationId) // SECURITY: Filter by org FIRST
            .Where(u => u.IsActive &&
                       (u.FirstName.ToLower().Contains(query) ||
                        u.LastName.ToLower().Contains(query) ||
                        u.Username.ToLower().Contains(query) ||
                        u.Email.ToLower().Contains(query)))
            .Take(5)
            .Select(u => new SearchResultDto(
                "user",
                u.Id,
                $"{u.FirstName} {u.LastName}",
                u.Email,
                $"User • {u.Username}",
                null // Users don't have a detail page yet
            ))
            .ToListAsync(cancellationToken);

        results.AddRange(users);
        _logger.LogDebug("Found {Count} users matching search query", users.Count);

        // Limit total results
        var limitedResults = results.Take(request.Limit).ToList();

        _logger.LogInformation(
            "Search completed. Query: {Query}, OrganizationId: {OrganizationId}, TotalResults: {TotalResults}",
            request.Query,
            userOrganizationId,
            limitedResults.Count);

        return new SearchResponse(limitedResults);
    }
}
