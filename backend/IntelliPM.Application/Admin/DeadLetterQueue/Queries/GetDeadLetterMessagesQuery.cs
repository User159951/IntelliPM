using MediatR;
using IntelliPM.Application.Common.Models;

namespace IntelliPM.Application.Admin.DeadLetterQueue.Queries;

public record GetDeadLetterMessagesQuery(
    int Page = 1,
    int PageSize = 20,
    string? EventType = null,
    DateTimeOffset? StartDate = null,
    DateTimeOffset? EndDate = null
) : IRequest<PagedResponse<DeadLetterMessageDto>>;

public record DeadLetterMessageDto(
    Guid Id,
    Guid OriginalMessageId,
    string EventType,
    string Payload,
    DateTime OriginalCreatedAt,
    DateTime MovedToDlqAt,
    int TotalRetryAttempts,
    string LastError,
    string? IdempotencyKey
);

