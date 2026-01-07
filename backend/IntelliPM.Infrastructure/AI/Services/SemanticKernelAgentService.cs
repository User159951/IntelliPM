using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using IntelliPM.Application.DTOs.Agent;
using IntelliPM.Application.Interfaces.Services;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Application.Services;
using IntelliPM.Domain.Constants;
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
    private readonly ICurrentUserService _currentUserService;
    private readonly IAIAvailabilityService _availabilityService;
    private readonly ICorrelationIdService _correlationIdService;
    private readonly int _timeoutSeconds;
    
    public SemanticKernelAgentService(
        Kernel kernel, 
        ILogger<SemanticKernelAgentService> logger,
        AppDbContext dbContext,
        IConfiguration configuration,
        ICurrentUserService currentUserService,
        IAIAvailabilityService availabilityService,
        ICorrelationIdService correlationIdService)
    {
        _kernel = kernel;
        _logger = logger;
        _dbContext = dbContext;
        _configuration = configuration;
        _currentUserService = currentUserService;
        _availabilityService = availabilityService;
        _correlationIdService = correlationIdService;
        
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
        var userId = _currentUserService.GetUserId();
        var organizationId = _currentUserService.GetOrganizationId();
        var correlationId = _correlationIdService.GetCorrelationId();
        var executionLog = new AgentExecutionLog
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId > 0 ? organizationId : throw new InvalidOperationException("OrganizationId is required for AgentExecutionLog"),
            AgentId = "task-improver",
            UserId = userId > 0 ? userId.ToString() : "system",
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
        var userId = _currentUserService.GetUserId();
        var organizationId = _currentUserService.GetOrganizationId();
        var correlationId = _correlationIdService.GetCorrelationId();
        var executionLog = new AgentExecutionLog
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId > 0 ? organizationId : throw new InvalidOperationException("OrganizationId is required for AgentExecutionLog"),
            AgentId = "risk-analyzer",
            UserId = userId > 0 ? userId.ToString() : "system",
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
        var stopwatch = Stopwatch.StartNew();
        var userId = _currentUserService.GetUserId();
        var organizationId = _currentUserService.GetOrganizationId();
        var correlationId = _correlationIdService.GetCorrelationId();
        
        var executionLog = new AgentExecutionLog
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId > 0 ? organizationId : throw new InvalidOperationException("OrganizationId is required for AgentExecutionLog"),
            AgentId = "sprint-planner",
            UserId = userId > 0 ? userId.ToString() : "system",
            UserInput = $"Suggest sprint plan for sprint {sprintId} in project {projectId}",
            Status = "Pending",
            ExecutionCostUsd = 0m,
            CreatedAt = DateTime.UtcNow
        };
        
        try
        {
            // 1. Vérifier si AI est activée et quota disponible
            if (organizationId > 0)
            {
                await _availabilityService.CheckQuotaAsync(organizationId, "Requests", cancellationToken);
            }
            
            // 2. Récupérer le sprint
            var sprint = await _dbContext.Sprints
                .AsNoTracking()
                .Include(s => s.Project)
                .FirstOrDefaultAsync(s => s.Id == sprintId, cancellationToken);
            
            if (sprint == null)
            {
                throw new NotFoundException($"Sprint {sprintId} not found");
            }
            
            if (sprint.ProjectId != projectId)
            {
                throw new ValidationException($"Sprint {sprintId} does not belong to project {projectId}");
            }
            
            _logger.LogInformation("🤖 Agent: Starting sprint plan suggestion for Sprint {SprintId} (Project {ProjectId})", sprintId, projectId);
            
            // 3. Enregistrer le plugin SprintPlanningPlugin
            var plugin = new SprintPlanningPlugin(_dbContext);
            _kernel.Plugins.AddFromObject(plugin, "SprintPlanningPlugin");
            
            // 4. Construire le prompt avec contexte
            var systemPrompt = @"You are an experienced Scrum master helping plan a sprint.
Use the available tools to understand:
- Backlog tasks (GetBacklogTasks)
- Team member capacity (GetTeamCapacity)
- Sprint capacity (GetSprintCapacity)

Based on the data, suggest which backlog items should be included in this sprint.
Consider:
1. Task priority and dependencies
2. Team member skills and availability
3. Story points vs remaining capacity
4. Risk of overcommitment

Return a clear, structured response with:
1. **Suggested Tasks**: List tasks to include (task ID, title, suggested assignee, story points)
2. **Total Story Points**: Sum of story points
3. **Capacity Utilization**: Percentage (0-150%)
4. **Risks**: Any concerns or warnings
5. **Reasoning**: Brief explanation of the plan

Format your response in clear sections with markdown. Be practical and consider team capacity limits.";
            
            var startDateStr = sprint.StartDate?.ToString("yyyy-MM-dd") ?? "Not set";
            var endDateStr = sprint.EndDate?.ToString("yyyy-MM-dd") ?? "Not set";
            
            var userPrompt = $@"Plan sprint {sprint.Number} for project {projectId}:
- Sprint: Sprint {sprint.Number}
- Goal: {sprint.Goal}
- Duration: {startDateStr} to {endDateStr}
- Status: {sprint.Status}

Use the tools to analyze backlog, team capacity, and sprint capacity, then suggest an optimal plan.";
            
            var chatHistory = new ChatHistory(systemPrompt);
            chatHistory.AddUserMessage(userPrompt);
            
            var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
            
            // 5. Exécuter avec Semantic Kernel
            var executionSettings = new OpenAIPromptExecutionSettings
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
                MaxTokens = 2000,
                Temperature = 0.3, // Low temp for deterministic planning
            };
            
            // Create timeout-enabled cancellation token (60 seconds max for sprint planning)
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(60));
            
            ChatMessageContent response;
            try
            {
                response = await chatCompletionService.GetChatMessageContentAsync(
                    chatHistory,
                    executionSettings,
                    _kernel,
                    timeoutCts.Token);
            }
            catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested)
            {
                _logger.LogWarning("⏱️ Agent: Sprint planning request timed out after 60s for sprint {SprintId}", sprintId);
                throw new TimeoutException("Sprint planning timed out after 60 seconds");
            }
            
            stopwatch.Stop();
            var responseContent = response.Content ?? "No sprint plan generated";
            
            _logger.LogInformation("🤖 Agent: Sprint plan suggested for Sprint {SprintId} in {ElapsedMs}ms", 
                sprintId, stopwatch.ElapsedMilliseconds);
            
            // 6. Logger l'exécution
            executionLog.Status = "Success";
            executionLog.AgentResponse = responseContent;
            executionLog.ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds;
            
            await _dbContext.AgentExecutionLogs.AddAsync(executionLog, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            return new AgentResponse
            {
                Content = responseContent,
                Status = "Success",
                RequiresApproval = true, // Sprint planning suggestions require approval
                ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds,
                ExecutionCostUsd = 0.0m,
                Timestamp = DateTimeOffset.UtcNow
            };
        }
        catch (NotFoundException)
        {
            throw; // Re-throw NotFoundException as-is
        }
        catch (ValidationException)
        {
            throw; // Re-throw ValidationException as-is
        }
        catch (TimeoutException)
        {
            stopwatch.Stop();
            _logger.LogWarning("⏱️ Agent: Sprint planning timed out for sprint {SprintId}", sprintId);
            
            executionLog.Status = "Error";
            executionLog.ErrorMessage = "Sprint planning timed out after 60 seconds";
            executionLog.ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds;
            
            await _dbContext.AgentExecutionLogs.AddAsync(executionLog, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            return new AgentResponse
            {
                Content = "Sprint planning timed out after 60 seconds. Please try again with a smaller backlog or reduce the scope.",
                Status = "Error",
                ErrorMessage = "Timeout",
                ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds,
                Timestamp = DateTimeOffset.UtcNow
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "❌ Agent: Error during sprint planning for sprint {SprintId}", sprintId);
            
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
    
    public async System.Threading.Tasks.Task<AgentResponse> AnalyzeTaskDependenciesAsync(
        int projectId,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var userId = _currentUserService.GetUserId();
        var organizationId = _currentUserService.GetOrganizationId();
        var correlationId = _correlationIdService.GetCorrelationId();
        
        var executionLog = new AgentExecutionLog
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId > 0 ? organizationId : throw new InvalidOperationException("OrganizationId is required for AgentExecutionLog"),
            AgentId = "task-dependency-analyzer",
            UserId = userId > 0 ? userId.ToString() : "system",
            UserInput = $"Analyze task dependencies for project {projectId}",
            Status = "Pending",
            ExecutionCostUsd = 0m,
            CreatedAt = DateTime.UtcNow
        };
        
        try
        {
            // 1. Vérifier si AI est activée et quota disponible
            if (organizationId > 0)
            {
                await _availabilityService.CheckQuotaAsync(organizationId, "Requests", cancellationToken);
            }
            
            // 2. Vérifier que le projet existe
            var project = await _dbContext.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);
            
            if (project == null)
            {
                throw new NotFoundException($"Project {projectId} not found");
            }
            
            _logger.LogInformation("🤖 Agent: Starting task dependency analysis for Project {ProjectId}", projectId);
            
            // 3. Enregistrer le plugin TaskDependencyPlugin
            var plugin = new TaskDependencyPlugin(_dbContext);
            _kernel.Plugins.AddFromObject(plugin, "TaskDependencyPlugin");
            
            // 4. Construire le prompt avec contexte
            var systemPrompt = @"You are an expert project manager analyzing task dependencies.
Use the available tools to understand:
- All project tasks (GetProjectTasks)
- Task dependencies (GetTaskWithDependencies)
- Circular dependencies (DetectCircularDependencies)
- Critical path (CalculateCriticalPath)

Based on the data, analyze dependencies and provide insights on:
1. Direct dependencies (which tasks depend on which)
2. Circular dependencies (if any)
3. Critical path analysis
4. Bottleneck risks
5. Recommendations for dependency management

Return a clear, structured response with:
1. **Dependency Overview**: Summary of dependency structure
2. **Circular Dependencies**: List any cycles found
3. **Critical Path**: Tasks on the critical path
4. **Bottleneck Analysis**: Identify potential bottlenecks
5. **Recommendations**: Actionable suggestions for managing dependencies

Format your response in clear sections with markdown. Be practical and focus on actionable insights.";
            
            var userPrompt = $@"Analyze task dependencies for project {projectId}: {project.Name}.

Use the available tools to:
1. Get all tasks in the project
2. Analyze dependencies for key tasks
3. Detect any circular dependencies
4. Calculate the critical path
5. Identify bottlenecks and risks

Provide a comprehensive dependency analysis with recommendations.";
            
            var chatHistory = new ChatHistory(systemPrompt);
            chatHistory.AddUserMessage(userPrompt);
            
            var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
            
            // 5. Exécuter avec Semantic Kernel
            var executionSettings = new OpenAIPromptExecutionSettings
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
                MaxTokens = 1500,
                Temperature = 0.2, // Low temp for deterministic analysis
            };
            
            // Create timeout-enabled cancellation token (3 seconds max for dependency analysis)
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(30)); // Increased to 30s for large projects
            
            ChatMessageContent response;
            try
            {
                response = await chatCompletionService.GetChatMessageContentAsync(
                    chatHistory,
                    executionSettings,
                    _kernel,
                    timeoutCts.Token);
            }
            catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested)
            {
                _logger.LogWarning("⏱️ Agent: Task dependency analysis request timed out after 30s for project {ProjectId}", projectId);
                throw new TimeoutException("Task dependency analysis timed out after 30 seconds");
            }
            
            stopwatch.Stop();
            var responseContent = response.Content ?? "No dependency analysis generated";
            
            _logger.LogInformation("🤖 Agent: Task dependency analysis completed for Project {ProjectId} in {ElapsedMs}ms", 
                projectId, stopwatch.ElapsedMilliseconds);
            
            // 6. Logger l'exécution
            executionLog.Status = "Success";
            executionLog.AgentResponse = responseContent;
            executionLog.ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds;
            
            await _dbContext.AgentExecutionLogs.AddAsync(executionLog, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            return new AgentResponse
            {
                Content = responseContent,
                Status = "Success",
                RequiresApproval = false, // Analysis doesn't require approval
                ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds,
                ExecutionCostUsd = 0.0m,
                Timestamp = DateTimeOffset.UtcNow
            };
        }
        catch (NotFoundException)
        {
            throw; // Re-throw NotFoundException as-is
        }
        catch (TimeoutException)
        {
            stopwatch.Stop();
            _logger.LogWarning("⏱️ Agent: Task dependency analysis timed out for project {ProjectId}", projectId);
            
            executionLog.Status = "Error";
            executionLog.ErrorMessage = "Task dependency analysis timed out after 30 seconds";
            executionLog.ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds;
            
            await _dbContext.AgentExecutionLogs.AddAsync(executionLog, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            return new AgentResponse
            {
                Content = "Task dependency analysis timed out after 30 seconds. The project may have too many tasks. Please try again or reduce the scope.",
                Status = "Error",
                ErrorMessage = "Timeout",
                ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds,
                Timestamp = DateTimeOffset.UtcNow
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "❌ Agent: Error during task dependency analysis for project {ProjectId}", projectId);
            
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
    
    public async System.Threading.Tasks.Task<AgentResponse> GenerateSprintRetrospectiveAsync(
        int sprintId,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var userId = _currentUserService.GetUserId();
        var organizationId = _currentUserService.GetOrganizationId();
        var correlationId = _correlationIdService.GetCorrelationId();
        
        var executionLog = new AgentExecutionLog
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId > 0 ? organizationId : throw new InvalidOperationException("OrganizationId is required for AgentExecutionLog"),
            AgentId = "sprint-retrospective-generator",
            UserId = userId > 0 ? userId.ToString() : "system",
            UserInput = $"Generate retrospective for sprint {sprintId}",
            Status = "Pending",
            ExecutionCostUsd = 0m,
            CreatedAt = DateTime.UtcNow
        };
        
        try
        {
            // 1. Vérifier si AI est activée et quota disponible
            if (organizationId > 0)
            {
                await _availabilityService.CheckQuotaAsync(organizationId, "Requests", cancellationToken);
            }
            
            // 2. Récupérer le sprint (avec tracking pour sauvegarder RetrospectiveNotes)
            var sprint = await _dbContext.Sprints
                .Include(s => s.Project)
                .FirstOrDefaultAsync(s => s.Id == sprintId, cancellationToken);
            
            if (sprint == null)
            {
                throw new NotFoundException($"Sprint {sprintId} not found");
            }
            
            // 3. Vérifier que le sprint est complété
            if (sprint.Status != SprintConstants.Statuses.Completed)
            {
                throw new InvalidOperationException($"Sprint {sprintId} must be completed before generating retrospective. Current status: {sprint.Status}");
            }
            
            // 4. Vérifier si retrospective déjà existante (avec option force=false par défaut)
            // Note: Pour l'instant, on régénère toujours. On pourrait ajouter un paramètre force plus tard.
            if (!string.IsNullOrWhiteSpace(sprint.RetrospectiveNotes))
            {
                _logger.LogInformation("📝 Sprint {SprintId} already has retrospective notes. Regenerating...", sprintId);
            }
            
            _logger.LogInformation("🤖 Agent: Starting sprint retrospective generation for Sprint {SprintId} (Project {ProjectId})", 
                sprintId, sprint.ProjectId);
            
            // 5. Enregistrer le plugin SprintRetrospectivePlugin
            var plugin = new SprintRetrospectivePlugin(_dbContext);
            _kernel.Plugins.AddFromObject(plugin, "SprintRetrospectivePlugin");
            
            // 6. Construire le prompt avec contexte
            var startDate = sprint.StartDate?.ToString("yyyy-MM-dd") ?? "N/A";
            var endDate = sprint.EndDate?.ToString("yyyy-MM-dd") ?? "N/A";
            
            var systemPrompt = @"You are an experienced Scrum master facilitating a sprint retrospective.
Use the available tools to gather comprehensive data:
- Sprint metrics (GetSprintMetrics): velocity, completion rate, story points
- Completed tasks (GetCompletedTasks): what was accomplished
- Incomplete tasks (GetIncompleteTasks): what didn't get done
- Defects (GetSprintDefects): quality issues discovered
- Team activity (GetTeamActivity): engagement and activity levels

Based on the data, generate a structured, constructive retrospective in JSON format with:
1. **Summary**: Overall sprint performance (2-3 sentences)
2. **What Went Well**: List of positive achievements (3-5 items)
3. **What Could Improve**: Areas for improvement (3-5 items)
4. **Action Items**: Specific, actionable improvements with priority and owner
5. **Metrics**: Key numbers (story points, velocity, defects, completion rate)
6. **Team Performance**: Engagement level, bottlenecks, strengths
7. **Recommendations**: Actionable suggestions for next sprint

Be specific, data-driven, and constructive. Focus on learning and continuous improvement.
Format as valid JSON only, no markdown.";
            
            var userPrompt = $@"Generate a comprehensive sprint retrospective for Sprint {sprint.Number}: {sprint.Goal}

Sprint Period: {startDate} to {endDate}

Use all available tools to gather data and provide a thorough analysis.
Return only valid JSON in the specified format.";
            
            var chatHistory = new ChatHistory(systemPrompt);
            chatHistory.AddUserMessage(userPrompt);
            
            var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
            
            // 7. Exécuter avec Semantic Kernel
            var executionSettings = new OpenAIPromptExecutionSettings
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
                MaxTokens = 3000,
                Temperature = 0.4, // Slightly creative for recommendations
            };
            
            // Create timeout-enabled cancellation token (5 seconds max for retrospective generation)
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(60)); // Increased to 60s for comprehensive analysis
            
            ChatMessageContent response;
            try
            {
                response = await chatCompletionService.GetChatMessageContentAsync(
                    chatHistory,
                    executionSettings,
                    _kernel,
                    timeoutCts.Token);
            }
            catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested)
            {
                _logger.LogWarning("⏱️ Agent: Sprint retrospective generation timed out after 60s for sprint {SprintId}", sprintId);
                throw new TimeoutException("Sprint retrospective generation timed out after 60 seconds");
            }
            
            stopwatch.Stop();
            var retrospective = response.Content ?? "No retrospective generated";
            
            // 8. Sauvegarder la rétrospective dans la DB
            sprint.RetrospectiveNotes = retrospective;
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("🤖 Agent: Sprint retrospective generated for Sprint {SprintId} in {ElapsedMs}ms", 
                sprintId, stopwatch.ElapsedMilliseconds);
            
            // 9. Logger l'exécution
            executionLog.Status = "Success";
            executionLog.AgentResponse = retrospective;
            executionLog.ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds;
            
            await _dbContext.AgentExecutionLogs.AddAsync(executionLog, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            return new AgentResponse
            {
                Content = retrospective,
                Status = "Success",
                RequiresApproval = false, // Retrospective doesn't require approval
                ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds,
                ExecutionCostUsd = 0.0m,
                Timestamp = DateTimeOffset.UtcNow
            };
        }
        catch (NotFoundException)
        {
            throw; // Re-throw NotFoundException as-is
        }
        catch (InvalidOperationException)
        {
            throw; // Re-throw InvalidOperationException as-is
        }
        catch (TimeoutException)
        {
            stopwatch.Stop();
            _logger.LogWarning("⏱️ Agent: Sprint retrospective generation timed out for sprint {SprintId}", sprintId);
            
            executionLog.Status = "Error";
            executionLog.ErrorMessage = "Sprint retrospective generation timed out after 60 seconds";
            executionLog.ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds;
            
            await _dbContext.AgentExecutionLogs.AddAsync(executionLog, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            return new AgentResponse
            {
                Content = "Sprint retrospective generation timed out after 60 seconds. Please try again.",
                Status = "Error",
                ErrorMessage = "Timeout",
                ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds,
                Timestamp = DateTimeOffset.UtcNow
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "❌ Agent: Error during sprint retrospective generation for sprint {SprintId}", sprintId);
            
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
}

