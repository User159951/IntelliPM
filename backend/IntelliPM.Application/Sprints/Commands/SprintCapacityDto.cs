namespace IntelliPM.Application.Sprints.Commands;

/// <summary>
/// DTO for sprint capacity information.
/// </summary>
public record SprintCapacityDto(
    int TotalStoryPoints,
    int PlannedCapacity,
    int RemainingCapacity,
    decimal CapacityUtilization
);

