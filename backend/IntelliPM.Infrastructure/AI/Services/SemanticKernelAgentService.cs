using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using IntelliPM.Application.DTOs.Agent;
using IntelliPM.Application.Interfaces.Services;
using IntelliPM.Infrastructure.Persistence;
using IntelliPM.Infrastructure.AI.Plugins;
using IntelliPM.Domain.Entities;

namespace IntelliPM.Infrastructure.AI.Services;

/// <summary>
/// Implementation of IAgentService using Semantic Kernel with automatic function calling
/// </summary>
public class SemanticKernelAgentService : IAgentService
{
    private readonly Kernel _kernel;
    private readonly ILogger<SemanticKernelAgentService> _logger;
    private readonly AppDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly int _timeoutSeconds;
    
    public SemanticKernelAgentService(
        Kernel kernel, 
        ILogger<SemanticKernelAgentService> logger,
        AppDbContext dbContext,
        IConfiguration configuration)
    {
        _kernel = kernel;
        _logger = logger;
        _dbContext = dbContext;
        _configuration = configuration;
        
        // Get timeout from configuration (default: 30 seconds)
        _timeoutSeconds = _configuration.GetValue<int>("Agent:TimeoutSeconds", 30);
        
        // Register plugins (tools available to agent)
        _kernel.Plugins.AddFromType<TaskQualityPlugin>();
        
        _logger.LogInformation("🤖 SemanticKernelAgentService initialized with TaskQualityPlugin (Timeout: {Timeout}s)", _timeoutSeconds);
    }
    
    public async System.Threading.Tasks.Task<AgentResponse> ImproveTaskDescriptionAsync(
        string taskDescription, 
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var executionLog = new AgentExecutionLog
        {
            Id = Guid.NewGuid(),
            AgentId = "task-improver",
            UserId = "system", // TODO: Get from HttpContext in controller
            UserInput = taskDescription,
            Status = "Pending",
            ExecutionCostUsd = 0m,
            CreatedAt = DateTime.UtcNow
        };
        
        try
        {
            _logger.LogInformation("🤖 Agent: Starting task improvement for input length {Length}", 
                taskDescription.Length);
            
            // System prompt optimized for llama3.2:3b
            var systemPrompt = @"You are an expert project management assistant powered by llama3.2:3b running locally.
Your job is to help improve task descriptions and make them clear, actionable, and complete.

Available tools:
- AnalyzeTaskQuality: Checks what's missing from a task description
- FormatTask: Structures a task into standard format with smart acceptance criteria

Process:
1. First, call AnalyzeTaskQuality to see what's missing
2. Based on the analysis, suggest improvements
3. Provide a well-structured task description with:
   - Clear title
   - Detailed description
   - Acceptance criteria
   - Definition of Done
   - Estimated story points
   - Priority level

Be concise, friendly, and helpful. Focus on clarity and completeness.
Format your response in markdown with clear sections.";

            var chatHistory = new ChatHistory(systemPrompt);
            chatHistory.AddUserMessage($"Improve this task description: {taskDescription}");
            
            var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
            
            // Enable automatic function calling (agent calls tools automatically)
            var executionSettings = new OpenAIPromptExecutionSettings
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
                MaxTokens = 2048,
                Temperature = 0.7
            };
            
            // Create timeout-enabled cancellation token
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(_timeoutSeconds));
            
