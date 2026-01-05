using MediatR;
using IntelliPM.Application.AI.DTOs;

namespace IntelliPM.Application.AI.Commands;

/// <summary>
/// Command to upsert (create or update) organization AI quota (SuperAdmin only).
/// </summary>
public record UpsertOrganizationAIQuotaCommand : IRequest<OrganizationAIQuotaDto>
{
    public int OrganizationId { get; init; }
    public long MonthlyTokenLimit { get; init; }
    public int? MonthlyRequestLimit { get; init; }
    public int? ResetDayOfMonth { get; init; }
    public bool? IsAIEnabled { get; init; }
}

