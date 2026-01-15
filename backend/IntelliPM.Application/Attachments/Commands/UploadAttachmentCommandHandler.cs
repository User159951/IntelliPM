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
/// Validates file size, extension, MIME type, and entity existence before saving.
/// Verifies user belongs to organization for tenant isolation.
/// </summary>
public class UploadAttachmentCommandHandler : IRequestHandler<UploadAttachmentCommand, AttachmentDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;
    private readonly IFileValidationService _fileValidationService;
    private readonly ISettingsService _settingsService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UploadAttachmentCommandHandler> _logger;

    public UploadAttachmentCommandHandler(
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorageService,
        IFileValidationService fileValidationService,
        ISettingsService settingsService,
        ICurrentUserService currentUserService,
        ILogger<UploadAttachmentCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
        _fileValidationService = fileValidationService ?? throw new ArgumentNullException(nameof(fileValidationService));
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async System.Threading.Tasks.Task<AttachmentDto> Handle(UploadAttachmentCommand request, CancellationToken cancellationToken)
    {
        // Verify user belongs to organization (tenant isolation)
        var userOrganizationId = _currentUserService.GetOrganizationId();
        if (userOrganizationId == 0)
        {
            throw new UnauthorizedException("User not authenticated or organization not found");
        }

        if (userOrganizationId != request.OrganizationId)
        {
            _logger.LogWarning(
                "User {UserId} attempted to upload file to organization {RequestOrgId} but belongs to {UserOrgId}",
                request.UploadedById, request.OrganizationId, userOrganizationId);
            throw new NotFoundException("Organization not found"); // Return 404, not 403, for security
        }

        // Verify user exists and belongs to the organization
        var user = await _unitOfWork.Repository<User>()
            .Query()
            .IgnoreQueryFilters() // Bypass tenant filter to check user's actual organization
            .FirstOrDefaultAsync(u => u.Id == request.UploadedById, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException($"User with ID {request.UploadedById} not found");
        }

        if (user.OrganizationId != request.OrganizationId)
        {
            _logger.LogWarning(
                "User {UserId} (Org: {UserOrgId}) attempted to upload file to organization {RequestOrgId}",
                request.UploadedById, user.OrganizationId, request.OrganizationId);
            throw new NotFoundException("Organization not found"); // Return 404, not 403, for security
        }

        // Get max file size from settings (with fallback to constant default)
        var maxFileSizeBytes = await _settingsService.GetSettingLongAsync(
            request.OrganizationId, 
            "Attachment.MaxFileSizeBytes", 
            cancellationToken) 
            ?? AttachmentConstants.MaxFileSizeBytes; // Fallback to constant if not found

        // Validate file using FileValidationService (MIME type, extension, size, path traversal)
        _fileValidationService.ValidateFile(request.File, maxFileSizeBytes);

        // Validate entity exists (based on EntityType) and belongs to organization
        await ValidateEntityExistsAsync(request.EntityType, request.EntityId, request.OrganizationId, cancellationToken);

        // Check total size for entity (optional - can be implemented later)
        // var totalSize = await GetTotalAttachmentSizeForEntityAsync(request.EntityType, request.EntityId, cancellationToken);
        // if (totalSize + request.File.Length > AttachmentConstants.MaxTotalSizePerEntity)
        // {
        //     throw new ValidationException("Total attachment size for this entity exceeds the limit");
        // }

        // Sanitize filename to prevent path traversal
        var sanitizedFileName = _fileValidationService.SanitizeFileName(request.File.FileName);

        // Generate unique filename
        var storedFileName = _fileStorageService.GenerateUniqueFileName(sanitizedFileName, request.OrganizationId);

        // Save file to storage
        using var fileStream = request.File.OpenReadStream();
        var savedFileName = await _fileStorageService.SaveFileAsync(fileStream, sanitizedFileName, request.OrganizationId, cancellationToken);

        // Determine content type
        var fileExtension = Path.GetExtension(sanitizedFileName).ToLowerInvariant();
        var contentType = AttachmentConstants.ContentTypeMappings.TryGetValue(fileExtension, out var mappedContentType)
            ? mappedContentType
            : request.File.ContentType ?? "application/octet-stream";

        // Create attachment entity
        var attachment = new Attachment
        {
            OrganizationId = request.OrganizationId,
            EntityType = request.EntityType,
            EntityId = request.EntityId,
            FileName = sanitizedFileName, // Store sanitized filename
            StoredFileName = savedFileName, // Includes organization path
            FileExtension = fileExtension,
            ContentType = contentType,
            FileSizeBytes = request.File.Length,
            StoragePath = savedFileName, // Relative path with organization ID
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

