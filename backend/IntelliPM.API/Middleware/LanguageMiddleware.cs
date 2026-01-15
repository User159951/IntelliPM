using IntelliPM.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Globalization;

namespace IntelliPM.API.Middleware;

/// <summary>
/// Middleware that sets the current request's culture based on user language preference.
/// Uses fallback chain: user preference -> organization default -> Accept-Language header -> 'en'
/// Must be placed after authentication middleware so user context is available.
/// </summary>
public class LanguageMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILanguageService _languageService;
    private readonly ICurrentUserService _currentUserService;
    private static readonly string[] SupportedCultures = { "en", "fr", "ar" };
    private const string DefaultCulture = "en";

    public LanguageMiddleware(
        RequestDelegate next,
        ILanguageService languageService,
        ICurrentUserService currentUserService)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _languageService = languageService ?? throw new ArgumentNullException(nameof(languageService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            var userId = _currentUserService.GetUserId();
            var organizationId = _currentUserService.GetOrganizationId();
            var acceptLanguageHeader = context.Request.Headers["Accept-Language"].ToString();

            // Get user's preferred language with fallback chain
            var language = await _languageService.GetUserLanguageAsync(userId, organizationId, acceptLanguageHeader);

            // Validate and set culture
            if (SupportedCultures.Contains(language, StringComparer.OrdinalIgnoreCase))
            {
                var culture = new CultureInfo(language);
                CultureInfo.CurrentCulture = culture;
                CultureInfo.CurrentUICulture = culture;
            }
            else
            {
                // Fallback to default if language is not supported
                var defaultCulture = new CultureInfo(DefaultCulture);
                CultureInfo.CurrentCulture = defaultCulture;
                CultureInfo.CurrentUICulture = defaultCulture;
            }
        }
        catch (Exception)
        {
            // If language service fails, use default culture
            // Don't log here to avoid noise - the language service already logs errors
            var defaultCulture = new CultureInfo(DefaultCulture);
            CultureInfo.CurrentCulture = defaultCulture;
            CultureInfo.CurrentUICulture = defaultCulture;
        }

        await _next(context);
    }
}
