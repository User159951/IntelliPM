using IntelliPM.Application.Attachments.Queries;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Services;
using IntelliPM.Domain.Constants;
using IntelliPM.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Attachments.Commands;

/// <summary>
/// Handler for uploading an attachment.
/// Validates file size, extension, and entity existence before saving.
/// </summary>
public class UploadAttachmentCommandHandler : IRequestHandler<UploadAttachmentCommand, AttachmentDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<UploadAttachmentCommandHandler> _logger;

    public UploadAttachmentCommandHandler(
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorageService,
        ILogger<UploadAttachmentCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async System.Threading.Tasks.Task<AttachmentDto> Handle(UploadAttachmentCommand request, CancellationToken cancellationToken)
    {
        // Validate file
        if (request.File == null || request.File.Length == 0)
        {
            throw new ValidationException("File is required");
        }

        // Validate file size
        if (request.File.Length > AttachmentConstants.MaxFileSizeBytes)
        {
            throw new ValidationException(
                $"File size exceeds maximum allowed size of {AttachmentConstants.MaxFileSizeBytes / (1024 * 1024)} MB");
        }

        // Validate file extension
        var fileExtension = Path.GetExtension(request.File.FileName).ToLowerInvariant();
        if (!AttachmentConstants.AllowedExtensions.Contains(fileExtension))
        {
            throw new ValidationException(
                $"File extension {fileExtension} is not allowed. Allowed extensions: {string.Join(", ", AttachmentConstants.AllowedExtensions)}");
        }

        // Validate entity exists (based on EntityType)
        await ValidateEntityExistsAsync(request.EntityType, request.EntityId, request.OrganizationId, cancellationToken);

        // Check total size for entity (optional - can be implemented later)
        // var totalSize = await GetTotalAttachmentSizeForEntityAsync(request.EntityType, request.EntityId, cancellationToken);
        // if (totalSize + request.File.Length > AttachmentConstants.MaxTotalSizePerEntity)
        // {
        //     throw new ValidationException("Total attachment size for this entity exceeds the limit");
        // }

        // Generate unique filename
        var storedFileName = _fileStorageService.GenerateUniqueFileName(request.File.FileName);

        // Save file to storage
        using var fileStream = request.File.OpenReadStream();
        var savedFileName = await _fileStorageService.SaveFileAsync(fileStream, request.File.FileName, cancellationToken);

        // Determine content type
        var contentType = AttachmentConstants.ContentTypeMappings.TryGetValue(fileExtension, out var mappedContentType)
            ? mappedContentType
            : request.File.ContentType ?? "application/octet-stream";

        // Create attachment entity
        var attachment = new Attachment
        {
            OrganizationId = request.OrganizationId,
            EntityType = request.EntityType,
            EntityId = request.EntityId,
            FileName = request.File.FileName,
            StoredFileName = savedFileName,
            FileExtension = fileExtension,
            ContentType = contentType,
            FileSizeBytes = request.File.Length,
            StoragePath = savedFileName, // Relative path
            UploadedById = request.UploadedById,
            UploadedAt = DateTimeOffset.UtcNow,
            IsDeleted = false
        };

        await _unitOfWork.Repository<Attachment>().AddAsync(attachment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Get uploaded by user info
        var uploadedBy = await _unitOfWork.Repository<User>()
            .GetByIdAsync(request.UploadedById, cancellationToken);

        if (uploadedBy == null)
        {
            throw new NotFoundException($"User with ID {request.UploadedById} not found");
        }

        var uploadedByName = $"{uploadedBy.FirstName} {uploadedBy.LastName}".Trim() != string.Empty
            ? $"{uploadedBy.FirstName} {uploadedBy.LastName}".Trim()
            : uploadedBy.Username;

        _logger.LogInformation(
            "Attachment {AttachmentId} uploaded for {EntityType} {EntityId} by user {UserId}",
            attachment.Id, request.EntityType, request.EntityId, request.UploadedById);

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
            UploadedBy = uploadedByName,
            UploadedAt = attachment.UploadedAt
        };
    }

    private async System.Threading.Tasks.Task ValidateEntityExistsAsync(string entityType, int entityId, int organizationId, CancellationToken cancellationToken)
    {
        var exists = entityType switch
        {
            AttachmentConstants.EntityTypes.Task => await _unitOfWork.Repository<ProjectTask>().Query()
                .AnyAsync(t => t.Id == entityId && t.OrganizationId == organizationId, cancellationToken),
            AttachmentConstants.EntityTypes.Project => await _unitOfWork.Repository<Project>().Query()
                .AnyAsync(p => p.Id == entityId && p.OrganizationId == organizationId, cancellationToken),
            AttachmentConstants.EntityTypes.Comment => await _unitOfWork.Repository<Comment>().Query()
                .AnyAsync(c => c.Id == entityId && c.OrganizationId == organizationId && !c.IsDeleted, cancellationToken),
            AttachmentConstants.EntityTypes.Defect => await _unitOfWork.Repository<Defect>().Query()
                .AnyAsync(d => d.Id == entityId && d.OrganizationId == organizationId, cancellationToken),
            _ => throw new ValidationException($"Invalid entity type: {entityType}")
        };

        if (!exists)
        {
            throw new NotFoundException($"{entityType} with ID {entityId} not found");
        }
    }
}

