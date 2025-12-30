using IntelliPM.Domain.Interfaces;
using IntelliPM.Domain.Enums;

namespace IntelliPM.Domain.Entities;

/// <summary>
/// Task dependency entity representing relationships between tasks.
/// Defines dependencies where one task (SourceTask) depends on another task (DependentTask).
/// Supports multiple dependency types (Finish-to-Start, Start-to-Start, etc.) for flexible project planning.
/// </summary>
public class TaskDependency : IAggregateRoot
{
    /// <summary>
    /// Unique identifier for the task dependency.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The task that depends on another task (the task that cannot proceed until the dependent task is completed/started).
    /// </summary>
    public int SourceTaskId { get; set; }

    /// <summary>
    /// The task being depended upon (the task that must be completed/started before the source task can proceed).
    /// </summary>
    public int DependentTaskId { get; set; }

    /// <summary>
    /// Type of dependency relationship (Finish-to-Start, Start-to-Start, Finish-to-Finish, Start-to-Finish).
    /// </summary>
    public DependencyType DependencyType { get; set; } = DependencyType.FinishToStart;

    /// <summary>
    /// Organization ID for multi-tenancy isolation.
    /// </summary>
    public int OrganizationId { get; set; }

    /// <summary>
    /// Date and time when the dependency was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// ID of the user who created the dependency.
    /// </summary>
    public int CreatedById { get; set; }

    // Navigation properties

    /// <summary>
    /// Navigation property to the source task (the task that depends on another).
    /// </summary>
    public ProjectTask SourceTask { get; set; } = null!;

    /// <summary>
    /// Navigation property to the dependent task (the task being depended upon).
    /// </summary>
    public ProjectTask DependentTask { get; set; } = null!;

    /// <summary>
    /// Navigation property to the user who created the dependency.
    /// </summary>
    public User CreatedBy { get; set; } = null!;
}

