namespace IntelliPM.Application.Agents.Services;

/// <summary>
/// Service for parsing JSON output from LLM agents with validation.
/// </summary>
public interface IAgentOutputParser
{
    /// <summary>
    /// Attempts to parse JSON string into the specified type with validation.
    /// </summary>
    /// <typeparam name="T">The type to parse into.</typeparam>
    /// <param name="json">The JSON string to parse.</param>
    /// <param name="result">The parsed result if successful, null otherwise.</param>
    /// <param name="errors">List of parsing or validation errors.</param>
    /// <returns>True if parsing and validation succeeded, false otherwise.</returns>
    bool TryParse<T>(string json, out T? result, out List<string> errors);
}

