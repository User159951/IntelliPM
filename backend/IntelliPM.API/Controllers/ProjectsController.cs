using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using IntelliPM.Application.Projects.Commands;
using IntelliPM.Application.Projects.Queries;
using IntelliPM.Application.Tasks.Queries;
using IntelliPM.Application.Tasks.DTOs;
using IntelliPM.Application.Common.Models;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Authorization;
using IntelliPM.Domain.Enums;
using System.Security.Claims;
using IntelliPM.API.Authorization;

namespace IntelliPM.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class ProjectsController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProjectsController> _logger;

    public ProjectsController(IMediator mediator, ILogger<ProjectsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all projects for the current user with pagination
    /// </summary>
    /// <param name="page">Page number (default: 1, minimum: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 20, minimum: 1, maximum: 100)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paginated list of projects</returns>
    /// <response code="200">Returns the paginated list of projects</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [RequirePermission("projects.view")]
    [ProducesResponseType(typeof(PagedResponse<ProjectListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProjects(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} retrieving their projects - Page: {Page}, PageSize: {PageSize}", userId, page, pageSize);
            
            // Validate pagination parameters
            if (page < 1)
            {
                return BadRequest(new { error = "Page must be greater than 0" });
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest(new { error = "PageSize must be between 1 and 100" });
            }
            
            var query = new GetUserProjectsQuery(userId, page, pageSize);
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access when retrieving projects");
            return Problem(
                title: "Unauthorized",
                detail: ex.Message,
                statusCode: StatusCodes.Status401Unauthorized
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving projects");
            return Problem(
                title: "Error retrieving projects",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get a specific project by ID
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Project details</returns>
    /// <response code="200">Returns the project details</response>
    /// <response code="404">Project not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id}")]
    [RequirePermission("projects.view")]
    [ProducesResponseType(typeof(GetProjectByIdResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProject(int id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Getting project by ID: {ProjectId}", id);
            
            var query = new GetProjectByIdQuery(id);
            var result = await _mediator.Send(query, ct);
            
            if (result == null)
            {
                _logger.LogWarning("Project {ProjectId} not found", id);
                return NotFound(new { message = $"Project with ID {id} not found" });
            }
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project {ProjectId}", id);
            return Problem(
                title: "Error retrieving project",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get the current user's role in a project
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>User's role in the project (or null if not a member)</returns>
    /// <response code="200">Returns the user's role in the project</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id}/my-role")]
    [RequirePermission("projects.view")]
    [ProducesResponseType(typeof(ProjectRole?), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMyRole(int id, CancellationToken ct = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Getting role for user {UserId} in project {ProjectId}", userId, id);
            
            var query = new GetUserRoleInProjectQuery(id, userId);
            var role = await _mediator.Send(query, ct);
            
            // Return null if user is not a member (simplifies frontend handling)
            return Ok(role);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access when getting user role in project {ProjectId}", id);
            return Problem(
                title: "Unauthorized",
                detail: ex.Message,
                statusCode: StatusCodes.Status401Unauthorized
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user role in project {ProjectId}", id);
            return Problem(
                title: "Error retrieving user role",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Create a new project
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/v1/Projects
    ///     {
    ///        "name": "My New Project",
    ///        "description": "Project description",
    ///        "type": "Scrum",
    ///        "sprintDurationDays": 14,
    ///        "status": "Active",
    ///        "startDate": "2025-01-01T00:00:00Z",
    ///        "memberIds": [1, 2, 3]
    ///     }
    /// 
    /// </remarks>
    /// <param name="req">Project creation request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Created project</returns>
    /// <response code="201">Project created successfully</response>
    /// <response code="400">Bad request - Validation failed</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="403">User does not have permission to create projects</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [RequirePermission("projects.create")]
    [ProducesResponseType(typeof(CreateProjectResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateProject(
        [FromBody] CreateProjectRequest req,
        CancellationToken ct = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} creating project: {ProjectName}", userId, req.Name);
            
            var cmd = new CreateProjectCommand(
                req.Name, 
                req.Description, 
                req.Type, 
                req.SprintDurationDays, 
                userId,
                req.Status ?? "Active",
                req.StartDate,
                req.MemberIds
            );
            var result = await _mediator.Send(cmd, ct);
            return CreatedAtAction(nameof(GetProject), new { id = result.Id }, result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access when creating project");
            return Problem(
                title: "Unauthorized",
                detail: ex.Message,
                statusCode: StatusCodes.Status401Unauthorized
            );
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument when creating project");
            return Problem(
                title: "Validation Error",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating project");
            return Problem(
                title: "Error creating project",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Update an existing project
    /// </summary>
    [HttpPut("{id}")]
    [RequirePermission("projects.edit")]
    [ProducesResponseType(typeof(UpdateProjectResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateProject(
        int id,
        [FromBody] UpdateProjectRequest req,
        CancellationToken ct = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} updating project {ProjectId}", userId, id);
            
            var cmd = new UpdateProjectCommand(
                id,
                userId,
                req.Name,
                req.Description,
                req.Status,
                req.Type,
                req.SprintDurationDays
            );
            var result = await _mediator.Send(cmd, ct);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "User not authorized to update project {ProjectId}", id);
            return Problem(
                title: "Forbidden",
                detail: "You are not authorized to update this project. Only the project owner can update it.",
                statusCode: StatusCodes.Status403Forbidden
            );
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            _logger.LogWarning(ex, "Project {ProjectId} not found when updating", id);
            return Problem(
                title: "Not Found",
                detail: ex.Message,
                statusCode: StatusCodes.Status404NotFound
            );
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument when updating project {ProjectId}", id);
            return Problem(
                title: "Validation Error",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating project {ProjectId}", id);
            return Problem(
                title: "Error updating project",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Delete (archive) a project
    /// </summary>
    [HttpDelete("{id}")]
    [RequirePermission("projects.delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteProject(int id, CancellationToken ct = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} archiving project {ProjectId}", userId, id);
            
            var cmd = new ArchiveProjectCommand(id, userId);
            await _mediator.Send(cmd, ct);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "User not authorized to delete project {ProjectId}", id);
            return Problem(
                title: "Forbidden",
                detail: "You are not authorized to delete this project. Only the project owner can delete it.",
                statusCode: StatusCodes.Status403Forbidden
            );
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            _logger.LogWarning(ex, "Project {ProjectId} not found when deleting", id);
            return Problem(
                title: "Not Found",
                detail: ex.Message,
                statusCode: StatusCodes.Status404NotFound
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting project {ProjectId}", id);
            return Problem(
                title: "Error deleting project",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Permanently delete a project and all its data
    /// </summary>
    [HttpDelete("{id}/permanent")]
    [RequirePermission("projects.delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteProjectPermanent(int id, CancellationToken ct = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogWarning("User {UserId} attempting to permanently delete project {ProjectId}", userId, id);
            
            var cmd = new DeleteProjectCommand(id, userId);
            await _mediator.Send(cmd, ct);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "User not authorized to delete project {ProjectId}", id);
            return Problem(
                title: "Forbidden",
                detail: "You are not authorized to delete this project. Only the project owner can delete it.",
                statusCode: StatusCodes.Status403Forbidden
            );
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            _logger.LogWarning(ex, "Project {ProjectId} not found when deleting", id);
            return Problem(
                title: "Not Found",
                detail: ex.Message,
                statusCode: StatusCodes.Status404NotFound
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error permanently deleting project {ProjectId}", id);
            return Problem(
                title: "Error deleting project",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get all members of a project
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of project members</returns>
    /// <response code="200">Returns the list of project members</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="403">User does not have permission to view project members</response>
    /// <response code="404">Project not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id}/members")]
    [RequirePermission("projects.view")]
    [ProducesResponseType(typeof(List<ProjectMemberDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProjectMembers(int id, CancellationToken ct = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} retrieving members for project {ProjectId}", userId, id);
            
            var query = new GetProjectMembersQuery(id, userId);
            var members = await _mediator.Send(query, ct);
            return Ok(members);
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access when retrieving project members");
            return Problem(
                title: "Forbidden",
                detail: ex.Message,
                statusCode: StatusCodes.Status403Forbidden
            );
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access when retrieving project members");
            return Problem(
                title: "Forbidden",
                detail: ex.Message,
                statusCode: StatusCodes.Status403Forbidden
            );
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Project {ProjectId} not found when retrieving members", id);
            return Problem(
                title: "Not Found",
                detail: ex.Message,
                statusCode: StatusCodes.Status404NotFound
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving members for project {ProjectId}", id);
            return Problem(
                title: "Error retrieving project members",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Invite a member to a project
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/v1/Projects/{id}/members
    ///     {
    ///        "email": "user@example.com",
    ///        "role": "Developer"
    ///     }
    /// 
    /// </remarks>
    /// <param name="id">Project ID</param>
    /// <param name="request">Invitation request with email and role</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Created member ID</returns>
    /// <response code="201">Member invited successfully</response>
    /// <response code="400">Bad request - Validation failed</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="403">User does not have permission to invite members</response>
    /// <response code="404">Project or user not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("{id}/members")]
    [RequirePermission("projects.members.invite")]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> InviteMember(
        int id,
        [FromBody] InviteMemberRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} inviting member {Email} to project {ProjectId}", userId, request.Email, id);
            
            var cmd = new InviteMemberCommand(id, userId, request.Email, request.Role);
            var memberId = await _mediator.Send(cmd, ct);
            
            return CreatedAtAction(
                nameof(GetProjectMembers), 
                new { id }, 
                new { memberId, email = request.Email, role = request.Role });
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access when inviting member to project {ProjectId}", id);
            return Problem(
                title: "Forbidden",
                detail: ex.Message,
                statusCode: StatusCodes.Status403Forbidden
            );
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access when inviting member to project {ProjectId}", id);
            return Problem(
                title: "Forbidden",
                detail: ex.Message,
                statusCode: StatusCodes.Status403Forbidden
            );
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error when inviting member to project {ProjectId}", id);
            return Problem(
                title: "Bad Request",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest
            );
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Project or user not found when inviting member to project {ProjectId}", id);
            return Problem(
                title: "Not Found",
                detail: ex.Message,
                statusCode: StatusCodes.Status404NotFound
            );
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when inviting member to project {ProjectId}", id);
            return Problem(
                title: "Bad Request",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inviting member to project {ProjectId}", id);
            return Problem(
                title: "Error inviting member",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Assign an entire team to a project
    /// </summary>
    /// <param name="projectId">The ID of the project</param>
    /// <param name="request">The team assignment request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Response containing list of assigned members</returns>
    /// <response code="200">Team successfully assigned to project</response>
    /// <response code="400">Bad request - Validation failed</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="403">Forbidden - User doesn't have permission to assign teams</response>
    /// <response code="404">Not Found - Project or team not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("{projectId}/assign-team")]
    [RequirePermission("projects.edit")]
    [ProducesResponseType(typeof(AssignTeamToProjectResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AssignTeamToProject(
        int projectId,
        [FromBody] AssignTeamToProjectRequest request,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Assigning team {TeamId} to project {ProjectId}", request.TeamId, projectId);

            var cmd = new AssignTeamToProjectCommand
            {
                ProjectId = projectId,
                TeamId = request.TeamId,
                DefaultRole = request.DefaultRole ?? ProjectRole.Developer,
                MemberRoleOverrides = request.MemberRoleOverrides
            };

            var result = await _mediator.Send(cmd, ct);

            return Ok(result);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error when assigning team to project {ProjectId}", projectId);
            return Problem(
                title: "Bad Request",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest
            );
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access when assigning team to project {ProjectId}", projectId);
            return Problem(
                title: "Forbidden",
                detail: ex.Message,
                statusCode: StatusCodes.Status403Forbidden
            );
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Project or team not found when assigning team to project {ProjectId}", projectId);
            return Problem(
                title: "Not Found",
                detail: ex.Message,
                statusCode: StatusCodes.Status404NotFound
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning team to project {ProjectId}", projectId);
            return Problem(
                title: "Error assigning team",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get all teams assigned to a project.
    /// Returns teams that are currently assigned (active) to the project.
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of assigned teams</returns>
    /// <response code="200">Returns the list of assigned teams</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="403">User does not have permission to view project teams</response>
    /// <response code="404">Project not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id}/assigned-teams")]
    [RequirePermission("projects.view")]
    [ProducesResponseType(typeof(List<ProjectAssignedTeamDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProjectAssignedTeams(int id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving assigned teams for project {ProjectId}", id);

            var query = new GetProjectAssignedTeamsQuery
            {
                ProjectId = id
            };

            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Project {ProjectId} not found when retrieving assigned teams", id);
            return Problem(
                title: "Not Found",
                detail: ex.Message,
                statusCode: StatusCodes.Status404NotFound
            );
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access when retrieving assigned teams for project {ProjectId}", id);
            return Problem(
                title: "Forbidden",
                detail: ex.Message,
                statusCode: StatusCodes.Status403Forbidden
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving assigned teams for project {ProjectId}", id);
            return Problem(
                title: "Error retrieving assigned teams",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Change a member's role in a project
    /// </summary>
    [HttpPut("{projectId}/members/{userId}/role")]
    [RequirePermission("projects.members.changeRole")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangeMemberRole(
        int projectId,
        int userId,
        [FromBody] ChangeRoleRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("User {CurrentUserId} changing role for user {UserId} in project {ProjectId}", currentUserId, userId, projectId);
            
            var cmd = new ChangeMemberRoleCommand(projectId, currentUserId, userId, request.NewRole);
            await _mediator.Send(cmd, ct);
            
            return NoContent();
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access when changing member role in project {ProjectId}", projectId);
            return Problem(
                title: "Forbidden",
                detail: ex.Message,
                statusCode: StatusCodes.Status403Forbidden
            );
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access when changing member role in project {ProjectId}", projectId);
            return Problem(
                title: "Forbidden",
                detail: ex.Message,
                statusCode: StatusCodes.Status403Forbidden
            );
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Project or member not found when changing role in project {ProjectId}", projectId);
            return Problem(
                title: "Not Found",
                detail: ex.Message,
                statusCode: StatusCodes.Status404NotFound
            );
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error when changing member role in project {ProjectId}", projectId);
            return Problem(
                title: "Bad Request",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest
            );
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when changing member role in project {ProjectId}", projectId);
            return Problem(
                title: "Bad Request",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing member role in project {ProjectId}", projectId);
            return Problem(
                title: "Error changing member role",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Remove a member from a project
    /// </summary>
    [HttpDelete("{projectId}/members/{userId}")]
    [RequirePermission("projects.members.remove")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveMember(
        int projectId,
        int userId,
        CancellationToken ct = default)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("User {CurrentUserId} removing user {UserId} from project {ProjectId}", currentUserId, userId, projectId);
            
            var cmd = new RemoveMemberCommand(projectId, currentUserId, userId);
            await _mediator.Send(cmd, ct);
            
            return NoContent();
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access when removing member from project {ProjectId}", projectId);
            return Problem(
                title: "Forbidden",
                detail: ex.Message,
                statusCode: StatusCodes.Status403Forbidden
            );
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access when removing member from project {ProjectId}", projectId);
            return Problem(
                title: "Forbidden",
                detail: ex.Message,
                statusCode: StatusCodes.Status403Forbidden
            );
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Project or member not found when removing from project {ProjectId}", projectId);
            return Problem(
                title: "Not Found",
                detail: ex.Message,
                statusCode: StatusCodes.Status404NotFound
            );
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error when removing member from project {ProjectId}", projectId);
            return Problem(
                title: "Bad Request",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest
            );
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when removing member from project {ProjectId}", projectId);
            return Problem(
                title: "Bad Request",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing member from project {ProjectId}", projectId);
            return Problem(
                title: "Error removing member",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get the complete dependency graph for a project
    /// </summary>
    /// <param name="projectId">The project ID to get the dependency graph for</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Dependency graph with nodes (tasks) and edges (dependencies)</returns>
    /// <response code="200">Dependency graph retrieved successfully</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{projectId}/dependency-graph")]
    [RequirePermission("tasks.view")]
    [ProducesResponseType(typeof(DependencyGraphDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProjectDependencyGraph(
        int projectId,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Getting dependency graph for project {ProjectId}", projectId);

            var query = new GetProjectDependencyGraphQuery
            {
                ProjectId = projectId
            };

            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dependency graph for project {ProjectId}", projectId);
            return Problem(
                title: "Error retrieving dependency graph",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get current user's permissions for a specific project
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>User's permissions and project role for the specified project</returns>
    /// <response code="200">Permissions retrieved successfully</response>
    /// <response code="404">Project not found or user is not a member</response>
    /// <response code="401">User is not authenticated</response>
    [HttpGet("{projectId}/permissions")]
    [RequirePermission("projects.view")]
    [ProducesResponseType(typeof(ProjectPermissionsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProjectPermissions(
        int projectId,
        CancellationToken ct = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new { error = "User ID not found in claims" });
            }

            _logger.LogInformation("User {UserId} retrieving permissions for project {ProjectId}", userId, projectId);

            // Get user's role in the project
            var roleQuery = new GetUserRoleInProjectQuery(projectId, userId);
            var projectRole = await _mediator.Send(roleQuery, ct);

            if (!projectRole.HasValue)
            {
                _logger.LogWarning("User {UserId} is not a member of project {ProjectId}", userId, projectId);
                return NotFound(new { error = "Project not found or user is not a member" });
            }

            // Build permissions list based on project role
            var permissions = new List<string>();

            // Always include projects.view if user is a member
            permissions.Add("projects.view");

            // Add permissions based on role
            if (ProjectPermissions.CanEditProject(projectRole.Value))
                permissions.Add("projects.edit");
            
            if (ProjectPermissions.CanDeleteProject(projectRole.Value))
                permissions.Add("projects.delete");
            
            if (ProjectPermissions.CanInviteMembers(projectRole.Value))
                permissions.Add("projects.members.invite");
            
            if (ProjectPermissions.CanRemoveMembers(projectRole.Value))
                permissions.Add("projects.members.remove");
            
            if (ProjectPermissions.CanChangeRoles(projectRole.Value))
                permissions.Add("projects.members.changeRole");
            
            if (ProjectPermissions.CanCreateTasks(projectRole.Value))
                permissions.Add("tasks.create");
            
            if (ProjectPermissions.CanEditTasks(projectRole.Value))
                permissions.Add("tasks.edit");
            
            if (ProjectPermissions.CanDeleteTasks(projectRole.Value))
                permissions.Add("tasks.delete");
            
            if (ProjectPermissions.CanCommentOnTasks(projectRole.Value))
                permissions.Add("tasks.comment");
            
            if (ProjectPermissions.CanManageSprints(projectRole.Value))
            {
                permissions.Add("sprints.manage");
                permissions.Add("sprints.create");
                permissions.Add("sprints.edit");
            }
            
            if (ProjectPermissions.CanStartSprint(projectRole.Value))
                permissions.Add("sprints.start");
            
            if (ProjectPermissions.CanCloseSprint(projectRole.Value))
                permissions.Add("sprints.complete");
            
            if (ProjectPermissions.CanApproveRelease(projectRole.Value))
                permissions.Add("releases.approve");
            
            if (ProjectPermissions.CanValidateQualityGate(projectRole.Value))
                permissions.Add("releases.validateQualityGate");
            
            if (ProjectPermissions.CanValidateMilestone(projectRole.Value))
            {
                permissions.Add("milestones.complete");
                permissions.Add("milestones.edit");
            }

            // Milestone permissions
            if (ProjectPermissions.CanEditProject(projectRole.Value))
            {
                permissions.Add("milestones.create");
                permissions.Add("milestones.view");
            }

            var response = new ProjectPermissionsResponse
            {
                Permissions = permissions.ToArray(),
                ProjectRole = projectRole.Value.ToString(),
                ProjectId = projectId
            };

            return Ok(response);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Project {ProjectId} not found", projectId);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving permissions for project {ProjectId}", projectId);
            return Problem(
                title: "Error retrieving project permissions",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }
}

public record CreateProjectRequest(
    string Name, 
    string Description, 
    string Type, 
    int SprintDurationDays,
    string? Status = null,
    DateTimeOffset? StartDate = null,
    List<int>? MemberIds = null
);

public record UpdateProjectRequest(
    string? Name = null,
    string? Description = null,
    string? Status = null,
    string? Type = null,
    int? SprintDurationDays = null
);

public record InviteMemberRequest(
    string Email,
    ProjectRole Role
);

public record ChangeRoleRequest(
    ProjectRole NewRole
);

public record AssignTeamToProjectRequest(
    int TeamId,
    ProjectRole? DefaultRole,
    Dictionary<int, ProjectRole>? MemberRoleOverrides
);

public class ProjectPermissionsResponse
{
    public string[] Permissions { get; set; } = Array.Empty<string>();
    public string? ProjectRole { get; set; }
    public int ProjectId { get; set; }
}

