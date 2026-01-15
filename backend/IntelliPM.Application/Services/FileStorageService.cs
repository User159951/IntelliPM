using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Application.Services;

/// <summary>
/// Interface for file storage operations.
/// Supports local file system or cloud storage implementations.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Save a file to storage and return the stored filename.
    /// </summary>
    /// <param name="fileStream">File stream to save</param>
    /// <param name="fileName">Original filename</param>
    /// <param name="organizationId">Organization ID for tenant isolation</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Stored filename (unique identifier, includes organization path)</returns>
    System.Threading.Tasks.Task<string> SaveFileAsync(Stream fileStream, string fileName, int organizationId, CancellationToken ct);

    /// <summary>
    /// Retrieve a file from storage by stored filename.
    /// </summary>
    /// <param name="storedFileName">Stored filename (unique identifier, includes organization path)</param>
    /// <param name="organizationId">Organization ID for tenant isolation verification</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>File stream</returns>
    System.Threading.Tasks.Task<Stream> GetFileAsync(string storedFileName, int organizationId, CancellationToken ct);

    /// <summary>
    /// Delete a file from storage.
    /// </summary>
    /// <param name="storedFileName">Stored filename (unique identifier, includes organization path)</param>
    /// <param name="organizationId">Organization ID for tenant isolation verification</param>
    /// <param name="ct">Cancellation token</param>
    System.Threading.Tasks.Task DeleteFileAsync(string storedFileName, int organizationId, CancellationToken ct);

    /// <summary>
    /// Generate a unique filename for storage.
    /// </summary>
    /// <param name="originalFileName">Original filename</param>
    /// <param name="organizationId">Organization ID for tenant isolation</param>
    /// <returns>Unique stored filename (includes organization path)</returns>
    string GenerateUniqueFileName(string originalFileName, int organizationId);
}

