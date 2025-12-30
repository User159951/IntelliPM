using MediatR;
using IntelliPM.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Runtime;

namespace IntelliPM.Application.Admin.SystemHealth.Queries;

public class GetSystemHealthQueryHandler : IRequestHandler<GetSystemHealthQuery, SystemHealthDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetSystemHealthQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<SystemHealthDto> Handle(GetSystemHealthQuery request, CancellationToken cancellationToken)
    {
        // Ensure only admins can access system health
        if (!_currentUserService.IsAdmin())
        {
            throw new UnauthorizedAccessException("Only administrators can view system health.");
        }

        // Get CPU usage (simplified - in production, use PerformanceCounter or similar)
        var cpuUsage = GetCpuUsage();

        // Get Memory usage
        var memoryInfo = GetMemoryUsage();

        // Check database status
        var (dbStatus, dbResponseTime) = await CheckDatabaseHealth(cancellationToken);

        // Check external services (placeholder for now)
        var externalServices = new Dictionary<string, ExternalServiceStatus>
        {
            { "Email Service", new ExternalServiceStatus("Email Service", true, "Operational", null, DateTime.UtcNow) },
            { "Cache Service", new ExternalServiceStatus("Cache Service", true, "Operational", null, DateTime.UtcNow) },
        };

        // Get Dead Letter Queue count
        var dlqCount = await GetDeadLetterQueueCount(cancellationToken);

        return new SystemHealthDto(
            CpuUsage: cpuUsage,
            MemoryUsage: memoryInfo.UsedPercentage,
            TotalMemoryBytes: memoryInfo.TotalBytes,
            UsedMemoryBytes: memoryInfo.UsedBytes,
            AvailableMemoryBytes: memoryInfo.AvailableBytes,
            DatabaseStatus: dbStatus,
            DatabaseResponseTimeMs: dbResponseTime.ToString(),
            ExternalServices: externalServices,
            DeadLetterQueueCount: dlqCount,
            Timestamp: DateTime.UtcNow
        );
    }

    private double GetCpuUsage()
    {
        try
        {
            // Simplified CPU usage calculation
            // In production, use PerformanceCounter or System.Diagnostics.PerformanceCounter
            var process = Process.GetCurrentProcess();
            var startTime = DateTime.UtcNow;
            var startCpuUsage = process.TotalProcessorTime;
            
            Thread.Sleep(100); // Sample period
            
            var endTime = DateTime.UtcNow;
            var endCpuUsage = process.TotalProcessorTime;
            
            var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
            var totalMsPassed = (endTime - startTime).TotalMilliseconds;
            var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
            
            return Math.Min(100.0, cpuUsageTotal * 100);
        }
        catch
        {
            return 0.0; // Fallback if unable to measure
        }
    }

    private (double UsedPercentage, long TotalBytes, long UsedBytes, long AvailableBytes) GetMemoryUsage()
    {
        try
        {
            var process = Process.GetCurrentProcess();
            var usedBytes = process.WorkingSet64;
            
            // Get total system memory (simplified - Windows only)
            long totalBytes = 0;
            try
            {
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    // On Windows, we can use WMI or PerformanceCounter
                    // For now, use a simplified approach
                    var gcMemory = GC.GetTotalMemory(false);
                    totalBytes = Math.Max(usedBytes * 4, gcMemory * 10); // Rough estimate
                }
                else
                {
                    // Linux/Mac - would need different approach
                    totalBytes = usedBytes * 4; // Rough estimate
                }
            }
            catch
            {
                totalBytes = usedBytes * 4; // Fallback estimate
            }

            var availableBytes = Math.Max(0, totalBytes - usedBytes);
            var usedPercentage = totalBytes > 0 ? (usedBytes * 100.0 / totalBytes) : 0;

            return (usedPercentage, totalBytes, usedBytes, availableBytes);
        }
        catch
        {
            return (0.0, 0, 0, 0);
        }
    }

    private async Task<(string Status, int ResponseTimeMs)> CheckDatabaseHealth(CancellationToken cancellationToken)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            
            // Simple database connectivity check
            var userRepo = _unitOfWork.Repository<Domain.Entities.User>();
            var canConnect = await userRepo.Query()
                .AnyAsync(cancellationToken);
            
            stopwatch.Stop();
            var responseTime = (int)stopwatch.ElapsedMilliseconds;

            if (canConnect && responseTime < 1000)
            {
                return ("Healthy", responseTime);
            }
            else if (canConnect && responseTime < 5000)
            {
                return ("Degraded", responseTime);
            }
            else
            {
                return ("Unhealthy", responseTime);
            }
        }
        catch (Exception)
        {
            return ("Unhealthy", -1);
        }
    }

    private async Task<int> GetDeadLetterQueueCount(CancellationToken cancellationToken)
    {
        try
        {
            var dlqRepo = _unitOfWork.Repository<Domain.Entities.DeadLetterMessage>();
            var count = await dlqRepo.Query()
                .AsNoTracking()
                .CountAsync(cancellationToken);
            return count;
        }
        catch (Exception)
        {
            return 0; // Return 0 if unable to count
        }
    }
}

