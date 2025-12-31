using Xunit;
using Moq;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using IntelliPM.API.Controllers;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace IntelliPM.Tests.API;

/// <summary>
/// Unit tests for RBAC authorization on ActivityController, SearchController, MetricsController, and InsightsController.
/// These tests verify that the controllers are properly configured with RequirePermission attributes.
/// Note: For full 403 Forbidden testing, integration tests with WebApplicationFactory are required (see RBAC_MissingControllers_Tests.cs).
/// </summary>
public class RBAC_MissingControllers_UnitTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ILogger<ActivityController>> _activityLoggerMock;
    private readonly Mock<ILogger<SearchController>> _searchLoggerMock;
    private readonly Mock<ILogger<MetricsController>> _metricsLoggerMock;

    public RBAC_MissingControllers_UnitTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _activityLoggerMock = new Mock<ILogger<ActivityController>>();
        _searchLoggerMock = new Mock<ILogger<SearchController>>();
        _metricsLoggerMock = new Mock<ILogger<MetricsController>>();
    }


    #region ActivityController Tests

    [Fact]
    public void ActivityController_HasRequirePermissionAttribute()
    {
        // Arrange & Act
        var method = typeof(ActivityController).GetMethod(nameof(ActivityController.GetRecentActivity));
        var attributes = method?.GetCustomAttributes(typeof(IntelliPM.API.Authorization.RequirePermissionAttribute), false);

        // Assert
        attributes.Should().NotBeNull();
        attributes.Should().HaveCount(1);
        var permissionAttr = attributes![0] as IntelliPM.API.Authorization.RequirePermissionAttribute;
        permissionAttr!.Permission.Should().Be("activity.view");
    }

    #endregion

    #region SearchController Tests

    [Fact]
    public void SearchController_HasRequirePermissionAttribute()
    {
        // Arrange & Act
        var method = typeof(SearchController).GetMethod(nameof(SearchController.Search));
        var attributes = method?.GetCustomAttributes(typeof(IntelliPM.API.Authorization.RequirePermissionAttribute), false);

        // Assert
        attributes.Should().NotBeNull();
        attributes.Should().HaveCount(1);
        var permissionAttr = attributes![0] as IntelliPM.API.Authorization.RequirePermissionAttribute;
        permissionAttr!.Permission.Should().Be("search.use");
    }

    #endregion

    #region MetricsController Tests

    [Fact]
    public void MetricsController_GetMetrics_HasRequirePermissionAttribute()
    {
        // Arrange & Act
        var method = typeof(MetricsController).GetMethod(nameof(MetricsController.GetMetrics));
        var attributes = method?.GetCustomAttributes(typeof(IntelliPM.API.Authorization.RequirePermissionAttribute), false);

        // Assert
        attributes.Should().NotBeNull();
        attributes.Should().HaveCount(1);
        var permissionAttr = attributes![0] as IntelliPM.API.Authorization.RequirePermissionAttribute;
        permissionAttr!.Permission.Should().Be("metrics.view");
    }

    [Fact]
    public void MetricsController_GetSprintVelocity_HasRequirePermissionAttribute()
    {
        // Arrange & Act
        var method = typeof(MetricsController).GetMethod(nameof(MetricsController.GetSprintVelocity));
        var attributes = method?.GetCustomAttributes(typeof(IntelliPM.API.Authorization.RequirePermissionAttribute), false);

        // Assert
        attributes.Should().NotBeNull();
        attributes.Should().HaveCount(1);
        var permissionAttr = attributes![0] as IntelliPM.API.Authorization.RequirePermissionAttribute;
        permissionAttr!.Permission.Should().Be("metrics.view");
    }

    [Fact]
    public void MetricsController_GetSprintVelocityChart_HasRequirePermissionAttribute()
    {
        // Arrange & Act
        var method = typeof(MetricsController).GetMethod(nameof(MetricsController.GetSprintVelocityChart));
        var attributes = method?.GetCustomAttributes(typeof(IntelliPM.API.Authorization.RequirePermissionAttribute), false);

        // Assert
        attributes.Should().NotBeNull();
        attributes.Should().HaveCount(1);
        var permissionAttr = attributes![0] as IntelliPM.API.Authorization.RequirePermissionAttribute;
        permissionAttr!.Permission.Should().Be("metrics.view");
    }

    [Fact]
    public void MetricsController_GetTaskDistribution_HasRequirePermissionAttribute()
    {
        // Arrange & Act
        var method = typeof(MetricsController).GetMethod(nameof(MetricsController.GetTaskDistribution));
        var attributes = method?.GetCustomAttributes(typeof(IntelliPM.API.Authorization.RequirePermissionAttribute), false);

        // Assert
        attributes.Should().NotBeNull();
        attributes.Should().HaveCount(1);
        var permissionAttr = attributes![0] as IntelliPM.API.Authorization.RequirePermissionAttribute;
        permissionAttr!.Permission.Should().Be("metrics.view");
    }

    [Fact]
    public void MetricsController_GetSprintBurndown_HasRequirePermissionAttribute()
    {
        // Arrange & Act
        var method = typeof(MetricsController).GetMethod(nameof(MetricsController.GetSprintBurndown));
        var attributes = method?.GetCustomAttributes(typeof(IntelliPM.API.Authorization.RequirePermissionAttribute), false);

        // Assert
        attributes.Should().NotBeNull();
        attributes.Should().HaveCount(1);
        var permissionAttr = attributes![0] as IntelliPM.API.Authorization.RequirePermissionAttribute;
        permissionAttr!.Permission.Should().Be("metrics.view");
    }

    [Fact]
    public void MetricsController_GetDefectsBySeverity_HasRequirePermissionAttribute()
    {
        // Arrange & Act
        var method = typeof(MetricsController).GetMethod(nameof(MetricsController.GetDefectsBySeverity));
        var attributes = method?.GetCustomAttributes(typeof(IntelliPM.API.Authorization.RequirePermissionAttribute), false);

        // Assert
        attributes.Should().NotBeNull();
        attributes.Should().HaveCount(1);
        var permissionAttr = attributes![0] as IntelliPM.API.Authorization.RequirePermissionAttribute;
        permissionAttr!.Permission.Should().Be("metrics.view");
    }

    [Fact]
    public void MetricsController_GetTeamVelocity_HasRequirePermissionAttribute()
    {
        // Arrange & Act
        var method = typeof(MetricsController).GetMethod(nameof(MetricsController.GetTeamVelocity));
        var attributes = method?.GetCustomAttributes(typeof(IntelliPM.API.Authorization.RequirePermissionAttribute), false);

        // Assert
        attributes.Should().NotBeNull();
        attributes.Should().HaveCount(1);
        var permissionAttr = attributes![0] as IntelliPM.API.Authorization.RequirePermissionAttribute;
        permissionAttr!.Permission.Should().Be("metrics.view");
    }

    #endregion

    #region InsightsController Tests

    [Fact]
    public void InsightsController_HasRequirePermissionAttribute()
    {
        // Arrange & Act
        var method = typeof(InsightsController).GetMethod(nameof(InsightsController.GetInsights));
        var attributes = method?.GetCustomAttributes(typeof(IntelliPM.API.Authorization.RequirePermissionAttribute), false);

        // Assert
        attributes.Should().NotBeNull();
        attributes.Should().HaveCount(1);
        var permissionAttr = attributes![0] as IntelliPM.API.Authorization.RequirePermissionAttribute;
        permissionAttr!.Permission.Should().Be("insights.view");
    }

    #endregion
}

