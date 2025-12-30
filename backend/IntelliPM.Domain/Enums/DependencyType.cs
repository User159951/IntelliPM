namespace IntelliPM.Domain.Enums;

/// <summary>
/// Types of task dependencies in project management.
/// </summary>
public enum DependencyType
{
    /// <summary>
    /// Finish-to-Start: The dependent task cannot start until the source task finishes.
    /// This is the most common dependency type.
    /// </summary>
    FinishToStart = 1,

    /// <summary>
    /// Start-to-Start: The dependent task cannot start until the source task starts.
    /// </summary>
    StartToStart = 2,

    /// <summary>
    /// Finish-to-Finish: The dependent task cannot finish until the source task finishes.
    /// </summary>
    FinishToFinish = 3,

    /// <summary>
    /// Start-to-Finish: The dependent task cannot finish until the source task starts.
    /// This is the least common dependency type.
    /// </summary>
    StartToFinish = 4
}

