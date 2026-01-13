using MediatR;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using IntelliPM.Application.Features.Releases.Commands;
using IntelliPM.Application.Features.Releases.Queries;
using IntelliPM.Application.Features.Releases.DTOs;
using ReleaseDto = IntelliPM.Application.Features.Releases.DTOs.ReleaseDto;
using ReleaseStatisticsDto = IntelliPM.Application.Features.Releases.DTOs.ReleaseStatisticsDto;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Domain.Enums;
using IntelliPM.API.Authorization;

namespace IntelliPM.API.Controllers;

/// <summary>
/// Controller for managing project releases.
/// Provides endpoints for creating, updating, deploying, and managing releases.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class ReleasesController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<ReleasesController> _logger;

    public ReleasesController(IMediator mediator, ILogger<ReleasesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all releases for a project with optional status filtering.
    /// </summary>
    /// <param name="projectId">The project ID</param>
    /// <param name="status">Optional status filter</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of releases</returns>
    /// <response code="200">Releases retrieved successfully</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="403">Insufficient permissions</response>
    [HttpGet("../projects/{projectId}/releases")]
    [RequirePermission("releases.view")]
    [ProducesResponseType(typeof(List<ReleaseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<ReleaseDto>>> GetProjectReleases(
        int projectId,
        [FromQuery] string? status = null,
        CancellationToken ct = default)
    {
        try
        {
            ReleaseStatus? statusEnum = null;
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<ReleaseStatus>(status, true, out var parsedStatus))
            {
                statusEnum = parsedStatus;
            }

            var query = new GetProjectReleasesQuery(projectId, statusEnum);
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting releases for project {ProjectId}", projectId);
            return Problem(
                title: "Error retrieving releases",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get a specific release by ID.
    /// </summary>
    /// <param name="id">The release ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Release details</returns>
    /// <response code="200">Release retrieved successfully</response>
    /// <response code="404">Release not found</response>
    /// <response code="403">Insufficient permissions</response>
    [HttpGet("{id}")]
    [RequirePermission("releases.view")]
    [ProducesResponseType(typeof(ReleaseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ReleaseDto>> GetReleaseById(
        int id,
        CancellationToken ct = default)
    {
        try
        {
            var query = new GetReleaseByIdQuery(id);
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
            _logger.LogError(ex, "Error getting release {ReleaseId}", id);
            return Problem(
                title: "Error retrieving release",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get available sprints that can be added to a release.
    /// </summary>
    /// <param name="projectId">The project ID</param>
    /// <param name="releaseId">Optional release ID (for editing existing release)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of available sprints</returns>
    /// <response code="200">Available sprints retrieved successfully</response>
    /// <response code="403">Insufficient permissions</response>
    [HttpGet("../projects/{projectId}/sprints/available")]
    [RequirePermission("sprints.view")]
    [ProducesResponseType(typeof(List<ReleaseSprintDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<ReleaseSprintDto>>> GetAvailableSprints(
        int projectId,
        [FromQuery] int? releaseId = null,
        CancellationToken ct = default)
    {
        try
        {
            var query = new GetAvailableSprintsForReleaseQuery
            {
                ProjectId = projectId,
                ReleaseId = releaseId
            };
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available sprints for project {ProjectId}", projectId);
            return Problem(
                title: "Error retrieving available sprints",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get release statistics for a project.
    /// </summary>
    /// <param name="projectId">The project ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Release statistics</returns>
    /// <response code="200">Statistics retrieved successfully</response>
    /// <response code="403">Insufficient permissions</response>
    [HttpGet("../projects/{projectId}/releases/statistics")]
    [RequirePermission("releases.view")]
    [ProducesResponseType(typeof(IntelliPM.Application.Features.Releases.DTOs.ReleaseStatisticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IntelliPM.Application.Features.Releases.DTOs.ReleaseStatisticsDto>> GetReleaseStatistics(
        int projectId,
        CancellationToken ct = default)
    {
        try
        {
            var query = new GetReleaseStatisticsQuery(projectId);
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting release statistics for project {ProjectId}", projectId);
            return Problem(
                title: "Error retrieving statistics",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Create a new release for a project.
    /// </summary>
    /// <param name="projectId">The project ID</param>
    /// <param name="request">Release creation data</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Created release</returns>
    /// <response code="201">Release created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="403">Insufficient permissions</response>
    [HttpPost("../projects/{projectId}/releases")]
    [RequirePermission("releases.create")]
    [ProducesResponseType(typeof(ReleaseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ReleaseDto>> CreateRelease(
        int projectId,
        [FromBody] CreateReleaseRequest request,
        CancellationToken ct = default)
    {
        try
        {
            if (!Enum.TryParse<ReleaseType>(request.Type, true, out var releaseType))
            {
                return BadRequest(new { error = $"Invalid release type: {request.Type}" });
            }

            var command = new CreateReleaseCommand
            {
                ProjectId = projectId,
                Name = request.Name,
                Version = request.Version,
                Description = request.Description,
                PlannedDate = request.PlannedDate,
                Type = releaseType,
                IsPreRelease = request.IsPreRelease ?? false,
                TagName = request.TagName,
                SprintIds = request.SprintIds ?? new List<int>()
            };

            var result = await _mediator.Send(command, ct);
            return CreatedAtAction(nameof(GetReleaseById), new { id = result.Id }, result);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error creating release for project {ProjectId}", projectId);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating release for project {ProjectId}", projectId);
            return Problem(
                title: "Error creating release",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Update an existing release.
    /// </summary>
    /// <param name="id">The release ID</param>
    /// <param name="request">Release update data</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Updated release</returns>
    /// <response code="200">Release updated successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">Release not found</response>
    /// <response code="403">Insufficient permissions</response>
    [HttpPut("{id}")]
    [RequirePermission("releases.edit")]
    [ProducesResponseType(typeof(ReleaseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ReleaseDto>> UpdateRelease(
        int id,
        [FromBody] UpdateReleaseRequest request,
        CancellationToken ct = default)
    {
        try
        {
            if (!Enum.TryParse<ReleaseStatus>(request.Status, true, out var releaseStatus))
            {
                return BadRequest(new { error = $"Invalid release status: {request.Status}" });
            }

            var command = new UpdateReleaseCommand
            {
                Id = id,
                Name = request.Name,
                Version = request.Version,
                Description = request.Description,
                PlannedDate = request.PlannedDate,
                Status = releaseStatus
            };

            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Release {ReleaseId} not found for update", id);
            return NotFound(new { message = ex.Message });
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error updating release {ReleaseId}", id);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating release {ReleaseId}", id);
            return Problem(
                title: "Error updating release",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Delete a release.
    /// </summary>
    /// <param name="id">The release ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">Release deleted successfully</response>
    /// <response code="404">Release not found</response>
    /// <response code="403">Insufficient permissions</response>
    [HttpDelete("{id}")]
    [RequirePermission("releases.delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteRelease(
        int id,
        CancellationToken ct = default)
    {
        try
        {
            var command = new DeleteReleaseCommand(Id: id);
            await _mediator.Send(command, ct);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Release {ReleaseId} not found for deletion", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting release {ReleaseId}", id);
            return Problem(
                title: "Error deleting release",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Deploy a release.
    /// </summary>
    /// <param name="id">The release ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Deployed release</returns>
    /// <response code="200">Release deployed successfully</response>
    /// <response code="400">Release cannot be deployed</response>
    /// <response code="404">Release not found</response>
    /// <response code="403">Insufficient permissions</response>
    [HttpPost("{id}/deploy")]
    [RequirePermission("releases.deploy")]
    [ProducesResponseType(typeof(ReleaseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ReleaseDto>> DeployRelease(
        int id,
        CancellationToken ct = default)
    {
        try
        {
            var command = new DeployReleaseCommand(Id: id);
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Release {ReleaseId} not found for deployment", id);
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot deploy release {ReleaseId}: {Message}", id, ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deploying release {ReleaseId}", id);
            return Problem(
                title: "Error deploying release",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Add a sprint to a release.
    /// </summary>
    /// <param name="releaseId">The release ID</param>
    /// <param name="sprintId">The sprint ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="200">Sprint added successfully</response>
    /// <response code="404">Release or sprint not found</response>
    /// <response code="403">Insufficient permissions</response>
    [HttpPost("{releaseId}/sprints/{sprintId}")]
    [RequirePermission("releases.edit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddSprintToRelease(
        int releaseId,
        int sprintId,
        CancellationToken ct = default)
    {
        try
        {
            var command = new AddSprintToReleaseCommand
            {
                ReleaseId = releaseId,
                SprintId = sprintId
            };
            await _mediator.Send(command, ct);
            return Ok();
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Release {ReleaseId} or sprint {SprintId} not found", releaseId, sprintId);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding sprint {SprintId} to release {ReleaseId}", sprintId, releaseId);
            return Problem(
                title: "Error adding sprint to release",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Bulk add sprints to a release.
    /// </summary>
    /// <param name="releaseId">The release ID</param>
    /// <param name="request">Request containing sprint IDs</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Number of sprints added</returns>
    /// <response code="200">Sprints added successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">Release not found</response>
    /// <response code="403">Insufficient permissions</response>
    [HttpPost("{releaseId}/sprints/bulk")]
    [RequirePermission("releases.edit")]
    [ProducesResponseType(typeof(BulkAddSprintsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<BulkAddSprintsResponse>> BulkAddSprintsToRelease(
        int releaseId,
        [FromBody] BulkAddSprintsRequest request,
        CancellationToken ct = default)
    {
        try
        {
            if (request.SprintIds == null || request.SprintIds.Count == 0)
            {
                return BadRequest(new { error = "SprintIds cannot be empty" });
            }

            var command = new BulkAddSprintsToReleaseCommand
            {
                ReleaseId = releaseId,
                SprintIds = request.SprintIds
            };
            var count = await _mediator.Send(command, ct);
            return Ok(new BulkAddSprintsResponse { AddedCount = count });
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Release {ReleaseId} not found for bulk add", releaseId);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk adding sprints to release {ReleaseId}", releaseId);
            return Problem(
                title: "Error bulk adding sprints",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Generate release notes for a release using AI.
    /// </summary>
    /// <param name="releaseId">The release ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Generated release notes</returns>
    /// <response code="200">Release notes generated successfully</response>
    /// <response code="404">Release not found</response>
    /// <response code="403">Insufficient permissions</response>
    [HttpPost("{releaseId}/notes/generate")]
    [RequirePermission("releases.notes.edit")]
    [ProducesResponseType(typeof(GenerateReleaseNotesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GenerateReleaseNotesResponse>> GenerateReleaseNotes(
        int releaseId,
        CancellationToken ct = default)
    {
        try
        {
            var command = new GenerateReleaseNotesCommand
            {
                ReleaseId = releaseId
            };
            var notes = await _mediator.Send(command, ct);
            return Ok(new GenerateReleaseNotesResponse { ReleaseNotes = notes });
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Release {ReleaseId} not found for notes generation", releaseId);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating release notes for release {ReleaseId}", releaseId);
            return Problem(
                title: "Error generating release notes",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Update release notes (auto-generate or manual).
    /// </summary>
    /// <param name="releaseId">The release ID</param>
    /// <param name="request">Request containing notes and auto-generate flag</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="200">Release notes updated successfully</response>
    /// <response code="404">Release not found</response>
    /// <response code="403">Insufficient permissions</response>
    [HttpPut("{releaseId}/notes")]
    [RequirePermission("releases.notes.edit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateReleaseNotes(
        int releaseId,
        [FromBody] UpdateReleaseNotesRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var command = new UpdateReleaseNotesCommand
            {
                ReleaseId = releaseId,
                ReleaseNotes = request.ReleaseNotes,
                AutoGenerate = request.AutoGenerate
            };
            await _mediator.Send(command, ct);
            return Ok();
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Release {ReleaseId} not found for notes update", releaseId);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating release notes for release {ReleaseId}", releaseId);
            return Problem(
                title: "Error updating release notes",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Generate changelog for a release using AI.
    /// </summary>
    /// <param name="releaseId">The release ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Generated changelog</returns>
    /// <response code="200">Changelog generated successfully</response>
    /// <response code="404">Release not found</response>
    /// <response code="403">Insufficient permissions</response>
    [HttpPost("{releaseId}/changelog/generate")]
    [RequirePermission("releases.notes.edit")]
    [ProducesResponseType(typeof(GenerateChangelogResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GenerateChangelogResponse>> GenerateChangelog(
        int releaseId,
        CancellationToken ct = default)
    {
        try
        {
            var command = new GenerateChangeLogCommand
            {
                ReleaseId = releaseId
            };
            var changelog = await _mediator.Send(command, ct);
            return Ok(new GenerateChangelogResponse { ChangeLog = changelog });
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Release {ReleaseId} not found for changelog generation", releaseId);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating changelog for release {ReleaseId}", releaseId);
            return Problem(
                title: "Error generating changelog",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Update changelog (auto-generate or manual).
    /// </summary>
    /// <param name="releaseId">The release ID</param>
    /// <param name="request">Request containing changelog and auto-generate flag</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="200">Changelog updated successfully</response>
    /// <response code="404">Release not found</response>
    /// <response code="403">Insufficient permissions</response>
    [HttpPut("{releaseId}/changelog")]
    [RequirePermission("releases.notes.edit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateChangelog(
        int releaseId,
        [FromBody] UpdateChangelogRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var command = new UpdateChangeLogCommand
            {
                ReleaseId = releaseId,
                ChangeLog = request.ChangeLog,
                AutoGenerate = request.AutoGenerate
            };
            await _mediator.Send(command, ct);
            return Ok();
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Release {ReleaseId} not found for changelog update", releaseId);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating changelog for release {ReleaseId}", releaseId);
            return Problem(
                title: "Error updating changelog",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Evaluate all quality gates for a release.
    /// </summary>
    /// <param name="releaseId">The release ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of quality gate evaluation results</returns>
    /// <response code="200">Quality gates evaluated successfully</response>
    /// <response code="404">Release not found</response>
    /// <response code="403">Insufficient permissions</response>
    [HttpPost("{releaseId}/quality-gates/evaluate")]
    [RequirePermission("releases.view")]
    [ProducesResponseType(typeof(List<QualityGateDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<QualityGateDto>>> EvaluateQualityGates(
        int releaseId,
        CancellationToken ct = default)
    {
        try
        {
            var command = new EvaluateQualityGatesCommand
            {
                ReleaseId = releaseId
            };
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Release {ReleaseId} not found for quality gate evaluation", releaseId);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating quality gates for release {ReleaseId}", releaseId);
            return Problem(
                title: "Error evaluating quality gates",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Approve a quality gate manually.
    /// </summary>
    /// <param name="releaseId">The release ID</param>
    /// <param name="request">Request containing gate type</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="200">Quality gate approved successfully</response>
    /// <response code="400">Invalid gate type or gate cannot be approved</response>
    /// <response code="404">Release not found</response>
    /// <response code="403">Insufficient permissions</response>
    [HttpPost("{releaseId}/quality-gates/approve")]
    [RequirePermission("releases.quality-gates.approve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ApproveQualityGate(
        int releaseId,
        [FromBody] ApproveQualityGateRequest request,
        CancellationToken ct = default)
    {
        try
        {
            if (!Enum.IsDefined(typeof(QualityGateType), request.GateType))
            {
                return BadRequest(new { error = $"Invalid quality gate type: {request.GateType}" });
            }
            var gateType = (QualityGateType)request.GateType;

            var command = new ApproveQualityGateCommand
            {
                ReleaseId = releaseId,
                GateType = gateType
            };
            await _mediator.Send(command, ct);
            return Ok();
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Release {ReleaseId} not found for quality gate approval", releaseId);
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot approve quality gate for release {ReleaseId}: {Message}", releaseId, ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving quality gate for release {ReleaseId}", releaseId);
            return Problem(
                title: "Error approving quality gate",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Remove a sprint from a release.
    /// </summary>
    /// <param name="sprintId">The sprint ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">Sprint removed successfully</response>
    /// <response code="404">Sprint not found</response>
    /// <response code="403">Insufficient permissions</response>
    [HttpDelete("sprints/{sprintId}")]
    [RequirePermission("releases.edit")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemoveSprintFromRelease(
        int sprintId,
        CancellationToken ct = default)
    {
        try
        {
            var command = new RemoveSprintFromReleaseCommand
            {
                SprintId = sprintId
            };
            await _mediator.Send(command, ct);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Sprint {SprintId} not found for removal from release", sprintId);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing sprint {SprintId} from release", sprintId);
            return Problem(
                title: "Error removing sprint from release",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    // Request/Response DTOs for the controller
    public class CreateReleaseRequest
    {
        public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = string.Empty;
    public DateTimeOffset PlannedDate { get; set; }
    public bool? IsPreRelease { get; set; }
    public string? TagName { get; set; }
        public List<int>? SprintIds { get; set; }
    }

    public class UpdateReleaseRequest
    {
        public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTimeOffset PlannedDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class BulkAddSprintsRequest
    {
        public List<int> SprintIds { get; set; } = new();
    }

    public class BulkAddSprintsResponse
    {
        public int AddedCount { get; set; }
    }

    public class UpdateReleaseNotesRequest
    {
        public string? ReleaseNotes { get; set; }
        public bool AutoGenerate { get; set; }
    }

    public class UpdateChangelogRequest
    {
        public string? ChangeLog { get; set; }
        public bool AutoGenerate { get; set; }
    }

    public class GenerateReleaseNotesResponse
    {
        public string ReleaseNotes { get; set; } = string.Empty;
    }

    public class GenerateChangelogResponse
    {
        public string ChangeLog { get; set; } = string.Empty;
    }

    public class ApproveQualityGateRequest
    {
        public int GateType { get; set; }
    }
}

