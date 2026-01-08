using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Application.Settings.Queries;

public class GetOrganizationSettingsQueryHandler : IRequestHandler<GetOrganizationSettingsQuery, Dictionary<string, string>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetOrganizationSettingsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Dictionary<string, string>> Handle(GetOrganizationSettingsQuery request, CancellationToken cancellationToken)
    {
        var settingsRepo = _unitOfWork.Repository<OrganizationSetting>();
        var query = settingsRepo.Query()
            .AsNoTracking()
            .Where(s => s.OrganizationId == request.OrganizationId);

        if (!string.IsNullOrEmpty(request.Category))
        {
            query = query.Where(s => s.Category == request.Category);
        }

        var settings = await query
            .ToDictionaryAsync(s => s.Key, s => s.Value, cancellationToken);

        return settings;
    }
}

