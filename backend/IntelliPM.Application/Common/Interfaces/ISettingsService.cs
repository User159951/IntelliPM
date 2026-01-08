namespace IntelliPM.Application.Common.Interfaces;

/// <summary>
/// Service for retrieving business configuration settings from database with caching.
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// Gets a global setting value by key.
    /// </summary>
    Task<string?> GetGlobalSettingAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a global setting value as integer.
    /// </summary>
    Task<int?> GetGlobalSettingIntAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a global setting value as long.
    /// </summary>
    Task<long?> GetGlobalSettingLongAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a global setting value as decimal.
    /// </summary>
    Task<decimal?> GetGlobalSettingDecimalAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a global setting value as boolean.
    /// </summary>
    Task<bool?> GetGlobalSettingBoolAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an organization setting value by key.
    /// </summary>
    Task<string?> GetOrganizationSettingAsync(int organizationId, string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an organization setting value as integer.
    /// </summary>
    Task<int?> GetOrganizationSettingIntAsync(int organizationId, string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an organization setting value as long.
    /// </summary>
    Task<long?> GetOrganizationSettingLongAsync(int organizationId, string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an organization setting value as decimal.
    /// </summary>
    Task<decimal?> GetOrganizationSettingDecimalAsync(int organizationId, string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an organization setting value as boolean.
    /// </summary>
    Task<bool?> GetOrganizationSettingBoolAsync(int organizationId, string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a setting value, checking organization first, then falling back to global.
    /// </summary>
    Task<string?> GetSettingAsync(int? organizationId, string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a setting value as integer, checking organization first, then falling back to global.
    /// </summary>
    Task<int?> GetSettingIntAsync(int? organizationId, string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a setting value as long, checking organization first, then falling back to global.
    /// </summary>
    Task<long?> GetSettingLongAsync(int? organizationId, string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a setting value as decimal, checking organization first, then falling back to global.
    /// </summary>
    Task<decimal?> GetSettingDecimalAsync(int? organizationId, string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a setting value as boolean, checking organization first, then falling back to global.
    /// </summary>
    Task<bool?> GetSettingBoolAsync(int? organizationId, string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates the cache for a specific setting key.
    /// </summary>
    void InvalidateCache(string key, int? organizationId = null);

    /// <summary>
    /// Invalidates all settings cache.
    /// </summary>
    void InvalidateAllCache();
}

