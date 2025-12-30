using IntelliPM.Application.DTOs.Agent;
using IntelliPM.Application.Interfaces.Services;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Infrastructure.AI.Services;

/// <summary>
/// Implementation of IAgentService using Semantic Kernel with Ollama
/// </summary>
public class AgentService : IAgentService
{
    private readonly Kernel _kernel;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AgentService> _logger;

    public AgentService(
        Kernel kernel,
        IUnitOfWork unitOfWork,
        ILogger<AgentService> logger)
    {
        _kernel = kernel;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<AgentResponse> ImproveTaskDescriptionAsync(
        string taskDescription,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var toolsCalled = new List<string> { "Ollama-LLM" };

        try
        {
            _logger.LogInformation("Improving task description using AI agent");

            var chatCompletion = _kernel.GetRequiredService<IChatCompletionService>();

            var prompt = $@"You are a professional project manager and technical writer. 
Improve the following task description by:
1. Making it clear and concise
2. Adding acceptance criteria
3. Identifying potential technical challenges
4. Suggesting a rough story point estimate (1, 2, 3, 5, 8, 13)

Original Task Description:
{taskDescription}

Provide your improved version in the following format:

**Improved Description:**
[Your improved description here]

**Acceptance Criteria:**
- [Criterion 1]
- [Criterion 2]
- [etc.]

**Technical Considerations:**
[Any technical notes or challenges]

**Suggested Story Points:** [X points]
**Reasoning:** [Brief explanation]";

            var result = await chatCompletion.GetChatMessageContentAsync(
                prompt,
                cancellationToken: cancellationToken);

            stopwatch.Stop();

            return new AgentResponse
            {
                Content = result.Content ?? "No response generated",
                Status = "Success",
                RequiresApproval = false,
                ExecutionCostUsd = 0.0001m, // Estimated cost for local LLM
                ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds,
                ToolsCalled = toolsCalled,
                Timestamp = DateTimeOffset.UtcNow
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error improving task description");

            return new AgentResponse
            {
                Content = string.Empty,
                Status = "Error",
                ErrorMessage = ex.Message,
                ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds,
                ToolsCalled = toolsCalled,
                Timestamp = DateTimeOffset.UtcNow
            };
        }
    }

    public async Task<AgentResponse> AnalyzeProjectRisksAsync(
        int projectId,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var toolsCalled = new List<string> { "Ollama-LLM", "Database-Query" };

        try
        {
            _logger.LogInformation("Analyzing risks for project {ProjectId}", projectId);

            // Fetch project data
            var projectRepo = _unitOfWork.Repository<Project>();
            var project = await projectRepo.Query()
                .Include(p => p.UserStories)
                .Include(p => p.Sprints)
                .Include(p => p.Risks)
                .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

            if (project == null)
            {
                throw new InvalidOperationException($"Project {projectId} not found");
            }

            // Fetch tasks
            var taskRepo = _unitOfWork.Repository<ProjectTask>();
            var tasks = await taskRepo.Query()
                .Where(t => t.ProjectId == projectId)
                .Include(t => t.Assignee)
                .ToListAsync(cancellationToken);

            // Build context for AI
            var projectContext = $@"
Project: {project.Name}
Description: {project.Description}
Type: {project.Type}
Status: {project.Status}

Total User Stories: {project.UserStories.Count}
Total Tasks: {tasks.Count}
Tasks in Progress: {tasks.Count(t => t.Status == "InProgress")}
Blocked Tasks: {tasks.Count(t => t.Status == "Blocked")}
Unassigned Tasks: {tasks.Count(t => t.AssigneeId == null)}

Active Sprints: {project.Sprints.Count(s => s.Status == "Active")}
Existing Risks: {project.Risks.Count}
";

            var chatCompletion = _kernel.GetRequiredService<IChatCompletionService>();

            var prompt = $@"You are an experienced project manager analyzing a software project for risks.

Project Context:
{projectContext}

Analyze the project and identify:
1. **High-Priority Risks**: Critical issues that need immediate attention
2. **Medium-Priority Risks**: Important concerns to monitor
3. **Low-Priority Risks**: Minor issues to be aware of

For each risk, provide:
- Risk description
- Impact (High/Medium/Low)
- Likelihood (High/Medium/Low)
- Mitigation strategy

Format your response as:

**High-Priority Risks:**
1. [Risk description]
   - Impact: [High/Medium/Low]
   - Likelihood: [High/Medium/Low]
   - Mitigation: [Strategy]

**Medium-Priority Risks:**
[Same format]

**Low-Priority Risks:**
[Same format]

**Overall Project Health:** [Green/Yellow/Red] - [Brief explanation]

**Recommendations:**
- [Actionable recommendation 1]
- [Actionable recommendation 2]";

            var result = await chatCompletion.GetChatMessageContentAsync(
                prompt,
                cancellationToken: cancellationToken);

            stopwatch.Stop();

            return new AgentResponse
            {
                Content = result.Content ?? "No analysis generated",
                Status = "Success",
                RequiresApproval = false,
                ExecutionCostUsd = 0.0002m,
                ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds,
                ToolsCalled = toolsCalled,
                Timestamp = DateTimeOffset.UtcNow,
                Metadata = new Dictionary<string, object>
                {
                    ["ProjectId"] = projectId,
                    ["ProjectName"] = project.Name,
                    ["TotalTasks"] = tasks.Count,
                    ["BlockedTasks"] = tasks.Count(t => t.Status == "Blocked")
                }
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error analyzing project risks for project {ProjectId}", projectId);

            return new AgentResponse
            {
                Content = string.Empty,
                Status = "Error",
                ErrorMessage = ex.Message,
                ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds,
                ToolsCalled = toolsCalled,
                Timestamp = DateTimeOffset.UtcNow
            };
        }
    }

    public async System.Threading.Tasks.Task<AgentResponse> SuggestSprintPlanAsync(
        int projectId,
        int sprintId,
        CancellationToken cancellationToken = default)
    {
        // TODO: Implement sprint planning suggestion
        await System.Threading.Tasks.Task.CompletedTask;
        return new AgentResponse
        {
            Content = "Sprint planning suggestion not yet implemented",
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
            Content = "Task dependency analysis not yet implemented",
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
            Content = "Sprint retrospective generation not yet implemented",
            Status = "Error",
            ErrorMessage = "Not implemented",
            Timestamp = DateTimeOffset.UtcNow
        };
    }
}

