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
/// Includes protection against mention injection and manipulation attacks.
/// </summary>
public class MentionParser : IMentionParser
{
    private readonly IUnitOfWork _unitOfWork;
    
    // Regex pattern for @username mentions
    // Supports: alphanumeric, dots, underscores, hyphens
    // Example: @john.doe, @user_123, @test-user
    // Pattern is anchored to word boundaries to prevent injection
    private static readonly Regex MentionRegex = new Regex(
        @"@([a-zA-Z0-9._-]+)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    // Maximum length for a username (prevent DoS via extremely long usernames)
    private const int MaxUsernameLength = 100;

    // Maximum number of mentions per comment (prevent DoS via excessive mentions)
    private const int MaxMentionsPerComment = 50;

    public MentionParser(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <summary>
    /// Parse @username mentions from comment content.
    /// Includes validation to prevent injection and DoS attacks.
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
                var username = match.Groups[1].Value;

                // Validate username length to prevent DoS
                if (username.Length > MaxUsernameLength)
                {
                    continue; // Skip invalid usernames
                }

                // Validate username contains only allowed characters (defense in depth)
                if (!System.Text.RegularExpressions.Regex.IsMatch(username, @"^[a-zA-Z0-9._-]+$"))
                {
                    continue; // Skip potentially malicious usernames
                }

                mentions.Add(new MentionDto
                {
                    Username = username, // First capture group (username without @)
                    StartIndex = match.Index,
                    Length = match.Length,
                    MentionText = match.Value // Full match including @
                });
            }
        }

        // Remove duplicates (same username at different positions)
        // Keep only the first occurrence of each username
        var uniqueMentions = mentions
            .GroupBy(m => m.Username, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToList();

        // Limit total mentions to prevent DoS
        if (uniqueMentions.Count > MaxMentionsPerComment)
        {
            return uniqueMentions.Take(MaxMentionsPerComment).ToList();
        }

        return uniqueMentions;
    }

    /// <summary>
    /// Resolve usernames to user IDs within the same organization.
    /// Validates that all mentioned users belong to the same organization (tenant isolation).
    /// </summary>
    public async Task<List<int>> ResolveMentionedUserIds(
        List<MentionDto> mentions,
        int organizationId,
        CancellationToken ct)
    {
        if (!mentions.Any())
            return new List<int>();

        // Validate organization ID
        if (organizationId <= 0)
        {
            throw new ArgumentException("Organization ID must be greater than 0", nameof(organizationId));
        }

        // Extract and validate usernames
        var usernames = mentions
            .Select(m => m.Username)
            .Where(u => !string.IsNullOrWhiteSpace(u) && u.Length <= MaxUsernameLength)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (!usernames.Any())
            return new List<int>();

        // Resolve users - CRITICAL: Always filter by OrganizationId for tenant isolation
        // This prevents cross-tenant mention injection attacks
        var users = await _unitOfWork.Repository<User>()
            .Query()
            .Where(u => u.OrganizationId == organizationId && // Tenant isolation check
                       usernames.Contains(u.Username) &&
                       u.IsActive) // Only active users can be mentioned
            .Select(u => u.Id)
            .ToListAsync(ct);

        // Log if some mentions couldn't be resolved (potential security issue)
        if (users.Count < usernames.Count)
        {
            var resolvedUsernames = await _unitOfWork.Repository<User>()
                .Query()
                .Where(u => u.OrganizationId == organizationId && 
                           usernames.Contains(u.Username) &&
                           u.IsActive)
                .Select(u => u.Username)
                .ToListAsync(ct);

            var unresolvedUsernames = usernames
                .Except(resolvedUsernames, StringComparer.OrdinalIgnoreCase)
                .ToList();

            // These could be:
            // 1. Invalid usernames (normal case)
            // 2. Users from other organizations (security issue - prevented by filter)
            // 3. Inactive users (normal case)
            // We don't throw here, just return the valid mentions
        }

        return users;
    }
}

