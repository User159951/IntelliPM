using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Services;
using IntelliPM.Application.Admin.SystemHealth.Queries;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Admin.Dashboard.Queries;

public class GetAdminDashboardStatsQueryHandler : IRequestHandler<GetAdminDashboardStatsQuery, AdminDashboardStatsDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly OrganizationScopingService _scopingService;
    private readonly IMediator _mediator;
    private readonly ILogger<GetAdminDashboardStatsQueryHandler> _logger;

    public GetAdminDashboardStatsQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        OrganizationScopingService scopingService,
        IMediator mediator,
        ILogger<GetAdminDashboardStatsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _scopingService = scopingService;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<AdminDashboardStatsDto> Handle(GetAdminDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting GetAdminDashboardStatsQueryHandler");
            
            _logger.LogInformation("Getting repositories");
            var userRepo = _unitOfWork.Repository<User>();
            var projectRepo = _unitOfWork.Repository<Project>();
            var organizationRepo = _unitOfWork.Repository<Organization>();
            var activityRepo = _unitOfWork.Repository<IntelliPM.Domain.Entities.Activity>();
            _logger.LogInformation("Repositories retrieved successfully");

            // User statistics (with organization scoping)
            _logger.LogInformation("Getting user statistics");
            var usersQuery = userRepo.Query()
                .AsNoTracking();
            
            // Apply organization scoping (SuperAdmin sees all, Admin sees only their org)
            usersQuery = _scopingService.ApplyOrganizationScope(usersQuery);

            var totalUsers = await usersQuery.CountAsync(cancellationToken);
            _logger.LogInformation("Total users: {TotalUsers}", totalUsers);
            
            var activeUsers = await usersQuery.CountAsync(u => u.IsActive, cancellationToken);
            var inactiveUsers = totalUsers - activeUsers;
            var adminCount = await usersQuery.CountAsync(u => u.GlobalRole == GlobalRole.Admin, cancellationToken);
            var userCount = await usersQuery.CountAsync(u => u.GlobalRole == GlobalRole.User, cancellationToken);
            _logger.LogInformation("User statistics completed");

            // Project statistics (with organization scoping)
            _logger.LogInformation("Getting project statistics");
            var projectsQuery = projectRepo.Query()
                .AsNoTracking();
            
            // Apply organization scoping (SuperAdmin sees all, Admin sees only their org)
            projectsQuery = _scopingService.ApplyOrganizationScope(projectsQuery);

            var totalProjects = await projectsQuery.CountAsync(cancellationToken);
            var activeProjects = await projectsQuery.CountAsync(p => p.Status == "Active", cancellationToken);
            _logger.LogInformation("Project statistics completed: Total={Total}, Active={Active}", totalProjects, activeProjects);

            // Organization count
            // SuperAdmin sees all organizations, Admin sees only their own (1)
            _logger.LogInformation("Getting organization count");
            int totalOrganizations;
            if (_currentUserService.IsSuperAdmin())
            {
                totalOrganizations = await organizationRepo.Query()
                    .AsNoTracking()
                    .CountAsync(cancellationToken);
            }
            else
            {
                totalOrganizations = 1; // Admin only sees their own organization
            }
            _logger.LogInformation("Organization count: {Count}", totalOrganizations);

            // User growth (last 6 months)
            _logger.LogInformation("Getting user growth data");
            var sixMonthsAgo = DateTimeOffset.UtcNow.AddMonths(-6);
            // Fetch users first, then group in memory to avoid EF Core translation issues with DateTimeOffset
            var usersInPeriod = await usersQuery
                .Where(u => u.CreatedAt >= sixMonthsAgo)
                .Select(u => new { u.CreatedAt })
                .ToListAsync(cancellationToken);
            
            var userGrowth = usersInPeriod
                .GroupBy(u => new { 
                    Year = u.CreatedAt.Year, 
                    Month = u.CreatedAt.Month 
                })
                .Select(g => new UserGrowthDto(
                    $"{g.Key.Year}-{g.Key.Month:D2}",
                    g.Count()
                ))
                .OrderBy(g => g.Month)
                .ToList();
            _logger.LogInformation("User growth data retrieved: {Count} months", userGrowth.Count);

            // Fill in missing months with 0
            var allMonths = new List<UserGrowthDto>();
            for (int i = 5; i >= 0; i--)
            {
                var date = DateTime.UtcNow.AddMonths(-i);
                var monthKey = $"{date.Year}-{date.Month:D2}";
                var existing = userGrowth.FirstOrDefault(g => g.Month == monthKey);
                allMonths.Add(existing ?? new UserGrowthDto(monthKey, 0));
            }

            // Recent activities (last 10) - filter by organization via Project
            // Get project IDs for the scoped organization(s) first, then filter activities
            _logger.LogInformation("Getting organization project IDs");
            var scopedProjectsQuery = projectRepo.Query()
                .AsNoTracking();
            
            // Apply organization scoping
            scopedProjectsQuery = _scopingService.ApplyOrganizationScope(scopedProjectsQuery);
            
            var organizationProjectIds = await scopedProjectsQuery
                .Select(p => p.Id)
                .ToListAsync(cancellationToken);
            _logger.LogInformation("Found {Count} projects for organization", organizationProjectIds.Count);

            List<RecentActivityDto> recentActivities;
            try
            {
                if (organizationProjectIds.Any())
                {
                    // Get activities with user info using a join query
                    var activities = await (from activity in activityRepo.Query().AsNoTracking()
                                            join user in userRepo.Query().AsNoTracking() 
                                                on activity.UserId equals user.Id into userGroup
                                            from user in userGroup.DefaultIfEmpty()
                                            where organizationProjectIds.Contains(activity.ProjectId)
                                            orderby activity.CreatedAt descending
                                            select new
                                            {
                                                ActivityType = activity.ActivityType,
                                                Username = user != null ? user.Username : "System",
                                                CreatedAt = activity.CreatedAt
                                            })
                                            .Take(10)
                                            .ToListAsync(cancellationToken);

                    recentActivities = activities.Select(a => new RecentActivityDto(
                        a.ActivityType ?? "Unknown",
                        a.Username ?? "System",
                        a.CreatedAt.UtcDateTime
                    )).ToList();
                }
                else
                {
                    recentActivities = new List<RecentActivityDto>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error retrieving recent activities, returning empty list: {Message}", ex.Message);
                recentActivities = new List<RecentActivityDto>();
            }

            // Get system health (handle errors gracefully)
            _logger.LogInformation("Getting system health");
            SystemHealthDto systemHealth;
            try
            {
                var systemHealthQuery = new GetSystemHealthQuery();
                systemHealth = await _mediator.Send(systemHealthQuery, cancellationToken);
                _logger.LogInformation("System health retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve system health, using default values: {Message}", ex.Message);
                // If system health fails, return a default healthy status
                systemHealth = new SystemHealthDto(
                    CpuUsage: 0.0,
                    MemoryUsage: 0.0,
                    TotalMemoryBytes: 0,
                    UsedMemoryBytes: 0,
                    AvailableMemoryBytes: 0,
                    DatabaseStatus: "Unknown",
                    DatabaseResponseTimeMs: "0",
                    ExternalServices: new Dictionary<string, ExternalServiceStatus>(),
                    DeadLetterQueueCount: 0,
                    Timestamp: DateTime.UtcNow
                );
            }

            _logger.LogInformation("Creating AdminDashboardStatsDto");
            var result = new AdminDashboardStatsDto(
                TotalUsers: totalUsers,
                ActiveUsers: activeUsers,
                InactiveUsers: inactiveUsers,
                AdminCount: adminCount,
                UserCount: userCount,
                TotalProjects: totalProjects,
                ActiveProjects: activeProjects,
                TotalOrganizations: totalOrganizations,
                UserGrowth: allMonths,
                RecentActivities: recentActivities,
                SystemHealth: systemHealth
            );
            _logger.LogInformation("AdminDashboardStatsDto created successfully");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetAdminDashboardStatsQueryHandler: {Message}\n{StackTrace}", 
                ex.Message, ex.StackTrace);
            throw;
        }
    }
}

