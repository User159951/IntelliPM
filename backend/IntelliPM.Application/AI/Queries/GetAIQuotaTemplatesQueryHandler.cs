using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.AI.Queries;

/// <summary>
/// Handler for getting all AI quota templates.
/// </summary>
public class GetAIQuotaTemplatesQueryHandler : IRequestHandler<GetAIQuotaTemplatesQuery, IEnumerable<AIQuotaTemplateDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAIQuotaTemplatesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<AIQuotaTemplateDto>> Handle(GetAIQuotaTemplatesQuery request, CancellationToken ct)
    {
        var query = _unitOfWork.Repository<AIQuotaTemplate>()
            .Query()
            .Where(t => t.DeletedAt == null);

        if (request.ActiveOnly)
        {
            query = query.Where(t => t.IsActive);
        }

        var templates = await query
            .OrderBy(t => t.DisplayOrder)
            .ThenBy(t => t.TierName)
            .ToListAsync(ct);

        return templates.Select(t => new AIQuotaTemplateDto(
            t.Id,
            t.TierName,
            t.Description,
            t.IsActive,
            t.IsSystemTemplate,
            t.MaxTokensPerPeriod,
            t.MaxRequestsPerPeriod,
            t.MaxDecisionsPerPeriod,
            t.MaxCostPerPeriod,
            t.AllowOverage,
            t.OverageRate,
            t.DefaultAlertThresholdPercentage,
            t.DisplayOrder,
            t.CreatedAt,
            t.UpdatedAt
        ));
    }
}

