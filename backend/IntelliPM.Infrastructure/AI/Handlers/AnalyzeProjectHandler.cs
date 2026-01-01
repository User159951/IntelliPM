using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using IntelliPM.Application.Agents.Commands;
using IntelliPM.Application.DTOs.Agent;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Infrastructure.AI.Plugins;
using IntelliPM.Infrastructure.Persistence;

namespace IntelliPM.Infrastructure.AI.Handlers;

/// <summary>
/// Uses Semantic Kernel + ProjectInsightPlugin to generate an executive summary
/// of a project's health in natural language.
/// </summary>
public class AnalyzeProjectHandler : IRequestHandler<AnalyzeProjectCommand, AgentResponse>
{
    private readonly Kernel _kernel;
    private readonly ILogger<AnalyzeProjectHandler> _logger;
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public AnalyzeProjectHandler(
        Kernel kernel,
        ILogger<AnalyzeProjectHandler> logger,
        AppDbContext context,
        ICurrentUserService currentUserService)
    {
        _kernel = kernel;
        _logger = logger;
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<AgentResponse> Handle(AnalyzeProjectCommand request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Register plugin with current context (tools for the agent)
            var plugin = new ProjectInsightPlugin(_context);
            _kernel.Plugins.AddFromObject(plugin, "ProjectInsightPlugin");

            var chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage(@"You are a project management analyst. Analyze project data and generate a concise executive summary.

Include:
1. Overall health (on track / at risk / blocked)
2. Progress (% complete, breakdown by status)
3. Key blockers requiring attention
4. Actionable recommendations

Be specific and use bullet points.");

            chatHistory.AddUserMessage($"Analyze the status of project ID {request.ProjectId}. Use available tools to gather data.");

            var executionSettings = new OpenAIPromptExecutionSettings
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
                Temperature = 0.7,
                MaxTokens = 1500
            };

            var chatService = _kernel.GetRequiredService<IChatCompletionService>();

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(45));

            var response = await chatService.GetChatMessageContentAsync(
                chatHistory,
                executionSettings,
                _kernel,
                cts.Token
            );

            stopwatch.Stop();

            // Log agent execution
            var userId = _currentUserService.GetUserId();
            var log = new Domain.Entities.AgentExecutionLog
            {
                Id = Guid.NewGuid(),
                AgentId = "project-insight",
                UserId = userId > 0 ? userId.ToString() : "system",
                UserInput = $"Analyze project {request.ProjectId}",
                AgentResponse = response.Content ?? "No summary generated",
                Status = "Success",
                ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds,
                ExecutionCostUsd = 0.0m,
                CreatedAt = DateTime.UtcNow
            };

            _context.AgentExecutionLogs.Add(log);

            // Also create an alert summarizing that a project insight is available
            _context.Alerts.Add(new Domain.Entities.Alert
            {
                ProjectId = request.ProjectId,
                Type = "ProjectInsight",
                Severity = "Info",
                Title = "Project insight available",
                Message = "An updated AI project insight summary is available.",
                CreatedAt = DateTimeOffset.UtcNow
            });

            await _context.SaveChangesAsync(cancellationToken);

            return new AgentResponse
            {
                Content = response.Content ?? "No summary generated",
                Status = "Success",
                ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds,
                ExecutionCostUsd = 0.0m,
                RequiresApproval = false,
                Timestamp = DateTimeOffset.UtcNow
            };
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            _logger.LogWarning("Project analysis timed out for project {ProjectId}", request.ProjectId);

            return new AgentResponse
            {
                Content = "Analysis timed out after 45 seconds",
                Status = "Error",
                ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds,
                Timestamp = DateTimeOffset.UtcNow
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error analyzing project {ProjectId}", request.ProjectId);

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

