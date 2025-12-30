namespace IntelliPM.Domain.Constants;

/// <summary>
/// Constants for Comment entity configuration and validation.
/// </summary>
public static class CommentConstants
{
    /// <summary>
    /// Valid entity types that can have comments.
    /// </summary>
    public static class EntityTypes
    {
        public const string Task = "Task";
        public const string Project = "Project";
        public const string Sprint = "Sprint";
        public const string Defect = "Defect";
        public const string BacklogItem = "BacklogItem";
    }

    /// <summary>
    /// Maximum length for comment content in characters.
    /// </summary>
    public const int MaxContentLength = 5000;

    /// <summary>
    /// Maximum nesting level for comment replies (thread depth).
    /// </summary>
    public const int MaxNestingLevel = 3;

    /// <summary>
    /// Array of valid entity types for validation.
    /// </summary>
    public static readonly string[] ValidEntityTypes =
    {
        EntityTypes.Task,
        EntityTypes.Project,
        EntityTypes.Sprint,
        EntityTypes.Defect,
        EntityTypes.BacklogItem
    };
}

