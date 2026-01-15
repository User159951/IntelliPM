using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Services;

/// <summary>
/// Service for managing user language preferences with fallback chain support
/// </summary>
public class LanguageService : ILanguageService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<LanguageService> _logger;
    private static readonly string[] SupportedLanguages = { "en", "fr", "ar" };
    private const string DefaultLanguage = "en";

    public LanguageService(IUnitOfWork unitOfWork, ILogger<LanguageService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<string> GetUserLanguageAsync(int userId, int organizationId, string? acceptLanguageHeader = null)
    {
        // Fallback chain:
        // 1. User's saved preference
        // 2. Organization default language
        // 3. Browser language (from Accept-Language header)
        // 4. System default ('en')

        // 1. Check user's saved preference
        if (userId > 0)
        {
            var user = await _unitOfWork.Repository<User>()
                .Query()
                .AsNoTracking()
                .IgnoreQueryFilters() // Need to bypass tenant filter for user lookup
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user != null && !string.IsNullOrWhiteSpace(user.PreferredLanguage))
            {
                if (IsSupportedLanguage(user.PreferredLanguage))
                {
                    return user.PreferredLanguage;
                }
                _logger.LogWarning("User {UserId} has unsupported language preference: {Language}", userId, user.PreferredLanguage);
            }
        }

        // 2. Check organization default language
        if (organizationId > 0)
        {
            var organization = await _unitOfWork.Repository<Organization>()
                .Query()
                .AsNoTracking()
                .IgnoreQueryFilters() // Need to bypass tenant filter for organization lookup
                .FirstOrDefaultAsync(o => o.Id == organizationId);

            if (organization != null && !string.IsNullOrWhiteSpace(organization.DefaultLanguage))
            {
                if (IsSupportedLanguage(organization.DefaultLanguage))
                {
                    return organization.DefaultLanguage;
                }
                _logger.LogWarning("Organization {OrganizationId} has unsupported default language: {Language}", organizationId, organization.DefaultLanguage);
            }
        }

        // 3. Check browser language (Accept-Language header)
        if (!string.IsNullOrWhiteSpace(acceptLanguageHeader))
        {
            var browserLanguage = ParseAcceptLanguageHeader(acceptLanguageHeader);
            if (browserLanguage != null)
            {
                return browserLanguage;
            }
        }

        // 4. System default
        return DefaultLanguage;
    }

    public async System.Threading.Tasks.Task UpdateUserLanguageAsync(int userId, string language)
    {
        if (!IsSupportedLanguage(language))
        {
            throw new ArgumentException($"Unsupported language: {language}. Supported languages are: {string.Join(", ", SupportedLanguages)}", nameof(language));
        }

        var userRepo = _unitOfWork.Repository<User>();
        var user = await userRepo.Query()
            .IgnoreQueryFilters() // Need to bypass tenant filter for user lookup
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new InvalidOperationException($"User with ID {userId} not found");
        }

        user.PreferredLanguage = language;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        userRepo.Update(user);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Updated language preference for user {UserId} to {Language}", userId, language);
    }

    public async Task<string?> GetOrganizationLanguageAsync(int organizationId)
    {
        var organization = await _unitOfWork.Repository<Organization>()
            .Query()
            .AsNoTracking()
            .IgnoreQueryFilters() // Need to bypass tenant filter for organization lookup
            .FirstOrDefaultAsync(o => o.Id == organizationId);

        return organization?.DefaultLanguage;
    }

    /// <summary>
    /// Parses Accept-Language header and returns the first supported language
    /// </summary>
    private string? ParseAcceptLanguageHeader(string acceptLanguageHeader)
    {
        if (string.IsNullOrWhiteSpace(acceptLanguageHeader))
        {
            return null;
        }

        // Parse Accept-Language header (e.g., "en-US,en;q=0.9,fr;q=0.8")
        var languages = acceptLanguageHeader
            .Split(',')
            .Select(lang => lang.Split(';')[0].Trim().Split('-')[0].ToLowerInvariant())
            .Where(lang => !string.IsNullOrWhiteSpace(lang))
            .Distinct()
            .ToList();

        // Find first supported language
        foreach (var lang in languages)
        {
            if (IsSupportedLanguage(lang))
            {
                return lang;
            }
        }

        return null;
    }

    /// <summary>
    /// Checks if a language code is supported
    /// </summary>
    private static bool IsSupportedLanguage(string language)
    {
        return SupportedLanguages.Contains(language?.ToLowerInvariant(), StringComparer.OrdinalIgnoreCase);
    }
}
