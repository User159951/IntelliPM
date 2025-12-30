using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.Search.Queries;

public class SearchQueryHandler : IRequestHandler<SearchQuery, SearchResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public SearchQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<SearchResponse> Handle(SearchQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
        {
            return new SearchResponse(new List<SearchResultDto>());
        }

        var query = request.Query.Trim().ToLower();
        var results = new List<SearchResultDto>();

        // Search Projects
        var projectRepo = _unitOfWork.Repository<Project>();
        var projects = await projectRepo.Query()
            .AsNoTracking()
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

        // Search Tasks
        var taskRepo = _unitOfWork.Repository<ProjectTask>();
        var tasks = await taskRepo.Query()
            .AsNoTracking()
            .Include(t => t.Project)
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

        // Search Users
        var userRepo = _unitOfWork.Repository<User>();
        var users = await userRepo.Query()
            .AsNoTracking()
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

        // Limit total results
        var limitedResults = results.Take(request.Limit).ToList();

        return new SearchResponse(limitedResults);
    }
}
