using MediatR;
using IntelliPM.Application.Common.Models;

namespace IntelliPM.Application.Admin.AuditLogs.Queries;

public record GetAuditLogsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Action = null,
    string? EntityType = null,
    int? UserId = null,
    DateTimeOffset? StartDate = null,
    DateTimeOffset? EndDate = null
) : IRequest<PagedResponse<AuditLogDto>>;

public record AuditLogDto(
    int Id,
    int? UserId,
    string? UserName,
    string Action,
    string EntityType,
    int? EntityId,
    string? EntityName,
    string? Changes,
    string? IpAddress,
    string? UserAgent,
    DateTimeOffset CreatedAt
);

