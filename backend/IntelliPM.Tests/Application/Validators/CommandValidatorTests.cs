using FluentAssertions;
using FluentValidation;
using IntelliPM.Application.Organizations.Commands;
using IntelliPM.Application.Projects.Commands;
using IntelliPM.Application.Tasks.Commands;
using Xunit;

namespace IntelliPM.Tests.Application.Validators;

public class CreateProjectCommandValidatorTests
{
    private readonly CreateProjectCommandValidator _validator;

    public CreateProjectCommandValidatorTests()
    {
        _validator = new CreateProjectCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldSucceed()
    {
        // Arrange
        var command = new CreateProjectCommand(
            "Test Project",
            "Test Description",
            "Scrum",
            14,
            1
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("ab")] // Too short
    public void Validate_WithInvalidName_ShouldFail(string name)
    {
        // Arrange
        var command = new CreateProjectCommand(
            name!,
            "Test Description",
            "Scrum",
            14,
            1
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Validate_WithNameExceedingMaxLength_ShouldFail()
    {
        // Arrange
        var command = new CreateProjectCommand(
            new string('A', 201), // 201 characters
            "Test Description",
            "Scrum",
            14,
            1
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("InvalidType")]
    public void Validate_WithInvalidType_ShouldFail(string type)
    {
        // Arrange
        var command = new CreateProjectCommand(
            "Test Project",
            "Test Description",
            type!,
            14,
            1
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Type");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(31)]
    public void Validate_WithInvalidSprintDuration_ShouldFail(int sprintDuration)
    {
        // Arrange
        var command = new CreateProjectCommand(
            "Test Project",
            "Test Description",
            "Scrum",
            sprintDuration,
            1
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "SprintDurationDays");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithInvalidOwnerId_ShouldFail(int ownerId)
    {
        // Arrange
        var command = new CreateProjectCommand(
            "Test Project",
            "Test Description",
            "Scrum",
            14,
            ownerId
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "OwnerId");
    }

    [Theory]
    [InlineData("InvalidStatus")]
    [InlineData("")]
    public void Validate_WithInvalidStatus_ShouldFail(string status)
    {
        // Arrange
        var command = new CreateProjectCommand(
            "Test Project",
            "Test Description",
            "Scrum",
            14,
            1,
            status
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Status");
    }

    [Fact]
    public void Validate_WithValidStatus_ShouldSucceed()
    {
        // Arrange
        var command = new CreateProjectCommand(
            "Test Project",
            "Test Description",
            "Scrum",
            14,
            1,
            "Active"
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithInvalidMemberIds_ShouldFail()
    {
        // Arrange
        var command = new CreateProjectCommand(
            "Test Project",
            "Test Description",
            "Scrum",
            14,
            1,
            "Active",
            null,
            new List<int> { 0, -1, 5 }
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "MemberIds");
    }
}

public class UpdateProjectCommandValidatorTests
{
    private readonly UpdateProjectCommandValidator _validator;

    public UpdateProjectCommandValidatorTests()
    {
        _validator = new UpdateProjectCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldSucceed()
    {
        // Arrange
        var command = new UpdateProjectCommand(
            1,
            1,
            "Updated Name",
            "Updated Description",
            "Active",
            "Scrum",
            14
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithInvalidProjectId_ShouldFail(int projectId)
    {
        // Arrange
        var command = new UpdateProjectCommand(
            projectId,
            1,
            "Updated Name"
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ProjectId");
    }

    [Fact]
    public void Validate_WithNameTooShort_ShouldFail()
    {
        // Arrange
        var command = new UpdateProjectCommand(
            1,
            1,
            "ab" // Too short
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Validate_WithInvalidSprintDuration_ShouldFail()
    {
        // Arrange
        var command = new UpdateProjectCommand(
            1,
            1,
            null,
            null,
            null,
            null,
            31 // Invalid
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "SprintDurationDays");
    }
}

public class CreateTaskCommandValidatorTests
{
    private readonly CreateTaskCommandValidator _validator;

    public CreateTaskCommandValidatorTests()
    {
        _validator = new CreateTaskCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldSucceed()
    {
        // Arrange
        var command = new CreateTaskCommand(
            "Test Task",
            "Test Description",
            1,
            "High",
            5,
            1,
            1
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_WithInvalidTitle_ShouldFail(string title)
    {
        // Arrange
        var command = new CreateTaskCommand(
            title!,
            "Test Description",
            1,
            "High",
            null,
            null,
            1
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title");
    }

    [Fact]
    public void Validate_WithTitleExceedingMaxLength_ShouldFail()
    {
        // Arrange
        var command = new CreateTaskCommand(
            new string('A', 501), // 501 characters
            "Test Description",
            1,
            "High",
            null,
            null,
            1
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_WithInvalidDescription_ShouldFail(string description)
    {
        // Arrange
        var command = new CreateTaskCommand(
            "Test Task",
            description!,
            1,
            "High",
            null,
            null,
            1
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Description");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("InvalidPriority")]
    public void Validate_WithInvalidPriority_ShouldFail(string priority)
    {
        // Arrange
        var command = new CreateTaskCommand(
            "Test Task",
            "Test Description",
            1,
            priority!,
            null,
            null,
            1
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Priority");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    [InlineData(-1)]
    public void Validate_WithInvalidStoryPoints_ShouldFail(int storyPoints)
    {
        // Arrange
        var command = new CreateTaskCommand(
            "Test Task",
            "Test Description",
            1,
            "High",
            storyPoints,
            null,
            1
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "StoryPoints");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithInvalidProjectId_ShouldFail(int projectId)
    {
        // Arrange
        var command = new CreateTaskCommand(
            "Test Task",
            "Test Description",
            projectId,
            "High",
            null,
            null,
            1
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ProjectId");
    }
}

public class UpdateTaskCommandValidatorTests
{
    private readonly UpdateTaskCommandValidator _validator;

    public UpdateTaskCommandValidatorTests()
    {
        _validator = new UpdateTaskCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldSucceed()
    {
        // Arrange
        var command = new UpdateTaskCommand(
            1,
            "Updated Title",
            "Updated Description",
            "High",
            5,
            1
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithInvalidTaskId_ShouldFail(int taskId)
    {
        // Arrange
        var command = new UpdateTaskCommand(
            taskId,
            "Updated Title",
            null,
            null,
            null,
            1
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "TaskId");
    }

    [Fact]
    public void Validate_WithEmptyTitle_ShouldFail()
    {
        // Arrange
        var command = new UpdateTaskCommand(
            1,
            "   ", // Whitespace only
            null,
            null,
            null,
            1
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title");
    }

    [Fact]
    public void Validate_WithInvalidPriority_ShouldFail()
    {
        // Arrange
        var command = new UpdateTaskCommand(
            1,
            null,
            null,
            "InvalidPriority",
            null,
            1
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Priority");
    }
}

public class ChangeTaskStatusCommandValidatorTests
{
    private readonly ChangeTaskStatusCommandValidator _validator;

    public ChangeTaskStatusCommandValidatorTests()
    {
        _validator = new ChangeTaskStatusCommandValidator();
    }

    [Theory]
    [InlineData("Todo")]
    [InlineData("InProgress")]
    [InlineData("InReview")]
    [InlineData("Done")]
    [InlineData("Blocked")]
    public void Validate_WithValidStatus_ShouldSucceed(string status)
    {
        // Arrange
        var command = new ChangeTaskStatusCommand(
            1,
            status,
            1
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("InvalidStatus")]
    [InlineData("inprogress")] // Case sensitive
    public void Validate_WithInvalidStatus_ShouldFail(string status)
    {
        // Arrange
        var command = new ChangeTaskStatusCommand(
            1,
            status!,
            1
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewStatus");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithInvalidTaskId_ShouldFail(int taskId)
    {
        // Arrange
        var command = new ChangeTaskStatusCommand(
            taskId,
            "Todo",
            1
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "TaskId");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithInvalidUpdatedBy_ShouldFail(int updatedBy)
    {
        // Arrange
        var command = new ChangeTaskStatusCommand(
            1,
            "Todo",
            updatedBy
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UpdatedBy");
    }
}

public class AssignTaskCommandValidatorTests
{
    private readonly AssignTaskCommandValidator _validator;

    public AssignTaskCommandValidatorTests()
    {
        _validator = new AssignTaskCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldSucceed()
    {
        // Arrange
        var command = new AssignTaskCommand(
            1,
            2,
            1
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithNullAssignee_ShouldSucceed()
    {
        // Arrange
        var command = new AssignTaskCommand(
            1,
            null,
            1
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithInvalidTaskId_ShouldFail(int taskId)
    {
        // Arrange
        var command = new AssignTaskCommand(
            taskId,
            2,
            1
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "TaskId");
    }

    [Fact]
    public void Validate_WithSelfAssignment_ShouldFail()
    {
        // Arrange
        var command = new AssignTaskCommand(
            1,
            1, // Same as UpdatedBy
            1
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "");
    }
}

public class DeleteOrganizationCommandValidatorTests
{
    private readonly DeleteOrganizationCommandValidator _validator;

    public DeleteOrganizationCommandValidatorTests()
    {
        _validator = new DeleteOrganizationCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldSucceed()
    {
        // Arrange
        var command = new DeleteOrganizationCommand
        {
            OrganizationId = 1
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithInvalidOrganizationId_ShouldFail(int organizationId)
    {
        // Arrange
        var command = new DeleteOrganizationCommand
        {
            OrganizationId = organizationId
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "OrganizationId");
    }
}
