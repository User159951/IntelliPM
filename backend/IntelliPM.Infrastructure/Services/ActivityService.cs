using IntelliPM.Domain.Entities;
using IntelliPM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Infrastructure.Services;

/// <summary>
/// Service to create activity logs for user actions
/// </summary>
public class ActivityService
{
    private readonly AppDbContext _context;

    public ActivityService(AppDbContext context)
    {
        _context = context;
    }

    public async System.Threading.Tasks.Task CreateActivityAsync(
        int userId,
        string activityType,
        string entityType,
        int entityId,
        int projectId,
        string? entityName = null,
        string? metadata = null,
        CancellationToken cancellationToken = default)
    {
        // Get project name for caching
        var project = await _context.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

        var activity = new Activity
        {
            UserId = userId,
            ActivityType = activityType,
            EntityType = entityType,
            EntityId = entityId,
            EntityName = entityName,
            ProjectId = projectId,
            ProjectName = project?.Name,
            Metadata = metadata,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        _context.Activities.Add(activity);
        // Don't await SaveChanges - let the calling handler save it in the same transaction
    }
}
