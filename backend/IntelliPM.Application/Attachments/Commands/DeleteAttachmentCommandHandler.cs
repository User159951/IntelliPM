using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Services;
using IntelliPM.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Attachments.Commands;

/// <summary>
/// Handler for deleting (soft deleting) an attachment.
/// Only the uploader or an admin can delete an attachment.
/// </summary>
public class DeleteAttachmentCommandHandler : IRequestHandler<DeleteAttachmentCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeleteAttachmentCommandHandler> _logger;

    public DeleteAttachmentCommandHandler(
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorageService,
        ICurrentUserService currentUserService,
        ILogger<DeleteAttachmentCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Unit> Handle(DeleteAttachmentCommand request, CancellationToken cancellationToken)
    {
        var attachmentRepo = _unitOfWork.Repository<Attachment>();

        var attachment = await attachmentRepo.Query()
            .FirstOrDefaultAsync(
                a => a.Id == request.AttachmentId &&
                     a.OrganizationId == request.OrganizationId &&
                     !a.IsDeleted,
                cancellationToken);

        if (attachment == null)
        {
            throw new NotFoundException($"Attachment with ID {request.AttachmentId} not found");
        }

        // Authorization: Only the uploader or an admin can delete
        var isAdmin = _currentUserService.IsAdmin();
        if (attachment.UploadedById != request.UserId && !isAdmin)
        {
            throw new UnauthorizedException("You can only delete your own attachments");
        }

        // Soft delete
        attachment.IsDeleted = true;
        attachment.DeletedAt = DateTimeOffset.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Optionally delete the physical file (can be done asynchronously or via background job)
        try
        {
            await _fileStorageService.DeleteFileAsync(attachment.StoredFileName, cancellationToken);
        }
        catch (Exception ex)
        {
            // Log but don't fail the request if file deletion fails
            _logger.LogWarning(ex, "Failed to delete physical file {StoredFileName} for attachment {AttachmentId}",
                attachment.StoredFileName, request.AttachmentId);
        }

        _logger.LogInformation(
            "Attachment {AttachmentId} deleted (soft delete) by user {UserId}",
            request.AttachmentId, request.UserId);

        return Unit.Value;
    }
}

