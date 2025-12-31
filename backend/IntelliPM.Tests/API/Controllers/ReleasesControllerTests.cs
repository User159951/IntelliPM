using Xunit;
using Moq;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using IntelliPM.API.Controllers;
using IntelliPM.Application.Features.Releases.Commands;
using IntelliPM.Application.Features.Releases.Queries;
using IntelliPM.Application.Features.Releases.DTOs;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Sprints.Queries;
using IntelliPM.Domain.Enums;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IntelliPM.Tests.API.Controllers;

/// <summary>
/// Unit tests for ReleasesController.
/// Tests all 17 endpoints with various scenarios including success cases, error cases, and edge cases.
/// </summary>
public class ReleasesControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ILogger<ReleasesController>> _loggerMock;
    private readonly ReleasesController _controller;

    public ReleasesControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _loggerMock = new Mock<ILogger<ReleasesController>>();
        _controller = new ReleasesController(_mediatorMock.Object, _loggerMock.Object);
    }

    #region GET /api/v1/projects/{projectId}/releases

    [Fact]
    public async Task GetProjectReleases_WithValidProjectId_ReturnsOkWithReleases()
    {
        // Arrange
        var projectId = 1;
        var expectedReleases = new List<ReleaseDto>
        {
            new() { Id = 1, Name = "v1.0.0", ProjectId = projectId, Version = "1.0.0", Status = "Planned", Type = "Major" },
            new() { Id = 2, Name = "v2.0.0", ProjectId = projectId, Version = "2.0.0", Status = "Deployed", Type = "Minor" }
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetProjectReleasesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedReleases);

        // Act
        var result = await _controller.GetProjectReleases(projectId, null, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var releases = okResult.Value.Should().BeAssignableTo<List<ReleaseDto>>().Subject;
        releases.Should().HaveCount(2);
        releases.Should().Contain(r => r.Name == "v1.0.0");
        releases.Should().Contain(r => r.Name == "v2.0.0");

        _mediatorMock.Verify(m => m.Send(
            It.Is<GetProjectReleasesQuery>(q => q.ProjectId == projectId && q.Status == null),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetProjectReleases_WithStatusFilter_ReturnsFilteredReleases()
    {
        // Arrange
        var projectId = 1;
        var status = "Planned";
        var expectedReleases = new List<ReleaseDto>
        {
            new() { Id = 1, Name = "v1.0.0", ProjectId = projectId, Version = "1.0.0", Status = "Planned", Type = "Major" }
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetProjectReleasesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedReleases);

        // Act
        var result = await _controller.GetProjectReleases(projectId, status, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var releases = okResult.Value.Should().BeAssignableTo<List<ReleaseDto>>().Subject;
        releases.Should().HaveCount(1);
        releases[0].Status.Should().Be("Planned");

        _mediatorMock.Verify(m => m.Send(
            It.Is<GetProjectReleasesQuery>(q => q.ProjectId == projectId && q.Status == ReleaseStatus.Planned),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetProjectReleases_ProjectNotFound_ReturnsProblem()
    {
        // Arrange
        var projectId = 999;
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetProjectReleasesQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Project with ID {projectId} not found."));

        // Act
        var result = await _controller.GetProjectReleases(projectId, null, CancellationToken.None);

        // Assert
        var problemResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        problemResult.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetProjectReleases_EmptyList_ReturnsOkWithEmptyArray()
    {
        // Arrange
        var projectId = 1;
        var expectedReleases = new List<ReleaseDto>();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetProjectReleasesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedReleases);

        // Act
        var result = await _controller.GetProjectReleases(projectId, null, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var releases = okResult.Value.Should().BeAssignableTo<List<ReleaseDto>>().Subject;
        releases.Should().BeEmpty();
    }

    #endregion

    #region GET /api/v1/releases/{id}

    [Fact]
    public async Task GetReleaseById_WithValidId_ReturnsOkWithReleaseDetails()
    {
        // Arrange
        var releaseId = 1;
        var expectedRelease = new ReleaseDto
        {
            Id = releaseId,
            Name = "v1.0.0",
            Version = "1.0.0",
            Status = "Planned",
            Type = "Major",
            ProjectId = 1,
            PlannedDate = DateTimeOffset.UtcNow.AddDays(30),
            Sprints = new List<ReleaseSprintDto>(),
            QualityGates = new List<QualityGateDto>()
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetReleaseByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedRelease);

        // Act
        var result = await _controller.GetReleaseById(releaseId, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var release = okResult.Value.Should().BeAssignableTo<ReleaseDto>().Subject;
        release.Id.Should().Be(releaseId);
        release.Name.Should().Be("v1.0.0");
        release.Version.Should().Be("1.0.0");

        _mediatorMock.Verify(m => m.Send(
            It.Is<GetReleaseByIdQuery>(q => q.Id == releaseId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetReleaseById_ReleaseNotFound_ReturnsNotFound()
    {
        // Arrange
        var releaseId = 999;
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetReleaseByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ReleaseDto?)null);

        // Act
        var result = await _controller.GetReleaseById(releaseId, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetReleaseById_ThrowsNotFoundException_ReturnsNotFound()
    {
        // Arrange
        var releaseId = 999;
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetReleaseByIdQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Release with ID {releaseId} not found."));

        // Act
        var result = await _controller.GetReleaseById(releaseId, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    #endregion

    #region GET /api/v1/projects/{projectId}/releases/statistics

    [Fact]
    public async Task GetReleaseStatistics_WithValidProjectId_ReturnsOkWithStatistics()
    {
        // Arrange
        var projectId = 1;
        var expectedStats = new ReleaseStatisticsDto
        {
            TotalReleases = 10,
            PlannedReleases = 3,
            DeployedReleases = 5,
            FailedReleases = 2,
            AverageLeadTime = 12.5
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetReleaseStatisticsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedStats);

        // Act
        var result = await _controller.GetReleaseStatistics(projectId, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var stats = okResult.Value.Should().BeAssignableTo<ReleaseStatisticsDto>().Subject;
        stats.TotalReleases.Should().Be(10);
        stats.DeployedReleases.Should().Be(5);
        stats.PlannedReleases.Should().Be(3);
        stats.FailedReleases.Should().Be(2);
        stats.AverageLeadTime.Should().Be(12.5);

        _mediatorMock.Verify(m => m.Send(
            It.Is<GetReleaseStatisticsQuery>(q => q.ProjectId == projectId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GET /api/v1/projects/{projectId}/sprints/available

    [Fact]
    public async Task GetAvailableSprints_WithValidProjectId_ReturnsOkWithSprints()
    {
        // Arrange
        var projectId = 1;
        var expectedSprints = new List<SprintDto>
        {
            new(1, projectId, "Test Project", 1, "Sprint Goal 1", DateTimeOffset.UtcNow.AddDays(-14), DateTimeOffset.UtcNow, "Completed", 10, DateTimeOffset.UtcNow),
            new(2, projectId, "Test Project", 2, "Sprint Goal 2", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(14), "Active", 5, DateTimeOffset.UtcNow)
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetAvailableSprintsForReleaseQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSprints);

        // Act
        var result = await _controller.GetAvailableSprints(projectId, null, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var sprints = okResult.Value.Should().BeAssignableTo<List<SprintDto>>().Subject;
        sprints.Should().HaveCount(2);

        _mediatorMock.Verify(m => m.Send(
            It.Is<GetAvailableSprintsForReleaseQuery>(q => q.ProjectId == projectId && q.ReleaseId == null),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAvailableSprints_WithReleaseId_ReturnsOkWithSprints()
    {
        // Arrange
        var projectId = 1;
        var releaseId = 5;
        var expectedSprints = new List<SprintDto>
        {
            new(1, projectId, "Test Project", 1, "Sprint Goal 1", DateTimeOffset.UtcNow.AddDays(-14), DateTimeOffset.UtcNow, "Completed", 10, DateTimeOffset.UtcNow)
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetAvailableSprintsForReleaseQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSprints);

        // Act
        var result = await _controller.GetAvailableSprints(projectId, releaseId, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var sprints = okResult.Value.Should().BeAssignableTo<List<SprintDto>>().Subject;
        sprints.Should().HaveCount(1);

        _mediatorMock.Verify(m => m.Send(
            It.Is<GetAvailableSprintsForReleaseQuery>(q => q.ProjectId == projectId && q.ReleaseId == releaseId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region POST /api/v1/projects/{projectId}/releases

    [Fact]
    public async Task CreateRelease_WithValidData_ReturnsCreatedAtAction()
    {
        // Arrange
        var projectId = 1;
        var request = new ReleasesController.CreateReleaseRequest
        {
            Name = "Release 1.0",
            Version = "1.0.0",
            Description = "First release",
            Type = "Major",
            PlannedDate = DateTimeOffset.UtcNow.AddDays(30),
            IsPreRelease = false,
            TagName = "v1.0.0",
            SprintIds = new List<int> { 1, 2 }
        };

        var expectedRelease = new ReleaseDto
        {
            Id = 1,
            Name = "Release 1.0",
            Version = "1.0.0",
            ProjectId = projectId,
            Status = "Planned",
            Type = "Major"
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CreateReleaseCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedRelease);

        // Act
        var result = await _controller.CreateRelease(projectId, request, CancellationToken.None);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
        createdResult.ActionName.Should().Be(nameof(_controller.GetReleaseById));

        var release = createdResult.Value.Should().BeAssignableTo<ReleaseDto>().Subject;
        release.Id.Should().Be(1);
        release.Name.Should().Be("Release 1.0");

        _mediatorMock.Verify(m => m.Send(
            It.Is<CreateReleaseCommand>(c => 
                c.ProjectId == projectId &&
                c.Name == request.Name &&
                c.Version == request.Version &&
                c.Type == ReleaseType.Major),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateRelease_InvalidReleaseType_ReturnsBadRequest()
    {
        // Arrange
        var projectId = 1;
        var request = new ReleasesController.CreateReleaseRequest
        {
            Name = "Release 1.0",
            Version = "1.0.0",
            Type = "InvalidType",
            PlannedDate = DateTimeOffset.UtcNow.AddDays(30)
        };

        // Act
        var result = await _controller.CreateRelease(projectId, request, CancellationToken.None);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);

        _mediatorMock.Verify(m => m.Send(
            It.IsAny<CreateReleaseCommand>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateRelease_ThrowsValidationException_ReturnsBadRequest()
    {
        // Arrange
        var projectId = 1;
        var request = new ReleasesController.CreateReleaseRequest
        {
            Name = "",
            Version = "invalid",
            Type = "Major",
            PlannedDate = DateTimeOffset.UtcNow.AddDays(-1)
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CreateReleaseCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("Validation failed: Name is required"));

        // Act
        var result = await _controller.CreateRelease(projectId, request, CancellationToken.None);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task CreateRelease_ThrowsNotFoundException_ReturnsProblem()
    {
        // Arrange
        var projectId = 999;
        var request = new ReleasesController.CreateReleaseRequest
        {
            Name = "Release 1.0",
            Version = "1.0.0",
            Type = "Major",
            PlannedDate = DateTimeOffset.UtcNow.AddDays(30)
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CreateReleaseCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Project with ID {projectId} not found."));

        // Act
        var result = await _controller.CreateRelease(projectId, request, CancellationToken.None);

        // Assert
        var problemResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        problemResult.StatusCode.Should().Be(500);
    }

    #endregion

    #region PUT /api/v1/releases/{id}

    [Fact]
    public async Task UpdateRelease_WithValidData_ReturnsOk()
    {
        // Arrange
        var releaseId = 1;
        var request = new ReleasesController.UpdateReleaseRequest
        {
            Name = "Updated Release",
            Version = "1.0.1",
            Description = "Updated description",
            PlannedDate = DateTimeOffset.UtcNow.AddDays(15),
            Status = "InProgress"
        };

        var expectedRelease = new ReleaseDto
        {
            Id = releaseId,
            Name = "Updated Release",
            Version = "1.0.1",
            Status = "InProgress"
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<UpdateReleaseCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedRelease);

        // Act
        var result = await _controller.UpdateRelease(releaseId, request, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var release = okResult.Value.Should().BeAssignableTo<ReleaseDto>().Subject;
        release.Id.Should().Be(releaseId);
        release.Name.Should().Be("Updated Release");

        _mediatorMock.Verify(m => m.Send(
            It.Is<UpdateReleaseCommand>(c => 
                c.Id == releaseId &&
                c.Name == request.Name &&
                c.Status == ReleaseStatus.InProgress),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateRelease_InvalidStatus_ReturnsBadRequest()
    {
        // Arrange
        var releaseId = 1;
        var request = new ReleasesController.UpdateReleaseRequest
        {
            Name = "Updated Release",
            Version = "1.0.1",
            Status = "InvalidStatus",
            PlannedDate = DateTimeOffset.UtcNow.AddDays(15)
        };

        // Act
        var result = await _controller.UpdateRelease(releaseId, request, CancellationToken.None);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);

        _mediatorMock.Verify(m => m.Send(
            It.IsAny<UpdateReleaseCommand>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateRelease_ThrowsNotFoundException_ReturnsNotFound()
    {
        // Arrange
        var releaseId = 999;
        var request = new ReleasesController.UpdateReleaseRequest
        {
            Name = "Updated Release",
            Version = "1.0.1",
            Status = "Planned",
            PlannedDate = DateTimeOffset.UtcNow.AddDays(15)
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<UpdateReleaseCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Release with ID {releaseId} not found."));

        // Act
        var result = await _controller.UpdateRelease(releaseId, request, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task UpdateRelease_ThrowsValidationException_ReturnsBadRequest()
    {
        // Arrange
        var releaseId = 1;
        var request = new ReleasesController.UpdateReleaseRequest
        {
            Name = "Updated Release",
            Version = "1.0.1",
            Status = "Deployed",
            PlannedDate = DateTimeOffset.UtcNow.AddDays(15)
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<UpdateReleaseCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("Cannot update a deployed release."));

        // Act
        var result = await _controller.UpdateRelease(releaseId, request, CancellationToken.None);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }

    #endregion

    #region DELETE /api/v1/releases/{id}

    [Fact]
    public async Task DeleteRelease_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var releaseId = 1;
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<DeleteReleaseCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MediatR.Unit.Value);

        // Act
        var result = await _controller.DeleteRelease(releaseId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();

        _mediatorMock.Verify(m => m.Send(
            It.Is<DeleteReleaseCommand>(c => c.Id == releaseId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteRelease_ThrowsNotFoundException_ReturnsNotFound()
    {
        // Arrange
        var releaseId = 999;
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<DeleteReleaseCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Release with ID {releaseId} not found."));

        // Act
        var result = await _controller.DeleteRelease(releaseId, CancellationToken.None);

        // Assert
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task DeleteRelease_ThrowsValidationException_ReturnsProblem()
    {
        // Arrange
        var releaseId = 1;
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<DeleteReleaseCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("Cannot delete a deployed release. Consider archiving instead."));

        // Act
        var result = await _controller.DeleteRelease(releaseId, CancellationToken.None);

        // Assert
        var problemResult = result.Should().BeOfType<ObjectResult>().Subject;
        problemResult.StatusCode.Should().Be(500);
    }

    #endregion

    #region POST /api/v1/releases/{id}/deploy

    [Fact]
    public async Task DeployRelease_WithValidId_ReturnsOk()
    {
        // Arrange
        var releaseId = 1;
        var expectedRelease = new ReleaseDto
        {
            Id = releaseId,
            Name = "v1.0.0",
            Version = "1.0.0",
            Status = "Deployed",
            Type = "Major"
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<DeployReleaseCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedRelease);

        // Act
        var result = await _controller.DeployRelease(releaseId, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var release = okResult.Value.Should().BeAssignableTo<ReleaseDto>().Subject;
        release.Id.Should().Be(releaseId);
        release.Status.Should().Be("Deployed");

        _mediatorMock.Verify(m => m.Send(
            It.Is<DeployReleaseCommand>(c => c.Id == releaseId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeployRelease_ThrowsNotFoundException_ReturnsNotFound()
    {
        // Arrange
        var releaseId = 999;
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<DeployReleaseCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Release with ID {releaseId} not found."));

        // Act
        var result = await _controller.DeployRelease(releaseId, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task DeployRelease_QualityGatesFailed_ReturnsBadRequest()
    {
        // Arrange
        var releaseId = 1;
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<DeployReleaseCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Cannot deploy release: not all required quality gates have passed."));

        // Act
        var result = await _controller.DeployRelease(releaseId, CancellationToken.None);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task DeployRelease_ThrowsValidationException_ReturnsBadRequest()
    {
        // Arrange
        var releaseId = 1;
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<DeployReleaseCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("Cannot deploy release: not all required quality gates have passed."));

        // Act
        var result = await _controller.DeployRelease(releaseId, CancellationToken.None);

        // Assert
        var problemResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        problemResult.StatusCode.Should().Be(500);
    }

    #endregion

    #region POST /api/v1/releases/{releaseId}/sprints/{sprintId}

    [Fact]
    public async Task AddSprintToRelease_WithValidIds_ReturnsOk()
    {
        // Arrange
        var releaseId = 1;
        var sprintId = 10;
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<AddSprintToReleaseCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MediatR.Unit.Value);

        // Act
        var result = await _controller.AddSprintToRelease(releaseId, sprintId, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        _mediatorMock.Verify(m => m.Send(
            It.Is<AddSprintToReleaseCommand>(c => c.ReleaseId == releaseId && c.SprintId == sprintId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddSprintToRelease_ThrowsNotFoundException_ReturnsNotFound()
    {
        // Arrange
        var releaseId = 1;
        var sprintId = 999;
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<AddSprintToReleaseCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Sprint with ID {sprintId} not found."));

        // Act
        var result = await _controller.AddSprintToRelease(releaseId, sprintId, CancellationToken.None);

        // Assert
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task AddSprintToRelease_SprintAlreadyAssigned_ReturnsProblem()
    {
        // Arrange
        var releaseId = 1;
        var sprintId = 10;
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<AddSprintToReleaseCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("Sprint is already assigned to release 2."));

        // Act
        var result = await _controller.AddSprintToRelease(releaseId, sprintId, CancellationToken.None);

        // Assert
        var problemResult = result.Should().BeOfType<ObjectResult>().Subject;
        problemResult.StatusCode.Should().Be(500);
    }

    #endregion

    #region POST /api/v1/releases/{releaseId}/sprints/bulk

    [Fact]
    public async Task BulkAddSprintsToRelease_WithValidData_ReturnsOk()
    {
        // Arrange
        var releaseId = 1;
        var request = new ReleasesController.BulkAddSprintsRequest
        {
            SprintIds = new List<int> { 10, 11, 12 }
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<BulkAddSprintsToReleaseCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        // Act
        var result = await _controller.BulkAddSprintsToRelease(releaseId, request, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<ReleasesController.BulkAddSprintsResponse>().Subject;
        response.AddedCount.Should().Be(3);

        _mediatorMock.Verify(m => m.Send(
            It.Is<BulkAddSprintsToReleaseCommand>(c => 
                c.ReleaseId == releaseId &&
                c.SprintIds.Count == 3),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BulkAddSprintsToRelease_EmptySprintIds_ReturnsBadRequest()
    {
        // Arrange
        var releaseId = 1;
        var request = new ReleasesController.BulkAddSprintsRequest
        {
            SprintIds = new List<int>()
        };

        // Act
        var result = await _controller.BulkAddSprintsToRelease(releaseId, request, CancellationToken.None);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);

        _mediatorMock.Verify(m => m.Send(
            It.IsAny<BulkAddSprintsToReleaseCommand>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task BulkAddSprintsToRelease_NullSprintIds_ReturnsBadRequest()
    {
        // Arrange
        var releaseId = 1;
        var request = new ReleasesController.BulkAddSprintsRequest
        {
            SprintIds = null!
        };

        // Act
        var result = await _controller.BulkAddSprintsToRelease(releaseId, request, CancellationToken.None);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task BulkAddSprintsToRelease_ThrowsNotFoundException_ReturnsNotFound()
    {
        // Arrange
        var releaseId = 999;
        var request = new ReleasesController.BulkAddSprintsRequest
        {
            SprintIds = new List<int> { 10, 11 }
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<BulkAddSprintsToReleaseCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("One or more sprints not found."));

        // Act
        var result = await _controller.BulkAddSprintsToRelease(releaseId, request, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    #endregion

    #region DELETE /api/v1/releases/sprints/{sprintId}

    [Fact]
    public async Task RemoveSprintFromRelease_WithValidSprintId_ReturnsNoContent()
    {
        // Arrange
        var sprintId = 10;
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<RemoveSprintFromReleaseCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MediatR.Unit.Value);

        // Act
        var result = await _controller.RemoveSprintFromRelease(sprintId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();

        _mediatorMock.Verify(m => m.Send(
            It.Is<RemoveSprintFromReleaseCommand>(c => c.SprintId == sprintId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveSprintFromRelease_ThrowsNotFoundException_ReturnsNotFound()
    {
        // Arrange
        var sprintId = 999;
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<RemoveSprintFromReleaseCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Sprint with ID {sprintId} not found."));

        // Act
        var result = await _controller.RemoveSprintFromRelease(sprintId, CancellationToken.None);

        // Assert
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    #endregion

    #region POST /api/v1/releases/{releaseId}/notes/generate

    [Fact]
    public async Task GenerateReleaseNotes_WithValidReleaseId_ReturnsOkWithNotes()
    {
        // Arrange
        var releaseId = 1;
        var expectedNotes = "# Release 1.0.0\n\n## New Features\n- Feature A\n- Feature B";

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GenerateReleaseNotesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedNotes);

        // Act
        var result = await _controller.GenerateReleaseNotes(releaseId, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<ReleasesController.GenerateReleaseNotesResponse>().Subject;
        response.ReleaseNotes.Should().Contain("Release 1.0.0");
        response.ReleaseNotes.Should().Contain("Feature A");

        _mediatorMock.Verify(m => m.Send(
            It.Is<GenerateReleaseNotesCommand>(c => c.ReleaseId == releaseId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GenerateReleaseNotes_ThrowsNotFoundException_ReturnsNotFound()
    {
        // Arrange
        var releaseId = 999;
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GenerateReleaseNotesCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Release with ID {releaseId} not found."));

        // Act
        var result = await _controller.GenerateReleaseNotes(releaseId, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    #endregion

    #region PUT /api/v1/releases/{releaseId}/notes

    [Fact]
    public async Task UpdateReleaseNotes_WithValidData_ReturnsOk()
    {
        // Arrange
        var releaseId = 1;
        var request = new ReleasesController.UpdateReleaseNotesRequest
        {
            ReleaseNotes = "Updated release notes",
            AutoGenerate = false
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<UpdateReleaseNotesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MediatR.Unit.Value);

        // Act
        var result = await _controller.UpdateReleaseNotes(releaseId, request, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        _mediatorMock.Verify(m => m.Send(
            It.Is<UpdateReleaseNotesCommand>(c => 
                c.ReleaseId == releaseId &&
                c.ReleaseNotes == request.ReleaseNotes &&
                c.AutoGenerate == false),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateReleaseNotes_WithAutoGenerate_ReturnsOk()
    {
        // Arrange
        var releaseId = 1;
        var request = new ReleasesController.UpdateReleaseNotesRequest
        {
            ReleaseNotes = null,
            AutoGenerate = true
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<UpdateReleaseNotesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MediatR.Unit.Value);

        // Act
        var result = await _controller.UpdateReleaseNotes(releaseId, request, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        _mediatorMock.Verify(m => m.Send(
            It.Is<UpdateReleaseNotesCommand>(c => 
                c.ReleaseId == releaseId &&
                c.AutoGenerate == true),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateReleaseNotes_ThrowsNotFoundException_ReturnsNotFound()
    {
        // Arrange
        var releaseId = 999;
        var request = new ReleasesController.UpdateReleaseNotesRequest
        {
            ReleaseNotes = "Updated notes",
            AutoGenerate = false
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<UpdateReleaseNotesCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Release with ID {releaseId} not found."));

        // Act
        var result = await _controller.UpdateReleaseNotes(releaseId, request, CancellationToken.None);

        // Assert
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    #endregion

    #region POST /api/v1/releases/{releaseId}/changelog/generate

    [Fact]
    public async Task GenerateChangelog_WithValidReleaseId_ReturnsOkWithChangelog()
    {
        // Arrange
        var releaseId = 1;
        var expectedChangelog = "## [1.0.0]\n### Added\n- New feature";

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GenerateChangeLogCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedChangelog);

        // Act
        var result = await _controller.GenerateChangelog(releaseId, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<ReleasesController.GenerateChangelogResponse>().Subject;
        response.ChangeLog.Should().Contain("1.0.0");
        response.ChangeLog.Should().Contain("Added");

        _mediatorMock.Verify(m => m.Send(
            It.Is<GenerateChangeLogCommand>(c => c.ReleaseId == releaseId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GenerateChangelog_ThrowsNotFoundException_ReturnsNotFound()
    {
        // Arrange
        var releaseId = 999;
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GenerateChangeLogCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Release with ID {releaseId} not found."));

        // Act
        var result = await _controller.GenerateChangelog(releaseId, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    #endregion

    #region PUT /api/v1/releases/{releaseId}/changelog

    [Fact]
    public async Task UpdateChangelog_WithValidData_ReturnsOk()
    {
        // Arrange
        var releaseId = 1;
        var request = new ReleasesController.UpdateChangelogRequest
        {
            ChangeLog = "Updated changelog",
            AutoGenerate = false
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<UpdateChangeLogCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MediatR.Unit.Value);

        // Act
        var result = await _controller.UpdateChangelog(releaseId, request, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        _mediatorMock.Verify(m => m.Send(
            It.Is<UpdateChangeLogCommand>(c => 
                c.ReleaseId == releaseId &&
                c.ChangeLog == request.ChangeLog &&
                c.AutoGenerate == false),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateChangelog_WithAutoGenerate_ReturnsOk()
    {
        // Arrange
        var releaseId = 1;
        var request = new ReleasesController.UpdateChangelogRequest
        {
            ChangeLog = null,
            AutoGenerate = true
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<UpdateChangeLogCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MediatR.Unit.Value);

        // Act
        var result = await _controller.UpdateChangelog(releaseId, request, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        _mediatorMock.Verify(m => m.Send(
            It.Is<UpdateChangeLogCommand>(c => 
                c.ReleaseId == releaseId &&
                c.AutoGenerate == true),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateChangelog_ThrowsNotFoundException_ReturnsNotFound()
    {
        // Arrange
        var releaseId = 999;
        var request = new ReleasesController.UpdateChangelogRequest
        {
            ChangeLog = "Updated changelog",
            AutoGenerate = false
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<UpdateChangeLogCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Release with ID {releaseId} not found."));

        // Act
        var result = await _controller.UpdateChangelog(releaseId, request, CancellationToken.None);

        // Assert
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    #endregion

    #region POST /api/v1/releases/{releaseId}/quality-gates/evaluate

    [Fact]
    public async Task EvaluateQualityGates_WithValidReleaseId_ReturnsOkWithResults()
    {
        // Arrange
        var releaseId = 1;
        var expectedQualityGates = new List<QualityGateDto>
        {
            new()
            {
                Id = 1,
                ReleaseId = releaseId,
                Type = "CodeCoverage",
                Status = "Passed",
                IsRequired = true,
                Message = "Coverage: 85%",
                CheckedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Id = 2,
                ReleaseId = releaseId,
                Type = "AllTasksCompleted",
                Status = "Passed",
                IsRequired = true,
                Message = "All tasks completed",
                CheckedAt = DateTimeOffset.UtcNow
            }
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<EvaluateQualityGatesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedQualityGates);

        // Act
        var result = await _controller.EvaluateQualityGates(releaseId, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var qualityGates = okResult.Value.Should().BeAssignableTo<List<QualityGateDto>>().Subject;
        qualityGates.Should().HaveCount(2);
        qualityGates.Should().AllSatisfy(qg => qg.Status.Should().Be("Passed"));

        _mediatorMock.Verify(m => m.Send(
            It.Is<EvaluateQualityGatesCommand>(c => c.ReleaseId == releaseId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EvaluateQualityGates_ThrowsNotFoundException_ReturnsNotFound()
    {
        // Arrange
        var releaseId = 999;
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<EvaluateQualityGatesCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Release with ID {releaseId} not found."));

        // Act
        var result = await _controller.EvaluateQualityGates(releaseId, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    #endregion

    #region POST /api/v1/releases/{releaseId}/quality-gates/approve

    [Fact]
    public async Task ApproveQualityGate_WithValidIds_ReturnsOk()
    {
        // Arrange
        var releaseId = 1;
        var request = new ReleasesController.ApproveQualityGateRequest
        {
            GateType = (int)QualityGateType.ManualApproval
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<ApproveQualityGateCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MediatR.Unit.Value);

        // Act
        var result = await _controller.ApproveQualityGate(releaseId, request, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        _mediatorMock.Verify(m => m.Send(
            It.Is<ApproveQualityGateCommand>(c => 
                c.ReleaseId == releaseId &&
                c.GateType == QualityGateType.ManualApproval),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ApproveQualityGate_InvalidGateType_ReturnsBadRequest()
    {
        // Arrange
        var releaseId = 1;
        var request = new ReleasesController.ApproveQualityGateRequest
        {
            GateType = 999 // Invalid enum value
        };

        // Act
        var result = await _controller.ApproveQualityGate(releaseId, request, CancellationToken.None);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);

        _mediatorMock.Verify(m => m.Send(
            It.IsAny<ApproveQualityGateCommand>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ApproveQualityGate_ThrowsNotFoundException_ReturnsNotFound()
    {
        // Arrange
        var releaseId = 999;
        var request = new ReleasesController.ApproveQualityGateRequest
        {
            GateType = (int)QualityGateType.ManualApproval
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<ApproveQualityGateCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Release with ID {releaseId} not found."));

        // Act
        var result = await _controller.ApproveQualityGate(releaseId, request, CancellationToken.None);

        // Assert
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task ApproveQualityGate_ThrowsInvalidOperationException_ReturnsBadRequest()
    {
        // Arrange
        var releaseId = 1;
        var request = new ReleasesController.ApproveQualityGateRequest
        {
            GateType = (int)QualityGateType.ManualApproval
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<ApproveQualityGateCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Quality gate is already approved."));

        // Act
        var result = await _controller.ApproveQualityGate(releaseId, request, CancellationToken.None);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }

    #endregion

    #region Additional Edge Cases and Theory Tests

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetReleaseById_InvalidId_ReturnsNotFound(int invalidId)
    {
        // Arrange
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetReleaseByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ReleaseDto?)null);

        // Act
        var result = await _controller.GetReleaseById(invalidId, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task DeleteRelease_InvalidId_ThrowsNotFoundException(int invalidId)
    {
        // Arrange
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<DeleteReleaseCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Release with ID {invalidId} not found."));

        // Act
        var result = await _controller.DeleteRelease(invalidId, CancellationToken.None);

        // Assert
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task CreateRelease_WithNullSprintIds_ReturnsCreated()
    {
        // Arrange
        var projectId = 1;
        var request = new ReleasesController.CreateReleaseRequest
        {
            Name = "Release 1.0",
            Version = "1.0.0",
            Description = "First release",
            Type = "Major",
            PlannedDate = DateTimeOffset.UtcNow.AddDays(30),
            IsPreRelease = false,
            TagName = "v1.0.0",
            SprintIds = null
        };

        var expectedRelease = new ReleaseDto
        {
            Id = 1,
            Name = "Release 1.0",
            Version = "1.0.0",
            ProjectId = projectId,
            Status = "Planned",
            Type = "Major"
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CreateReleaseCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedRelease);

        // Act
        var result = await _controller.CreateRelease(projectId, request, CancellationToken.None);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task CreateRelease_WithEmptySprintIds_ReturnsCreated()
    {
        // Arrange
        var projectId = 1;
        var request = new ReleasesController.CreateReleaseRequest
        {
            Name = "Release 1.0",
            Version = "1.0.0",
            Description = "First release",
            Type = "Major",
            PlannedDate = DateTimeOffset.UtcNow.AddDays(30),
            IsPreRelease = false,
            TagName = "v1.0.0",
            SprintIds = new List<int>()
        };

        var expectedRelease = new ReleaseDto
        {
            Id = 1,
            Name = "Release 1.0",
            Version = "1.0.0",
            ProjectId = projectId,
            Status = "Planned",
            Type = "Major"
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CreateReleaseCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedRelease);

        // Act
        var result = await _controller.CreateRelease(projectId, request, CancellationToken.None);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
    }

    [Theory]
    [InlineData("Major")]
    [InlineData("Minor")]
    [InlineData("Patch")]
    [InlineData("Hotfix")]
    public async Task CreateRelease_WithValidReleaseTypes_ReturnsCreated(string releaseType)
    {
        // Arrange
        var projectId = 1;
        var request = new ReleasesController.CreateReleaseRequest
        {
            Name = "Release 1.0",
            Version = "1.0.0",
            Description = "First release",
            Type = releaseType,
            PlannedDate = DateTimeOffset.UtcNow.AddDays(30)
        };

        var expectedRelease = new ReleaseDto
        {
            Id = 1,
            Name = "Release 1.0",
            Version = "1.0.0",
            ProjectId = projectId,
            Status = "Planned",
            Type = releaseType
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CreateReleaseCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedRelease);

        // Act
        var result = await _controller.CreateRelease(projectId, request, CancellationToken.None);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
    }

    [Theory]
    [InlineData("Planned")]
    [InlineData("InProgress")]
    [InlineData("Deployed")]
    [InlineData("Cancelled")]
    public async Task UpdateRelease_WithValidStatuses_ReturnsOk(string status)
    {
        // Arrange
        var releaseId = 1;
        var request = new ReleasesController.UpdateReleaseRequest
        {
            Name = "Updated Release",
            Version = "1.0.1",
            Description = "Updated description",
            PlannedDate = DateTimeOffset.UtcNow.AddDays(15),
            Status = status
        };

        var expectedRelease = new ReleaseDto
        {
            Id = releaseId,
            Name = "Updated Release",
            Version = "1.0.1",
            Status = status
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<UpdateReleaseCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedRelease);

        // Act
        var result = await _controller.UpdateRelease(releaseId, request, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GetProjectReleases_WithInvalidStatus_ReturnsOkWithAllReleases()
    {
        // Arrange
        var projectId = 1;
        var invalidStatus = "InvalidStatus";
        var expectedReleases = new List<ReleaseDto>
        {
            new() { Id = 1, Name = "v1.0.0", ProjectId = projectId, Version = "1.0.0", Status = "Planned", Type = "Major" }
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetProjectReleasesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedReleases);

        // Act
        var result = await _controller.GetProjectReleases(projectId, invalidStatus, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var releases = okResult.Value.Should().BeAssignableTo<List<ReleaseDto>>().Subject;
        
        // Should still return results (invalid status is ignored)
        _mediatorMock.Verify(m => m.Send(
            It.Is<GetProjectReleasesQuery>(q => q.ProjectId == projectId && q.Status == null),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateReleaseNotes_WithNullNotes_ReturnsOk()
    {
        // Arrange
        var releaseId = 1;
        var request = new ReleasesController.UpdateReleaseNotesRequest
        {
            ReleaseNotes = null,
            AutoGenerate = false
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<UpdateReleaseNotesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MediatR.Unit.Value);

        // Act
        var result = await _controller.UpdateReleaseNotes(releaseId, request, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task UpdateChangelog_WithNullChangelog_ReturnsOk()
    {
        // Arrange
        var releaseId = 1;
        var request = new ReleasesController.UpdateChangelogRequest
        {
            ChangeLog = null,
            AutoGenerate = false
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<UpdateChangeLogCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MediatR.Unit.Value);

        // Act
        var result = await _controller.UpdateChangelog(releaseId, request, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task EvaluateQualityGates_WithNoQualityGates_ReturnsOkWithEmptyList()
    {
        // Arrange
        var releaseId = 1;
        var expectedQualityGates = new List<QualityGateDto>();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<EvaluateQualityGatesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedQualityGates);

        // Act
        var result = await _controller.EvaluateQualityGates(releaseId, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var qualityGates = okResult.Value.Should().BeAssignableTo<List<QualityGateDto>>().Subject;
        qualityGates.Should().BeEmpty();
    }

    [Fact]
    public async Task BulkAddSprintsToRelease_WithSingleSprint_ReturnsOk()
    {
        // Arrange
        var releaseId = 1;
        var request = new ReleasesController.BulkAddSprintsRequest
        {
            SprintIds = new List<int> { 10 }
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<BulkAddSprintsToReleaseCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _controller.BulkAddSprintsToRelease(releaseId, request, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<ReleasesController.BulkAddSprintsResponse>().Subject;
        response.AddedCount.Should().Be(1);
    }

    #endregion
}

