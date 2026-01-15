using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using IntelliPM.Domain.Entities;
using IntelliPM.Domain.Enums;
using IntelliPM.Infrastructure.Identity;
using IntelliPM.Infrastructure.Persistence;
using IntelliPM.Tests.Infrastructure.TestAuthentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using Xunit;

namespace IntelliPM.Tests.Integration.API;

/// <summary>
/// Integration tests to verify that command validation returns 400 Bad Request
/// instead of 500 Internal Server Error for invalid input.
/// </summary>
public class CommandValidationIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly IServiceScope _scope;
    private readonly AppDbContext _context;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public CommandValidationIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, configBuilder) =>
            {
                configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "Jwt:SecretKey", "YourSecureSecretKeyOfAt32CharactersMinimumForTesting!" },
                    { "Jwt:Issuer", "TestIssuer" },
                    { "Jwt:Audience", "TestAudience" }
                });
            });

            builder.ConfigureServices(services =>
            {
                // Remove real DbContext
                var dbDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (dbDescriptor != null)
                {
                    services.Remove(dbDescriptor);
                }

                // Add InMemory database
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase($"ValidationTestDb_{Guid.NewGuid()}");
                });

                // Add test authentication
                services.AddTestAuthentication();
            });
        });

        _client = _factory.CreateClient();
        _scope = _factory.Services.CreateScope();
        _context = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        SeedTestData();
    }

    private void SeedTestData()
    {
        _context.Database.EnsureCreated();

        var org = new Organization
        {
            Id = 1,
            Name = "Test Organization",
            Code = "test-org",
            CreatedAt = DateTimeOffset.UtcNow
        };
        _context.Organizations.Add(org);

        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            GlobalRole = GlobalRole.User,
            OrganizationId = 1,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            PasswordHash = "hash",
            PasswordSalt = "salt"
        };
        _context.Users.Add(user);

        var project = new Project
        {
            Id = 1,
            Name = "Test Project",
            Description = "Test Description",
            Type = "Scrum",
            Status = "Active",
            OwnerId = 1,
            OrganizationId = 1,
            SprintDurationDays = 14,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _context.Projects.Add(project);

        _context.SaveChanges();
    }

    #region CreateProjectCommand Validation Tests

    [Fact]
    public async Task CreateProject_WithEmptyName_Returns400BadRequest()
    {
        // Arrange
        var token = GenerateJwtToken(1, "testuser", "test@example.com");
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new
        {
            Name = "",
            Description = "Test Description",
            Type = "Scrum",
            SprintDurationDays = 14
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/Projects", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Project name is required");
    }

    [Fact]
    public async Task CreateProject_WithNameTooShort_Returns400BadRequest()
    {
        // Arrange
        var token = GenerateJwtToken(1, "testuser", "test@example.com");
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new
        {
            Name = "ab", // Too short
            Description = "Test Description",
            Type = "Scrum",
            SprintDurationDays = 14
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/Projects", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Project name must be at least 3 characters");
    }

    [Fact]
    public async Task CreateProject_WithInvalidType_Returns400BadRequest()
    {
        // Arrange
        var token = GenerateJwtToken(1, "testuser", "test@example.com");
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new
        {
            Name = "Test Project",
            Description = "Test Description",
            Type = "InvalidType",
            SprintDurationDays = 14
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/Projects", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Project type");
    }

    [Fact]
    public async Task CreateProject_WithInvalidSprintDuration_Returns400BadRequest()
    {
        // Arrange
        var token = GenerateJwtToken(1, "testuser", "test@example.com");
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new
        {
            Name = "Test Project",
            Description = "Test Description",
            Type = "Scrum",
            SprintDurationDays = 31 // Invalid
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/Projects", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Sprint duration");
    }

    #endregion

    #region CreateTaskCommand Validation Tests

    [Fact]
    public async Task CreateTask_WithEmptyTitle_Returns400BadRequest()
    {
        // Arrange
        var token = GenerateJwtToken(1, "testuser", "test@example.com");
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new
        {
            Title = "",
            Description = "Test Description",
            ProjectId = 1,
            Priority = "High"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/Tasks", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Task title is required");
    }

    [Fact]
    public async Task CreateTask_WithEmptyDescription_Returns400BadRequest()
    {
        // Arrange
        var token = GenerateJwtToken(1, "testuser", "test@example.com");
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new
        {
            Title = "Test Task",
            Description = "",
            ProjectId = 1,
            Priority = "High"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/Tasks", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Task description is required");
    }

    [Fact]
    public async Task CreateTask_WithInvalidPriority_Returns400BadRequest()
    {
        // Arrange
        var token = GenerateJwtToken(1, "testuser", "test@example.com");
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new
        {
            Title = "Test Task",
            Description = "Test Description",
            ProjectId = 1,
            Priority = "InvalidPriority"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/Tasks", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Priority");
    }

    [Fact]
    public async Task CreateTask_WithInvalidProjectId_Returns400BadRequest()
    {
        // Arrange
        var token = GenerateJwtToken(1, "testuser", "test@example.com");
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new
        {
            Title = "Test Task",
            Description = "Test Description",
            ProjectId = 0, // Invalid
            Priority = "High"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/Tasks", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Project ID");
    }

    [Fact]
    public async Task CreateTask_WithInvalidStoryPoints_Returns400BadRequest()
    {
        // Arrange
        var token = GenerateJwtToken(1, "testuser", "test@example.com");
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new
        {
            Title = "Test Task",
            Description = "Test Description",
            ProjectId = 1,
            Priority = "High",
            StoryPoints = 101 // Invalid
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/Tasks", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Story points");
    }

    #endregion

    #region UpdateTaskCommand Validation Tests

    [Fact]
    public async Task UpdateTask_WithInvalidTaskId_Returns400BadRequest()
    {
        // Arrange
        var token = GenerateJwtToken(1, "testuser", "test@example.com");
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new
        {
            Title = "Updated Title"
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/v1/Tasks/0", request); // Invalid task ID

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Task ID");
    }

    [Fact]
    public async Task UpdateTask_WithInvalidPriority_Returns400BadRequest()
    {
        // Arrange
        var token = GenerateJwtToken(1, "testuser", "test@example.com");
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new
        {
            Priority = "InvalidPriority"
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/v1/Tasks/1", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Priority");
    }

    #endregion

    #region ChangeTaskStatusCommand Validation Tests

    [Fact]
    public async Task ChangeTaskStatus_WithInvalidStatus_Returns400BadRequest()
    {
        // Arrange
        var token = GenerateJwtToken(1, "testuser", "test@example.com");
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new
        {
            NewStatus = "InvalidStatus"
        };

        // Act
        var response = await _client.PatchAsJsonAsync("/api/v1/Tasks/1/status", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Status");
    }

    [Fact]
    public async Task ChangeTaskStatus_WithEmptyStatus_Returns400BadRequest()
    {
        // Arrange
        var token = GenerateJwtToken(1, "testuser", "test@example.com");
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new
        {
            NewStatus = ""
        };

        // Act
        var response = await _client.PatchAsJsonAsync("/api/v1/Tasks/1/status", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("New status is required");
    }

    #endregion

    #region AssignTaskCommand Validation Tests

    [Fact]
    public async Task AssignTask_WithInvalidTaskId_Returns400BadRequest()
    {
        // Arrange
        var token = GenerateJwtToken(1, "testuser", "test@example.com");
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new
        {
            AssigneeId = 2
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/Tasks/0/assign", request); // Invalid task ID

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Task ID");
    }

    #endregion

    #region UpdateProjectCommand Validation Tests

    [Fact]
    public async Task UpdateProject_WithInvalidProjectId_Returns400BadRequest()
    {
        // Arrange
        var token = GenerateJwtToken(1, "testuser", "test@example.com");
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new
        {
            Name = "Updated Name"
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/v1/Projects/0", request); // Invalid project ID

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Project ID");
    }

    [Fact]
    public async Task UpdateProject_WithInvalidSprintDuration_Returns400BadRequest()
    {
        // Arrange
        var token = GenerateJwtToken(1, "testuser", "test@example.com");
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new
        {
            SprintDurationDays = 31 // Invalid
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/v1/Projects/1", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Sprint duration");
    }

    #endregion

    private string GenerateJwtToken(int userId, string username, string email)
    {
        var configuration = _factory.Services.GetRequiredService<IConfiguration>();
        var tokenService = new JwtTokenService(configuration);
        return tokenService.GenerateAccessToken(userId, username, email, new List<string>());
    }

    public void Dispose()
    {
        _scope?.Dispose();
        _client?.Dispose();
        _context?.Dispose();
    }
}
