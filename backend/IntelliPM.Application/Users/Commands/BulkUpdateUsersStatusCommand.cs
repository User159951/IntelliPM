using MediatR;

namespace IntelliPM.Application.Users.Commands;

public record BulkUpdateUsersStatusCommand(
    List<int> UserIds,
    bool IsActive
) : IRequest<BulkUpdateUsersStatusResponse>;

public record BulkUpdateUsersStatusResponse(
    int SuccessCount,
    int FailureCount,
    List<string> Errors
);

