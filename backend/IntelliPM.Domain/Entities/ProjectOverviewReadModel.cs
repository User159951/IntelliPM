using IntelliPM.Domain.Interfaces;
using System.Text.Json;

namespace IntelliPM.Domain.Entities;

/// <summary>
/// Read model for optimized project overview queries.
/// Denormalized view that aggregates project metrics, team statistics, and health indicators for fast dashboard rendering.
/// </summary>
public class ProjectOverviewReadModel : IAggregateRoot
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public int OrganizationId { get; set; }

    // Project basic info (denormalized)
    public string ProjectName { get; set; } = string.Empty;
    public string ProjectType { get; set; } = string.Empty; // Scrum, Kanban
    public string Status { get; set; } = string.Empty; // Active, Archived
    public int OwnerId { get; set; }
    public string OwnerName { get; set; } = string.Empty;

    // Team statistics
    public int TotalMembers { get; set; }
    public int ActiveMembers { get; set; }
    public string TeamMembersJson { get; set; } = "[]"; // JSON: List<MemberSummaryDto>

    // Sprint statistics
    public int TotalSprints { get; set; }
    public int ActiveSprintsCount { get; set; }
    public int CompletedSprintsCount { get; set; }
    public int? CurrentSprintId { get; set; }
    public string? CurrentSprintName { get; set; }

    // Task statistics
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int TodoTasks { get; set; }
    public int BlockedTasks { get; set; }
    public int OverdueTasks { get; set; }

    // Story points
    public int TotalStoryPoints { get; set; }
    public int CompletedStoryPoints { get; set; }
    public int RemainingStoryPoints { get; set; }

    // Defect statistics
    public int TotalDefects { get; set; }
    public int OpenDefects { get; set; }
    public int CriticalDefects { get; set; }

    // Velocity metrics
    public decimal AverageVelocity { get; set; }
    public decimal LastSprintVelocity { get; set; }
    public string VelocityTrendJson { get; set; } = "[]"; // JSON: List<VelocityTrendDto>

    // Health indicators
    public decimal ProjectHealth { get; set; } // 0-100 score
    public string HealthStatus { get; set; } = "Unknown"; // Excellent, Good, Fair, Poor
    public string RiskFactors { get; set; } = "[]"; // JSON: List<string>

    // Activity metrics
    public DateTimeOffset LastActivityAt { get; set; }
    public int ActivitiesLast7Days { get; set; }
    public int ActivitiesLast30Days { get; set; }

    // Progress metrics
    public decimal OverallProgress { get; set; } // 0-100
    public decimal SprintProgress { get; set; } // Current sprint progress
    public int DaysUntilNextMilestone { get; set; }

    // Milestone information
    public string? NextMilestoneName { get; set; }
    public DateTimeOffset? NextMilestoneDueDate { get; set; }
    public int UpcomingMilestonesCount { get; set; }

    // Metadata
    public DateTimeOffset LastUpdated { get; set; }
    public int Version { get; set; }

    // Navigation properties
    public Project Project { get; set; } = null!;

    // Helper methods for JSON serialization
    public List<MemberSummaryDto> GetTeamMembers()
    {
        return JsonSerializer.Deserialize<List<MemberSummaryDto>>(TeamMembersJson) ?? new();
    }

    public void SetTeamMembers(List<MemberSummaryDto> members)
    {
        TeamMembersJson = JsonSerializer.Serialize(members);
    }

    public List<VelocityTrendDto> GetVelocityTrend()
    {
        return JsonSerializer.Deserialize<List<VelocityTrendDto>>(VelocityTrendJson) ?? new();
    }

    public void SetVelocityTrend(List<VelocityTrendDto> trend)
    {
        VelocityTrendJson = JsonSerializer.Serialize(trend);
    }

    public List<string> GetRiskFactors()
    {
        return JsonSerializer.Deserialize<List<string>>(RiskFactors) ?? new();
    }

    public void SetRiskFactors(List<string> risks)
    {
        RiskFactors = JsonSerializer.Serialize(risks);
    }

    public void CalculateHealth()
    {
        decimal healthScore = 100;
        var riskFactors = new List<string>();

        // Reduce score based on various factors
        if (OverdueTasks > 0)
        {
            var overduePenalty = Math.Min(20, OverdueTasks * 2);
            healthScore -= overduePenalty;
            if (OverdueTasks > 5)
                riskFactors.Add($"High number of overdue tasks ({OverdueTasks})");
        }

        if (BlockedTasks > 0)
        {
            var blockedPenalty = Math.Min(15, BlockedTasks * 3);
            healthScore -= blockedPenalty;
            if (BlockedTasks > 3)
                riskFactors.Add($"Multiple blocked tasks ({BlockedTasks})");
        }

        if (CriticalDefects > 0)
        {
            var defectPenalty = Math.Min(25, CriticalDefects * 5);
            healthScore -= defectPenalty;
            riskFactors.Add($"Critical defects present ({CriticalDefects})");
        }

        if (ActivitiesLast7Days == 0)
        {
            healthScore -= 10;
            riskFactors.Add("No activity in the last 7 days");
        }

        if (AverageVelocity > 0 && LastSprintVelocity < AverageVelocity * 0.7m)
        {
            healthScore -= 10;
            riskFactors.Add("Velocity declining significantly");
        }

        if (TotalTasks > 0 && (decimal)BlockedTasks / TotalTasks > 0.2m)
        {
            healthScore -= 5;
            riskFactors.Add("High percentage of blocked tasks");
        }

        if (TotalDefects > 0 && (decimal)OpenDefects / TotalDefects > 0.5m)
        {
            healthScore -= 5;
            riskFactors.Add("High percentage of open defects");
        }

        ProjectHealth = Math.Max(0, Math.Min(100, healthScore));

        // Set health status
        HealthStatus = ProjectHealth switch
        {
            >= 80 => "Excellent",
            >= 60 => "Good",
            >= 40 => "Fair",
            _ => "Poor"
        };

        // Calculate progress
        OverallProgress = TotalTasks > 0
            ? Math.Round((decimal)CompletedTasks / TotalTasks * 100, 2)
            : 0;

        // Calculate remaining story points
        RemainingStoryPoints = TotalStoryPoints - CompletedStoryPoints;

        // Store risk factors
        SetRiskFactors(riskFactors);

        LastUpdated = DateTimeOffset.UtcNow;
    }
}

/// <summary>
/// DTO for team member summary data.
/// Contains essential member information for project overview.
/// </summary>
public class MemberSummaryDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int TasksAssigned { get; set; }
    public int TasksCompleted { get; set; }
}

/// <summary>
/// DTO for velocity trend data points.
/// Represents velocity over time for trend visualization.
/// </summary>
public class VelocityTrendDto
{
    public string SprintName { get; set; } = string.Empty;
    public int Velocity { get; set; }
    public DateTime Date { get; set; }
}

