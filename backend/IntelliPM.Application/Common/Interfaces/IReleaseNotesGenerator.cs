namespace IntelliPM.Application.Common.Interfaces;

/// <summary>
/// Service for generating release notes and changelogs automatically based on sprints, tasks, and changes.
/// </summary>
public interface IReleaseNotesGenerator
{
    /// <summary>
    /// Generates release notes for a release based on included sprints and tasks.
    /// </summary>
    /// <param name="releaseId">ID of the release to generate notes for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Generated release notes in markdown format.</returns>
    System.Threading.Tasks.Task<string> GenerateReleaseNotesAsync(int releaseId, CancellationToken cancellationToken);

    /// <summary>
    /// Generates changelog for a release.
    /// </summary>
    /// <param name="releaseId">ID of the release to generate changelog for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Generated changelog in markdown format.</returns>
    System.Threading.Tasks.Task<string> GenerateChangeLogAsync(int releaseId, CancellationToken cancellationToken);

    /// <summary>
    /// Generates release notes from a list of sprint IDs.
    /// </summary>
    /// <param name="projectId">ID of the project.</param>
    /// <param name="sprintIds">List of sprint IDs to include in the release notes.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Generated release notes in markdown format.</returns>
    System.Threading.Tasks.Task<string> GenerateReleaseNotesFromSprintsAsync(int projectId, List<int> sprintIds, CancellationToken cancellationToken);
}
