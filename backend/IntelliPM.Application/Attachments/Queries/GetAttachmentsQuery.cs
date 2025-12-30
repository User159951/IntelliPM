using MediatR;

namespace IntelliPM.Application.Attachments.Queries;

/// <summary>
/// Query to retrieve all attachments for a specific entity.
/// </summary>
public class GetAttachmentsQuery : IRequest<List<AttachmentDto>>
{
    /// <summary>
    /// The type of entity to get attachments for (Task, Project, Comment, Defect).
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the entity to get attachments for.
    /// </summary>
    public int EntityId { get; set; }

    /// <summary>
    /// The organization ID for multi-tenancy filtering.
    /// </summary>
    public int OrganizationId { get; set; }
}

/// <summary>
/// Query to retrieve a single attachment by ID.
/// </summary>
public class GetAttachmentByIdQuery : IRequest<AttachmentDto>
{
    /// <summary>
    /// The ID of the attachment.
    /// </summary>
    public int AttachmentId { get; set; }

    /// <summary>
    /// The organization ID for multi-tenancy filtering.
    /// </summary>
    public int OrganizationId { get; set; }
}

/// <summary>
/// DTO representing an attachment for API responses.
/// </summary>
public class AttachmentDto
{
    public int Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public int UploadedById { get; set; }
    public string UploadedBy { get; set; } = string.Empty;
    public DateTimeOffset UploadedAt { get; set; }
}

