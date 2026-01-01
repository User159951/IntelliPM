namespace IntelliPM.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when AI features are disabled for an organization.
/// </summary>
public class AIDisabledException : ApplicationException
{
    /// <summary>
    /// Organization ID for which AI is disabled.
    /// </summary>
    public int OrganizationId { get; }

    /// <summary>
    /// Reason why AI was disabled.
    /// </summary>
    public string Reason { get; }

    public AIDisabledException(
        string message,
        int organizationId,
        string reason)
        : base(message)
    {
        OrganizationId = organizationId;
        Reason = reason;
    }
}

