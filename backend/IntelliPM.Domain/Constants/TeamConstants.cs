namespace IntelliPM.Domain.Constants;

/// <summary>
/// Constants for Team entity
/// </summary>
public static class TeamConstants
{
    public static class Roles
    {
        public const string Lead = "Lead";
        public const string Member = "Member";
        public const string Admin = "Admin";
        
        public static readonly string[] All = { Lead, Member, Admin };
    }
    
    public static class Validation
    {
        public const int NameMaxLength = 200;
        public const int MinCapacity = 1;
        public const int MaxCapacity = 10000;
    }
}

