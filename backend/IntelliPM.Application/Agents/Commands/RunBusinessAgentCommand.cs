using MediatR;
using IntelliPM.Application.Agents.Services;

namespace IntelliPM.Application.Agents.Commands;

public record RunBusinessAgentCommand(int ProjectId) : IRequest<BusinessAgentOutput>;

