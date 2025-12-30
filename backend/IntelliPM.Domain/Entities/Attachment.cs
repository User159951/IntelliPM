using IntelliPM.Domain.Interfaces;

namespace IntelliPM.Domain.Entities;

/// <summary>
/// Attachment entity for file uploads and downloads.
/// Supports polymorphic relationship to attach files to tasks, comments, projects, and other entities.
/// </summary>
public class Attachment : IAggregateRoot
{
    public int Id { get; set; }
    public int OrganizationId { get; set; } // Multi-tenancy

    // Polymorphic relationship
    public string EntityType { get; set; } = string.Empty; // "Task", "Project", "Comment", etc.
    public int EntityId { get; set; } // ID of the entity

    // File information
    public string FileName { get; set; } = string.Empty; // Original filename
    public string StoredFileName { get; set; } = string.Empty; // Unique filename on disk
    public string FileExtension { get; set; } = string.Empty; // .pdf, .png, etc.
    public string ContentType { get; set; } = string.Empty; // MIME type
    public long FileSizeBytes { get; set; }
    public string StoragePath { get; set; } = string.Empty; // Relative path or cloud URL

    // Upload information
    public int UploadedById { get; set; }
    public DateTimeOffset UploadedAt { get; set; }

    // Soft delete
    public bool IsDeleted { get; set; } = false;
    public DateTimeOffset? DeletedAt { get; set; }

    // Navigation properties
    public User UploadedBy { get; set; } = null!;
}

