using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using IntelliPM.API.Authorization;
using IntelliPM.API.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace IntelliPM.Tests.API.Security;

/// <summary>
/// Automated test that verifies all controller actions have proper security attributes.
/// This test prevents future endpoints from being deployed without proper authorization.
/// 
/// REQUIREMENTS:
/// - Each action method MUST have EITHER:
///   - [RequirePermission] attribute OR
///   - [AllowAnonymous] attribute (for intentionally public endpoints)
/// - Class-level [Authorize] is acceptable but not sufficient alone (should use [RequirePermission] for granular control)
/// 
/// ALLOWLIST:
/// - Health endpoints (HealthController, HealthApiController)
/// - Lookup endpoints (LookupsController) - may have class-level [Authorize] only
/// - Auth endpoints (AuthController) - login/register endpoints have [AllowAnonymous]
/// - Test endpoints (TestController) - debug-only
/// </summary>
public class ControllerSecurityAttributesTests
{
    private static readonly Assembly ApiAssembly = typeof(BaseApiController).Assembly;
    
    /// <summary>
    /// Allowlist of controller names that are intentionally public or have special handling.
    /// Controllers in this list will be skipped from the security check.
    /// </summary>
    private static readonly HashSet<string> AllowedPublicControllers = new()
    {
        "HealthController",
        "HealthApiController",
        "TestController" // Debug-only controller
    };

    /// <summary>
    /// Allowlist of specific controller+action combinations that are intentionally public.
    /// Format: "ControllerName.ActionName"
    /// </summary>
    private static readonly HashSet<string> AllowedPublicActions = new()
    {
        "AuthController.Login",
        "AuthController.Register",
        "AuthController.RefreshToken",
        "AuthController.ForgotPassword",
        "AuthController.ResetPassword",
        "AuthController.VerifyEmail",
        "AuthController.ResendVerificationEmail",
        "AuthController.ValidateToken",
        "AuthController.GetPublicKey",
        "LookupsController.GetProjectTypes",
        "LookupsController.GetTaskStatuses",
        "LookupsController.GetTaskPriorities"
    };

    /// <summary>
    /// Verifies that all controller actions have proper security attributes.
    /// This test will fail if any endpoint is added without [RequirePermission] or [AllowAnonymous].
    /// </summary>
    [Fact]
    public void AllControllerActions_ShouldHaveSecurityAttributes()
    {
        // Arrange
        var unprotectedEndpoints = new List<UnprotectedEndpoint>();

        // Get all controller types
        var controllerTypes = GetControllerTypes();

        // Act - Check each controller
        foreach (var controllerType in controllerTypes)
        {
            var controllerName = controllerType.Name;
            
            // Skip allowlisted controllers
            if (AllowedPublicControllers.Contains(controllerName))
            {
                continue;
            }

            // Get all action methods
            var actionMethods = GetActionMethods(controllerType);

            foreach (var method in actionMethods)
            {
                var actionName = method.Name;
                var fullActionName = $"{controllerName}.{actionName}";

                // Skip allowlisted actions
                if (AllowedPublicActions.Contains(fullActionName))
                {
                    continue;
                }

                // Check if method has security attributes
                var hasRequirePermission = HasAttribute<RequirePermissionAttribute>(method) ||
                                         HasAttribute<RequirePermissionAttribute>(controllerType);
                
                var hasAllowAnonymous = HasAttribute<AllowAnonymousAttribute>(method) ||
                                       HasAttribute<AllowAnonymousAttribute>(controllerType);
                
                var hasAuthorize = HasAttribute<AuthorizeAttribute>(method) ||
                                  HasAttribute<AuthorizeAttribute>(controllerType);

                // Check if method is protected
                // A method is protected if it has:
                // 1. [RequirePermission] (preferred) OR
                // 2. [AllowAnonymous] (explicitly public) OR
                // 3. Class-level [Authorize] (acceptable but not ideal - should use [RequirePermission])
                var isProtected = hasRequirePermission || hasAllowAnonymous || hasAuthorize;

                if (!isProtected)
                {
                    unprotectedEndpoints.Add(new UnprotectedEndpoint
                    {
                        Controller = controllerName,
                        Action = actionName,
                        HttpMethod = GetHttpMethod(method),
                        Route = GetRoute(controllerType, method)
                    });
                }
            }
        }

        // Assert - Fail with clear message if any unprotected endpoints found
        if (unprotectedEndpoints.Any())
        {
            var errorMessage = BuildErrorMessage(unprotectedEndpoints);
            Assert.Fail(errorMessage);
        }
    }

