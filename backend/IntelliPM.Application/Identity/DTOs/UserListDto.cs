using IntelliPM.Domain.Enums;

namespace IntelliPM.Application.Identity.DTOs;

/// <summary>
/// DTO for user list display with essential user information.
/// </summary>
public record UserListDto(
    int Id,
    string Username,
    string Email,
    string? FirstName,
    string? LastName,
    GlobalRole Role,
    bool IsActive,
    int OrganizationId,
    string OrganizationName,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastLoginAt
);

