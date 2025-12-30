using IntelliPM.Application.DTOs.Agent;

namespace IntelliPM.Application.Interfaces.Services;

/// <summary>
/// Service interface for AI Agent operations using Semantic Kernel
/// </summary>
public interface IAgentService
{
    /// <summary>
    /// Uses AI to improve and enhance a task description with better clarity, acceptance criteria, and structure
    /// </summary>
    /// <param name="taskDescription">The original task description to improve</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AgentResponse with improved task description</returns>
    Task<AgentResponse> ImproveTaskDescriptionAsync(
        string taskDescription,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyzes a project to identify potential risks, blockers, and areas of concern
    /// </summary>
    /// <param name="projectId">The ID of the project to analyze</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AgentResponse with risk analysis and recommendations</returns>
    Task<AgentResponse> AnalyzeProjectRisksAsync(
        int projectId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Suggests optimal sprint planning based on team capacity, velocity, and backlog
    /// </summary>
    /// <param name="projectId">The ID of the project</param>
    /// <param name="sprintId">The ID of the sprint to plan</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AgentResponse with sprint planning suggestions</returns>
    Task<AgentResponse> SuggestSprintPlanAsync(
        int projectId,
        int sprintId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyzes task dependencies and suggests optimal task ordering
    /// </summary>
    /// <param name="projectId">The ID of the project</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AgentResponse with task ordering recommendations</returns>
    Task<AgentResponse> AnalyzeTaskDependenciesAsync(
        int projectId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a summary and insights for a completed sprint
    /// </summary>
    /// <param name="sprintId">The ID of the completed sprint</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AgentResponse with sprint retrospective insights</returns>
    Task<AgentResponse> GenerateSprintRetrospectiveAsync(
        int sprintId,
        CancellationToken cancellationToken = default);
}

