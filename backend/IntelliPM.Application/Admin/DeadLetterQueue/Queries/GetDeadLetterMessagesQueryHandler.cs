using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Models;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.Admin.DeadLetterQueue.Queries;

public class GetDeadLetterMessagesQueryHandler : IRequestHandler<GetDeadLetterMessagesQuery, PagedResponse<DeadLetterMessageDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetDeadLetterMessagesQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    public async Task<PagedResponse<DeadLetterMessageDto>> Handle(GetDeadLetterMessagesQuery request, CancellationToken cancellationToken)
    {
        // Ensure only admins can access dead letter queue
        if (!_currentUserService.IsAdmin())
        {
            throw new UnauthorizedAccessException("Only administrators can view dead letter queue messages.");
        }

        var dlqRepo = _unitOfWork.Repository<DeadLetterMessage>();
        var query = dlqRepo.Query()
            .AsNoTracking()
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(request.EventType))
        {
            query = query.Where(d => d.EventType.Contains(request.EventType));
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(d => d.MovedToDlqAt >= request.StartDate.Value.DateTime);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(d => d.MovedToDlqAt <= request.EndDate.Value.DateTime);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var items = await query
            .OrderByDescending(d => d.MovedToDlqAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(d => new DeadLetterMessageDto(
                d.Id,
                d.OriginalMessageId,
                d.EventType,
                d.Payload,
                d.OriginalCreatedAt,
                d.MovedToDlqAt,
                d.TotalRetryAttempts,
                d.LastError,
                d.IdempotencyKey
            ))
            .ToListAsync(cancellationToken);

        return new PagedResponse<DeadLetterMessageDto>(
            items,
            request.Page,
            request.PageSize,
            totalCount);
    }
}

