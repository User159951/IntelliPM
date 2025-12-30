namespace IntelliPM.Domain.Enums;

/// <summary>
/// Types of milestones in project management.
/// </summary>
public enum MilestoneType
{
    /// <summary>
    /// Software release milestone - marks a product release or version deployment.
    /// </summary>
    Release = 0,

    /// <summary>
    /// Sprint completion milestone - marks the completion of a sprint or iteration.
    /// </summary>
    Sprint = 1,

    /// <summary>
    /// Project deadline milestone - marks an important project deadline or milestone date.
    /// </summary>
    Deadline = 2,

    /// <summary>
    /// User-defined custom milestone - allows teams to create custom milestone types.
    /// </summary>
    Custom = 3
}

