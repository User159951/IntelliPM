using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Comments.Queries;

/// <summary>
/// Handler for retrieving comments for a specific entity.
/// </summary>
public class GetCommentsQueryHandler : IRequestHandler<GetCommentsQuery, List<CommentDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetCommentsQueryHandler> _logger;

    public GetCommentsQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetCommentsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<CommentDto>> Handle(GetCommentsQuery request, CancellationToken cancellationToken)
    {
        var commentRepo = _unitOfWork.Repository<Comment>();

        var comments = await commentRepo.Query()
            .Where(c => c.EntityType == request.EntityType &&
                       c.EntityId == request.EntityId &&
                       c.OrganizationId == request.OrganizationId &&
                       !c.IsDeleted)
            .Include(c => c.Author)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new CommentDto
            {
                Id = c.Id,
                EntityType = c.EntityType,
                EntityId = c.EntityId,
                Content = c.Content,
                AuthorId = c.AuthorId,
                AuthorName = $"{c.Author.FirstName} {c.Author.LastName}".Trim() != string.Empty
                    ? $"{c.Author.FirstName} {c.Author.LastName}".Trim()
                    : c.Author.Username,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                IsEdited = c.IsEdited,
                ParentCommentId = c.ParentCommentId
            })
            .ToListAsync(cancellationToken);

        _logger.LogInformation(
            "Retrieved {Count} comments for {EntityType} {EntityId} in organization {OrganizationId}",
            comments.Count, request.EntityType, request.EntityId, request.OrganizationId);

        return comments;
    }
}

