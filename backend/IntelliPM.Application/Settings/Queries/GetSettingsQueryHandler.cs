using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace IntelliPM.Application.Settings.Queries;

public class GetSettingsQueryHandler : IRequestHandler<GetSettingsQuery, Dictionary<string, string>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetSettingsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Dictionary<string, string>> Handle(GetSettingsQuery request, CancellationToken cancellationToken)
    {
        var settingsRepo = _unitOfWork.Repository<GlobalSetting>();
        var query = settingsRepo.Query().AsNoTracking();

        if (!string.IsNullOrEmpty(request.Category))
        {
            query = query.Where(s => s.Category == request.Category);
        }

        var settings = await query
            .ToDictionaryAsync(s => s.Key, s => s.Value, cancellationToken);

        return settings;
    }
}

