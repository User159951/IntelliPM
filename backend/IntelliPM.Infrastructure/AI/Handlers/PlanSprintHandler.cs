using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using IntelliPM.Application.Agents.Commands;
using IntelliPM.Application.DTOs.Agent;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Services;
using IntelliPM.Infrastructure.AI.Plugins;
using IntelliPM.Infrastructure.AI.Helpers;
using IntelliPM.Infrastructure.Persistence;

namespace IntelliPM.Infrastructure.AI.Handlers;

/// <summary>
/// Uses Semantic Kernel + SprintPlanningPlugin to propose sprint plans and task assignments.
/// </summary>
public class PlanSprintHandler : IRequestHandler<PlanSprintCommand, AgentResponse>
{
    private readonly Kernel _kernel;
    private readonly ILogger<PlanSprintHandler> _logger;
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICorrelationIdService _correlationIdService;
    private readonly IAIAvailabilityService _availabilityService;

    public PlanSprintHandler(
        Kernel kernel,
        ILogger<PlanSprintHandler> logger,
        AppDbContext context,
        ICurrentUserService currentUserService,
        ICorrelationIdService correlationIdService,
        IAIAvailabilityService availabilityService)
    {
        _kernel = kernel;
        _logger = logger;
        _context = context;
        _currentUserService = currentUserService;
        _correlationIdService = correlationIdService;
        _availabilityService = availabilityService;
    }

    public async Task<AgentResponse> Handle(PlanSprintCommand request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Check AI quota before execution
            var organizationId = _currentUserService.GetOrganizationId();
            if (organizationId > 0)
            {
                await _availabilityService.CheckQuotaAsync(organizationId, "Requests", cancellationToken);
            }

            var plugin = new SprintPlanningPlugin(_context);
            _kernel.Plugins.AddFromObject(plugin, "SprintPlanningPlugin");

            var chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage(@"You are an experienced Scrum master helping plan a sprint.
Use tools to understand:
- Backlog tasks
- Team member capacity
- Sprint capacity

Then propose:
1. Which tasks to include in the sprint (with reasoning)
2. Proposed assignees based on capacity
3. Risks or trade-offs of this plan
4. A short summary for the planning meeting.

Return clear bullet points and sections. Treat this as a proposal that still requires human approval.");

            chatHistory.AddUserMessage(
                $"Plan sprint with ID {request.SprintId}. Use available tools to see backlog, team capacity and sprint capacity.");

            var executionSettings = new OpenAIPromptExecutionSettings
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
                Temperature = 0.6,
                MaxTokens = 2200
            };

            var chatService = _kernel.GetRequiredService<IChatCompletionService>();

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(60));

            var response = await chatService.GetChatMessageContentAsync(
                chatHistory,
                executionSettings,
                _kernel,
                cts.Token
            );

            stopwatch.Stop();

            // Extract token usage from response
            var (promptTokens, completionTokens, totalTokens) = TokenUsageHelper.ExtractTokenUsage(response);

            var userId = _currentUserService.GetUserId();
            var correlationId = _correlationIdService.GetCorrelationId();
            var log = new Domain.Entities.AgentExecutionLog
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId > 0 ? organizationId : throw new InvalidOperationException("OrganizationId is required for AgentExecutionLog"),
                AgentId = "sprint-planner",
                UserId = userId > 0 ? userId.ToString() : "system",
                UserInput = $"Plan sprint {request.SprintId}",
                AgentResponse = response.Content ?? "No sprint plan generated",
                Status = "Success",
                ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds,
                ExecutionCostUsd = 0.0m,
                CreatedAt = DateTime.UtcNow
            };

            _context.AgentExecutionLogs.Add(log);
            await _context.SaveChangesAsync(cancellationToken);

            return new AgentResponse
            {
                Content = response.Content ?? "No sprint plan generated",
                Status = "Success",
                ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds,
                ExecutionCostUsd = 0.0m,
                // Sprint planning suggestions should be approved before execution
                RequiresApproval = true,
                Timestamp = DateTimeOffset.UtcNow,
                PromptTokens = promptTokens,
                CompletionTokens = completionTokens,
                Model = "llama3.2:3b"
            };
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            _logger.LogWarning("Sprint planning timed out for sprint {SprintId}", request.SprintId);

            return new AgentResponse
            {
                Content = "Sprint planning timed out after 60 seconds",
                Status = "Error",
                ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds,
                Timestamp = DateTimeOffset.UtcNow
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error planning sprint {SprintId}", request.SprintId);

            return new AgentResponse
            {
                Content = $"Error: {ex.Message}",
                Status = "Error",
                ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds,
                Timestamp = DateTimeOffset.UtcNow
            };
        }
    }
}

