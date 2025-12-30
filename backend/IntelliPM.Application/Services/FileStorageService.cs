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
    /// <param name="ct">Cancellation token</param>
    /// <returns>Stored filename (unique identifier)</returns>
    System.Threading.Tasks.Task<string> SaveFileAsync(Stream fileStream, string fileName, CancellationToken ct);

    /// <summary>
    /// Retrieve a file from storage by stored filename.
    /// </summary>
    /// <param name="storedFileName">Stored filename (unique identifier)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>File stream</returns>
    System.Threading.Tasks.Task<Stream> GetFileAsync(string storedFileName, CancellationToken ct);

    /// <summary>
    /// Delete a file from storage.
    /// </summary>
    /// <param name="storedFileName">Stored filename (unique identifier)</param>
    /// <param name="ct">Cancellation token</param>
    System.Threading.Tasks.Task DeleteFileAsync(string storedFileName, CancellationToken ct);

    /// <summary>
    /// Generate a unique filename for storage.
    /// </summary>
    /// <param name="originalFileName">Original filename</param>
    /// <returns>Unique stored filename</returns>
    string GenerateUniqueFileName(string originalFileName);
}

/// <summary>
/// Local file system implementation of file storage service.
/// Stores files in a local directory on the server.
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly ILogger<LocalFileStorageService> _logger;
    private readonly string _uploadDirectory;

    public LocalFileStorageService(
        IConfiguration configuration,
        ILogger<LocalFileStorageService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _uploadDirectory = configuration["FileStorage:UploadDirectory"] ?? "uploads";

        // Ensure directory exists
        if (!Directory.Exists(_uploadDirectory))
        {
            Directory.CreateDirectory(_uploadDirectory);
            _logger.LogInformation("Created upload directory: {UploadDirectory}", _uploadDirectory);
        }
    }

    /// <summary>
    /// Save a file to local storage.
    /// </summary>
    public async System.Threading.Tasks.Task<string> SaveFileAsync(Stream fileStream, string fileName, CancellationToken ct)
    {
        try
        {
            var storedFileName = GenerateUniqueFileName(fileName);
            var filePath = Path.Combine(_uploadDirectory, storedFileName);

            using var fileStreamOut = File.Create(filePath);
            await fileStream.CopyToAsync(fileStreamOut, ct);

            _logger.LogInformation("File saved successfully: {FileName} -> {StoredFileName}", fileName, storedFileName);
            return storedFileName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving file: {FileName}", fileName);
            throw;
        }
    }

    /// <summary>
    /// Retrieve a file from local storage.
    /// </summary>
    public System.Threading.Tasks.Task<Stream> GetFileAsync(string storedFileName, CancellationToken ct)
    {
        var filePath = Path.Combine(_uploadDirectory, storedFileName);

        if (!File.Exists(filePath))
        {
            _logger.LogWarning("File not found: {StoredFileName}", storedFileName);
            throw new FileNotFoundException($"File not found: {storedFileName}");
        }

        return System.Threading.Tasks.Task.FromResult<Stream>(File.OpenRead(filePath));
    }

    /// <summary>
    /// Delete a file from local storage.
    /// </summary>
    public System.Threading.Tasks.Task DeleteFileAsync(string storedFileName, CancellationToken ct)
    {
        try
        {
            var filePath = Path.Combine(_uploadDirectory, storedFileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("File deleted successfully: {StoredFileName}", storedFileName);
            }
            else
            {
                _logger.LogWarning("File not found for deletion: {StoredFileName}", storedFileName);
            }

            return System.Threading.Tasks.Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {StoredFileName}", storedFileName);
            throw;
        }
    }

    /// <summary>
    /// Generate a unique filename using timestamp and GUID.
    /// Format: {timestamp}_{guid}{extension}
    /// </summary>
    public string GenerateUniqueFileName(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName);
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var guid = Guid.NewGuid().ToString("N")[..8];
        return $"{timestamp}_{guid}{extension}";
    }
}

