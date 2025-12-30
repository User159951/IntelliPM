using Xunit;
using FluentAssertions;
using IntelliPM.Application.Common.Authorization;
using IntelliPM.Domain.Enums;

namespace IntelliPM.Tests.Application.Authorization;

public class PermissionTests
{
    public class GlobalPermissionsTests
    {
        [Fact]
        public void CanManageUsers_Admin_ReturnsTrue()
        {
            // Arrange
            var role = GlobalRole.Admin;

            // Act
            var result = GlobalPermissions.CanManageUsers(role);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void CanManageUsers_User_ReturnsFalse()
        {
            // Arrange
            var role = GlobalRole.User;

            // Act
            var result = GlobalPermissions.CanManageUsers(role);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void CanAccessSystem_ActiveUser_ReturnsTrue()
        {
            // Arrange
            var role = GlobalRole.User;
            var isActive = true;

            // Act
            var result = GlobalPermissions.CanAccessSystem(role, isActive);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void CanAccessSystem_InactiveUser_ReturnsFalse()
        {
            // Arrange
            var role = GlobalRole.User;
            var isActive = false;

            // Act
            var result = GlobalPermissions.CanAccessSystem(role, isActive);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void CanAccessSystem_ActiveAdmin_ReturnsTrue()
        {
            // Arrange
            var role = GlobalRole.Admin;
            var isActive = true;

            // Act
            var result = GlobalPermissions.CanAccessSystem(role, isActive);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void CanAccessSystem_InactiveAdmin_ReturnsFalse()
        {
            // Arrange
            var role = GlobalRole.Admin;
            var isActive = false;

            // Act
            var result = GlobalPermissions.CanAccessSystem(role, isActive);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void CanManageGlobalSettings_Admin_ReturnsTrue()
        {
            // Arrange
            var role = GlobalRole.Admin;

            // Act
            var result = GlobalPermissions.CanManageGlobalSettings(role);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void CanManageGlobalSettings_User_ReturnsFalse()
        {
            // Arrange
            var role = GlobalRole.User;

            // Act
            var result = GlobalPermissions.CanManageGlobalSettings(role);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void CanViewAllProjects_Admin_ReturnsTrue()
        {
            // Arrange
            var role = GlobalRole.Admin;

            // Act
            var result = GlobalPermissions.CanViewAllProjects(role);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void CanViewAllProjects_User_ReturnsFalse()
        {
            // Arrange
            var role = GlobalRole.User;

            // Act
            var result = GlobalPermissions.CanViewAllProjects(role);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void CanDeleteAnyProject_Admin_ReturnsTrue()
        {
            // Arrange
            var role = GlobalRole.Admin;

            // Act
            var result = GlobalPermissions.CanDeleteAnyProject(role);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void CanDeleteAnyProject_User_ReturnsFalse()
        {
            // Arrange
            var role = GlobalRole.User;

            // Act
            var result = GlobalPermissions.CanDeleteAnyProject(role);

            // Assert
            result.Should().BeFalse();
        }
    }

    public class ProjectPermissionsTests
    {
        [Fact]
        public void CanEditProject_ProductOwner_ReturnsTrue()
        {
            // Arrange
            var role = ProjectRole.ProductOwner;

            // Act
            var result = ProjectPermissions.CanEditProject(role);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void CanEditProject_ScrumMaster_ReturnsTrue()
        {
            // Arrange
            var role = ProjectRole.ScrumMaster;

            // Act
            var result = ProjectPermissions.CanEditProject(role);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void CanEditProject_Developer_ReturnsFalse()
        {
            // Arrange
            var role = ProjectRole.Developer;

            // Act
            var result = ProjectPermissions.CanEditProject(role);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void CanEditProject_Tester_ReturnsFalse()
        {
            // Arrange
            var role = ProjectRole.Tester;

            // Act
            var result = ProjectPermissions.CanEditProject(role);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void CanEditProject_Viewer_ReturnsFalse()
        {
            // Arrange
            var role = ProjectRole.Viewer;

            // Act
            var result = ProjectPermissions.CanEditProject(role);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void CanDeleteProject_ProductOwner_ReturnsTrue()
        {
            // Arrange
            var role = ProjectRole.ProductOwner;

            // Act
            var result = ProjectPermissions.CanDeleteProject(role);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void CanDeleteProject_ScrumMaster_ReturnsFalse()
        {
            // Arrange
            var role = ProjectRole.ScrumMaster;

            // Act
            var result = ProjectPermissions.CanDeleteProject(role);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void CanDeleteProject_Developer_ReturnsFalse()
        {
            // Arrange
            var role = ProjectRole.Developer;

            // Act
            var result = ProjectPermissions.CanDeleteProject(role);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void CanInviteMembers_ProductOwner_ReturnsTrue()
        {
            // Arrange
            var role = ProjectRole.ProductOwner;

            // Act
            var result = ProjectPermissions.CanInviteMembers(role);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void CanInviteMembers_ScrumMaster_ReturnsTrue()
        {
            // Arrange
            var role = ProjectRole.ScrumMaster;

            // Act
            var result = ProjectPermissions.CanInviteMembers(role);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void CanInviteMembers_Developer_ReturnsFalse()
        {
            // Arrange
            var role = ProjectRole.Developer;

            // Act
            var result = ProjectPermissions.CanInviteMembers(role);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void CanInviteMembers_Tester_ReturnsFalse()
        {
            // Arrange
            var role = ProjectRole.Tester;

            // Act
            var result = ProjectPermissions.CanInviteMembers(role);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void CanInviteMembers_Viewer_ReturnsFalse()
        {
            // Arrange
            var role = ProjectRole.Viewer;

            // Act
            var result = ProjectPermissions.CanInviteMembers(role);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void CanChangeRoles_ProductOwner_ReturnsTrue()
        {
            // Arrange
            var role = ProjectRole.ProductOwner;

            // Act
            var result = ProjectPermissions.CanChangeRoles(role);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void CanChangeRoles_ScrumMaster_ReturnsFalse()
        {
            // Arrange
            var role = ProjectRole.ScrumMaster;

            // Act
            var result = ProjectPermissions.CanChangeRoles(role);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void CanChangeRoles_Developer_ReturnsFalse()
        {
            // Arrange
            var role = ProjectRole.Developer;

            // Act
            var result = ProjectPermissions.CanChangeRoles(role);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void CanCreateTasks_ProductOwner_ReturnsTrue()
        {
            // Arrange
            var role = ProjectRole.ProductOwner;

            // Act
            var result = ProjectPermissions.CanCreateTasks(role);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void CanCreateTasks_ScrumMaster_ReturnsTrue()
        {
            // Arrange
            var role = ProjectRole.ScrumMaster;

            // Act
            var result = ProjectPermissions.CanCreateTasks(role);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void CanCreateTasks_Developer_ReturnsTrue()
        {
            // Arrange
            var role = ProjectRole.Developer;

            // Act
            var result = ProjectPermissions.CanCreateTasks(role);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void CanCreateTasks_Tester_ReturnsTrue()
        {
            // Arrange
            var role = ProjectRole.Tester;

            // Act
            var result = ProjectPermissions.CanCreateTasks(role);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void CanCreateTasks_Viewer_ReturnsFalse()
        {
            // Arrange
            var role = ProjectRole.Viewer;

            // Act
            var result = ProjectPermissions.CanCreateTasks(role);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void CanEditTasks_ProductOwner_ReturnsTrue()
        {
            // Arrange
            var role = ProjectRole.ProductOwner;

            // Act
            var result = ProjectPermissions.CanEditTasks(role);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void CanEditTasks_Viewer_ReturnsFalse()
        {
            // Arrange
            var role = ProjectRole.Viewer;

            // Act
            var result = ProjectPermissions.CanEditTasks(role);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void CanDeleteTasks_ProductOwner_ReturnsTrue()
        {
            // Arrange
            var role = ProjectRole.ProductOwner;

            // Act
            var result = ProjectPermissions.CanDeleteTasks(role);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void CanDeleteTasks_ScrumMaster_ReturnsTrue()
        {
            // Arrange
            var role = ProjectRole.ScrumMaster;

            // Act
            var result = ProjectPermissions.CanDeleteTasks(role);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void CanDeleteTasks_Developer_ReturnsFalse()
        {
            // Arrange
            var role = ProjectRole.Developer;

            // Act
            var result = ProjectPermissions.CanDeleteTasks(role);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void CanDeleteTasks_Tester_ReturnsFalse()
        {
            // Arrange
            var role = ProjectRole.Tester;

            // Act
            var result = ProjectPermissions.CanDeleteTasks(role);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void CanDeleteTasks_Viewer_ReturnsFalse()
        {
            // Arrange
            var role = ProjectRole.Viewer;

            // Act
            var result = ProjectPermissions.CanDeleteTasks(role);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void CanManageSprints_ProductOwner_ReturnsTrue()
        {
            // Arrange
            var role = ProjectRole.ProductOwner;

            // Act
            var result = ProjectPermissions.CanManageSprints(role);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void CanManageSprints_ScrumMaster_ReturnsTrue()
        {
            // Arrange
            var role = ProjectRole.ScrumMaster;

            // Act
            var result = ProjectPermissions.CanManageSprints(role);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void CanManageSprints_Developer_ReturnsFalse()
        {
            // Arrange
            var role = ProjectRole.Developer;

            // Act
            var result = ProjectPermissions.CanManageSprints(role);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void CanManageSprints_Tester_ReturnsFalse()
        {
            // Arrange
            var role = ProjectRole.Tester;

            // Act
            var result = ProjectPermissions.CanManageSprints(role);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void CanManageSprints_Viewer_ReturnsFalse()
        {
            // Arrange
            var role = ProjectRole.Viewer;

            // Act
            var result = ProjectPermissions.CanManageSprints(role);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void CanRemoveMembers_ProductOwner_ReturnsTrue()
        {
            // Arrange
            var role = ProjectRole.ProductOwner;

            // Act
            var result = ProjectPermissions.CanRemoveMembers(role);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void CanRemoveMembers_ScrumMaster_ReturnsTrue()
        {
            // Arrange
            var role = ProjectRole.ScrumMaster;

            // Act
            var result = ProjectPermissions.CanRemoveMembers(role);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void CanRemoveMembers_Developer_ReturnsFalse()
        {
            // Arrange
            var role = ProjectRole.Developer;

            // Act
            var result = ProjectPermissions.CanRemoveMembers(role);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void CanViewOnly_Viewer_ReturnsTrue()
        {
            // Arrange
            var role = ProjectRole.Viewer;

            // Act
            var result = ProjectPermissions.CanViewOnly(role);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void CanViewOnly_ProductOwner_ReturnsFalse()
        {
            // Arrange
            var role = ProjectRole.ProductOwner;

            // Act
            var result = ProjectPermissions.CanViewOnly(role);

            // Assert
            result.Should().BeFalse();
        }
    }
}

