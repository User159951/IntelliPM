using IntelliPM.Application.Services;
using Xunit;

namespace IntelliPM.Tests.Application.Services;

/// <summary>
/// Unit tests for CommentSanitizationService to verify XSS prevention.
/// </summary>
public class CommentSanitizationServiceTests
{
    private readonly ICommentSanitizationService _sanitizationService;

    public CommentSanitizationServiceTests()
    {
        _sanitizationService = new CommentSanitizationService();
    }

    [Fact]
    public void SanitizeForStorage_ScriptTag_IsRemoved()
    {
        // Arrange
        var content = "Hello <script>alert('xss')</script> world";

        // Act
        var sanitized = _sanitizationService.SanitizeForStorage(content);

        // Assert
        Assert.DoesNotContain("<script>", sanitized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("alert", sanitized, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SanitizeForStorage_JavaScriptProtocol_IsRemoved()
    {
        // Arrange
        var content = "Check this javascript:alert('xss')";

        // Act
        var sanitized = _sanitizationService.SanitizeForStorage(content);

        // Assert
        Assert.DoesNotContain("javascript:", sanitized, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SanitizeForStorage_OnClickHandler_IsRemoved()
    {
        // Arrange
        var content = "Click <div onclick='alert(1)'>here</div>";

        // Act
        var sanitized = _sanitizationService.SanitizeForStorage(content);

        // Assert
        Assert.DoesNotContain("onclick", sanitized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<div", sanitized, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SanitizeForStorage_SafeMarkdown_Preserved()
    {
        // Arrange
        var content = "**Bold** text and *italic* text";

        // Act
        var sanitized = _sanitizationService.SanitizeForStorage(content);

        // Assert
        Assert.Contains("Bold", sanitized);
        Assert.Contains("italic", sanitized);
    }

    [Fact]
    public void SanitizeForStorage_EmptyContent_ReturnsEmpty()
    {
        // Arrange
        var content = "";

        // Act
        var sanitized = _sanitizationService.SanitizeForStorage(content);

        // Assert
        Assert.Empty(sanitized);
    }

    [Fact]
    public void RenderMarkdownToHtml_SafeMarkdown_RendersHtml()
    {
        // Arrange
        var content = "**Bold** text";

        // Act
        var html = _sanitizationService.RenderMarkdownToHtml(content);

        // Assert
        Assert.Contains("<strong>", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Bold", html);
    }

    [Fact]
    public void RenderMarkdownToHtml_ScriptInMarkdown_IsRemoved()
    {
        // Arrange
        var content = "Text <script>alert('xss')</script>";

        // Act
        var html = _sanitizationService.RenderMarkdownToHtml(content);

        // Assert
        // Script tags should be escaped (converted to &lt;script&gt;) or removed entirely
        // The key security concern is that unescaped <script> tags are not present
        Assert.DoesNotContain("<script>", html, StringComparison.OrdinalIgnoreCase);
        // Note: Escaped content like &lt;script&gt;alert('xss')&lt;/script&gt; is safe
        // because it will be displayed as text, not executed as JavaScript
    }

    [Fact]
    public void RenderMarkdownToHtml_LinkWithJavaScript_IsSanitized()
    {
        // Arrange
        var content = "[Click](javascript:alert('xss'))";

        // Act
        var html = _sanitizationService.RenderMarkdownToHtml(content);

        // Assert
        // JavaScript protocol should be removed or sanitized
        Assert.DoesNotContain("javascript:", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RenderMarkdownToHtml_CodeBlock_Preserved()
    {
        // Arrange
        var content = "```csharp\nvar x = 1;\n```";

        // Act
        var html = _sanitizationService.RenderMarkdownToHtml(content);

        // Assert
        Assert.Contains("code", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("var x = 1", html);
    }

    [Fact]
    public void SanitizeForStorage_MentionPreserved()
    {
        // Arrange
        var content = "Hey @john.doe, check this out!";

        // Act
        var sanitized = _sanitizationService.SanitizeForStorage(content);

        // Assert
        Assert.Contains("@john.doe", sanitized);
    }
}
