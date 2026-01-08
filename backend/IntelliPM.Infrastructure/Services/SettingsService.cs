using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Infrastructure.Services;

public class SettingsService : ISettingsService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;
    private readonly ILogger<SettingsService> _logger;
    private const int CacheExpirationMinutes = 15;

    public SettingsService(
        IUnitOfWork unitOfWork,
        IMemoryCache cache,
        ILogger<SettingsService> logger)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }

    public async Task<string?> GetGlobalSettingAsync(string key, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"global_setting_{key}";
        
        if (_cache.TryGetValue(cacheKey, out string? cachedValue))
        {
            return cachedValue;
        }

        var repo = _unitOfWork.Repository<GlobalSetting>();
        var setting = await repo.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Key == key, cancellationToken);

        var value = setting?.Value;
        
        if (value != null)
        {
            _cache.Set(cacheKey, value, TimeSpan.FromMinutes(CacheExpirationMinutes));
        }

        return value;
    }

    public async Task<int?> GetGlobalSettingIntAsync(string key, CancellationToken cancellationToken = default)
    {
        var value = await GetGlobalSettingAsync(key, cancellationToken);
        return int.TryParse(value, out var intValue) ? intValue : null;
    }

    public async Task<long?> GetGlobalSettingLongAsync(string key, CancellationToken cancellationToken = default)
    {
        var value = await GetGlobalSettingAsync(key, cancellationToken);
        return long.TryParse(value, out var longValue) ? longValue : null;
    }

    public async Task<decimal?> GetGlobalSettingDecimalAsync(string key, CancellationToken cancellationToken = default)
    {
        var value = await GetGlobalSettingAsync(key, cancellationToken);
        return decimal.TryParse(value, out var decimalValue) ? decimalValue : null;
    }

    public async Task<bool?> GetGlobalSettingBoolAsync(string key, CancellationToken cancellationToken = default)
    {
        var value = await GetGlobalSettingAsync(key, cancellationToken);
        if (string.IsNullOrEmpty(value))
            return null;
        
        return bool.TryParse(value, out var boolValue) 
            ? boolValue 
            : (value.Equals("1", StringComparison.OrdinalIgnoreCase) || value.Equals("true", StringComparison.OrdinalIgnoreCase));
    }

    public async Task<string?> GetOrganizationSettingAsync(int organizationId, string key, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"org_setting_{organizationId}_{key}";
        
        if (_cache.TryGetValue(cacheKey, out string? cachedValue))
        {
            return cachedValue;
        }

        var repo = _unitOfWork.Repository<OrganizationSetting>();
        var setting = await repo.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.OrganizationId == organizationId && s.Key == key, cancellationToken);

        var value = setting?.Value;
        
        if (value != null)
        {
            _cache.Set(cacheKey, value, TimeSpan.FromMinutes(CacheExpirationMinutes));
        }

        return value;
    }

    public async Task<int?> GetOrganizationSettingIntAsync(int organizationId, string key, CancellationToken cancellationToken = default)
    {
        var value = await GetOrganizationSettingAsync(organizationId, key, cancellationToken);
        return int.TryParse(value, out var intValue) ? intValue : null;
    }

    public async Task<long?> GetOrganizationSettingLongAsync(int organizationId, string key, CancellationToken cancellationToken = default)
    {
        var value = await GetOrganizationSettingAsync(organizationId, key, cancellationToken);
        return long.TryParse(value, out var longValue) ? longValue : null;
    }

    public async Task<decimal?> GetOrganizationSettingDecimalAsync(int organizationId, string key, CancellationToken cancellationToken = default)
    {
        var value = await GetOrganizationSettingAsync(organizationId, key, cancellationToken);
        return decimal.TryParse(value, out var decimalValue) ? decimalValue : null;
    }

    public async Task<bool?> GetOrganizationSettingBoolAsync(int organizationId, string key, CancellationToken cancellationToken = default)
    {
        var value = await GetOrganizationSettingAsync(organizationId, key, cancellationToken);
        if (string.IsNullOrEmpty(value))
            return null;
        
        return bool.TryParse(value, out var boolValue) 
            ? boolValue 
            : (value.Equals("1", StringComparison.OrdinalIgnoreCase) || value.Equals("true", StringComparison.OrdinalIgnoreCase));
    }

    public async Task<string?> GetSettingAsync(int? organizationId, string key, CancellationToken cancellationToken = default)
    {
        // Try organization setting first if organizationId is provided
        if (organizationId.HasValue)
        {
            var orgValue = await GetOrganizationSettingAsync(organizationId.Value, key, cancellationToken);
            if (orgValue != null)
                return orgValue;
        }

        // Fall back to global setting
        return await GetGlobalSettingAsync(key, cancellationToken);
    }

    public async Task<int?> GetSettingIntAsync(int? organizationId, string key, CancellationToken cancellationToken = default)
    {
        var value = await GetSettingAsync(organizationId, key, cancellationToken);
        return int.TryParse(value, out var intValue) ? intValue : null;
    }

    public async Task<long?> GetSettingLongAsync(int? organizationId, string key, CancellationToken cancellationToken = default)
    {
        var value = await GetSettingAsync(organizationId, key, cancellationToken);
        return long.TryParse(value, out var longValue) ? longValue : null;
    }

    public async Task<decimal?> GetSettingDecimalAsync(int? organizationId, string key, CancellationToken cancellationToken = default)
    {
        var value = await GetSettingAsync(organizationId, key, cancellationToken);
        return decimal.TryParse(value, out var decimalValue) ? decimalValue : null;
    }

    public async Task<bool?> GetSettingBoolAsync(int? organizationId, string key, CancellationToken cancellationToken = default)
    {
        var value = await GetSettingAsync(organizationId, key, cancellationToken);
        if (string.IsNullOrEmpty(value))
            return null;
        
        return bool.TryParse(value, out var boolValue) 
            ? boolValue 
            : (value.Equals("1", StringComparison.OrdinalIgnoreCase) || value.Equals("true", StringComparison.OrdinalIgnoreCase));
    }

    public void InvalidateCache(string key, int? organizationId = null)
    {
        if (organizationId.HasValue)
        {
            var cacheKey = $"org_setting_{organizationId.Value}_{key}";
            _cache.Remove(cacheKey);
        }
        
        var globalCacheKey = $"global_setting_{key}";
        _cache.Remove(globalCacheKey);
    }

    public void InvalidateAllCache()
    {
        // Note: IMemoryCache doesn't have a Clear() method, so we'd need to track keys
        // For now, we'll just log and let cache expire naturally
        _logger.LogWarning("InvalidateAllCache called - cache will expire naturally");
    }
}

