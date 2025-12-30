using IntelliPM.Domain.Enums;
using System.Security.Cryptography;

namespace IntelliPM.Domain.Entities;

/// <summary>
/// Represents an invitation to join an organization with a specific role.
/// Invitations expire after 72 hours and can only be used once.
/// </summary>
public class OrganizationInvitation
{
    /// <summary>
    /// Unique identifier for the invitation.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Email address of the person being invited.
    /// </summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>
    /// Global role to assign to the user upon acceptance (Admin or User).
    /// </summary>
    public GlobalRole Role { get; private set; }

    /// <summary>
    /// Organization ID for multi-tenancy.
    /// </summary>
    public int OrganizationId { get; private set; }

    /// <summary>
    /// ID of the admin user who sent the invitation.
    /// </summary>
    public int InvitedById { get; private set; }

    /// <summary>
    /// Unique, secure random token used to accept the invitation.
    /// </summary>
    public string Token { get; private set; } = string.Empty;

    /// <summary>
    /// Date and time when the invitation expires (72 hours from creation).
    /// </summary>
    public DateTime ExpiresAt { get; private set; }

    /// <summary>
    /// Date and time when the invitation was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Date and time when the invitation was accepted.
    /// Null if the invitation has not been accepted yet.
    /// </summary>
    public DateTime? AcceptedAt { get; private set; }

    /// <summary>
    /// Indicates whether the invitation has been used/accepted.
    /// Default is false.
    /// </summary>
    public bool IsUsed { get; private set; }

    /// <summary>
    /// Private parameterless constructor for Entity Framework Core.
    /// EF Core requires a parameterless constructor for materialization.
    /// </summary>
    private OrganizationInvitation()
    {
        // Required by EF Core
    }

    /// <summary>
    /// Creates a new organization invitation.
    /// </summary>
    /// <param name="email">Email address of the person being invited</param>
    /// <param name="role">Global role to assign (Admin or User)</param>
    /// <param name="organizationId">Organization ID for multi-tenancy</param>
    /// <param name="invitedById">ID of the admin user who sent the invitation</param>
    /// <returns>A new OrganizationInvitation instance</returns>
    /// <exception cref="ArgumentNullException">Thrown when email is null or empty</exception>
    public static OrganizationInvitation Create(string email, GlobalRole role, int organizationId, int invitedById)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentNullException(nameof(email), "Email cannot be null or empty");
        }

        var now = DateTime.UtcNow;
        var token = GenerateSecureToken();

        return new OrganizationInvitation
        {
            Id = Guid.NewGuid(),
            Email = email,
            Role = role,
            OrganizationId = organizationId,
            InvitedById = invitedById,
            Token = token,
            ExpiresAt = now.AddHours(72), // 72 hours from creation
            CreatedAt = now,
            AcceptedAt = null,
            IsUsed = false
        };
    }

    /// <summary>
    /// Marks the invitation as accepted.
    /// Sets AcceptedAt to the current time and IsUsed to true.
    /// </summary>
    public void MarkAsAccepted()
    {
        AcceptedAt = DateTime.UtcNow;
        IsUsed = true;
    }

    /// <summary>
    /// Indicates whether the invitation has expired.
    /// </summary>
    /// <returns>True if the current time is past the ExpiresAt time, false otherwise</returns>
    public bool IsExpired() => DateTime.UtcNow > ExpiresAt;

    /// <summary>
    /// Indicates whether the invitation can be accepted.
    /// An invitation can be accepted if it hasn't been used and hasn't expired.
    /// </summary>
    /// <returns>True if the invitation can be accepted, false otherwise</returns>
    public bool CanBeAccepted() => !IsUsed && !IsExpired();

    /// <summary>
    /// Generates a secure random token for the invitation.
    /// Uses cryptographically secure random number generation with Base64Url encoding.
    /// </summary>
    /// <returns>A secure random token string</returns>
    private static string GenerateSecureToken()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[32];
        rng.GetBytes(bytes);
        // Use Base64Url encoding (URL-safe base64)
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .Replace("=", "");
    }
}

