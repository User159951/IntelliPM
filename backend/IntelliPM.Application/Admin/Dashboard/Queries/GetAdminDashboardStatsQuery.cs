using MediatR;
using IntelliPM.Application.Admin.SystemHealth.Queries;

namespace IntelliPM.Application.Admin.Dashboard.Queries;

public record GetAdminDashboardStatsQuery() : IRequest<AdminDashboardStatsDto>;

public record AdminDashboardStatsDto(
    int TotalUsers,
    int ActiveUsers,
    int InactiveUsers,
    int AdminCount,
    int UserCount,
    int TotalProjects,
    int ActiveProjects,
    int TotalOrganizations,
    List<UserGrowthDto> UserGrowth,
    List<RecentActivityDto> RecentActivities,
    SystemHealthDto SystemHealth
);

public record UserGrowthDto(
    string Month,
    int Count
);

public record RecentActivityDto(
    string Action,
    string UserName,
    DateTime Timestamp
);

