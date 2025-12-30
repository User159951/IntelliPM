using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IntelliPM.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the Comment entity.
/// Configures the table structure, indexes, and relationships for comments.
/// </summary>
public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("Comments");

        builder.HasKey(c => c.Id);

        // Indexes for common queries
        builder.HasIndex(c => new { c.EntityType, c.EntityId })
            .HasDatabaseName("IX_Comments_EntityType_EntityId");

        builder.HasIndex(c => c.OrganizationId)
            .HasDatabaseName("IX_Comments_OrganizationId");

        builder.HasIndex(c => c.AuthorId)
            .HasDatabaseName("IX_Comments_AuthorId");

        builder.HasIndex(c => c.CreatedAt)
            .HasDatabaseName("IX_Comments_CreatedAt");

        builder.HasIndex(c => c.ParentCommentId)
            .HasDatabaseName("IX_Comments_ParentCommentId");

        // Composite index for entity comments query (excludes soft-deleted)
        builder.HasIndex(c => new { c.EntityType, c.EntityId, c.IsDeleted, c.CreatedAt })
            .HasDatabaseName("IX_Comments_Entity_NotDeleted_CreatedAt");

        // Required fields
        builder.Property(c => c.EntityType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.EntityId)
            .IsRequired();

        builder.Property(c => c.Content)
            .IsRequired()
            .HasMaxLength(5000); // Max comment length

        builder.Property(c => c.OrganizationId)
            .IsRequired();

        builder.Property(c => c.AuthorId)
            .IsRequired();

        // Relationships
        builder.HasOne(c => c.Author)
            .WithMany()
            .HasForeignKey(c => c.AuthorId)
            .OnDelete(DeleteBehavior.Restrict); // Don't delete comments when user deleted

        // Self-referencing relationship for threading
        builder.HasOne(c => c.ParentComment)
            .WithMany(c => c.Replies)
            .HasForeignKey(c => c.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict); // Don't cascade delete replies

        // Global query filter to exclude soft-deleted comments by default
        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}

