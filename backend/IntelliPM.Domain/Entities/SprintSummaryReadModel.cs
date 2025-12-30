using IntelliPM.Domain.Interfaces;
using System.Text.Json;

namespace IntelliPM.Domain.Entities;

/// <summary>
/// Read model for optimized sprint summary queries.
/// Denormalized view that aggregates sprint metrics, velocity, and burndown data for fast dashboard rendering.
/// </summary>
public class SprintSummaryReadModel : IAggregateRoot
{
    public int Id { get; set; }
    public int SprintId { get; set; }
    public int ProjectId { get; set; }
    public int OrganizationId { get; set; }

    // Sprint basic info (denormalized)
    public string SprintName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // Active, Completed, Planned
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public int? PlannedCapacity { get; set; }

    // Task counts
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int TodoTasks { get; set; }

    // Story points
    public int TotalStoryPoints { get; set; }
    public int CompletedStoryPoints { get; set; }
    public int InProgressStoryPoints { get; set; }
    public int RemainingStoryPoints { get; set; }

    // Calculated metrics
    public decimal CompletionPercentage { get; set; } // CompletedTasks / TotalTasks * 100
    public decimal VelocityPercentage { get; set; } // CompletedStoryPoints / TotalStoryPoints * 100
    public decimal CapacityUtilization { get; set; } // TotalStoryPoints / PlannedCapacity * 100
    public int EstimatedDaysRemaining { get; set; } // Based on current velocity

    // Burndown data (JSON serialized)
    public string BurndownData { get; set; } = "[]"; // JSON: List<BurndownPointDto>

    // Team velocity data
    public decimal AverageVelocity { get; set; } // Average from previous sprints
    public bool IsOnTrack { get; set; } // Compare current progress to ideal burndown

    // Metadata
    public DateTimeOffset LastUpdated { get; set; }
    public int Version { get; set; }

    // Navigation properties
    public Sprint Sprint { get; set; } = null!;
    public Project Project { get; set; } = null!;

    // Helper methods for JSON serialization
    public List<BurndownPointDto> GetBurndownData()
    {
        return JsonSerializer.Deserialize<List<BurndownPointDto>>(BurndownData) ?? new();
    }

    public void SetBurndownData(List<BurndownPointDto> data)
    {
        BurndownData = JsonSerializer.Serialize(data);
    }

    public void RecalculateMetrics()
    {
        // Completion percentage
        CompletionPercentage = TotalTasks > 0
            ? Math.Round((decimal)CompletedTasks / TotalTasks * 100, 2)
            : 0;

        // Velocity percentage
        VelocityPercentage = TotalStoryPoints > 0
            ? Math.Round((decimal)CompletedStoryPoints / TotalStoryPoints * 100, 2)
            : 0;

        // Capacity utilization
        CapacityUtilization = PlannedCapacity.HasValue && PlannedCapacity > 0
            ? Math.Round((decimal)TotalStoryPoints / PlannedCapacity.Value * 100, 2)
            : 0;

        // Remaining points
        RemainingStoryPoints = TotalStoryPoints - CompletedStoryPoints;

        // On track check (compare to ideal burndown)
        var daysElapsed = (DateTime.UtcNow.Date - StartDate.Date).Days;
        var totalDays = (EndDate.Date - StartDate.Date).Days;
        var idealProgress = totalDays > 0 ? (decimal)daysElapsed / totalDays : 0;
        var actualProgress = VelocityPercentage / 100;
        IsOnTrack = actualProgress >= idealProgress * 0.9m; // Allow 10% tolerance

        // Estimated days remaining (based on current velocity)
        if (VelocityPercentage > 0 && daysElapsed > 0)
        {
            var dailyVelocity = (decimal)CompletedStoryPoints / daysElapsed;
            EstimatedDaysRemaining = dailyVelocity > 0
                ? (int)Math.Ceiling(RemainingStoryPoints / dailyVelocity)
                : 0;
        }
        else
        {
            EstimatedDaysRemaining = 0;
        }

        LastUpdated = DateTimeOffset.UtcNow;
    }
}

/// <summary>
/// DTO for burndown chart data points.
/// Represents remaining story points at a specific date for burndown visualization.
/// </summary>
public class BurndownPointDto
{
    public DateTime Date { get; set; }
    public int RemainingStoryPoints { get; set; }
    public int IdealRemainingPoints { get; set; }
}

