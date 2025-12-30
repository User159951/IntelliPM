using IntelliPM.Domain.Interfaces;
using System.Text.Json;

namespace IntelliPM.Domain.Entities;

/// <summary>
/// Read model for optimized TaskBoard queries.
/// Denormalized view that aggregates task data by status for fast Kanban board rendering.
/// </summary>
public class TaskBoardReadModel : IAggregateRoot
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public int OrganizationId { get; set; } // Multi-tenancy

    // Aggregated counts by status
    public int TodoCount { get; set; }
    public int InProgressCount { get; set; }
    public int DoneCount { get; set; }
    public int TotalTaskCount { get; set; }

    // Aggregated story points by status
    public int TodoStoryPoints { get; set; }
    public int InProgressStoryPoints { get; set; }
    public int DoneStoryPoints { get; set; }
    public int TotalStoryPoints { get; set; }

    // Task data grouped by status (JSON serialized)
    public string TodoTasks { get; set; } = "[]"; // JSON: List<TaskSummaryDto>
    public string InProgressTasks { get; set; } = "[]";
    public string DoneTasks { get; set; } = "[]";

    // Metadata
    public DateTimeOffset LastUpdated { get; set; }
    public int Version { get; set; } // Optimistic concurrency

    // Navigation properties
    public Project Project { get; set; } = null!;

    // Helper methods for JSON serialization
    public List<TaskSummaryDto> GetTodoTasks()
    {
        return JsonSerializer.Deserialize<List<TaskSummaryDto>>(TodoTasks) ?? new();
    }

    public void SetTodoTasks(List<TaskSummaryDto> tasks)
    {
        TodoTasks = JsonSerializer.Serialize(tasks);
    }

    public List<TaskSummaryDto> GetInProgressTasks()
    {
        return JsonSerializer.Deserialize<List<TaskSummaryDto>>(InProgressTasks) ?? new();
    }

    public void SetInProgressTasks(List<TaskSummaryDto> tasks)
    {
        InProgressTasks = JsonSerializer.Serialize(tasks);
    }

    public List<TaskSummaryDto> GetDoneTasks()
    {
        return JsonSerializer.Deserialize<List<TaskSummaryDto>>(DoneTasks) ?? new();
    }

    public void SetDoneTasks(List<TaskSummaryDto> tasks)
    {
        DoneTasks = JsonSerializer.Serialize(tasks);
    }

    public void UpdateCounts()
    {
        TodoCount = GetTodoTasks().Count;
        InProgressCount = GetInProgressTasks().Count;
        DoneCount = GetDoneTasks().Count;
        TotalTaskCount = TodoCount + InProgressCount + DoneCount;

        TodoStoryPoints = GetTodoTasks().Sum(t => t.StoryPoints ?? 0);
        InProgressStoryPoints = GetInProgressTasks().Sum(t => t.StoryPoints ?? 0);
        DoneStoryPoints = GetDoneTasks().Sum(t => t.StoryPoints ?? 0);
        TotalStoryPoints = TodoStoryPoints + InProgressStoryPoints + DoneStoryPoints;
    }
}

/// <summary>
/// DTO for serialized task data in TaskBoardReadModel.
/// Contains only essential fields needed for Kanban board display.
/// </summary>
public class TaskSummaryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public int? StoryPoints { get; set; }
    public int? AssigneeId { get; set; }
    public string? AssigneeName { get; set; }
    public string? AssigneeAvatar { get; set; }
    public DateTimeOffset? DueDate { get; set; }
    public int DisplayOrder { get; set; }
}

