using MediatR;

namespace IntelliPM.Application.Settings.Commands;

public record UpdateSettingCommand(string Key, string Value, string? Category = null) : IRequest<UpdateSettingResponse>;

public record UpdateSettingResponse(string Key, string Value, string Category);

