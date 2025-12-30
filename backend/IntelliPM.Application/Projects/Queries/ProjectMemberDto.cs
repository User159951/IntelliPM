using IntelliPM.Domain.Enums;

namespace IntelliPM.Application.Projects.Queries;

public record ProjectMemberDto(
    int Id,
    int UserId,
    string UserName,
    string Email,
    ProjectRole Role,
    DateTime InvitedAt,
    string InvitedByName
);

