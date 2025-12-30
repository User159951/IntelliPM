using MediatR;
using IntelliPM.Application.Common.Models;

namespace IntelliPM.Application.Projects.Queries;

public record GetUserProjectsQuery(int UserId, int Page = 1, int PageSize = 20) : IRequest<PagedResponse<ProjectListDto>>;

public record ProjectListDto(
    int Id, 
    string Name, 
    string Description, 
    string Type, 
    string Status, 
    DateTimeOffset CreatedAt,
    List<ProjectMemberListDto> Members
);

public record ProjectMemberListDto(
    int UserId,
    string FirstName,
    string LastName,
    string Email,
    string? Avatar
);

