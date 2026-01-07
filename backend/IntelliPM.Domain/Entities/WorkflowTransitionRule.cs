using IntelliPM.Domain.Interfaces;
using System.Text.Json;

namespace IntelliPM.Domain.Entities;

/// <summary>
/// Defines rules for workflow status transitions, specifying which roles can perform
/// transitions and what conditions must be met.
/// </summary>
public class WorkflowTransitionRule : IAggregateRoot
{
    public int Id { get; set; }

    /// <summary>
    /// Type of entity this rule applies to (Task, Sprint, Release).
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Source status for the transition.
    /// </summary>
    public string FromStatus { get; set; } = string.Empty;

    /// <summary>
    /// Target status for the transition.
    /// </summary>
    public string ToStatus { get; set; } = string.Empty;

    /// <summary>
    /// JSON array of allowed project roles that can perform this transition.
    /// Example: ["Developer", "Tester", "ScrumMaster"]
    /// </summary>
    public string AllowedRolesJson { get; set; } = "[]";

    /// <summary>
    /// JSON array of required conditions that must be met for this transition.
    /// Example: ["QAApproval", "AllDependenciesCompleted"]
    /// </summary>
    public string RequiredConditionsJson { get; set; } = "[]";

    /// <summary>
    /// Whether this rule is active. Inactive rules are ignored.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Optional description of this rule.
    /// </summary>
    public string? Description { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets the allowed roles as a list.
    /// </summary>
    public List<string> GetAllowedRoles()
    {
        if (string.IsNullOrWhiteSpace(AllowedRolesJson) || AllowedRolesJson == "[]")
            return new List<string>();

        try
        {
            return JsonSerializer.Deserialize<List<string>>(AllowedRolesJson) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    /// <summary>
    /// Sets the allowed roles from a list.
    /// </summary>
    public void SetAllowedRoles(List<string> roles)
    {
        AllowedRolesJson = JsonSerializer.Serialize(roles);
    }

    /// <summary>
    /// Gets the required conditions as a list.
    /// </summary>
    public List<string> GetRequiredConditions()
    {
        if (string.IsNullOrWhiteSpace(RequiredConditionsJson) || RequiredConditionsJson == "[]")
            return new List<string>();

        try
        {
            return JsonSerializer.Deserialize<List<string>>(RequiredConditionsJson) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    /// <summary>
    /// Sets the required conditions from a list.
    /// </summary>
    public void SetRequiredConditions(List<string> conditions)
    {
        RequiredConditionsJson = JsonSerializer.Serialize(conditions);
    }
}

