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
/// Uses Semantic Kernel + RiskDetectionPlugin to surface project risks and recommendations.
/// </summary>
public class DetectRisksHandler : IRequestHandler<DetectRisksCommand, AgentResponse>
{
    private readonly Kernel _kernel;
    private readonly ILogger<DetectRisksHandler> _logger;
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DetectRisksHandler(
        Kernel kernel,
        ILogger<DetectRisksHandler> logger,
        AppDbContext context,
        ICurrentUserService currentUserService)
    {
        _kernel = kernel;
        _logger = logger;
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<AgentResponse> Handle(DetectRisksCommand request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var plugin = new RiskDetectionPlugin(_context);
            _kernel.Plugins.AddFromObject(plugin, "RiskDetectionPlugin");

            var chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage(@"You are a proactive risk management assistant.
Use the available tools to analyze project risks, prioritise them, and provide clear recommendations.

Structure your response as:
1. High-priority risks (with impact and likelihood)
2. Medium-priority risks
3. Low-priority observations
4. Recommended next actions (bullet list).");

            chatHistory.AddUserMessage(
                $"Detect and summarize risks for project ID {request.ProjectId}. Use tools to inspect tasks and sprints.");

            var executionSettings = new OpenAIPromptExecutionSettings
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
                Temperature = 0.4,
                MaxTokens = 2000
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

            var userId = _currentUserService.GetUserId();
            var log = new Domain.Entities.AgentExecutionLog
            {
                Id = Guid.NewGuid(),
                AgentId = "risk-detector",
                UserId = userId > 0 ? userId.ToString() : "system",
                UserInput = $"Detect risks for project {request.ProjectId}",
                AgentResponse = response.Content ?? "No risk analysis generated",
                Status = "Success",
                ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds,
                ExecutionCostUsd = 0.0m,
                CreatedAt = DateTime.UtcNow
            };

            _context.AgentExecutionLogs.Add(log);

            // Also create an alert summarizing that a risk analysis is available
            _context.Alerts.Add(new Domain.Entities.Alert
            {
                ProjectId = request.ProjectId,
                Type = "RiskAnalysis",
                Severity = "Warning",
                Title = "Risk analysis completed",
                Message = "An updated AI risk analysis is available for this project.",
                CreatedAt = DateTimeOffset.UtcNow
            });

            await _context.SaveChangesAsync(cancellationToken);

            return new AgentResponse
            {
                Content = response.Content ?? "No risk analysis generated",
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
            _logger.LogWarning("Risk detection timed out for project {ProjectId}", request.ProjectId);

            return new AgentResponse
            {
                Content = "Risk detection timed out after 45 seconds",
                Status = "Error",
                ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds,
                Timestamp = DateTimeOffset.UtcNow
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error detecting risks for project {ProjectId}", request.ProjectId);

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

