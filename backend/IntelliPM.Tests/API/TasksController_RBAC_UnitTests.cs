using Xunit;
using FluentAssertions;
using System.Reflection;
using System.Linq;
using IntelliPM.API.Controllers;
using IntelliPM.API.Authorization;

namespace IntelliPM.Tests.API;

/// <summary>
/// Unit tests for RBAC authorization on TasksController.
/// These tests verify that all controller actions are properly configured with RequirePermission attributes.
/// Note: For full 403 Forbidden testing, integration tests with WebApplicationFactory are required (see TasksController_RBAC_Tests.cs).
/// </summary>
public class TasksController_RBAC_UnitTests
{
    [Fact]
    public void TasksController_GetTasksByProject_HasRequirePermissionAttribute()
    {
        // Arrange & Act
        var method = typeof(TasksController).GetMethod(nameof(TasksController.GetTasksByProject));
        var attributes = method?.GetCustomAttributes(typeof(RequirePermissionAttribute), false);

        // Assert
        attributes.Should().NotBeNull();
        attributes.Should().HaveCount(1);
        var permissionAttr = attributes![0] as RequirePermissionAttribute;
        permissionAttr!.Permission.Should().Be("tasks.view");
    }

    [Fact]
    public void TasksController_GetTaskById_HasRequirePermissionAttribute()
    {
        // Arrange & Act
        var method = typeof(TasksController).GetMethod(nameof(TasksController.GetTaskById));
        var attributes = method?.GetCustomAttributes(typeof(RequirePermissionAttribute), false);

        // Assert
        attributes.Should().NotBeNull();
        attributes.Should().HaveCount(1);
        var permissionAttr = attributes![0] as RequirePermissionAttribute;
        permissionAttr!.Permission.Should().Be("tasks.view");
    }

    [Fact]
    public void TasksController_GetBlockedTasks_HasRequirePermissionAttribute()
    {
        // Arrange & Act
        var method = typeof(TasksController).GetMethod(nameof(TasksController.GetBlockedTasks));
        var attributes = method?.GetCustomAttributes(typeof(RequirePermissionAttribute), false);

        // Assert
        attributes.Should().NotBeNull();
        attributes.Should().HaveCount(1);
        var permissionAttr = attributes![0] as RequirePermissionAttribute;
        permissionAttr!.Permission.Should().Be("tasks.view");
    }

    [Fact]
    public void TasksController_GetTasksByAssignee_HasRequirePermissionAttribute()
    {
        // Arrange & Act
        var method = typeof(TasksController).GetMethod(nameof(TasksController.GetTasksByAssignee));
        var attributes = method?.GetCustomAttributes(typeof(RequirePermissionAttribute), false);

        // Assert
        attributes.Should().NotBeNull();
        attributes.Should().HaveCount(1);
        var permissionAttr = attributes![0] as RequirePermissionAttribute;
        permissionAttr!.Permission.Should().Be("tasks.view");
    }

    [Fact]
    public void TasksController_CreateTask_HasRequirePermissionAttribute()
    {
        // Arrange & Act
        var method = typeof(TasksController).GetMethod(nameof(TasksController.CreateTask));
        var attributes = method?.GetCustomAttributes(typeof(RequirePermissionAttribute), false);

        // Assert
        attributes.Should().NotBeNull();
        attributes.Should().HaveCount(1);
        var permissionAttr = attributes![0] as RequirePermissionAttribute;
        permissionAttr!.Permission.Should().Be("tasks.create");
    }

    [Fact]
    public void TasksController_UpdateTask_HasRequirePermissionAttribute()
    {
        // Arrange & Act
        var method = typeof(TasksController).GetMethod(nameof(TasksController.UpdateTask));
        var attributes = method?.GetCustomAttributes(typeof(RequirePermissionAttribute), false);

        // Assert
        attributes.Should().NotBeNull();
        attributes.Should().HaveCount(1);
        var permissionAttr = attributes![0] as RequirePermissionAttribute;
        permissionAttr!.Permission.Should().Be("tasks.edit");
    }

