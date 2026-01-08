using MediatR;

namespace IntelliPM.Application.Settings.Queries;

public record GetOrganizationSettingsQuery(int OrganizationId, string? Category = null) : IRequest<Dictionary<string, string>>;

