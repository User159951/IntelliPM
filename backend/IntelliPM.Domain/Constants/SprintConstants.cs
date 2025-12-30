namespace IntelliPM.Domain.Constants;

/// <summary>
/// Constants for Sprint entity
/// </summary>
public static class SprintConstants
{
    public static class Statuses
    {
        public const string NotStarted = "NotStarted";
        public const string Planned = "Planned";
        public const string Active = "Active";
        public const string Completed = "Completed";
        
        public static readonly string[] All = { NotStarted, Planned, Active, Completed };
    }
    
    public static class Validation
    {
        public const int GoalMaxLength = 500;
        public const int MinCapacity = 1;
        public const int MaxCapacity = 1000;
    }
}

