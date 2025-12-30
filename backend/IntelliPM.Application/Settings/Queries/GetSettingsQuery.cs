using MediatR;

namespace IntelliPM.Application.Settings.Queries;

public record GetSettingsQuery(string? Category = null) : IRequest<Dictionary<string, string>>;

