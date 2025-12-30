using MediatR;

namespace IntelliPM.Application.Attachments.Commands;

/// <summary>
/// Command to delete (soft delete) an attachment.
/// </summary>
public record DeleteAttachmentCommand : IRequest<Unit>
{
    /// <summary>
    /// The ID of the attachment to delete.
    /// </summary>
    public int AttachmentId { get; init; }

    /// <summary>
    /// The ID of the user making the deletion (for authorization).
    /// </summary>
    public int UserId { get; init; }

    /// <summary>
    /// The organization ID for multi-tenancy filtering.
    /// </summary>
    public int OrganizationId { get; init; }
}

