using MediatR;
using IntelliPM.Application.Agents.Services;

namespace IntelliPM.Application.Agents.Commands;

public record RunQAAgentCommand(int ProjectId) : IRequest<QAAgentOutput>;

