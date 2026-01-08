using IntelliPM.Application.AI.DTOs;
using MediatR;

namespace IntelliPM.Application.AI.Commands;

/// <summary>
/// Command to update an existing AI quota template.
/// </summary>
public class UpdateAIQuotaTemplateCommand : IRequest<AIQuotaTemplateDto>
{
    public int Id { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
    public int? MaxTokensPerPeriod { get; set; }
    public int? MaxRequestsPerPeriod { get; set; }
    public int? MaxDecisionsPerPeriod { get; set; }
    public decimal? MaxCostPerPeriod { get; set; }
    public bool? AllowOverage { get; set; }
    public decimal? OverageRate { get; set; }
    public decimal? DefaultAlertThresholdPercentage { get; set; }
    public int? DisplayOrder { get; set; }
}

