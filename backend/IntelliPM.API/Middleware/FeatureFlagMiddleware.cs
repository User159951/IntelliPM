using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.API.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace IntelliPM.API.Middleware;

/// <summary>
/// Middleware to check feature flags in the request pipeline.
/// Supports checking feature flags via:
/// 1. [RequireFeatureFlag("FeatureName")] attribute on controllers/actions
/// 2. X-Feature-Flag header (legacy support)
/// Returns 403 Forbidden if the feature is disabled.
/// </summary>
public class FeatureFlagMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<FeatureFlagMiddleware> _logger;
    private const string FeatureFlagHeaderName = "X-Feature-Flag";

    public FeatureFlagMiddleware(
        RequestDelegate next,
        ILogger<FeatureFlagMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(
        HttpContext context,
        IFeatureFlagService featureFlagService,
        ICurrentUserService currentUserService)
    {
        // Extract feature flag name from attribute or header
        var featureFlagName = ExtractFeatureFlagName(context);

        // If no feature flag specified, continue pipeline
        if (string.IsNullOrWhiteSpace(featureFlagName))
        {
            await _next(context);
            return;
        }

        _logger.LogDebug(
            "Checking feature flag {FeatureFlagName} for request {Path}",
            featureFlagName,
            context.Request.Path);

        try
        {
            // Get organization ID from current user context
            int? organizationId = null;
            
            // Only get organization ID if user is authenticated
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var orgId = currentUserService.GetOrganizationId();
                if (orgId > 0)
                {
                    organizationId = orgId;
                }
            }

            // Check if feature is enabled (strict mode: throws FeatureFlagNotFoundException if not found)
            var isEnabled = await featureFlagService.IsEnabledAsync(featureFlagName, organizationId);

            if (!isEnabled)
            {
                _logger.LogWarning(
                    "Feature flag {FeatureFlagName} is disabled for OrganizationId {OrganizationId}. Request {Path} blocked.",
                    featureFlagName,
                    organizationId?.ToString() ?? "Global",
                    context.Request.Path);

                // Return 403 Forbidden
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                context.Response.ContentType = "application/json";

                var response = new
                {
                    error = "Feature not available",
                    message = $"Feature '{featureFlagName}' is not enabled for your organization.",
                    featureFlag = featureFlagName
                };

                var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                await context.Response.WriteAsync(jsonResponse);
                return;
            }

            _logger.LogDebug(
                "Feature flag {FeatureFlagName} is enabled for OrganizationId {OrganizationId}. Request {Path} allowed.",
                featureFlagName,
                organizationId?.ToString() ?? "Global",
                context.Request.Path);

            // Feature is enabled, continue pipeline
            await _next(context);
        }
        catch (FeatureFlagNotFoundException ex)
        {
            // Strict mode: flag not found in database - return 403 Forbidden
            _logger.LogWarning(
                "Feature flag {FeatureFlagName} not found for OrganizationId {OrganizationId}. Request {Path} blocked. Error: {Message}",
                featureFlagName,
                ex.OrganizationId?.ToString() ?? "Global",
                context.Request.Path,
                ex.Message);

            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            context.Response.ContentType = "application/json";

            var response = new
            {
                error = "Feature not available",
                message = $"Feature '{featureFlagName}' is not available (flag not found in database).",
                featureFlag = featureFlagName
            };

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
            return;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error checking feature flag {FeatureFlagName} for request {Path}. Exception: {ExceptionType}, Message: {Message}",
                featureFlagName,
                context.Request.Path,
                ex.GetType().Name,
                ex.Message);

            // On error, allow the request to continue (fail open)
            // This prevents feature flag service issues from breaking the entire application
            await _next(context);
        }
    }

    /// <summary>
    /// Extracts the feature flag name from the request.
    /// Checks in order:
    /// 1. [RequireFeatureFlag] attribute on controller/action (action-level takes precedence)
    /// 2. X-Feature-Flag header (legacy support)
    /// </summary>
    private string? ExtractFeatureFlagName(HttpContext context)
    {
        // First, check for RequireFeatureFlag attribute on the endpoint
        var endpoint = context.GetEndpoint();
        if (endpoint != null)
        {
            // Get all RequireFeatureFlag attributes (both controller and action level)
            var attributes = endpoint.Metadata.GetOrderedMetadata<RequireFeatureFlagAttribute>();
            
            if (attributes.Any())
            {
                // In ASP.NET Core, action-level attributes come after controller-level in ordered metadata
                // So the last one is the most specific (action-level if present, otherwise controller-level)
                var attribute = attributes.Last();
                
                _logger.LogDebug(
                    "Found RequireFeatureFlag attribute: {FeatureFlagName} for {Path} (Total attributes: {Count})",
                    attribute.FeatureFlagName,
                    context.Request.Path,
                    attributes.Count());
                
                return attribute.FeatureFlagName;
            }
        }

        // Second, check header (legacy support)
        if (context.Request.Headers.TryGetValue(FeatureFlagHeaderName, out var headerValue))
        {
            var featureName = headerValue.ToString().Trim();
            if (!string.IsNullOrWhiteSpace(featureName))
            {
                _logger.LogDebug(
                    "Found feature flag in header: {FeatureFlagName} for {Path}",
                    featureName,
                    context.Request.Path);
                return featureName;
            }
        }

        return null;
    }
}

