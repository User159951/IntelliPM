using MediatR;

namespace IntelliPM.Application.Organizations.Commands;

/// <summary>
/// Command to create a new organization (SuperAdmin only).
/// </summary>
public record CreateOrganizationCommand : IRequest<CreateOrganizationResponse>
{
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
}

/// <summary>
/// Response for creating an organization.
/// </summary>
public record CreateOrganizationResponse(
    int OrganizationId,
    string Name,
    string Code,
    DateTimeOffset CreatedAt
);

