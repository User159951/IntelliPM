using Asp.Versioning;
using IntelliPM.Application.Attachments.Commands;
using IntelliPM.Application.Attachments.Queries;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Services;
using IntelliPM.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IntelliPM.API.Authorization;

namespace IntelliPM.API.Controllers;

/// <summary>
/// Controller for managing file attachments on entities (Tasks, Projects, Comments, etc.).
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class AttachmentsController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AttachmentsController> _logger;

    public AttachmentsController(
        IMediator mediator,
        ICurrentUserService currentUserService,
        IFileStorageService fileStorageService,
        IUnitOfWork unitOfWork,
        ILogger<AttachmentsController> logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _fileStorageService = fileStorageService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Get all attachments for a specific entity
    /// </summary>
    /// <param name="entityType">Type of entity (Task, Project, Comment, Defect)</param>
    /// <param name="entityId">ID of the entity</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of attachments for the entity</returns>
    /// <response code="200">Attachments retrieved successfully</response>
    /// <response code="400">Bad request - Invalid entity type or ID</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [RequirePermission("tasks.view")] // Attachments are typically on tasks
    [ProducesResponseType(typeof(List<AttachmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAttachments(
        [FromQuery] string entityType,
        [FromQuery] int entityId,
        CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(entityType))
            {
                return BadRequest(new { error = "Entity type is required" });
            }

            if (entityId <= 0)
            {
                return BadRequest(new { error = "Entity ID must be greater than 0" });
            }

            var organizationId = _currentUserService.GetOrganizationId();
            if (organizationId == 0)
            {
                return Unauthorized(new { error = "Organization ID not found" });
            }

            var query = new GetAttachmentsQuery
            {
                EntityType = entityType,
                EntityId = entityId,
                OrganizationId = organizationId
            };

            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting attachments for {EntityType} {EntityId}", entityType, entityId);
            return Problem(
                title: "Error retrieving attachments",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Upload a new attachment to an entity
    /// </summary>
    /// <param name="file">The file to upload</param>
    /// <param name="entityType">Type of entity (Task, Project, Comment, Defect)</param>
    /// <param name="entityId">ID of the entity</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Created attachment information</returns>
    /// <response code="200">Attachment uploaded successfully</response>
    /// <response code="400">Bad request - Validation failed</response>
    /// <response code="404">Entity not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("upload")]
    [RequirePermission("tasks.edit")] // Uploading attachments requires edit permission
    [ProducesResponseType(typeof(AttachmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UploadAttachment(
        [FromForm] IFormFile file,
        [FromForm] string entityType,
        [FromForm] int entityId,
        CancellationToken ct = default)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { error = "File is required" });
            }

            if (string.IsNullOrWhiteSpace(entityType))
            {
                return BadRequest(new { error = "Entity type is required" });
            }

            if (entityId <= 0)
            {
                return BadRequest(new { error = "Entity ID must be greater than 0" });
            }

            var userId = GetCurrentUserId();
            var organizationId = _currentUserService.GetOrganizationId();

            if (organizationId == 0)
            {
                return Unauthorized(new { error = "Organization ID not found" });
            }

            var command = new UploadAttachmentCommand
            {
                EntityType = entityType,
                EntityId = entityId,
                File = file,
                UploadedById = userId,
                OrganizationId = organizationId
            };

            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading attachment for {EntityType} {EntityId}", entityType, entityId);

            if (ex is Application.Common.Exceptions.NotFoundException)
            {
                return NotFound(new { error = ex.Message });
            }

            if (ex is Application.Common.Exceptions.ValidationException)
            {
                return BadRequest(new { error = ex.Message });
            }

            return Problem(
                title: "Error uploading attachment",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Download an attachment by ID
    /// </summary>
    /// <param name="id">Attachment ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>File stream</returns>
    /// <response code="200">File downloaded successfully</response>
    /// <response code="404">Attachment not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id}")]
    [RequirePermission("tasks.view")] // Downloading attachments requires view permission
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DownloadAttachment(
        int id,
        CancellationToken ct = default)
    {
        try
        {
            var organizationId = _currentUserService.GetOrganizationId();
            if (organizationId == 0)
            {
                return Unauthorized(new { error = "Organization ID not found" });
            }

            // Get attachment entity to retrieve stored filename and metadata
            var attachmentRepo = _unitOfWork.Repository<Attachment>();
            var attachmentEntity = await attachmentRepo.Query()
                .FirstOrDefaultAsync(
                    a => a.Id == id &&
                         a.OrganizationId == organizationId &&
                         !a.IsDeleted,
                    ct);
            
            if (attachmentEntity == null)
            {
                return NotFound(new { error = "Attachment not found" });
            }

            // Get file stream using stored filename
            var fileStream = await _fileStorageService.GetFileAsync(attachmentEntity.StoredFileName, ct);

            // Return file with appropriate content type
            return File(
                fileStream,
                attachmentEntity.ContentType,
                attachmentEntity.FileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading attachment {AttachmentId}", id);

            if (ex is Application.Common.Exceptions.NotFoundException || ex is FileNotFoundException)
            {
                return NotFound(new { error = "Attachment not found" });
            }

            return Problem(
                title: "Error downloading attachment",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Delete an attachment (soft delete)
    /// </summary>
    /// <param name="id">Attachment ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">Attachment deleted successfully</response>
    /// <response code="401">Unauthorized - Not the uploader</response>
    /// <response code="404">Attachment not found</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("{id}")]
    [RequirePermission("tasks.edit")] // Users can delete their own attachments (checked in handler)
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteAttachment(
        int id,
        CancellationToken ct = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var organizationId = _currentUserService.GetOrganizationId();

            if (organizationId == 0)
            {
                return Unauthorized(new { error = "Organization ID not found" });
            }

            var command = new DeleteAttachmentCommand
            {
                AttachmentId = id,
                UserId = userId,
                OrganizationId = organizationId
            };

            await _mediator.Send(command, ct);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting attachment {AttachmentId}", id);

            if (ex is Application.Common.Exceptions.NotFoundException)
            {
                return NotFound(new { error = ex.Message });
            }

            if (ex is Application.Common.Exceptions.UnauthorizedException)
            {
                return Unauthorized(new { error = ex.Message });
            }

            return Problem(
                title: "Error deleting attachment",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }
}

