using MediatR;
using IntelliPM.Application.Agents.Services;

namespace IntelliPM.Application.Agents.Commands;

public record RunProductAgentCommand(int ProjectId) : IRequest<ProductAgentOutput>;

