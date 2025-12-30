using MediatR;

namespace IntelliPM.Application.Users.Commands;

public record DeleteUserCommand(int UserId) : IRequest<DeleteUserResponse>;

public record DeleteUserResponse(bool Success);

