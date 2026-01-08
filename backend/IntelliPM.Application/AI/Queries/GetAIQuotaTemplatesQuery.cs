using IntelliPM.Application.AI.DTOs;
using MediatR;

namespace IntelliPM.Application.AI.Queries;

/// <summary>
/// Query to get all AI quota templates.
/// </summary>
public class GetAIQuotaTemplatesQuery : IRequest<IEnumerable<AIQuotaTemplateDto>>
{
    /// <summary>
    /// If true, only return active templates. If false, return all templates (including inactive).
    /// </summary>
    public bool ActiveOnly { get; set; } = true;
}

