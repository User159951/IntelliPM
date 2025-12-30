using MediatR;

namespace IntelliPM.Application.Settings.Commands;

public record SendTestEmailCommand(string Email) : IRequest<SendTestEmailResponse>;

public record SendTestEmailResponse(bool Success, string Message);

