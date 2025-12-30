namespace IntelliPM.Application.Teams.Queries;

public record TeamDto(
    int Id,
    string Name,
    int Capacity,
    List<TeamMemberDto> Members,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);

public record TeamMemberDto(
    int UserId,
    string Username,
    string Email,
    string? FirstName,
    string? LastName
);

