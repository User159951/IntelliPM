using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Attachments.Queries;

/// <summary>
/// Handler for retrieving attachments for a specific entity.
/// </summary>
public class GetAttachmentsQueryHandler : IRequestHandler<GetAttachmentsQuery, List<AttachmentDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetAttachmentsQueryHandler> _logger;

    public GetAttachmentsQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetAttachmentsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<AttachmentDto>> Handle(GetAttachmentsQuery request, CancellationToken cancellationToken)
    {
        var attachmentRepo = _unitOfWork.Repository<Attachment>();

        var attachments = await attachmentRepo.Query()
            .Where(a => a.EntityType == request.EntityType &&
                       a.EntityId == request.EntityId &&
                       a.OrganizationId == request.OrganizationId &&
                       !a.IsDeleted)
            .Include(a => a.UploadedBy)
            .OrderByDescending(a => a.UploadedAt)
            .Select(a => new AttachmentDto
            {
                Id = a.Id,
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                FileName = a.FileName,
                FileExtension = a.FileExtension,
                FileSizeBytes = a.FileSizeBytes,
                ContentType = a.ContentType,
                UploadedById = a.UploadedById,
                UploadedBy = $"{a.UploadedBy.FirstName} {a.UploadedBy.LastName}".Trim() != string.Empty
                    ? $"{a.UploadedBy.FirstName} {a.UploadedBy.LastName}".Trim()
                    : a.UploadedBy.Username,
                UploadedAt = a.UploadedAt
            })
            .ToListAsync(cancellationToken);

        _logger.LogInformation(
            "Retrieved {Count} attachments for {EntityType} {EntityId} in organization {OrganizationId}",
            attachments.Count, request.EntityType, request.EntityId, request.OrganizationId);

        return attachments;
    }
}

/// <summary>
/// Handler for retrieving a single attachment by ID.
/// </summary>
public class GetAttachmentByIdQueryHandler : IRequestHandler<GetAttachmentByIdQuery, AttachmentDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetAttachmentByIdQueryHandler> _logger;

    public GetAttachmentByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetAttachmentByIdQueryHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<AttachmentDto> Handle(GetAttachmentByIdQuery request, CancellationToken cancellationToken)
    {
        var attachmentRepo = _unitOfWork.Repository<Attachment>();

        var attachment = await attachmentRepo.Query()
            .Include(a => a.UploadedBy)
            .FirstOrDefaultAsync(
                a => a.Id == request.AttachmentId &&
                     a.OrganizationId == request.OrganizationId &&
                     !a.IsDeleted,
                cancellationToken);

        if (attachment == null)
        {
            throw new Application.Common.Exceptions.NotFoundException(
                $"Attachment with ID {request.AttachmentId} not found");
        }

        return new AttachmentDto
        {
            Id = attachment.Id,
            EntityType = attachment.EntityType,
            EntityId = attachment.EntityId,
            FileName = attachment.FileName,
            FileExtension = attachment.FileExtension,
            FileSizeBytes = attachment.FileSizeBytes,
            ContentType = attachment.ContentType,
            UploadedById = attachment.UploadedById,
            UploadedBy = $"{attachment.UploadedBy.FirstName} {attachment.UploadedBy.LastName}".Trim() != string.Empty
                ? $"{attachment.UploadedBy.FirstName} {attachment.UploadedBy.LastName}".Trim()
                : attachment.UploadedBy.Username,
            UploadedAt = attachment.UploadedAt
        };
    }
}

