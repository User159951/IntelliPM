namespace IntelliPM.Domain.Constants;

/// <summary>
/// Constants for Task entity
/// </summary>
public static class TaskConstants
{
    public static class Statuses
    {
        public const string Todo = "Todo";
        public const string InProgress = "InProgress";
        public const string Review = "Review";
        public const string Blocked = "Blocked";
        public const string Done = "Done";
        
        public static readonly string[] All = { Todo, InProgress, Review, Blocked, Done };
    }
    
    public static class Priorities
    {
        public const string Low = "Low";
        public const string Medium = "Medium";
        public const string High = "High";
        public const string Critical = "Critical";
        
        public static readonly string[] All = { Low, Medium, High, Critical };
    }
    
    public static class Validation
    {
        public const int TitleMaxLength = 200;
        public const int DescriptionMaxLength = 5000;
        public const int MinStoryPoints = 0;
        public const int MaxStoryPoints = 100;
    }
}

