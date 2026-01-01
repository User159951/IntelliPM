using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Moq;
using IntelliPM.Infrastructure.Persistence;
using IntelliPM.Infrastructure.VectorStore;
using IntelliPM.Application.Common.Interfaces;

namespace IntelliPM.Tests.Integration.Handlers;

/// <summary>
/// WebApplicationFactory for AI agent handler integration tests.
/// Mocks IChatCompletionService to avoid dependency on Ollama.
/// </summary>
public class AIAgentHandlerTestFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Configure content root
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        builder.UseContentRoot(tempDir);
        
        // Set environment variables
        Environment.SetEnvironmentVariable("Jwt__SecretKey", "YourSecureSecretKeyOfAt32CharactersMinimumForTesting!");
        Environment.SetEnvironmentVariable("Jwt__Issuer", "TestIssuer");
        Environment.SetEnvironmentVariable("Jwt__Audience", "TestAudience");
        
        builder.ConfigureAppConfiguration((context, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Jwt:SecretKey", "YourSecureSecretKeyOfAt32CharactersMinimumForTesting!" },
                { "Jwt:Issuer", "TestIssuer" },
                { "Jwt:Audience", "TestAudience" },
                { "Ollama:Endpoint", "http://localhost:11434" },
                { "Ollama:Model", "llama3.2:3b" }
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
                options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}");
            });

            // Remove VectorDbContext
            var vectorDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<VectorDbContext>));
            if (vectorDescriptor != null)
            {
                services.Remove(vectorDescriptor);
            }
            
            services.AddDbContext<VectorDbContext>(options =>
            {
                options.UseInMemoryDatabase($"TestVectorDb_{Guid.NewGuid()}");
            });

            // Remove existing Kernel registration
            var kernelDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(Kernel));
            if (kernelDescriptor != null)
            {
                services.Remove(kernelDescriptor);
            }

            // Remove existing IChatCompletionService registrations
            var chatCompletionDescriptors = services
                .Where(d => d.ServiceType == typeof(IChatCompletionService))
                .ToList();
            foreach (var descriptor in chatCompletionDescriptors)
            {
                services.Remove(descriptor);
            }

            // Create a mock IChatCompletionService using Moq
            var mockChatCompletion = new Mock<IChatCompletionService>();
            
            // Setup default response - returns a mock response with content
            mockChatCompletion
                .Setup(x => x.GetChatMessageContentAsync(
                    It.IsAny<ChatHistory>(),
                    It.IsAny<PromptExecutionSettings>(),
                    It.IsAny<Kernel>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((ChatHistory history, PromptExecutionSettings settings, Kernel kernel, CancellationToken ct) =>
                {
                    // Extract the user message to generate context-aware response
                    var userMessage = history.LastOrDefault(m => m.Role == AuthorRole.User);
                    var responseContent = GenerateMockResponse(userMessage?.Content ?? "");
                    
                    // Create ChatMessageContent using TextContent (subclass of ChatMessageContent)
                    var response = new ChatMessageContent(
                        AuthorRole.Assistant,
                        responseContent);
                    return response;
                });

            // Register mock IChatCompletionService
            services.AddSingleton<IChatCompletionService>(mockChatCompletion.Object);

            // Register Kernel with mock service
            services.AddSingleton<Kernel>(serviceProvider =>
            {
                var kernelBuilder = Kernel.CreateBuilder();
                kernelBuilder.Services.AddSingleton(serviceProvider.GetRequiredService<IChatCompletionService>());
                return kernelBuilder.Build();
            });

            // Ensure database is created
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        });

        builder.UseEnvironment("Testing");
    }

    private static string GenerateMockResponse(string userMessage)
    {
        // Generate context-aware mock responses based on the user message
        if (userMessage.Contains("project", StringComparison.OrdinalIgnoreCase))
        {
            return @"**Executive Summary**

**Overall Health:** On Track

**Progress:**
- 75% of tasks completed
- 3 sprints completed successfully
- Velocity: 28 story points per sprint

**Key Blockers:**
- Dependency on external API integration (High priority)
- Resource allocation for QA team (Medium priority)

**Recommendations:**
1. Escalate external API dependency to stakeholder
2. Allocate additional QA resources for next sprint
3. Consider extending sprint deadline if blocker persists";
        }
        else if (userMessage.Contains("risk", StringComparison.OrdinalIgnoreCase))
        {
            return @"**Risk Analysis**

**High-Priority Risks:**
1. External API dependency delay (Impact: High, Likelihood: Medium)
   - Mitigation: Establish backup integration plan

2. Resource availability (Impact: Medium, Likelihood: High)
   - Mitigation: Cross-train team members

**Medium-Priority Risks:**
1. Scope creep in current sprint
   - Mitigation: Enforce strict sprint scope boundaries

**Recommended Actions:**
1. Schedule risk review meeting
2. Update risk register
3. Assign risk owners";
        }
        else if (userMessage.Contains("sprint", StringComparison.OrdinalIgnoreCase) && userMessage.Contains("plan", StringComparison.OrdinalIgnoreCase))
        {
            return @"**Sprint Plan Proposal**

**Tasks to Include:**
1. User Authentication Module (8 story points) - Assign to Developer A
2. Payment Integration (5 story points) - Assign to Developer B
3. API Documentation (3 story points) - Assign to Developer C

**Total Capacity:** 16 story points (matches sprint capacity)

**Risks:**
- Payment integration may require external API access
- Authentication module is critical path

**Summary:**
Proposed sprint plan balances team capacity with priority tasks. All tasks align with sprint goal.";
        }
        
        return "Mock AI response for testing purposes.";
    }
}