            ChatMessageContent response;
            try
            {
                response = await chatCompletionService.GetChatMessageContentAsync(
                    chatHistory,
                    executionSettings,
                    _kernel,
                    timeoutCts.Token
                );
            }
            catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
            {
                // Timeout occurred
                stopwatch.Stop();
                _logger.LogWarning("⏱️ Agent: Request timed out after {Timeout}s", _timeoutSeconds);
                
                executionLog.Status = "Error";
                executionLog.ErrorMessage = $"Request timed out after {_timeoutSeconds} seconds";
                executionLog.ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds;
                executionLog.AgentResponse = string.Empty;
                
                await _dbContext.AgentExecutionLogs.AddAsync(executionLog, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
                
                return new AgentResponse
                {
                    Content = string.Empty,
                    Status = "Error",
                    ErrorMessage = $"Request timed out after {_timeoutSeconds} seconds. Please try again with a shorter description.",
                    RequiresApproval = false,
                    ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds,
                    ToolsCalled = new List<string>(),
                    Timestamp = DateTimeOffset.UtcNow
                };
            }
            
            stopwatch.Stop();
            
            // Track which tools were called
            var toolsCalled = new List<string>();
            foreach (var item in chatHistory)
            {
                if (item.Role == AuthorRole.Tool)
                {
                    var functionName = item.Metadata?.ContainsKey("FunctionName") == true 
                        ? item.Metadata["FunctionName"]?.ToString() 
                        : "unknown";
                    if (!string.IsNullOrEmpty(functionName))
                        toolsCalled.Add(functionName);
                }
            }
            
            executionLog.Status = "Success";
            executionLog.AgentResponse = response.Content ?? "No response generated";
            executionLog.ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds;
            executionLog.ToolsCalled = string.Join(",", toolsCalled);
            
            await _dbContext.AgentExecutionLogs.AddAsync(executionLog, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation(
                "✅ Agent: Task improvement completed in {Ms}ms. Tools called: {Tools}",
                stopwatch.ElapsedMilliseconds,
                executionLog.ToolsCalled ?? "none"
            );
            
            return new AgentResponse
            {
                Content = response.Content ?? "No response generated",
                Status = "Success",
                RequiresApproval = true,
                ExecutionCostUsd = 0m, // llama3.2:3b local = $0
                ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds,
                ToolsCalled = toolsCalled,
                Timestamp = DateTimeOffset.UtcNow,
                Metadata = new Dictionary<string, object>
                {
                    ["AgentType"] = "TaskImprover",
                    ["ModelUsed"] = "llama3.2:3b",
                    ["PluginsUsed"] = "TaskQualityPlugin"
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Agent: Error during task improvement");
            stopwatch.Stop();
            
            executionLog.Status = "Error";
            executionLog.ErrorMessage = ex.Message;
            executionLog.ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds;
            executionLog.AgentResponse = string.Empty;
            
            await _dbContext.AgentExecutionLogs.AddAsync(executionLog, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            return new AgentResponse
            {
                Content = string.Empty,
                Status = "Error",
                ErrorMessage = $"Error: {ex.Message}",
                RequiresApproval = false,
                ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds,
                Timestamp = DateTimeOffset.UtcNow
            };
        }
    }
    
    public async System.Threading.Tasks.Task<AgentResponse> AnalyzeProjectRisksAsync(
        int projectId, 
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var executionLog = new AgentExecutionLog
        {
            Id = Guid.NewGuid(),
            AgentId = "risk-analyzer",
            UserId = "system", // TODO: Get from HttpContext in controller
            UserInput = $"Analyze risks for project {projectId}",
            Status = "Pending",
            ExecutionCostUsd = 0m,
            CreatedAt = DateTime.UtcNow
        };
        
        try
        {
            _logger.LogInformation("🤖 Agent: Starting risk analysis for project {ProjectId}", projectId);
            
            // Fetch project data
            var project = await _dbContext.Projects
                .Include(p => p.UserStories)
                .Include(p => p.Sprints)
                .Include(p => p.Members)
                .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);
            
            if (project == null)
            {
                throw new InvalidOperationException($"Project {projectId} not found");
            }
            
            var tasks = await _dbContext.ProjectTasks
                .Where(t => t.ProjectId == projectId)
                .ToListAsync(cancellationToken);
            
            // Build context for AI
            var projectContext = $@"
**Project: {project.Name}**
Description: {project.Description}
Type: {project.Type}
Status: {project.Status}
Sprint Duration: {project.SprintDurationDays} days

**Statistics:**
- User Stories: {project.UserStories.Count}
- Total Tasks: {tasks.Count}
- Tasks In Progress: {tasks.Count(t => t.Status == "InProgress")}
- Blocked Tasks: {tasks.Count(t => t.Status == "Blocked")}
- Unassigned Tasks: {tasks.Count(t => t.AssigneeId == null)}
- Active Sprints: {project.Sprints.Count(s => s.Status == "Active")}
- Team Members: {project.Members.Count}
";

            var systemPrompt = @"You are an experienced project manager analyzing software projects for risks.
Analyze the project data and identify potential risks, blockers, and areas of concern.
Provide actionable recommendations.";

            var chatHistory = new ChatHistory(systemPrompt);
            chatHistory.AddUserMessage($@"Analyze the following project for risks:

{projectContext}

Provide a structured risk analysis with:
1. **High-Priority Risks** (critical issues)
2. **Medium-Priority Risks** (important concerns)
3. **Low-Priority Risks** (minor issues)

For each risk:
- Description
- Impact (High/Medium/Low)
- Likelihood (High/Medium/Low)
- Mitigation strategy

End with:
- **Overall Project Health**: Green/Yellow/Red with explanation
- **Top 3 Recommendations**: Actionable items");
            
            var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
            
            var executionSettings = new OpenAIPromptExecutionSettings
            {
                MaxTokens = 3000,
                Temperature = 0.5 // Lower temperature for more consistent analysis
            };
            
            var response = await chatCompletionService.GetChatMessageContentAsync(
                chatHistory,
                executionSettings,
                _kernel,
                cancellationToken
            );
            
            stopwatch.Stop();
            
            executionLog.Status = "Success";
            executionLog.AgentResponse = response.Content ?? "No analysis generated";
            executionLog.ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds;
            
            await _dbContext.AgentExecutionLogs.AddAsync(executionLog, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation(
                "✅ Agent: Risk analysis completed in {Ms}ms for project {ProjectId}",
                stopwatch.ElapsedMilliseconds,
                projectId
            );
            
            return new AgentResponse
            {
                Content = response.Content ?? "No analysis generated",
                Status = "Success",
                RequiresApproval = false,
                ExecutionCostUsd = 0m,
                ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds,
                ToolsCalled = new List<string> { "LLM-Analysis" },
                Timestamp = DateTimeOffset.UtcNow,
                Metadata = new Dictionary<string, object>
                {
                    ["ProjectId"] = projectId,
                    ["ProjectName"] = project.Name,
                    ["TotalTasks"] = tasks.Count,
                    ["BlockedTasks"] = tasks.Count(t => t.Status == "Blocked"),
                    ["AgentType"] = "RiskAnalyzer"
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Agent: Error during risk analysis for project {ProjectId}", projectId);
            stopwatch.Stop();
            
            executionLog.Status = "Error";
            executionLog.ErrorMessage = ex.Message;
            executionLog.ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds;
            
            await _dbContext.AgentExecutionLogs.AddAsync(executionLog, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            return new AgentResponse
            {
                Content = string.Empty,
                Status = "Error",
                ErrorMessage = $"Error: {ex.Message}",
                ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds,
                Timestamp = DateTimeOffset.UtcNow
            };
        }
    }
    
    public async System.Threading.Tasks.Task<AgentResponse> SuggestSprintPlanAsync(
        int projectId,
        int sprintId,
        CancellationToken cancellationToken = default)
    {
        // TODO: Implement sprint planning suggestions
        await System.Threading.Tasks.Task.CompletedTask;
        
        return new AgentResponse
        {
            Content = "Sprint planning suggestion not yet implemented. Coming in Phase 2.",
            Status = "Error",
            ErrorMessage = "Not implemented",
            Timestamp = DateTimeOffset.UtcNow
        };
    }
    
    public async System.Threading.Tasks.Task<AgentResponse> AnalyzeTaskDependenciesAsync(
        int projectId,
        CancellationToken cancellationToken = default)
    {
        // TODO: Implement task dependency analysis
        await System.Threading.Tasks.Task.CompletedTask;
        
        return new AgentResponse
        {
            Content = "Task dependency analysis not yet implemented. Coming in Phase 2.",
            Status = "Error",
            ErrorMessage = "Not implemented",
            Timestamp = DateTimeOffset.UtcNow
        };
    }
    
    public async System.Threading.Tasks.Task<AgentResponse> GenerateSprintRetrospectiveAsync(
        int sprintId,
        CancellationToken cancellationToken = default)
    {
        // TODO: Implement sprint retrospective generation
        await System.Threading.Tasks.Task.CompletedTask;
        
        return new AgentResponse
        {
            Content = "Sprint retrospective generation not yet implemented. Coming in Phase 2.",
            Status = "Error",
            ErrorMessage = "Not implemented",
            Timestamp = DateTimeOffset.UtcNow
        };
    }
}

