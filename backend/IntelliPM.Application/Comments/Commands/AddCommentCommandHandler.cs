using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Services;
using IntelliPM.Domain.Constants;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace IntelliPM.Application.Comments.Commands;

/// <summary>
/// Handler for adding comments to entities.
/// Creates the comment, parses mentions, and publishes domain events via Outbox pattern.
/// </summary>
public class AddCommentCommandHandler : IRequestHandler<AddCommentCommand, AddCommentResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMentionParser _mentionParser;
    private readonly ILogger<AddCommentCommandHandler> _logger;

    public AddCommentCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IMentionParser mentionParser,
        ILogger<AddCommentCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _mentionParser = mentionParser ?? throw new ArgumentNullException(nameof(mentionParser));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async System.Threading.Tasks.Task<AddCommentResponse> Handle(AddCommentCommand request, CancellationToken ct)
    {
        var userId = _currentUserService.GetUserId();
        var organizationId = _currentUserService.GetOrganizationId();

        if (userId == 0 || organizationId == 0)
        {
            throw new UnauthorizedException("User must be authenticated");
        }

        // Validate entity exists (based on EntityType)
        await ValidateEntityExistsAsync(request.EntityType, request.EntityId, organizationId, ct);

        // Create comment
        var comment = new Comment
        {
            OrganizationId = organizationId,
            EntityType = request.EntityType,
            EntityId = request.EntityId,
            Content = request.Content,
            AuthorId = userId,
            ParentCommentId = request.ParentCommentId,
            CreatedAt = DateTimeOffset.UtcNow,
            IsEdited = false,
            IsDeleted = false
        };

        await _unitOfWork.Repository<Comment>().AddAsync(comment, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // Parse mentions from content
        var parsedMentions = _mentionParser.ParseMentions(request.Content);
        var mentionedUserIds = await _mentionParser.ResolveMentionedUserIds(parsedMentions, organizationId, ct);

        // Create mention entities
        var mentionEntities = new List<Mention>();
        foreach (var parsedMention in parsedMentions)
        {
            var user = await _unitOfWork.Repository<User>()
                .Query()
                .FirstOrDefaultAsync(u => u.Username == parsedMention.Username && 
                                         u.OrganizationId == organizationId && 
                                         u.IsActive, ct);

            if (user != null && mentionedUserIds.Contains(user.Id))
            {
                var mention = new Mention
                {
                    OrganizationId = organizationId,
                    CommentId = comment.Id,
                    MentionedUserId = user.Id,
                    StartIndex = parsedMention.StartIndex,
                    Length = parsedMention.Length,
                    MentionText = parsedMention.MentionText,
                    NotificationSent = false,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                await _unitOfWork.Repository<Mention>().AddAsync(mention, ct);
                mentionEntities.Add(mention);
            }
        }

        await _unitOfWork.SaveChangesAsync(ct);

        // Get author info
        var author = await _unitOfWork.Repository<User>().GetByIdAsync(userId, ct);
        if (author == null)
        {
            throw new NotFoundException($"User with ID {userId} not found");
        }

        var authorName = $"{author.FirstName} {author.LastName}".Trim();
        if (string.IsNullOrEmpty(authorName))
        {
            authorName = author.Username;
        }

        // Publish CommentAddedEvent via Outbox
        var commentAddedEvent = new CommentAddedEvent
        {
            CommentId = comment.Id,
            AuthorId = userId,
            AuthorName = authorName,
            EntityType = request.EntityType,
            EntityId = request.EntityId,
            Content = request.Content,
            ParentCommentId = request.ParentCommentId,
            OrganizationId = organizationId
        };

        var outboxMessage = OutboxMessage.Create(
            typeof(CommentAddedEvent).FullName!,
            JsonSerializer.Serialize(commentAddedEvent),
            $"comment-added-{comment.Id}-{DateTimeOffset.UtcNow.Ticks}"
        );

        await _unitOfWork.Repository<OutboxMessage>().AddAsync(outboxMessage, ct);

        // Publish UserMentionedEvent for each mention via Outbox
        var entityTitle = await GetEntityTitleAsync(request.EntityType, request.EntityId, ct);

        foreach (var mentionEntity in mentionEntities)
        {
            var userMentionedEvent = new UserMentionedEvent
            {
                MentionId = mentionEntity.Id,
                MentionedUserId = mentionEntity.MentionedUserId,
                CommentId = comment.Id,
                CommentAuthorId = userId,
                CommentAuthorName = authorName,
                EntityType = request.EntityType,
                EntityId = request.EntityId,
                EntityTitle = entityTitle,
                MentionText = mentionEntity.MentionText,
                CommentContent = request.Content,
                OrganizationId = organizationId
            };

            var mentionOutboxMessage = OutboxMessage.Create(
                typeof(UserMentionedEvent).FullName!,
                JsonSerializer.Serialize(userMentionedEvent),
                $"user-mentioned-{mentionEntity.MentionedUserId}-{comment.Id}-{DateTimeOffset.UtcNow.Ticks}"
            );

            await _unitOfWork.Repository<OutboxMessage>().AddAsync(mentionOutboxMessage, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Comment {CommentId} added to {EntityType} {EntityId} by user {UserId}", 
            comment.Id, request.EntityType, request.EntityId, userId);

        return new AddCommentResponse(
            comment.Id,
            userId,
            authorName,
            request.Content,
            comment.CreatedAt,
            mentionedUserIds.Distinct().ToList()
        );
    }

    private async System.Threading.Tasks.Task ValidateEntityExistsAsync(string entityType, int entityId, int organizationId, CancellationToken ct)
    {
        var exists = entityType switch
        {
            CommentConstants.EntityTypes.Task => await _unitOfWork.Repository<ProjectTask>().Query()
                .AnyAsync(t => t.Id == entityId && t.OrganizationId == organizationId, ct),
            CommentConstants.EntityTypes.Project => await _unitOfWork.Repository<Project>().Query()
                .AnyAsync(p => p.Id == entityId && p.OrganizationId == organizationId, ct),
            CommentConstants.EntityTypes.Sprint => await _unitOfWork.Repository<Sprint>().Query()
                .AnyAsync(s => s.Id == entityId && s.OrganizationId == organizationId, ct),
            CommentConstants.EntityTypes.Defect => await _unitOfWork.Repository<Defect>().Query()
                .AnyAsync(d => d.Id == entityId && d.OrganizationId == organizationId, ct),
            _ => throw new Common.Exceptions.ValidationException($"Invalid entity type: {entityType}")
        };

        if (!exists)
        {
            throw new NotFoundException($"{entityType} with ID {entityId} not found");
        }
    }

    private async System.Threading.Tasks.Task<string> GetEntityTitleAsync(string entityType, int entityId, CancellationToken ct)
    {
        return entityType switch
        {
            CommentConstants.EntityTypes.Task => (await _unitOfWork.Repository<ProjectTask>().GetByIdAsync(entityId, ct))?.Title ?? "Task",
            CommentConstants.EntityTypes.Project => (await _unitOfWork.Repository<Project>().GetByIdAsync(entityId, ct))?.Name ?? "Project",
            CommentConstants.EntityTypes.Sprint => (await _unitOfWork.Repository<Sprint>().GetByIdAsync(entityId, ct)) is Sprint sprint 
                ? $"Sprint {sprint.Number}" 
                : "Sprint",
            CommentConstants.EntityTypes.Defect => (await _unitOfWork.Repository<Defect>().GetByIdAsync(entityId, ct))?.Title ?? "Defect",
            _ => "Item"
        };
    }
}

