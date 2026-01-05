using MediatR;

namespace IntelliPM.Application.Organizations.Commands;

/// <summary>
/// Command to update an organization (SuperAdmin only).
/// </summary>
public record UpdateOrganizationCommand : IRequest<UpdateOrganizationResponse>
{
    public int OrganizationId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
}

/// <summary>
/// Response for updating an organization.
/// </summary>
public record UpdateOrganizationResponse(
    int OrganizationId,
    string Name,
    string Code,
    DateTimeOffset UpdatedAt
);