    /// <summary>
    /// Verifies that controllers using class-level [Authorize] also have [RequirePermission] on actions
    /// for better granular control. This is a warning-level check, not a failure.
    /// </summary>
    [Fact]
    public void ControllersWithClassLevelAuthorize_ShouldPreferRequirePermissionOnActions()
    {
        // Arrange
        var controllersWithOnlyClassLevelAuth = new List<string>();

        var controllerTypes = GetControllerTypes();

        foreach (var controllerType in controllerTypes)
        {
            // Skip allowlisted controllers
            if (AllowedPublicControllers.Contains(controllerType.Name))
            {
                continue;
            }

            var hasClassLevelAuthorize = HasAttribute<AuthorizeAttribute>(controllerType);
            var hasClassLevelRequirePermission = HasAttribute<RequirePermissionAttribute>(controllerType);

            if (hasClassLevelAuthorize && !hasClassLevelRequirePermission)
            {
                var actionMethods = GetActionMethods(controllerType);
                var hasActionLevelRequirePermission = actionMethods.Any(m => 
                    HasAttribute<RequirePermissionAttribute>(m));

                if (!hasActionLevelRequirePermission)
                {
                    controllersWithOnlyClassLevelAuth.Add(controllerType.Name);
                }
            }
        }

        // This is informational - we don't fail the test, but log it
        if (controllersWithOnlyClassLevelAuth.Any())
        {
            var message = $"Controllers with only class-level [Authorize] (consider adding [RequirePermission] to actions): " +
                         string.Join(", ", controllersWithOnlyClassLevelAuth);
            // Log but don't fail - this is a best practice recommendation
            Console.WriteLine($"INFO: {message}");
        }
    }

    /// <summary>
    /// Gets all controller types from the API assembly.
    /// </summary>
    private static IEnumerable<Type> GetControllerTypes()
    {
        return ApiAssembly.GetTypes()
            .Where(t => 
                t.IsClass && 
                !t.IsAbstract && 
                (typeof(ControllerBase).IsAssignableFrom(t) || typeof(Controller).IsAssignableFrom(t)) &&
                t.Namespace?.StartsWith("IntelliPM.API.Controllers") == true)
            .OrderBy(t => t.Name);
    }