    [Fact]
    public void TasksController_ChangeTaskStatus_HasRequirePermissionAttribute()
    {
        // Arrange & Act
        var method = typeof(TasksController).GetMethod(nameof(TasksController.ChangeTaskStatus));
        var attributes = method?.GetCustomAttributes(typeof(RequirePermissionAttribute), false);

        // Assert
        attributes.Should().NotBeNull();
        attributes.Should().HaveCount(1);
        var permissionAttr = attributes![0] as RequirePermissionAttribute;
        permissionAttr!.Permission.Should().Be("tasks.edit");
    }

    [Fact]
    public void TasksController_AssignTask_HasRequirePermissionAttribute()
    {
        // Arrange & Act
        var method = typeof(TasksController).GetMethod(nameof(TasksController.AssignTask));
        var attributes = method?.GetCustomAttributes(typeof(RequirePermissionAttribute), false);

        // Assert
        attributes.Should().NotBeNull();
        attributes.Should().HaveCount(1);
        var permissionAttr = attributes![0] as RequirePermissionAttribute;
        permissionAttr!.Permission.Should().Be("tasks.assign");
    }

    [Fact]
    public void TasksController_AddTaskDependency_HasRequirePermissionAttribute()
    {
        // Arrange & Act
        var method = typeof(TasksController).GetMethod(nameof(TasksController.AddTaskDependency));
        var attributes = method?.GetCustomAttributes(typeof(RequirePermissionAttribute), false);

        // Assert
        attributes.Should().NotBeNull();
        attributes.Should().HaveCount(1);
        var permissionAttr = attributes![0] as RequirePermissionAttribute;
        permissionAttr!.Permission.Should().Be("tasks.dependencies.create");
    }

    [Fact]
    public void TasksController_RemoveTaskDependency_HasRequirePermissionAttribute()
    {
        // Arrange & Act
        var method = typeof(TasksController).GetMethod(nameof(TasksController.RemoveTaskDependency));
        var attributes = method?.GetCustomAttributes(typeof(RequirePermissionAttribute), false);

        // Assert
        attributes.Should().NotBeNull();
        attributes.Should().HaveCount(1);
        var permissionAttr = attributes![0] as RequirePermissionAttribute;
        permissionAttr!.Permission.Should().Be("tasks.dependencies.delete");
    }

    [Fact]
    public void TasksController_GetTaskDependencies_HasRequirePermissionAttribute()
    {
        // Arrange & Act
        var method = typeof(TasksController).GetMethod(nameof(TasksController.GetTaskDependencies));
        var attributes = method?.GetCustomAttributes(typeof(RequirePermissionAttribute), false);

        // Assert
        attributes.Should().NotBeNull();
        attributes.Should().HaveCount(1);
        var permissionAttr = attributes![0] as RequirePermissionAttribute;
        permissionAttr!.Permission.Should().Be("tasks.view");
    }

    /// <summary>
    /// Comprehensive test that verifies all public action methods in TasksController have RequirePermission attributes.
    /// This ensures no endpoint is left unprotected.
    /// </summary>
    [Fact]
    public void TasksController_AllPublicActions_HaveRequirePermissionAttributes()
    {
        // Arrange
        var controllerType = typeof(TasksController);
        var publicMethods = controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.ReturnType == typeof(Task<Microsoft.AspNetCore.Mvc.IActionResult>) ||
                       m.ReturnType == typeof(Task<Microsoft.AspNetCore.Mvc.ActionResult>))
            .Where(m => !m.IsSpecialName) // Exclude properties
            .ToList();

        // Act & Assert
        foreach (var method in publicMethods)
        {
            var attributes = method.GetCustomAttributes(typeof(RequirePermissionAttribute), false);
            attributes.Should().NotBeEmpty(
                $"Method {method.Name} should have a RequirePermission attribute");
        }
    }
}
