using Microsoft.Extensions.Diagnostics.HealthChecks;
using IntelliPM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Infrastructure.Health;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly AppDbContext _context;

    public DatabaseHealthCheck(AppDbContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Try to execute a simple query
            await _context.Database.ExecuteSqlRawAsync("SELECT 1", cancellationToken);
            
            // Check pending migrations
            var pendingMigrations = await _context.Database.GetPendingMigrationsAsync(cancellationToken);
            if (pendingMigrations.Any())
            {
                return HealthCheckResult.Degraded($"Database has {pendingMigrations.Count()} pending migrations");
            }

            return HealthCheckResult.Healthy("Database is accessible and up to date");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database is not accessible", ex);
        }
    }
}

