using MediatR;
using IntelliPM.Application.DTOs.Agent;

namespace IntelliPM.Application.Agents.Commands;

public record AnalyzeProjectCommand(int ProjectId) : IRequest<AgentResponse>;

