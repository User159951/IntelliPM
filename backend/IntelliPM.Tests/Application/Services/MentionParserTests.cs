using IntelliPM.Application.Services;
using IntelliPM.Tests.API;
using IntelliPM.Tests.Infrastructure.TestAuthentication;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IntelliPM.Tests.Application.Services;

/// <summary>
/// Unit tests for MentionParser to verify mention parsing and injection prevention.
/// </summary>
public class MentionParserTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly IMentionParser _mentionParser;
    private readonly CustomWebApplicationFactory _factory;

    public MentionParserTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        var scope = factory.Services.CreateScope();
        _mentionParser = scope.ServiceProvider.GetRequiredService<IMentionParser>();
    }

    [Fact]
    public void ParseMentions_SimpleMention_ReturnsMention()
    {
        // Arrange
        var content = "Hello @john.doe, how are you?";

        // Act
        var mentions = _mentionParser.ParseMentions(content);

        // Assert
        Assert.Single(mentions);
        Assert.Equal("john.doe", mentions[0].Username, ignoreCase: true);
        Assert.Equal("@john.doe", mentions[0].MentionText);
    }

    [Fact]
    public void ParseMentions_MultipleMentions_ReturnsAllMentions()
    {
        // Arrange
        var content = "Hey @alice and @bob, check this out!";

        // Act
        var mentions = _mentionParser.ParseMentions(content);

        // Assert
        Assert.Equal(2, mentions.Count);
        Assert.Contains(mentions, m => m.Username.Equals("alice", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(mentions, m => m.Username.Equals("bob", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ParseMentions_DuplicateMentions_ReturnsUniqueMentions()
    {
        // Arrange
        var content = "@john.doe mentioned @john.doe again";

        // Act
        var mentions = _mentionParser.ParseMentions(content);

        // Assert
        Assert.Single(mentions);
        Assert.Equal("john.doe", mentions[0].Username, ignoreCase: true);
    }

    [Fact]
    public void ParseMentions_UsernameWithSpecialChars_ReturnsMention()
    {
        // Arrange
        var content = "Contact @user_123 or @test-user";

        // Act
        var mentions = _mentionParser.ParseMentions(content);

        // Assert
        Assert.Equal(2, mentions.Count);
        Assert.Contains(mentions, m => m.Username == "user_123");
        Assert.Contains(mentions, m => m.Username == "test-user");
    }

    [Fact]
    public void ParseMentions_EmptyContent_ReturnsEmptyList()
    {
        // Arrange
        var content = "";

        // Act
        var mentions = _mentionParser.ParseMentions(content);

        // Assert
        Assert.Empty(mentions);
    }

    [Fact]
    public void ParseMentions_NoMentions_ReturnsEmptyList()
    {
        // Arrange
        var content = "This is a regular comment without mentions";

        // Act
        var mentions = _mentionParser.ParseMentions(content);

        // Assert
        Assert.Empty(mentions);
    }

    [Fact]
    public void ParseMentions_EmailAddress_DoesNotMatch()
    {
        // Arrange
        var content = "Contact me at user@example.com";

        // Act
        var mentions = _mentionParser.ParseMentions(content);

        // Assert
        // Should not match email addresses (no @ at start)
        Assert.Empty(mentions);
    }

    [Fact]
    public void ParseMentions_MentionAtStart_ReturnsMention()
    {
        // Arrange
        var content = "@admin please check this";

        // Act
        var mentions = _mentionParser.ParseMentions(content);

        // Assert
        Assert.Single(mentions);
        Assert.Equal("admin", mentions[0].Username, ignoreCase: true);
    }

    [Fact]
    public void ParseMentions_MentionAtEnd_ReturnsMention()
    {
        // Arrange
        var content = "Thanks @john.doe";

        // Act
        var mentions = _mentionParser.ParseMentions(content);

        // Assert
        Assert.Single(mentions);
        Assert.Equal("john.doe", mentions[0].Username, ignoreCase: true);
    }

    [Fact]
    public void ParseMentions_CaseInsensitive_ReturnsMention()
    {
        // Arrange
        var content = "Hey @JOHN.DOE and @john.doe";

        // Act
        var mentions = _mentionParser.ParseMentions(content);

        // Assert
        // Should be treated as duplicate (case-insensitive)
        Assert.Single(mentions);
        Assert.Equal("JOHN.DOE", mentions[0].Username);
    }

    [Fact]
    public void ParseMentions_ExtremelyLongUsername_IsFiltered()
    {
        // Arrange
        var longUsername = new string('a', 150); // Exceeds MaxUsernameLength (100)
        var content = $"@{longUsername}";

        // Act
        var mentions = _mentionParser.ParseMentions(content);

        // Assert
        // Should be filtered out due to length
        Assert.Empty(mentions);
    }

    [Fact]
    public void ParseMentions_ManyMentions_IsLimited()
    {
        // Arrange
        var content = string.Join(" ", Enumerable.Range(1, 100).Select(i => $"@user{i}"));

        // Act
        var mentions = _mentionParser.ParseMentions(content);

        // Assert
        // Should be limited to MaxMentionsPerComment (50)
        Assert.True(mentions.Count <= 50);
    }

    [Fact]
    public void ParseMentions_MaliciousScriptInjection_IsFiltered()
    {
        // Arrange
        var content = "@user<script>alert('xss')</script>";

        // Act
        var mentions = _mentionParser.ParseMentions(content);

        // Assert
        // Should only match valid username part, script tag should be filtered
        var mention = mentions.FirstOrDefault();
        if (mention != null)
        {
            // Username should not contain script tags
            Assert.DoesNotContain("<script>", mention.Username, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void ParseMentions_InvalidCharacters_AreFiltered()
    {
        // Arrange
        var content = "@user<script> or @user@domain.com";

        // Act
        var mentions = _mentionParser.ParseMentions(content);

        // Assert
        // Should only match valid usernames
        var validMentions = mentions.Where(m => 
            System.Text.RegularExpressions.Regex.IsMatch(m.Username, @"^[a-zA-Z0-9._-]+$")).ToList();
        Assert.Equal(mentions.Count, validMentions.Count);
    }
}
