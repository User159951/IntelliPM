namespace IntelliPM.Domain.Constants;

/// <summary>
/// Constants for Release entity.
/// </summary>
public static class ReleaseConstants
{
    /// <summary>
    /// Maximum length for release name.
    /// </summary>
    public const int MaxNameLength = 200;

    /// <summary>
    /// Maximum length for release version.
    /// </summary>
    public const int MaxVersionLength = 50;

    /// <summary>
    /// Maximum length for release description.
    /// </summary>
    public const int MaxDescriptionLength = 2000;

    /// <summary>
    /// Maximum length for release notes.
    /// </summary>
    public const int MaxReleaseNotesLength = 5000;

    /// <summary>
    /// Maximum length for change log.
    /// </summary>
    public const int MaxChangeLogLength = 5000;

    /// <summary>
    /// Maximum length for tag name.
    /// </summary>
    public const int MaxTagNameLength = 100;

    /// <summary>
    /// Validation error messages.
    /// </summary>
    public static class Validation
    {
        public const string ErrorNameRequired = "Release name is required.";
        public const string ErrorVersionRequired = "Release version is required.";
        public const string ErrorVersionInvalid = "Release version must follow semantic versioning (e.g., 1.0.0).";
        public const string ErrorPlannedDateInPast = "Planned date cannot be in the past.";
        public const string ErrorDuplicateVersion = "A release with this version already exists for the project.";
    }

    /// <summary>
    /// Release notes section titles and formatting constants.
    /// </summary>
    public static class ReleaseNotes
    {
        public const string FeaturesSectionTitle = "✨ New Features";
        public const string BugFixesSectionTitle = "🐛 Bug Fixes";
        public const string ImprovementsSectionTitle = "🚀 Improvements";
        public const string StatisticsSectionTitle = "📊 Statistics";
        public const string SprintsSectionTitle = "📅 Sprints Included";
        public const string ContributorsSectionTitle = "👥 Contributors";
    }

    /// <summary>
    /// Quality gates constants and thresholds.
    /// </summary>
    public static class QualityGates
    {
        /// <summary>
        /// Minimum code coverage threshold (80%).
        /// </summary>
        public const int MinCodeCoverageThreshold = 80;

        /// <summary>
        /// Maximum number of open critical bugs allowed (0).
        /// </summary>
        public const int MaxOpenCriticalBugs = 0;

        /// <summary>
        /// Maximum number of open high priority bugs allowed (3).
        /// </summary>
        public const int MaxOpenHighPriorityBugs = 3;

        /// <summary>
        /// Error message when not all tasks are completed.
        /// </summary>
        public const string ErrorAllTasksNotCompleted = "Not all tasks are completed";

        /// <summary>
        /// Error message when open bugs exist.
        /// </summary>
        public const string ErrorOpenBugsExist = "Critical or high priority bugs are still open";

        /// <summary>
        /// Error message when manual approval is pending.
        /// </summary>
        public const string ErrorManualApprovalPending = "Manual approval is pending";
    }
}

