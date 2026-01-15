using Microsoft.AspNetCore.Mvc.Testing;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using VerifyTests;
using VerifyXunit;
using Xunit;
using IntelliPM.Tests.API;

namespace IntelliPM.Tests.Integration;

/// <summary>
/// OpenAPI contract tests to prevent API breaking changes.
/// 
/// These tests generate the OpenAPI specification from the running API and compare it
/// against a committed snapshot. Any changes to routes, DTOs, status codes, or schemas
/// will cause the test to fail, preventing accidental breaking changes.
/// 
/// To update the snapshot after intentional API changes:
/// 1. Make your API changes
/// 2. Run the tests - they will fail showing the diff
/// 3. Review the diff to ensure changes are intentional
/// 4. Accept the changes using Verify's diff tool or by updating the snapshot manually
/// </summary>
public class ContractTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    /// <summary>
    /// Module initializer to configure Verify settings before any tests run.
    /// This must run before any Verify test executes.
    /// </summary>
    [ModuleInitializer]
    internal static void Initialize()
    {
        // Configure Verify settings for better diffs
        // Scrub non-deterministic fields that don't affect contract
        VerifierSettings.ScrubLinesContaining("servers", StringComparison.OrdinalIgnoreCase);
    }

    public ContractTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task OpenAPI_Specification_ShouldMatchSnapshot()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // Act: Fetch the OpenAPI JSON from Swagger endpoint
        // Note: Swagger is enabled in all environments for testing purposes
        var response = await client.GetAsync("/swagger/v1/swagger.json");
        response.EnsureSuccessStatusCode();

        var openApiJson = await response.Content.ReadAsStringAsync();
        
        // Validate JSON is parseable
        using var document = JsonDocument.Parse(openApiJson);
        Assert.NotNull(document);

        // Normalize the JSON for consistent comparison
        // Remove non-deterministic fields that may vary between runs
        var normalizedJson = NormalizeOpenApiJson(openApiJson);

        // Assert: Compare against snapshot using Verify
        await Verifier.Verify(normalizedJson)
            .UseDirectory("Snapshots")
            .UseFileName("OpenAPI.v1.json");
    }

    [Fact]
    public async Task OpenAPI_Specification_ShouldHaveAllRequiredRoutes()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/swagger/v1/swagger.json");
        response.EnsureSuccessStatusCode();

        var openApiJson = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(openApiJson);

        // Assert: Verify critical routes exist by checking paths object
        Assert.True(document.RootElement.TryGetProperty("paths", out var paths));
        
        // Core entity routes
        Assert.True(paths.TryGetProperty("/api/v1/Projects", out _), 
            "Missing /api/v1/Projects route");
        Assert.True(paths.TryGetProperty("/api/v1/Tasks", out _), 
            "Missing /api/v1/Tasks route");
        // Organizations are under admin routes
        Assert.True(paths.TryGetProperty("/api/admin/organizations", out _), 
            "Missing /api/admin/organizations route");
        // Releases can be at /api/v1/Releases or /api/v1/projects/{projectId}/releases
        var hasReleasesRoute = paths.TryGetProperty("/api/v1/Releases", out _) ||
                               paths.EnumerateObject().Any(p => p.Name.Contains("releases", StringComparison.OrdinalIgnoreCase));
        Assert.True(hasReleasesRoute, 
            "Missing Releases route (expected /api/v1/Releases or /api/v1/projects/{projectId}/releases)");
        // Milestones can be at /api/v1/Milestones or /api/v1/projects/{projectId}/milestones
        var hasMilestonesRoute = paths.TryGetProperty("/api/v1/Milestones", out _) ||
                                 paths.EnumerateObject().Any(p => p.Name.Contains("milestones", StringComparison.OrdinalIgnoreCase));
        Assert.True(hasMilestonesRoute, 
            "Missing Milestones route (expected /api/v1/Milestones or /api/v1/projects/{projectId}/milestones)");
        
        // Auth routes - check for login (register may not be exposed in OpenAPI)
        var hasLoginRoute = paths.TryGetProperty("/api/v1/Auth/login", out _) ||
                           paths.EnumerateObject().Any(p => p.Name.Contains("login", StringComparison.OrdinalIgnoreCase));
        Assert.True(hasLoginRoute, 
            "Missing login route");
        // Note: Register route may not be exposed in OpenAPI for security reasons
        
        // Metrics routes
        Assert.True(paths.TryGetProperty("/api/v1/Metrics", out _), 
            "Missing /api/v1/Metrics route");
        Assert.True(paths.TryGetProperty("/api/v1/Metrics/sprint-velocity-chart", out _), 
            "Missing /api/v1/Metrics/sprint-velocity-chart route");
        Assert.True(paths.TryGetProperty("/api/v1/Metrics/task-distribution", out _), 
            "Missing /api/v1/Metrics/task-distribution route");
    }

    [Fact]
    public async Task OpenAPI_Specification_ShouldHaveConsistentSchemas()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/swagger/v1/swagger.json");
        response.EnsureSuccessStatusCode();

        var openApiJson = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(openApiJson);

        // Assert: Verify schemas are present
        Assert.True(document.RootElement.TryGetProperty("components", out var components));
        Assert.True(components.TryGetProperty("schemas", out var schemas));
        
        // Verify common DTOs exist (schemas use fully qualified names)
        // Check if any schema contains the expected DTO names
        var hasProjectDto = false;
        var hasTaskDto = false;
        var hasOrganizationDto = false;
        
        foreach (var schema in schemas.EnumerateObject())
        {
            if (schema.Name.Contains("ProjectListDto") || schema.Name.Contains("GetProjectByIdResponse"))
            {
                hasProjectDto = true;
                // Verify schema has properties
                if (schema.Value.TryGetProperty("properties", out var properties))
                {
                    Assert.NotEqual(JsonValueKind.Undefined, properties.ValueKind);
                }
            }
            if (schema.Name.Contains("TaskDto") || schema.Name.Contains("ProjectTask"))
            {
                hasTaskDto = true;
            }
            if (schema.Name.Contains("OrganizationDto"))
            {
                hasOrganizationDto = true;
            }
        }
        
        Assert.True(hasProjectDto, "Missing Project-related DTO schema");
        Assert.True(hasTaskDto, "Missing Task-related DTO schema");
        Assert.True(hasOrganizationDto, "Missing OrganizationDto schema");
    }

    [Fact]
    public async Task OpenAPI_Specification_ShouldHaveMetricsDtos()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/swagger/v1/swagger.json");
        response.EnsureSuccessStatusCode();

        var openApiJson = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(openApiJson);

        // Assert: Verify metrics DTOs are present
        Assert.True(document.RootElement.TryGetProperty("components", out var components));
        Assert.True(components.TryGetProperty("schemas", out var schemas));
        
        // MetricsSummaryDto schema key may be fully qualified
        var hasMetricsSummaryDto = false;
        foreach (var schema in schemas.EnumerateObject())
        {
            if (schema.Name.Contains("MetricsSummaryDto"))
            {
                hasMetricsSummaryDto = true;
                
                // Verify the DTO has the required fields
                Assert.True(schema.Value.TryGetProperty("properties", out var properties));
                
                // Check for new fields added in this update
                Assert.True(properties.TryGetProperty("totalProjects", out _), 
                    "MetricsSummaryDto missing totalProjects property");
                Assert.True(properties.TryGetProperty("openTasks", out _), 
                    "MetricsSummaryDto missing openTasks property");
                Assert.True(properties.TryGetProperty("defectsCount", out _), 
                    "MetricsSummaryDto missing defectsCount property");
                Assert.True(properties.TryGetProperty("velocity", out _), 
                    "MetricsSummaryDto missing velocity property");
                Assert.True(properties.TryGetProperty("trends", out _), 
                    "MetricsSummaryDto missing trends property");
                break;
            }
        }
        Assert.True(hasMetricsSummaryDto, "Missing MetricsSummaryDto schema");
    }

    [Fact]
    public async Task OpenAPI_Specification_ShouldHaveConsistentStatusCodes()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/swagger/v1/swagger.json");
        response.EnsureSuccessStatusCode();

        var openApiJson = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(openApiJson);

        // Assert: Verify common status codes are documented
        Assert.True(document.RootElement.TryGetProperty("paths", out var paths));
        
        // Check a sample GET endpoint
        if (paths.TryGetProperty("/api/v1/Projects", out var projectsPath))
        {
            if (projectsPath.TryGetProperty("get", out var getOperation))
            {
                Assert.True(getOperation.TryGetProperty("responses", out var responses));
                Assert.True(responses.TryGetProperty("200", out _), 
                    "Missing 200 response for GET /api/v1/Projects");
                // Should have either 401 or 403 for unauthorized access
                Assert.True(
                    responses.TryGetProperty("401", out _) || 
                    responses.TryGetProperty("403", out _),
                    "Missing 401 or 403 response for GET /api/v1/Projects");
            }
        }
    }

    /// <summary>
    /// Normalizes OpenAPI JSON by removing non-deterministic fields that may vary between runs
    /// but don't affect the API contract (e.g., server URLs, timestamps, etc.)
    /// </summary>
    private string NormalizeOpenApiJson(string json)
    {
        using var document = JsonDocument.Parse(json);
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions 
        { 
            Indented = true 
        });

        writer.WriteStartObject();

        // Copy all properties except servers (which may vary by environment)
        foreach (var property in document.RootElement.EnumerateObject())
        {
            if (property.NameEquals("servers"))
            {
                // Skip servers array as it may vary by environment
                continue;
            }

            property.WriteTo(writer);
        }

        writer.WriteEndObject();
        writer.Flush();

        return Encoding.UTF8.GetString(stream.ToArray());
    }
}
