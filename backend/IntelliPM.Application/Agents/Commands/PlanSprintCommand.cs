using MediatR;
using IntelliPM.Application.DTOs.Agent;

namespace IntelliPM.Application.Agents.Commands;

public record PlanSprintCommand(int SprintId) : IRequest<AgentResponse>;

