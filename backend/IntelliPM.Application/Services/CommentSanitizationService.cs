using Markdig;
using Ganss.Xss;
using System.Text.RegularExpressions;

namespace IntelliPM.Application.Services;

/// <summary>
/// Interface for sanitizing comment content to prevent XSS attacks.
/// </summary>
public interface ICommentSanitizationService
{
    /// <summary>
    /// Sanitizes and validates comment content before storage.
    /// Removes dangerous HTML/script tags while preserving markdown formatting.
    /// </summary>
    /// <param name="content">Raw comment content from user input</param>
    /// <returns>Sanitized content safe for storage and display</returns>
    string SanitizeForStorage(string content);

    /// <summary>
    /// Renders markdown content to HTML safely, with XSS protection.
    /// Used when displaying comments to users.
    /// </summary>
    /// <param name="markdownContent">Markdown content to render</param>
    /// <returns>Safe HTML output</returns>
    string RenderMarkdownToHtml(string markdownContent);
}

/// <summary>
/// Service for sanitizing comment content to prevent XSS attacks.
/// Uses HtmlSanitizer for HTML sanitization and Markdig for safe markdown rendering.
/// </summary>
public class CommentSanitizationService : ICommentSanitizationService
{
    private readonly HtmlSanitizer _htmlSanitizer;
    private readonly MarkdownPipeline _markdownPipeline;

    // Pattern to detect potentially dangerous script injections in markdown
    private static readonly Regex DangerousScriptPattern = new Regex(
        @"<script[^>]*>.*?</script>|javascript:|on\w+\s*=",
        RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline
    );

    public CommentSanitizationService()
    {
        // Configure HTML sanitizer with safe defaults
        _htmlSanitizer = new HtmlSanitizer();
        
        // Allow safe HTML tags for markdown rendering
        _htmlSanitizer.AllowedTags.Clear();
        _htmlSanitizer.AllowedTags.UnionWith(new[]
        {
            "p", "br", "strong", "em", "u", "s", "h1", "h2", "h3", "h4", "h5", "h6",
            "ul", "ol", "li", "blockquote", "code", "pre", "a", "img", "hr", "table",
            "thead", "tbody", "tr", "th", "td"
        });

        // Allow safe attributes only
        _htmlSanitizer.AllowedAttributes.Clear();
        _htmlSanitizer.AllowedAttributes.UnionWith(new[]
        {
            "href", "title", "alt", "src", "class"
        });

        // Note: HtmlSanitizer v9 doesn't have AllowedCssClasses property
        // CSS classes are sanitized automatically, safe classes like "code" and "pre" are allowed

        // Allow safe URL schemes only (no javascript:, data:, etc.)
        _htmlSanitizer.AllowedSchemes.Clear();
        _htmlSanitizer.AllowedSchemes.UnionWith(new[]
        {
            "http", "https", "mailto"
        });

        // Configure markdown pipeline with safe options
        // DisableHtml() prevents raw HTML injection in markdown
        _markdownPipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .DisableHtml() // Critical: Disable raw HTML in markdown for security
            .Build();
    }

    /// <summary>
    /// Sanitizes and validates comment content before storage.
    /// Removes dangerous HTML/script tags while preserving markdown formatting.
    /// </summary>
    public string SanitizeForStorage(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return string.Empty;

        // Check for dangerous script patterns first
        if (DangerousScriptPattern.IsMatch(content))
        {
            // Remove dangerous patterns
            content = DangerousScriptPattern.Replace(content, string.Empty);
        }

        // Remove any raw HTML tags that might have been injected
        // We only want markdown, not raw HTML
        var htmlTagPattern = new Regex(@"<[^>]+>", RegexOptions.Compiled);
        content = htmlTagPattern.Replace(content, string.Empty);

        // Trim and normalize whitespace
        content = content.Trim();

        return content;
    }

    /// <summary>
    /// Renders markdown content to HTML safely, with XSS protection.
    /// Used when displaying comments to users.
    /// </summary>
    public string RenderMarkdownToHtml(string markdownContent)
    {
        if (string.IsNullOrWhiteSpace(markdownContent))
            return string.Empty;

        // Convert markdown to HTML using safe pipeline (no raw HTML allowed)
        var html = Markdown.ToHtml(markdownContent, _markdownPipeline);
        
        // Sanitize the rendered HTML to remove any potentially dangerous content
        // This is defense in depth - even if markdown parser has a bug, sanitizer catches it
        var sanitized = _htmlSanitizer.Sanitize(html);
        
        return sanitized;
    }
}
