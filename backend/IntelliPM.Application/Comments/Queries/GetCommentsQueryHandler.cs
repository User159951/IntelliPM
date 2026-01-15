using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Constants;
using IntelliPM.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Comments.Queries;

/// <summary>
/// Handler for retrieving comments for a specific entity.
/// Includes tenant isolation checks to prevent cross-organization access.
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
        // Validate organization ID
        if (request.OrganizationId <= 0)
        {
            _logger.LogWarning("Invalid organization ID {OrganizationId} in GetCommentsQuery", request.OrganizationId);
            return new List<CommentDto>();
        }

        // Validate that the entity belongs to the organization (tenant isolation)
        // This prevents users from querying comments on entities from other organizations
        var entityBelongsToOrg = await ValidateEntityBelongsToOrganizationAsync(
            request.EntityType, 
            request.EntityId, 
            request.OrganizationId, 
            cancellationToken);

        if (!entityBelongsToOrg)
        {
            _logger.LogWarning(
                "Entity {EntityType} {EntityId} does not belong to organization {OrganizationId}",
                request.EntityType, request.EntityId, request.OrganizationId);
            return new List<CommentDto>(); // Return empty list instead of throwing to prevent information leakage
        }

        var commentRepo = _unitOfWork.Repository<Comment>();

        // CRITICAL: Always filter by OrganizationId for tenant isolation
        var comments = await commentRepo.Query()
            .Where(c => c.EntityType == request.EntityType &&
                       c.EntityId == request.EntityId &&
                       c.OrganizationId == request.OrganizationId && // Tenant isolation filter
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

    /// <summary>
    /// Validates that the entity belongs to the specified organization (tenant isolation).
    /// </summary>
    private async Task<bool> ValidateEntityBelongsToOrganizationAsync(
        string entityType, 
        int entityId, 
        int organizationId, 
        CancellationToken ct)
    {
        return entityType switch
        {
            CommentConstants.EntityTypes.Task => await _unitOfWork.Repository<ProjectTask>().Query()
                .AnyAsync(t => t.Id == entityId && t.OrganizationId == organizationId, ct),
            CommentConstants.EntityTypes.Project => await _unitOfWork.Repository<Project>().Query()
                .AnyAsync(p => p.Id == entityId && p.OrganizationId == organizationId, ct),
            CommentConstants.EntityTypes.Sprint => await _unitOfWork.Repository<Sprint>().Query()
                .AnyAsync(s => s.Id == entityId && s.OrganizationId == organizationId, ct),
            CommentConstants.EntityTypes.Defect => await _unitOfWork.Repository<Defect>().Query()
                .AnyAsync(d => d.Id == entityId && d.OrganizationId == organizationId, ct),
            _ => false // Unknown entity type
        };
    }
}

