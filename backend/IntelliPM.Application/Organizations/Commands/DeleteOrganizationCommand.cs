using MediatR;

namespace IntelliPM.Application.Organizations.Commands;

/// <summary>
/// Command to delete an organization (SuperAdmin only).
/// </summary>
public record DeleteOrganizationCommand : IRequest<DeleteOrganizationResponse>
{
    public int OrganizationId { get; init; }
}

/// <summary>
/// Response for deleting an organization.
/// </summary>
public record DeleteOrganizationResponse(
    int OrganizationId,
    bool Success,
    string Message
);

