using MediatR;
using Microsoft.AspNetCore.Mvc;
using IntelliPM.Application.AI.Commands;
using IntelliPM.Application.AI.Queries;
using IntelliPM.Application.AI.DTOs;
using IntelliPM.API.Authorization;

namespace IntelliPM.API.Controllers.Admin;

/// <summary>
/// Admin controller for managing AI quota templates.
/// Provides CRUD operations for quota tier templates.
/// SuperAdmin only - templates are system-level configuration.
/// </summary>
[ApiController]
[Route("api/admin/ai-quota-templates")]
[RequireSuperAdmin]
public class AIQuotaTemplatesController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<AIQuotaTemplatesController> _logger;

    public AIQuotaTemplatesController(IMediator mediator, ILogger<AIQuotaTemplatesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all AI quota templates.
    /// </summary>
    /// <param name="activeOnly">If true, only return active templates (default: true)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of quota templates</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AIQuotaTemplateDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true, CancellationToken ct = default)
    {
        try
        {
            var query = new GetAIQuotaTemplatesQuery { ActiveOnly = activeOnly };
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting AI quota templates");
            return Problem(
                title: "Error retrieving templates",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Create a new AI quota template.
    /// </summary>
    /// <param name="request">Template creation request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Created template</returns>
    [HttpPost]
    [ProducesResponseType(typeof(AIQuotaTemplateDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateAIQuotaTemplateRequest request, CancellationToken ct)
    {
        try
        {
            var command = new CreateAIQuotaTemplateCommand
            {
                TierName = request.TierName,
                Description = request.Description,
                MaxTokensPerPeriod = request.MaxTokensPerPeriod,
                MaxRequestsPerPeriod = request.MaxRequestsPerPeriod,
                MaxDecisionsPerPeriod = request.MaxDecisionsPerPeriod,
                MaxCostPerPeriod = request.MaxCostPerPeriod,
                AllowOverage = request.AllowOverage,
                OverageRate = request.OverageRate,
                DefaultAlertThresholdPercentage = request.DefaultAlertThresholdPercentage,
                DisplayOrder = request.DisplayOrder
            };

            var result = await _mediator.Send(command, ct);
            return CreatedAtAction(nameof(GetAll), new { activeOnly = true }, result);
        }
        catch (Application.Common.Exceptions.ValidationException ex)
        {
            return BadRequest(new { message = ex.Message, errors = ex.Errors });
        }
        catch (Application.Common.Exceptions.UnauthorizedException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating AI quota template");
            return Problem(
                title: "Error creating template",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Update an existing AI quota template.
    /// System templates can be updated but their TierName cannot be changed.
    /// </summary>
    /// <param name="id">Template ID</param>
    /// <param name="request">Template update request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Updated template</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(AIQuotaTemplateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAIQuotaTemplateRequest request, CancellationToken ct)
    {
        try
        {
            var command = new UpdateAIQuotaTemplateCommand
            {
                Id = id,
                Description = request.Description,
                IsActive = request.IsActive,
                MaxTokensPerPeriod = request.MaxTokensPerPeriod,
                MaxRequestsPerPeriod = request.MaxRequestsPerPeriod,
                MaxDecisionsPerPeriod = request.MaxDecisionsPerPeriod,
                MaxCostPerPeriod = request.MaxCostPerPeriod,
                AllowOverage = request.AllowOverage,
                OverageRate = request.OverageRate,
                DefaultAlertThresholdPercentage = request.DefaultAlertThresholdPercentage,
                DisplayOrder = request.DisplayOrder
            };

            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }
        catch (Application.Common.Exceptions.NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Application.Common.Exceptions.ValidationException ex)
        {
            return BadRequest(new { message = ex.Message, errors = ex.Errors });
        }
        catch (Application.Common.Exceptions.UnauthorizedException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating AI quota template {TemplateId}", id);
            return Problem(
                title: "Error updating template",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }
}

/// <summary>
/// Request model for creating an AI quota template.
/// </summary>
public record CreateAIQuotaTemplateRequest(
    string TierName,
    string? Description,
    int MaxTokensPerPeriod,
    int MaxRequestsPerPeriod,
    int MaxDecisionsPerPeriod,
    decimal MaxCostPerPeriod,
    bool AllowOverage,
    decimal OverageRate,
    decimal DefaultAlertThresholdPercentage,
    int DisplayOrder
);

/// <summary>
/// Request model for updating an AI quota template.
/// </summary>
public record UpdateAIQuotaTemplateRequest(
    string? Description,
    bool? IsActive,
    int? MaxTokensPerPeriod,
    int? MaxRequestsPerPeriod,
    int? MaxDecisionsPerPeriod,
    decimal? MaxCostPerPeriod,
    bool? AllowOverage,
    decimal? OverageRate,
    decimal? DefaultAlertThresholdPercentage,
    int? DisplayOrder
);

