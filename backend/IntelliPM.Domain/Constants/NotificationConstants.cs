namespace IntelliPM.Domain.Constants;

/// <summary>
/// Constants for notification types, frequencies, and default preferences.
/// </summary>
public static class NotificationConstants
{
    /// <summary>
    /// Notification type constants.
    /// </summary>
    public static class Types
    {
        public const string TaskAssigned = "TaskAssigned";
        public const string TaskStatusChanged = "TaskStatusChanged";
        public const string TaskCommented = "TaskCommented";
        public const string Mentioned = "Mentioned";
        public const string ProjectInvite = "ProjectInvite";
        public const string SprintStarted = "SprintStarted";
        public const string SprintCompleted = "SprintCompleted";
        public const string DefectReported = "DefectReported";
        public const string DeadlineApproaching = "DeadlineApproaching";
    }

    /// <summary>
    /// Notification frequency constants.
    /// </summary>
    public static class Frequencies
    {
        public const string Instant = "Instant";
        public const string Daily = "Daily";
        public const string Weekly = "Weekly";
        public const string Never = "Never";
    }

    /// <summary>
    /// All available notification types.
    /// </summary>
    public static readonly string[] AllNotificationTypes =
    {
        Types.TaskAssigned,
        Types.TaskStatusChanged,
        Types.TaskCommented,
        Types.Mentioned,
        Types.ProjectInvite,
        Types.SprintStarted,
        Types.SprintCompleted,
        Types.DefectReported,
        Types.DeadlineApproaching
    };

    /// <summary>
    /// All available notification frequencies.
    /// </summary>
    public static readonly string[] AllFrequencies =
    {
        Frequencies.Instant,
        Frequencies.Daily,
        Frequencies.Weekly,
        Frequencies.Never
    };

    /// <summary>
    /// Default preferences for new users.
    /// Format: (EmailEnabled, InAppEnabled, Frequency)
    /// </summary>
    public static readonly Dictionary<string, (bool Email, bool InApp, string Frequency)> DefaultPreferences = new()
    {
        { Types.TaskAssigned, (true, true, Frequencies.Instant) },
        { Types.TaskStatusChanged, (false, true, Frequencies.Instant) },
        { Types.TaskCommented, (false, true, Frequencies.Instant) },
        { Types.Mentioned, (true, true, Frequencies.Instant) },
        { Types.ProjectInvite, (true, true, Frequencies.Instant) },
        { Types.SprintStarted, (true, true, Frequencies.Instant) },
        { Types.SprintCompleted, (true, true, Frequencies.Instant) },
        { Types.DefectReported, (true, true, Frequencies.Instant) },
        { Types.DeadlineApproaching, (true, true, Frequencies.Daily) },
    };
}

