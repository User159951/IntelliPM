using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace IntelliPM.Application.Services;

/// <summary>
/// DTO for parsed mention information.
/// </summary>
public class MentionDto
{
    public string Username { get; set; } = string.Empty;
    public int StartIndex { get; set; }
    public int Length { get; set; }
    public string MentionText { get; set; } = string.Empty;
}

/// <summary>
/// Interface for parsing and resolving user mentions from comment content.
/// </summary>
public interface IMentionParser
{
    /// <summary>
    /// Parse @username mentions from comment content.
    /// </summary>
    /// <param name="content">Comment content to parse</param>
    /// <returns>List of parsed mentions with position information</returns>
    List<MentionDto> ParseMentions(string content);

    /// <summary>
    /// Resolve usernames to user IDs within the same organization.
    /// </summary>
    /// <param name="mentions">List of parsed mentions</param>
    /// <param name="organizationId">Organization ID to filter users</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of user IDs that were found</returns>
    Task<List<int>> ResolveMentionedUserIds(
        List<MentionDto> mentions,
        int organizationId,
        CancellationToken ct);
}

/// <summary>
/// Service for parsing and resolving user mentions in comments.
/// Uses regex to find @username patterns and resolves them to user IDs.
/// </summary>
public class MentionParser : IMentionParser
{
    private readonly IUnitOfWork _unitOfWork;
    
    // Regex pattern for @username mentions
    // Supports: alphanumeric, dots, underscores, hyphens
    // Example: @john.doe, @user_123, @test-user
    private static readonly Regex MentionRegex = new Regex(
        @"@([a-zA-Z0-9._-]+)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    public MentionParser(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <summary>
    /// Parse @username mentions from comment content.
    /// </summary>
    public List<MentionDto> ParseMentions(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return new List<MentionDto>();

        var mentions = new List<MentionDto>();
        var matches = MentionRegex.Matches(content);

        foreach (Match match in matches)
        {
            if (match.Success && match.Groups.Count > 1)
            {
                mentions.Add(new MentionDto
                {
                    Username = match.Groups[1].Value, // First capture group (username without @)
                    StartIndex = match.Index,
                    Length = match.Length,
                    MentionText = match.Value // Full match including @
                });
            }
        }

        // Remove duplicates (same username at different positions)
        // Keep only the first occurrence of each username
        return mentions
            .GroupBy(m => m.Username, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToList();
    }

    /// <summary>
    /// Resolve usernames to user IDs within the same organization.
    /// </summary>
    public async Task<List<int>> ResolveMentionedUserIds(
        List<MentionDto> mentions,
        int organizationId,
        CancellationToken ct)
    {
        if (!mentions.Any())
            return new List<int>();

        var usernames = mentions.Select(m => m.Username).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        var users = await _unitOfWork.Repository<User>()
            .Query()
            .Where(u => u.OrganizationId == organizationId && 
                       usernames.Contains(u.Username) &&
                       u.IsActive) // Only active users
            .Select(u => u.Id)
            .ToListAsync(ct);

        return users;
    }
}

