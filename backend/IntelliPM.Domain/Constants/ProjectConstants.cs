namespace IntelliPM.Domain.Constants;

/// <summary>
/// Constants for Project entity
/// </summary>
public static class ProjectConstants
{
    public static class Types
    {
        public const string Scrum = "Scrum";
        public const string Kanban = "Kanban";
        
        public static readonly string[] All = { Scrum, Kanban };
    }
    
    public static class Statuses
    {
        public const string Active = "Active";
        public const string Archived = "Archived";
        
        public static readonly string[] All = { Active, Archived };
    }
    
    public static class Validation
    {
        public const int NameMaxLength = 200;
        public const int DescriptionMaxLength = 2000;
        public const int MinSprintDuration = 1;
        public const int MaxSprintDuration = 30;
    }
}

