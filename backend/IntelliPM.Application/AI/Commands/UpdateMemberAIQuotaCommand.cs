using MediatR;
using IntelliPM.Application.AI.DTOs;

namespace IntelliPM.Application.AI.Commands;

/// <summary>
/// Command to update or create a user AI quota override (Admin only - own organization).
/// </summary>
public record UpdateMemberAIQuotaCommand : IRequest<MemberAIQuotaDto>
{
    public int UserId { get; init; }
    public long? MonthlyTokenLimitOverride { get; init; }
    public int? MonthlyRequestLimitOverride { get; init; }
    public bool? IsAIEnabledOverride { get; init; }
}

