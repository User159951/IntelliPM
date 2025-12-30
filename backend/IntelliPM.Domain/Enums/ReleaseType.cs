namespace IntelliPM.Domain.Enums;

/// <summary>
/// Type of release based on semantic versioning and release purpose.
/// </summary>
public enum ReleaseType
{
    /// <summary>
    /// Major - Major version release with breaking changes (e.g., 2.0.0).
    /// </summary>
    Major = 0,

    /// <summary>
    /// Minor - Minor version release with new features (e.g., 1.1.0).
    /// </summary>
    Minor = 1,

    /// <summary>
    /// Patch - Patch release with bug fixes (e.g., 1.0.1).
    /// </summary>
    Patch = 2,

    /// <summary>
    /// Hotfix - Emergency hotfix release for critical issues (e.g., 1.0.1-hotfix).
    /// </summary>
    Hotfix = 3
}