/// <summary>
/// Local file system implementation of file storage service.
/// Stores files in a local directory on the server, organized by organization ID for tenant isolation.
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly ILogger<LocalFileStorageService> _logger;
    private readonly IFileValidationService _fileValidationService;
    private readonly string _uploadDirectory;

    public LocalFileStorageService(
        IConfiguration configuration,
        IFileValidationService fileValidationService,
        ILogger<LocalFileStorageService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _fileValidationService = fileValidationService ?? throw new ArgumentNullException(nameof(fileValidationService));
        _uploadDirectory = configuration["FileStorage:UploadDirectory"] ?? "uploads";

        // Ensure base directory exists
        if (!Directory.Exists(_uploadDirectory))
        {
            Directory.CreateDirectory(_uploadDirectory);
            _logger.LogInformation("Created upload directory: {UploadDirectory}", _uploadDirectory);
        }
    }

    /// <summary>
    /// Save a file to local storage, organized by organization ID.
    /// </summary>
    public async System.Threading.Tasks.Task<string> SaveFileAsync(Stream fileStream, string fileName, int organizationId, CancellationToken ct)
    {
        try
        {
            // Sanitize filename to prevent path traversal
            var sanitizedFileName = _fileValidationService.SanitizeFileName(fileName);
            var storedFileName = GenerateUniqueFileName(sanitizedFileName, organizationId);
            
            // Create organization-specific directory
            var orgDirectory = Path.Combine(_uploadDirectory, organizationId.ToString());
            if (!Directory.Exists(orgDirectory))
            {
                Directory.CreateDirectory(orgDirectory);
                _logger.LogDebug("Created organization directory: {OrgDirectory}", orgDirectory);
            }

            // Extract just the filename from storedFileName (which is "{orgId}/{filename}")
            var fileNameOnly = Path.GetFileName(storedFileName.Replace('/', Path.DirectorySeparatorChar));
            var filePath = Path.Combine(orgDirectory, fileNameOnly);

            // Ensure we're not escaping the organization directory (defense in depth)
            var fullOrgPath = Path.GetFullPath(orgDirectory);
            var fullFilePath = Path.GetFullPath(filePath);
            if (!fullFilePath.StartsWith(fullOrgPath, StringComparison.Ordinal))
            {
                _logger.LogError("Path traversal attempt detected: {FilePath} does not start with {OrgDirectory}", 
                    fullFilePath, fullOrgPath);
                throw new InvalidOperationException("Invalid file path detected");
            }

            using var fileStreamOut = File.Create(filePath);
            await fileStream.CopyToAsync(fileStreamOut, ct);

            _logger.LogInformation("File saved successfully: {FileName} -> {StoredFileName} (Org: {OrganizationId})", 
                fileName, storedFileName, organizationId);
            
            // Return relative path from upload directory: {organizationId}/{filename}
            return storedFileName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving file: {FileName} for organization {OrganizationId}", fileName, organizationId);
            throw;
        }
    }

    /// <summary>
    /// Retrieve a file from local storage, verifying organization ownership.
    /// </summary>
    public System.Threading.Tasks.Task<Stream> GetFileAsync(string storedFileName, int organizationId, CancellationToken ct)
    {
        try
        {
            // Validate that stored filename belongs to the organization
            // Expected format: {organizationId}/{filename} or just filename (legacy)
            var filePath = GetFilePath(storedFileName, organizationId);

            // Verify path is within organization directory (defense in depth)
            var orgDirectory = Path.Combine(_uploadDirectory, organizationId.ToString());
            var fullOrgPath = Path.GetFullPath(orgDirectory);
            var fullFilePath = Path.GetFullPath(filePath);
            
            if (!fullFilePath.StartsWith(fullOrgPath, StringComparison.Ordinal))
            {
                _logger.LogWarning("Cross-organization file access attempt: {FilePath} not in {OrgDirectory}", 
                    fullFilePath, fullOrgPath);
                throw new FileNotFoundException("File not found");
            }

            if (!File.Exists(filePath))
            {
                _logger.LogWarning("File not found: {StoredFileName} for organization {OrganizationId}", 
                    storedFileName, organizationId);
                throw new FileNotFoundException($"File not found: {storedFileName}");
            }

            return System.Threading.Tasks.Task.FromResult<Stream>(File.OpenRead(filePath));
        }
        catch (FileNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving file: {StoredFileName} for organization {OrganizationId}", 
                storedFileName, organizationId);
            throw;
        }
    }

    /// <summary>
    /// Delete a file from local storage, verifying organization ownership.
    /// </summary>
    public System.Threading.Tasks.Task DeleteFileAsync(string storedFileName, int organizationId, CancellationToken ct)
    {
        try
        {
            var filePath = GetFilePath(storedFileName, organizationId);

            // Verify path is within organization directory (defense in depth)
            var orgDirectory = Path.Combine(_uploadDirectory, organizationId.ToString());
            var fullOrgPath = Path.GetFullPath(orgDirectory);
            var fullFilePath = Path.GetFullPath(filePath);
            
            if (!fullFilePath.StartsWith(fullOrgPath, StringComparison.Ordinal))
            {
                _logger.LogWarning("Cross-organization file deletion attempt: {FilePath} not in {OrgDirectory}", 
                    fullFilePath, fullOrgPath);
                throw new FileNotFoundException("File not found");
            }

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("File deleted successfully: {StoredFileName} (Org: {OrganizationId})", 
                    storedFileName, organizationId);
            }
            else
            {
                _logger.LogWarning("File not found for deletion: {StoredFileName} (Org: {OrganizationId})", 
                    storedFileName, organizationId);
            }

            return System.Threading.Tasks.Task.CompletedTask;
        }
        catch (FileNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {StoredFileName} for organization {OrganizationId}", 
                storedFileName, organizationId);
            throw;
        }
    }

    /// <summary>
    /// Generate a unique filename using timestamp and GUID, organized by organization ID.
    /// Format: {organizationId}/{timestamp}_{guid}{extension}
    /// </summary>
    public string GenerateUniqueFileName(string originalFileName, int organizationId)
    {
        var extension = Path.GetExtension(originalFileName);
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var guid = Guid.NewGuid().ToString("N")[..8];
        var fileName = $"{timestamp}_{guid}{extension}";
        
        // Return relative path: {organizationId}/{filename}
        return Path.Combine(organizationId.ToString(), fileName).Replace('\\', '/');
    }

    /// <summary>
    /// Gets the full file path from stored filename, handling both new format ({orgId}/{filename}) and legacy format (filename only).
    /// </summary>
    private string GetFilePath(string storedFileName, int organizationId)
    {
        // Handle new format: {organizationId}/{filename}
        if (storedFileName.Contains('/') || storedFileName.Contains('\\'))
        {
            // Extract organization ID from path and verify it matches
            var parts = storedFileName.Replace('\\', '/').Split('/');
            if (parts.Length >= 2 && int.TryParse(parts[0], out var pathOrgId))
            {
                if (pathOrgId != organizationId)
                {
                    _logger.LogWarning("Organization ID mismatch: path has {PathOrgId}, expected {OrganizationId}", 
                        pathOrgId, organizationId);
                    throw new FileNotFoundException("File not found");
                }
                var fileName = parts[^1];
                return Path.Combine(_uploadDirectory, organizationId.ToString(), fileName);
            }
        }

        // Legacy format: just filename (for backward compatibility)
        // Still verify it's in the correct organization directory
        return Path.Combine(_uploadDirectory, organizationId.ToString(), Path.GetFileName(storedFileName));
    }
}

