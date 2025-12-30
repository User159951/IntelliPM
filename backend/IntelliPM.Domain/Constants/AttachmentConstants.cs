namespace IntelliPM.Domain.Constants;

/// <summary>
/// Constants for attachment entity types, file size limits, and allowed file types.
/// </summary>
public static class AttachmentConstants
{
    /// <summary>
    /// Entity types that can have attachments.
    /// </summary>
    public static class EntityTypes
    {
        public const string Task = "Task";
        public const string Project = "Project";
        public const string Comment = "Comment";
        public const string Defect = "Defect";
    }

    // File size limits (in bytes)
    public const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB
    public const long MaxTotalSizePerEntity = 50 * 1024 * 1024; // 50 MB per entity

    /// <summary>
    /// Allowed file extensions.
    /// </summary>
    public static readonly string[] AllowedExtensions =
    {
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
        ".txt", ".csv", ".json", ".xml",
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".svg",
        ".zip", ".rar", ".7z"
    };

    /// <summary>
    /// Content type mappings for file extensions.
    /// </summary>
    public static readonly Dictionary<string, string> ContentTypeMappings = new()
    {
        { ".pdf", "application/pdf" },
        { ".doc", "application/msword" },
        { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
        { ".xls", "application/vnd.ms-excel" },
        { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
        { ".ppt", "application/vnd.ms-powerpoint" },
        { ".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
        { ".txt", "text/plain" },
        { ".csv", "text/csv" },
        { ".json", "application/json" },
        { ".xml", "application/xml" },
        { ".jpg", "image/jpeg" },
        { ".jpeg", "image/jpeg" },
        { ".png", "image/png" },
        { ".gif", "image/gif" },
        { ".bmp", "image/bmp" },
        { ".svg", "image/svg+xml" },
        { ".zip", "application/zip" },
        { ".rar", "application/x-rar-compressed" },
        { ".7z", "application/x-7z-compressed" }
    };
}

