namespace IntelliPM.Domain.Enums;

public static class UserRole
{
    public const string Admin = "Admin";
    public const string ProductOwner = "ProductOwner";
    public const string ProjectManager = "ProjectManager";
    public const string Developer = "Developer";
    public const string QA = "QA";
    public const string Viewer = "Viewer";

    public static readonly string[] AllRoles = { Admin, ProductOwner, ProjectManager, Developer, QA, Viewer };
}

