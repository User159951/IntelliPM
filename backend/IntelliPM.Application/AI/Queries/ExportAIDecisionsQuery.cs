using MediatR;

namespace IntelliPM.Application.AI.Queries;

/// <summary>
/// Query to export AI decisions to CSV format (Admin only).
/// </summary>
public record ExportAIDecisionsQuery : IRequest<string>
{
    public int? OrganizationId { get; init; }
    public DateTimeOffset StartDate { get; init; }
    public DateTimeOffset EndDate { get; init; }
}

