namespace IntelliPM.Application.Common.Interfaces;

/// <summary>
/// Service for managing user language preferences with fallback chain support
/// </summary>
public interface ILanguageService
{
    /// <summary>
    /// Gets the preferred language for the current user with fallback chain:
    /// 1. User's saved preference
    /// 2. Organization default language
    /// 3. Browser language (from Accept-Language header)
    /// 4. System default ('en')
    /// </summary>
    /// <param name="userId">User ID (0 means not authenticated)</param>
    /// <param name="organizationId">Organization ID</param>
    /// <param name="acceptLanguageHeader">Accept-Language header value</param>
    /// <returns>Language code (ISO 639-1: en, fr, ar)</returns>
    Task<string> GetUserLanguageAsync(int userId, int organizationId, string? acceptLanguageHeader = null);

    /// <summary>
    /// Updates the user's preferred language preference
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="language">Language code (ISO 639-1: en, fr, ar)</param>
    Task UpdateUserLanguageAsync(int userId, string language);

    /// <summary>
    /// Gets the organization's default language
    /// </summary>
    /// <param name="organizationId">Organization ID</param>
    /// <returns>Language code or null if not set</returns>
    Task<string?> GetOrganizationLanguageAsync(int organizationId);
}
