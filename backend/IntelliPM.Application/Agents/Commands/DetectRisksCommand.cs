using MediatR;
using IntelliPM.Application.DTOs.Agent;

namespace IntelliPM.Application.Agents.Commands;

public record DetectRisksCommand(int ProjectId) : IRequest<AgentResponse>;

