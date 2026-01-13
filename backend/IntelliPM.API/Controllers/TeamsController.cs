using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using IntelliPM.Application.Teams.Commands;
using IntelliPM.Application.Teams.Queries;
using System.Security.Claims;
using IntelliPM.API.Authorization;

namespace IntelliPM.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class TeamsController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<TeamsController> _logger;

    public TeamsController(IMediator mediator, ILogger<TeamsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// List all teams user has access to
    /// </summary>
    [HttpGet]
    [RequirePermission("teams.view")]
    [ProducesResponseType(typeof(GetAllTeamsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllTeams(CancellationToken ct = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} retrieving all accessible teams", userId);
            
            var query = new GetAllTeamsQuery(userId);
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access when retrieving teams");
            return Problem(
                title: "Unauthorized",
                detail: ex.Message,
                statusCode: StatusCodes.Status401Unauthorized
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving teams");
            return Problem(
                title: "Error retrieving teams",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get team by ID with members
    /// </summary>
    [HttpGet("{id}")]
    [RequirePermission("projects.view")] // Teams are project-related
    [ProducesResponseType(typeof(TeamDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTeamById(int id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Getting team by ID: {TeamId}", id);
            
            var query = new GetTeamByIdQuery(id);
            var result = await _mediator.Send(query, ct);

            return Ok(result);
        }
        catch (IntelliPM.Application.Common.Exceptions.NotFoundException)
        {
            // NotFoundException is handled by global exception handler
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting team {TeamId}", id);
            return Problem(
                title: "Error retrieving team",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Register a new team
    /// </summary>
    [HttpPost]
    [RequirePermission("teams.create")]
    [ProducesResponseType(typeof(TeamDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RegisterTeam(
        [FromBody] RegisterTeamRequest req,
        CancellationToken ct = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} registering team: {TeamName} with {MemberCount} members", 
                userId, req.Name, req.MemberIds.Count);
            
            var cmd = new RegisterTeamCommand(req.Name, req.MemberIds, req.TotalCapacity);
            var result = await _mediator.Send(cmd, ct);
            return CreatedAtAction(nameof(GetTeamById), new { id = result.Id }, result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access when registering team");
            return Problem(
                title: "Unauthorized",
                detail: ex.Message,
                statusCode: StatusCodes.Status401Unauthorized
            );
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument when registering team");
            return Problem(
                title: "Validation Error",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest
            );
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when registering team: {Message}", ex.Message);
            return Problem(
                title: "Invalid Operation",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering team");
            return Problem(
                title: "Error registering team",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Update team capacity
    /// </summary>
    [HttpPatch("{id}/capacity")]
    [RequirePermission("teams.edit")]
    [ProducesResponseType(typeof(UpdateTeamCapacityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateTeamCapacity(
        int id,
        [FromBody] UpdateTeamCapacityRequest req,
        CancellationToken ct = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} updating capacity of team {TeamId} to {NewCapacity}", 
                userId, id, req.NewCapacity);
            
            var cmd = new UpdateTeamCapacityCommand(id, req.NewCapacity, userId);
            var result = await _mediator.Send(cmd, ct);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access when updating team {TeamId} capacity", id);
            return Problem(
                title: "Unauthorized",
                detail: ex.Message,
                statusCode: StatusCodes.Status401Unauthorized
            );
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            _logger.LogWarning(ex, "Team {TeamId} not found when updating capacity", id);
            return Problem(
                title: "Not Found",
                detail: ex.Message,
                statusCode: StatusCodes.Status404NotFound
            );
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid capacity for team {TeamId}", id);
            return Problem(
                title: "Validation Error",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating capacity of team {TeamId}", id);
            return Problem(
                title: "Error updating team capacity",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get team availability (capacity with active sprint calculations)
    /// </summary>
    [HttpGet("{id}/availability")]
    [RequirePermission("teams.view.availability")]
    [ProducesResponseType(typeof(TeamCapacityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTeamAvailability(int id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Getting availability for team {TeamId}", id);
            
            var query = new GetTeamCapacityQuery(id);
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            _logger.LogWarning(ex, "Team {TeamId} not found when getting availability", id);
            return Problem(
                title: "Not Found",
                detail: ex.Message,
                statusCode: StatusCodes.Status404NotFound
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting availability for team {TeamId}", id);
            return Problem(
                title: "Error retrieving team availability",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Add a member to a team
    /// </summary>
    [HttpPost("{id}/members")]
    [RequirePermission("teams.members.add")]
    [ProducesResponseType(typeof(TeamDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddTeamMember(
        int id,
        [FromBody] AddTeamMemberRequest req,
        CancellationToken ct = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} adding member {MemberId} to team {TeamId}", 
                userId, req.UserId, id);
            
            var cmd = new AddTeamMemberCommand(id, req.UserId, userId);
            var result = await _mediator.Send(cmd, ct);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access when adding member to team {TeamId}", id);
            return Problem(
                title: "Unauthorized",
                detail: ex.Message,
                statusCode: StatusCodes.Status401Unauthorized
            );
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when adding member to team {TeamId}: {Message}", id, ex.Message);
            return Problem(
                title: ex.Message.Contains("not found") ? "Not Found" : "Invalid Operation",
                detail: ex.Message,
                statusCode: ex.Message.Contains("not found") ? StatusCodes.Status404NotFound : StatusCodes.Status400BadRequest
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding member to team {TeamId}", id);
            return Problem(
                title: "Error adding team member",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Remove a member from a team
    /// </summary>
    [HttpDelete("{id}/members/{userId}")]
    [RequirePermission("teams.members.remove")]
    [ProducesResponseType(typeof(TeamDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveTeamMember(
        int id,
        int userId,
        CancellationToken ct = default)
    {
        try
        {
            var requestedByUserId = GetCurrentUserId();
            _logger.LogInformation("User {RequestedByUserId} removing member {MemberId} from team {TeamId}", 
                requestedByUserId, userId, id);
            
            var cmd = new RemoveTeamMemberCommand(id, userId, requestedByUserId);
            var result = await _mediator.Send(cmd, ct);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access when removing member from team {TeamId}", id);
            return Problem(
                title: "Unauthorized",
                detail: ex.Message,
                statusCode: StatusCodes.Status401Unauthorized
            );
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when removing member from team {TeamId}: {Message}", id, ex.Message);
            return Problem(
                title: ex.Message.Contains("not found") ? "Not Found" : "Invalid Operation",
                detail: ex.Message,
                statusCode: ex.Message.Contains("not found") ? StatusCodes.Status404NotFound : StatusCodes.Status400BadRequest
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing member from team {TeamId}", id);
            return Problem(
                title: "Error removing team member",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }
}

public record RegisterTeamRequest(
    string Name,
    List<int> MemberIds,
    int TotalCapacity
);

public record UpdateTeamCapacityRequest(int NewCapacity);

public record AddTeamMemberRequest(int UserId);
