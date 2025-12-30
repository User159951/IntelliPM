namespace IntelliPM.Domain.Constants;

/// <summary>
/// Constants for Milestone entity
/// </summary>
public static class MilestoneConstants
{
    /// <summary>
    /// Maximum length for milestone name.
    /// </summary>
    public const int MaxNameLength = 200;

    /// <summary>
    /// Maximum length for milestone description.
    /// </summary>
    public const int MaxDescriptionLength = 1000;

    /// <summary>
    /// Minimum progress value (0%).
    /// </summary>
    public const int MinProgress = 0;

    /// <summary>
    /// Maximum progress value (100%).
    /// </summary>
    public const int MaxProgress = 100;

    /// <summary>
    /// Default progress value when milestone is created.
    /// </summary>
    public const int DefaultProgress = 0;

    /// <summary>
    /// Validation error messages
    /// </summary>
    public static class Validation
    {
        public const string NameRequired = "Milestone name is required.";
        public const string NameMaxLength = "Milestone name cannot exceed 200 characters.";
        public const string DescriptionMaxLength = "Milestone description cannot exceed 1000 characters.";
        public const string DueDateRequired = "Milestone due date is required.";
        public const string ProgressRange = "Progress must be between 0 and 100.";
        public const string ErrorDueDateInPast = "Due date must be in the future.";
        public const string ErrorDueDateBeforeProjectStart = "Due date cannot be before project start date.";
        public const string ErrorDueDateAfterProjectEnd = "Due date cannot be after project end date.";
        public const string ErrorInvalidStatusTransition = "Invalid status transition from {0} to {1}.";
        public const string ErrorCompletedAtInFuture = "Completion date cannot be in the future.";
        public const string ErrorCompletedAtBeforeCreated = "Completion date cannot be before creation date.";
        public const string ErrorCompletedWithoutCompletedAt = "Completed milestone must have completion date.";
    }
}

