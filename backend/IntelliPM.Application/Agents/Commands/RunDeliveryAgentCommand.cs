using MediatR;
using IntelliPM.Application.Agents.Services;

namespace IntelliPM.Application.Agents.Commands;

public record RunDeliveryAgentCommand(int ProjectId) : IRequest<DeliveryAgentOutput>;

