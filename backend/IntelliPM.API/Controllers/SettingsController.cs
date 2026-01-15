using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using IntelliPM.Application.Settings.Queries;
using IntelliPM.Application.Settings.Commands;
using SendTestEmailCommand = IntelliPM.Application.Settings.Commands.SendTestEmailCommand;
using SendTestEmailResponse = IntelliPM.Application.Settings.Commands.SendTestEmailResponse;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.API.Authorization;

namespace IntelliPM.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class SettingsController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<SettingsController> _logger;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILanguageService _languageService;

    public SettingsController(
        IMediator mediator, 
        ILogger<SettingsController> logger, 
        ICurrentUserService currentUserService,
        ILanguageService languageService)
    {
        _mediator = mediator;
        _logger = logger;
        _currentUserService = currentUserService;
        _languageService = languageService;
    }

    /// <summary>
    /// Get all global settings (admin only)
    /// </summary>
    [HttpGet("global")]
    [RequirePermission("admin.settings.update")] // Admin only
    [ProducesResponseType(typeof(Dictionary<string, string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetGlobalSettings(CancellationToken cancellationToken = default)
    {
        try
        {
            // Ensure only admins can access this endpoint
            if (!_currentUserService.IsAdmin())
            {
                return Forbid();
            }

            var category = Request.Query.ContainsKey("category") ? Request.Query["category"].ToString() : null;
            var query = new GetSettingsQuery(category);
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving settings");
            return Problem(
                title: "Error retrieving settings",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Update a global setting (admin only)
    /// </summary>
    [HttpPut("{key}")]
    [RequirePermission("admin.settings.update")] // Admin only
    [ProducesResponseType(typeof(UpdateSettingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateSetting(
        string key,
        [FromBody] UpdateSettingRequest req,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Ensure only admins can access this endpoint
            if (!_currentUserService.IsAdmin())
            {
                return Forbid();
            }

            var category = req.Category;
            var cmd = new UpdateSettingCommand(key, req.Value, category);
            var result = await _mediator.Send(cmd, cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedException)
        {
            return Forbid();
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { error = ex.Message, errors = ex.Errors });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating setting {Key}", key);
            return Problem(
                title: "Error updating setting",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Send a test email (admin only)
    /// </summary>
    [HttpPost("test-email")]
    [ProducesResponseType(typeof(SendTestEmailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SendTestEmail(
        [FromBody] SendTestEmailRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_currentUserService.IsAdmin())
            {
                return Forbid();
            }

            var cmd = new SendTestEmailCommand(request.Email);
            var result = await _mediator.Send(cmd, cancellationToken);
            
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending test email to {Email}", request.Email);
            return Problem(
                title: "Error sending test email",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get all organization settings for the current user's organization
    /// </summary>
    [HttpGet("organization")]
    [ProducesResponseType(typeof(Dictionary<string, string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrganizationSettings(CancellationToken cancellationToken = default)
    {
        try
        {
            var organizationId = _currentUserService.GetOrganizationId();
            if (organizationId == 0)
            {
                return Forbid();
            }

            var category = Request.Query.ContainsKey("category") ? Request.Query["category"].ToString() : null;
            var query = new GetOrganizationSettingsQuery(organizationId, category);
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving organization settings");
            return Problem(
                title: "Error retrieving organization settings",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get current user's language preference
    /// </summary>
    [HttpGet("language")]
    [ProducesResponseType(typeof(LanguageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLanguage(CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = _currentUserService.GetUserId();
            var organizationId = _currentUserService.GetOrganizationId();
            
            if (userId == 0)
            {
                return Unauthorized();
            }

            var acceptLanguageHeader = Request.Headers["Accept-Language"].ToString();
            var language = await _languageService.GetUserLanguageAsync(userId, organizationId, acceptLanguageHeader);
            
            return Ok(new LanguageResponse(language));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user language");
            return Problem(
                title: "Error retrieving language",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Update current user's language preference
    /// </summary>
    [HttpPut("language")]
    [ProducesResponseType(typeof(LanguageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateLanguage(
        [FromBody] UpdateLanguageRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = _currentUserService.GetUserId();
            
            if (userId == 0)
            {
                return Unauthorized();
            }

            await _languageService.UpdateUserLanguageAsync(userId, request.Language);
            
            return Ok(new LanguageResponse(request.Language));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user language");
            return Problem(
                title: "Error updating language",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get all settings (backward compatibility - redirects to global)
    /// </summary>
    [HttpGet]
    [RequirePermission("admin.settings.update")] // Admin only
    [ProducesResponseType(typeof(Dictionary<string, string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllSettings(CancellationToken cancellationToken = default)
    {
        return await GetGlobalSettings(cancellationToken);
    }
}

public record UpdateSettingRequest(string Value, string? Category = null);
public record SendTestEmailRequest(string Email);
public record LanguageResponse(string Language);
public record UpdateLanguageRequest(string Language);

