using IntelliPM.Application.Attachments.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace IntelliPM.Application.Attachments.Commands;

/// <summary>
/// Command to upload a new attachment.
/// </summary>
public class UploadAttachmentCommand : IRequest<AttachmentDto>
{
    /// <summary>
    /// The type of entity to attach the file to (Task, Project, Comment, Defect).
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the entity to attach the file to.
    /// </summary>
    public int EntityId { get; set; }

    /// <summary>
    /// The file to upload.
    /// </summary>
    public IFormFile File { get; set; } = null!;

    /// <summary>
    /// The ID of the user uploading the file.
    /// </summary>
    public int UploadedById { get; set; }

    /// <summary>
    /// The organization ID for multi-tenancy filtering.
    /// </summary>
    public int OrganizationId { get; set; }
}

