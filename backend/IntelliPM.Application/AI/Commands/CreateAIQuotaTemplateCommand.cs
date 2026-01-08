using IntelliPM.Application.AI.DTOs;
using MediatR;

namespace IntelliPM.Application.AI.Commands;

/// <summary>
/// Command to create a new AI quota template.
/// </summary>
public class CreateAIQuotaTemplateCommand : IRequest<AIQuotaTemplateDto>
{
    public string TierName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int MaxTokensPerPeriod { get; set; }
    public int MaxRequestsPerPeriod { get; set; }
    public int MaxDecisionsPerPeriod { get; set; }
    public decimal MaxCostPerPeriod { get; set; }
    public bool AllowOverage { get; set; }
    public decimal OverageRate { get; set; }
    public decimal DefaultAlertThresholdPercentage { get; set; } = 80m;
    public int DisplayOrder { get; set; } = 0;
}

