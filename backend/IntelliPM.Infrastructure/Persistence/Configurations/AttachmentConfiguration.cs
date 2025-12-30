using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IntelliPM.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the Attachment entity.
/// Configures the table structure, indexes, and relationships for file attachments.
/// </summary>
public class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> builder)
    {
        builder.ToTable("Attachments");

        builder.HasKey(a => a.Id);

        // Indexes for queries
        builder.HasIndex(a => new { a.EntityType, a.EntityId })
            .HasDatabaseName("IX_Attachments_EntityType_EntityId");

        builder.HasIndex(a => a.OrganizationId)
            .HasDatabaseName("IX_Attachments_OrganizationId");

        builder.HasIndex(a => a.UploadedById)
            .HasDatabaseName("IX_Attachments_UploadedById");

        builder.HasIndex(a => a.UploadedAt)
            .HasDatabaseName("IX_Attachments_UploadedAt");

        builder.HasIndex(a => a.StoredFileName)
            .IsUnique()
            .HasDatabaseName("IX_Attachments_StoredFileName");

        // Composite index for entity attachments query
        builder.HasIndex(a => new { a.EntityType, a.EntityId, a.IsDeleted })
            .HasDatabaseName("IX_Attachments_Entity_NotDeleted");

        // Required fields
        builder.Property(a => a.EntityType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.EntityId)
            .IsRequired();

        builder.Property(a => a.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(a => a.StoredFileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(a => a.FileExtension)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(a => a.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.StoragePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(a => a.OrganizationId)
            .IsRequired();

        builder.Property(a => a.UploadedById)
            .IsRequired();

        // Relationships
        builder.HasOne(a => a.UploadedBy)
            .WithMany()
            .HasForeignKey(a => a.UploadedById)
            .OnDelete(DeleteBehavior.Restrict);

        // Global query filter to exclude soft-deleted attachments
        builder.HasQueryFilter(a => !a.IsDeleted);
    }
}

