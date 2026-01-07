using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using IntelliPM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace IntelliPM.Infrastructure.BackgroundServices;

/// <summary>
/// Background service that processes scheduled AI quota changes.
/// Activates quotas when their EffectiveDate is reached.
/// </summary>
public class ScheduledQuotaProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<ScheduledQuotaProcessor> _logger;
    private const int CheckIntervalMinutes = 15; // Check every 15 minutes

    public ScheduledQuotaProcessor(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<ScheduledQuotaProcessor> logger)
    {
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async System.Threading.Tasks.Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ScheduledQuotaProcessor background service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessScheduledQuotasAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ScheduledQuotaProcessor main loop. Will retry in {Interval} minutes", CheckIntervalMinutes);
            }

            // Wait for the check interval before next iteration
            try
            {
                await System.Threading.Tasks.Task.Delay(TimeSpan.FromMinutes(CheckIntervalMinutes), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested - break the loop gracefully
                _logger.LogInformation("ScheduledQuotaProcessor cancellation requested, shutting down gracefully");
                break;
            }
        }

        _logger.LogInformation("ScheduledQuotaProcessor background service stopped");
    }

    private async System.Threading.Tasks.Task ProcessScheduledQuotasAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var now = DateTimeOffset.UtcNow;

        // Find scheduled quotas that are ready to be activated
        // - Not currently active (IsActive = false)
        // - Has EffectiveDate set
        // - EffectiveDate <= now
        var scheduledQuotas = await dbContext.AIQuotas
            .Where(q => !q.IsActive && q.EffectiveDate != null && q.EffectiveDate <= now)
            .Include(q => q.Organization)
            .ToListAsync(cancellationToken);

        if (scheduledQuotas.Count == 0)
        {
            _logger.LogDebug("No scheduled quotas ready to be activated");
            return;
        }

        _logger.LogInformation(
            "Found {Count} scheduled quota(s) ready to be activated",
            scheduledQuotas.Count);

        foreach (var scheduledQuota in scheduledQuotas)
        {
            try
            {
                await ActivateScheduledQuotaAsync(scheduledQuota, dbContext, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to activate scheduled quota {QuotaId} for organization {OrganizationId}. Error: {ErrorMessage}",
                    scheduledQuota.Id,
                    scheduledQuota.OrganizationId,
                    ex.Message);
            }
        }
    }

    private async System.Threading.Tasks.Task ActivateScheduledQuotaAsync(
        AIQuota scheduledQuota,
        AppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var organizationId = scheduledQuota.OrganizationId;
        var now = DateTimeOffset.UtcNow;

        _logger.LogInformation(
            "Activating scheduled quota {QuotaId} for organization {OrganizationId} (Tier: {TierName}, EffectiveDate: {EffectiveDate})",
            scheduledQuota.Id,
            organizationId,
            scheduledQuota.TierName,
            scheduledQuota.EffectiveDate);

        // Find current active quota for this organization
        var currentActiveQuota = await dbContext.AIQuotas
            .FirstOrDefaultAsync(
                q => q.OrganizationId == organizationId && q.IsActive,
                cancellationToken);

        // Deactivate current quota if exists
        if (currentActiveQuota != null)
        {
            currentActiveQuota.IsActive = false;
            currentActiveQuota.UpdatedAt = now;
            
            _logger.LogInformation(
                "Deactivated current quota {CurrentQuotaId} for organization {OrganizationId}",
                currentActiveQuota.Id,
                organizationId);
        }

        // Activate the scheduled quota
        scheduledQuota.IsActive = true;
        scheduledQuota.PeriodStartDate = scheduledQuota.EffectiveDate ?? now;
        scheduledQuota.PeriodEndDate = scheduledQuota.PeriodStartDate.AddDays(Domain.Constants.AIQuotaConstants.QuotaPeriodDays);
        scheduledQuota.UpdatedAt = now;

        // Create audit log for quota activation
        var auditLog = new AuditLog
        {
            UserId = 0, // System action
            Action = "ActivateScheduledQuota",
            EntityType = "AIQuota",
            EntityId = scheduledQuota.Id,
            EntityName = $"Scheduled quota for {scheduledQuota.Organization?.Name ?? $"Organization {organizationId}"}",
            Changes = JsonSerializer.Serialize(new
            {
                OrganizationId = organizationId,
                QuotaId = scheduledQuota.Id,
                TierName = scheduledQuota.TierName,
                EffectiveDate = scheduledQuota.EffectiveDate,
                ActivatedAt = now,
                PreviousQuotaId = currentActiveQuota?.Id,
                PreviousTier = currentActiveQuota?.TierName
            }),
            CreatedAt = now
        };

        dbContext.AuditLogs.Add(auditLog);

        // Save all changes
        var savedCount = await dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Successfully activated scheduled quota {QuotaId} for organization {OrganizationId}. " +
            "Previous quota {PreviousQuotaId} was deactivated. Audit log created.",
            scheduledQuota.Id,
            organizationId,
            currentActiveQuota?.Id);
    }
}