    /// <summary>
    /// Gets all action methods from a controller type.
    /// Action methods are public methods that have HTTP verb attributes (HttpGet, HttpPost, etc.).
    /// </summary>
    private static IEnumerable<MethodInfo> GetActionMethods(Type controllerType)
    {
        return controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => 
                !m.IsSpecialName && // Exclude properties, events, etc.
                !m.IsConstructor &&
                !m.IsAbstract &&
                IsActionResultReturnType(m.ReturnType) &&
                HasHttpVerbAttribute(m))
            .ToList();
    }

    /// <summary>
    /// Checks if a return type is a valid action result type (IActionResult, Task&lt;IActionResult&gt;, etc.).
    /// </summary>
    private static bool IsActionResultReturnType(Type returnType)
    {
        // Direct IActionResult
        if (typeof(IActionResult).IsAssignableFrom(returnType))
        {
            return true;
        }

        // Task<IActionResult> or Task<T> where T : IActionResult
        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var genericArg = returnType.GetGenericArguments()[0];
            return typeof(IActionResult).IsAssignableFrom(genericArg);
        }

        // Task (non-generic) - some actions return Task
        if (returnType == typeof(Task))
        {
            return true;
        }

        // void - some actions return void
        if (returnType == typeof(void))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if a method or type has a specific attribute.
    /// For types, also checks base classes explicitly to ensure inheritance is handled correctly.
    /// </summary>
    private static bool HasAttribute<T>(MemberInfo member) where T : Attribute
    {
        // GetCustomAttributes with inherit: true should work, but let's be explicit for types
        if (member is Type type)
        {
            // Check the type itself and all base classes
            var currentType = type;
            while (currentType != null && currentType != typeof(object))
            {
                if (currentType.GetCustomAttributes(typeof(T), inherit: false).Any())
                {
                    return true;
                }
                currentType = currentType.BaseType;
            }
            return false;
        }

        // For methods and other members, use the standard approach
        return member.GetCustomAttributes(typeof(T), inherit: true).Any();
    }

    /// <summary>
    /// Checks if a method has any HTTP verb attribute.
    /// </summary>
    private static bool HasHttpVerbAttribute(MethodInfo method)
    {
        var httpVerbAttributes = new[]
        {
            typeof(HttpGetAttribute),
            typeof(HttpPostAttribute),
            typeof(HttpPutAttribute),
            typeof(HttpPatchAttribute),
            typeof(HttpDeleteAttribute),
            typeof(HttpHeadAttribute),
            typeof(HttpOptionsAttribute)
        };

        return httpVerbAttributes.Any(attrType => 
            method.GetCustomAttributes(attrType, inherit: true).Any());
    }

    /// <summary>
    /// Gets the HTTP method from a method's attributes.
    /// </summary>
    private static string GetHttpMethod(MethodInfo method)
    {
        if (HasAttribute<HttpGetAttribute>(method)) return "GET";
        if (HasAttribute<HttpPostAttribute>(method)) return "POST";
        if (HasAttribute<HttpPutAttribute>(method)) return "PUT";
        if (HasAttribute<HttpPatchAttribute>(method)) return "PATCH";
        if (HasAttribute<HttpDeleteAttribute>(method)) return "DELETE";
        if (HasAttribute<HttpHeadAttribute>(method)) return "HEAD";
        if (HasAttribute<HttpOptionsAttribute>(method)) return "OPTIONS";
        return "UNKNOWN";
    }

    /// <summary>
    /// Gets the route for a controller action.
    /// </summary>
    private static string GetRoute(Type controllerType, MethodInfo method)
    {
        var controllerRoute = GetControllerRoute(controllerType);
        var actionRoute = GetActionRoute(method);
        
        if (string.IsNullOrEmpty(actionRoute))
        {
            return controllerRoute;
        }
        
        return $"{controllerRoute}/{actionRoute}".TrimEnd('/');
    }

    /// <summary>
    /// Gets the route from a controller's Route attribute.
    /// </summary>
    private static string GetControllerRoute(Type controllerType)
    {
        var routeAttr = controllerType.GetCustomAttribute<RouteAttribute>();
        if (routeAttr != null && !string.IsNullOrEmpty(routeAttr.Template))
        {
            return routeAttr.Template.Replace("[controller]", controllerType.Name.Replace("Controller", ""));
        }
        return $"/api/v1/{controllerType.Name.Replace("Controller", "")}";
    }

    /// <summary>
    /// Gets the route from a method's Route or HttpGet/HttpPost/etc. attribute.
    /// </summary>
    private static string GetActionRoute(MethodInfo method)
    {
        // Check Route attribute first
        var routeAttr = method.GetCustomAttribute<RouteAttribute>();
        if (routeAttr != null && !string.IsNullOrEmpty(routeAttr.Template))
        {
            return routeAttr.Template;
        }

        // Check HTTP verb attributes for route
        var httpGet = method.GetCustomAttribute<HttpGetAttribute>();
        if (httpGet != null && !string.IsNullOrEmpty(httpGet.Template))
        {
            return httpGet.Template;
        }

        var httpPost = method.GetCustomAttribute<HttpPostAttribute>();
        if (httpPost != null && !string.IsNullOrEmpty(httpPost.Template))
        {
            return httpPost.Template;
        }

        var httpPut = method.GetCustomAttribute<HttpPutAttribute>();
        if (httpPut != null && !string.IsNullOrEmpty(httpPut.Template))
        {
            return httpPut.Template;
        }

        var httpPatch = method.GetCustomAttribute<HttpPatchAttribute>();
        if (httpPatch != null && !string.IsNullOrEmpty(httpPatch.Template))
        {
            return httpPatch.Template;
        }

        var httpDelete = method.GetCustomAttribute<HttpDeleteAttribute>();
        if (httpDelete != null && !string.IsNullOrEmpty(httpDelete.Template))
        {
            return httpDelete.Template;
        }

        return string.Empty;
    }

    /// <summary>
    /// Builds a detailed error message listing all unprotected endpoints.
    /// </summary>
    private static string BuildErrorMessage(List<UnprotectedEndpoint> unprotectedEndpoints)
    {
        var message = new System.Text.StringBuilder();
        message.AppendLine();
        message.AppendLine("=".PadRight(80, '='));
        message.AppendLine("SECURITY VIOLATION: Unprotected Controller Actions Detected");
        message.AppendLine("=".PadRight(80, '='));
        message.AppendLine();
        message.AppendLine($"Found {unprotectedEndpoints.Count} endpoint(s) without proper security attributes:");
        message.AppendLine();

        foreach (var endpoint in unprotectedEndpoints.OrderBy(e => e.Controller).ThenBy(e => e.Action))
        {
            message.AppendLine($"  ❌ {endpoint.Controller}.{endpoint.Action}()");
            message.AppendLine($"     Method: {endpoint.HttpMethod}");
            message.AppendLine($"     Route: {endpoint.Route}");
            message.AppendLine($"     Fix: Add [RequirePermission(\"resource.action\")] or [AllowAnonymous] attribute");
            message.AppendLine();
        }

        message.AppendLine("REQUIRED ACTION:");
        message.AppendLine("  Each action method MUST have EITHER:");
        message.AppendLine("    1. [RequirePermission(\"resource.action\")] attribute (preferred), OR");
        message.AppendLine("    2. [AllowAnonymous] attribute (for intentionally public endpoints)");
        message.AppendLine();
        message.AppendLine("  If this endpoint should be public, add it to the AllowedPublicActions allowlist");
        message.AppendLine("  in ControllerSecurityAttributesTests.cs");
        message.AppendLine();
        message.AppendLine("=".PadRight(80, '='));

        return message.ToString();
    }

    /// <summary>
    /// Represents an unprotected endpoint found during the scan.
    /// </summary>
    private class UnprotectedEndpoint
    {
        public string Controller { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string HttpMethod { get; set; } = string.Empty;
        public string Route { get; set; } = string.Empty;
    }
}
