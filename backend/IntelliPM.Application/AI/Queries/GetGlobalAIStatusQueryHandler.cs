using MediatR;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace IntelliPM.Application.AI.Queries;

/// <summary>
/// Handler for getting global AI kill switch status.
/// </summary>
public class GetGlobalAIStatusQueryHandler : IRequestHandler<GetGlobalAIStatusQuery, GlobalAIStatusResponse>
{
    private const string GlobalAIEnabledKey = "AI.Enabled";
    private const string CacheKey = "ai_global_enabled";

    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;

    public GetGlobalAIStatusQueryHandler(
        IUnitOfWork unitOfWork,
        IMemoryCache cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<GlobalAIStatusResponse> Handle(GetGlobalAIStatusQuery request, CancellationToken ct)
    {
        // Check cache first
        if (_cache.TryGetValue<bool>(CacheKey, out var cachedEnabled))
        {
            var globalSetting = await _unitOfWork.Repository<GlobalSetting>()
                .Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(gs => gs.Key == GlobalAIEnabledKey, ct);

            return new GlobalAIStatusResponse(
                Enabled: cachedEnabled,
                LastUpdated: globalSetting?.UpdatedAt,
                UpdatedById: globalSetting?.UpdatedById,
                Reason: null
            );
        }

        // Query database
        var globalSetting = await _unitOfWork.Repository<GlobalSetting>()
            .Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(gs => gs.Key == GlobalAIEnabledKey, ct);

        // Default to enabled if setting doesn't exist (backward compatibility)
        var isEnabled = globalSetting == null || 
                       bool.TryParse(globalSetting.Value, out var parsed) && parsed;

        // Cache result
        _cache.Set(CacheKey, isEnabled, TimeSpan.FromMinutes(5));

        return new GlobalAIStatusResponse(
            Enabled: isEnabled,
            LastUpdated: globalSetting?.UpdatedAt,
            UpdatedById: globalSetting?.UpdatedById,
            Reason: null
        );
    }
}
