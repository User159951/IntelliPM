using MediatR;
using IntelliPM.Application.Agents.Services;

namespace IntelliPM.Application.Agents.Commands;

public record RunManagerAgentCommand(int ProjectId) : IRequest<ManagerAgentOutput>;

