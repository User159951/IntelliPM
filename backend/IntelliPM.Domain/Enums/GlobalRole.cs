namespace IntelliPM.Domain.Enums;

/// <summary>
/// Global role enumeration for user permissions.
/// - User: Standard user with limited permissions
/// - Admin: Organization administrator (manages only their own organization)
/// - SuperAdmin: System administrator (manages all organizations)
/// </summary>
public enum GlobalRole
{
    User = 1,
    Admin = 2,
    SuperAdmin = 3
}

