using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Domain.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Services;

/// <summary>
/// Service for validating file uploads with security checks.
/// Validates MIME types, file extensions, file sizes, and prevents path traversal attacks.
/// </summary>
public interface IFileValidationService
{
    /// <summary>
    /// Validates a file upload for security and compliance.
    /// </summary>
    /// <param name="file">The file to validate</param>
    /// <param name="maxFileSizeBytes">Maximum allowed file size in bytes</param>
    /// <exception cref="ValidationException">Thrown if validation fails</exception>
    void ValidateFile(IFormFile file, long maxFileSizeBytes);

    /// <summary>
    /// Sanitizes a filename to prevent path traversal attacks.
    /// Removes directory separators and other dangerous characters.
    /// </summary>
    /// <param name="fileName">Original filename</param>
    /// <returns>Sanitized filename safe for storage</returns>
    string SanitizeFileName(string fileName);
}

/// <summary>
/// Implementation of file validation service with security checks.
/// </summary>
public class FileValidationService : IFileValidationService
{
    private readonly ILogger<FileValidationService> _logger;

    /// <summary>
    /// Allowed MIME types for file uploads.
    /// </summary>
    private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        // Documents
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document", // .docx
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", // .xlsx
        "application/vnd.ms-powerpoint",
        "application/vnd.openxmlformats-officedocument.presentationml.presentation", // .pptx
        "text/plain",
        "text/csv",
        "application/json",
        "application/xml",
        "text/xml",
        
        // Images
        "image/jpeg",
        "image/png",
        "image/gif",
        "image/bmp",
        "image/svg+xml",
        "image/webp",
        
        // Archives
        "application/zip",
        "application/x-rar-compressed",
        "application/x-7z-compressed",
        "application/x-tar",
        "application/gzip"
    };

    /// <summary>
    /// Dangerous filename patterns that indicate path traversal attempts.
    /// </summary>
    private static readonly string[] DangerousPatterns = 
    {
        "..",
        "/",
        "\\",
        ":",
        "*",
        "?",
        "\"",
        "<",
        ">",
        "|",
        "\0" // Null byte
    };

    public FileValidationService(ILogger<FileValidationService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void ValidateFile(IFormFile file, long maxFileSizeBytes)
    {
        if (file == null)
        {
            throw new ValidationException("File is required");
        }

        if (file.Length == 0)
        {
            throw new ValidationException("File cannot be empty");
        }

        // Validate file size
        if (file.Length > maxFileSizeBytes)
        {
            var maxSizeMB = maxFileSizeBytes / (1024.0 * 1024.0);
            throw new ValidationException(
                $"File size ({file.Length / (1024.0 * 1024.0):F2} MB) exceeds maximum allowed size of {maxSizeMB:F2} MB");
        }

        // Validate file extension
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(fileExtension))
        {
            throw new ValidationException("File must have an extension");
        }

        if (!AttachmentConstants.AllowedExtensions.Contains(fileExtension))
        {
            throw new ValidationException(
                $"File extension '{fileExtension}' is not allowed. Allowed extensions: {string.Join(", ", AttachmentConstants.AllowedExtensions)}");
        }

        // Validate MIME type
        var contentType = file.ContentType;
        if (string.IsNullOrWhiteSpace(contentType))
        {
            _logger.LogWarning("File {FileName} has no ContentType, will use extension-based validation only", file.FileName);
        }
        else
        {
            // Check if MIME type is in whitelist
            if (!AllowedMimeTypes.Contains(contentType))
            {
                // Allow if extension is valid (some browsers send incorrect MIME types)
                // But log a warning for security monitoring
                _logger.LogWarning(
                    "File {FileName} has MIME type '{ContentType}' which is not in whitelist, but extension '{Extension}' is allowed",
                    file.FileName, contentType, fileExtension);
            }
        }

        // Validate filename for path traversal
        if (ContainsDangerousPatterns(file.FileName))
        {
            _logger.LogWarning("Potential path traversal attempt detected in filename: {FileName}", file.FileName);
            throw new ValidationException("Filename contains invalid characters");
        }

        _logger.LogDebug("File validation passed for {FileName} ({Size} bytes, {ContentType})", 
            file.FileName, file.Length, contentType);
    }

    public string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("Filename cannot be null or empty", nameof(fileName));
        }

        // Remove path traversal patterns and dangerous characters
        var sanitized = fileName;
        
        foreach (var pattern in DangerousPatterns)
        {
            sanitized = sanitized.Replace(pattern, "_", StringComparison.Ordinal);
        }

        // Remove leading/trailing whitespace and dots
        sanitized = sanitized.Trim().TrimStart('.').TrimEnd('.');

        // Ensure filename is not empty after sanitization
        // Also check if the result only contains underscores (from replaced dangerous chars)
        if (string.IsNullOrWhiteSpace(sanitized) || sanitized.All(c => c == '_'))
        {
            sanitized = "file";
        }

        // Limit filename length to prevent issues
        const int maxFileNameLength = 255;
        if (sanitized.Length > maxFileNameLength)
        {
            var extension = Path.GetExtension(sanitized);
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(sanitized);
            var maxNameLength = maxFileNameLength - extension.Length;
            sanitized = nameWithoutExtension[..Math.Min(nameWithoutExtension.Length, maxNameLength)] + extension;
        }

        return sanitized;
    }

    private static bool ContainsDangerousPatterns(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return false;
        }

        foreach (var pattern in DangerousPatterns)
        {
            if (fileName.Contains(pattern, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
