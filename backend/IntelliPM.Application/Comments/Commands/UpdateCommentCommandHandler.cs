using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Comments.Queries;
using IntelliPM.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Comments.Commands;

/// <summary>
/// Handler for updating a comment.
/// Only the comment author or an admin can update a comment.
/// </summary>
public class UpdateCommentCommandHandler : IRequestHandler<UpdateCommentCommand, CommentDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdateCommentCommandHandler> _logger;

    public UpdateCommentCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<UpdateCommentCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CommentDto> Handle(UpdateCommentCommand request, CancellationToken cancellationToken)
    {
        var commentRepo = _unitOfWork.Repository<Comment>();

        var comment = await commentRepo.Query()
            .Include(c => c.Author)
            .FirstOrDefaultAsync(
                c => c.Id == request.CommentId &&
                     c.OrganizationId == request.OrganizationId &&
                     !c.IsDeleted,
                cancellationToken);

        if (comment == null)
        {
            throw new NotFoundException($"Comment with ID {request.CommentId} not found");
        }

        // Authorization: Only the author or an admin can update
        var isAdmin = _currentUserService.IsAdmin();
        if (comment.AuthorId != request.UserId && !isAdmin)
        {
            throw new UnauthorizedException("You can only update your own comments");
        }

        // Update comment
        comment.Content = request.Content;
        comment.UpdatedAt = DateTimeOffset.UtcNow;
        comment.IsEdited = true;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Comment {CommentId} updated by user {UserId}",
            request.CommentId, request.UserId);

        return new CommentDto
        {
            Id = comment.Id,
            EntityType = comment.EntityType,
            EntityId = comment.EntityId,
            Content = comment.Content,
            AuthorId = comment.AuthorId,
            AuthorName = $"{comment.Author.FirstName} {comment.Author.LastName}".Trim() != string.Empty
                ? $"{comment.Author.FirstName} {comment.Author.LastName}".Trim()
                : comment.Author.Username,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt,
            IsEdited = comment.IsEdited,
            ParentCommentId = comment.ParentCommentId
        };
    }
}

