using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Comments.Commands;

/// <summary>
/// Handler for deleting (soft deleting) a comment.
/// Only the comment author or an admin can delete a comment.
/// </summary>
public class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeleteCommentCommandHandler> _logger;

    public DeleteCommentCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<DeleteCommentCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Unit> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        var commentRepo = _unitOfWork.Repository<Comment>();

        var comment = await commentRepo.Query()
            .FirstOrDefaultAsync(
                c => c.Id == request.CommentId &&
                     c.OrganizationId == request.OrganizationId &&
                     !c.IsDeleted,
                cancellationToken);

        if (comment == null)
        {
            throw new NotFoundException($"Comment with ID {request.CommentId} not found");
        }

        // Authorization: Only the author or an admin can delete
        var isAdmin = _currentUserService.IsAdmin();
        if (comment.AuthorId != request.UserId && !isAdmin)
        {
            throw new UnauthorizedException("You can only delete your own comments");
        }

        // Soft delete
        comment.IsDeleted = true;
        comment.DeletedAt = DateTimeOffset.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Comment {CommentId} deleted (soft delete) by user {UserId}",
            request.CommentId, request.UserId);

        return Unit.Value;
    }
}

