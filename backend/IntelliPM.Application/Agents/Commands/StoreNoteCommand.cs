using MediatR;

namespace IntelliPM.Application.Agents.Commands;

public record StoreNoteCommand(int ProjectId, string Type, string Content) : IRequest<StoreNoteResponse>;

public record StoreNoteResponse(int Id, bool Stored);

